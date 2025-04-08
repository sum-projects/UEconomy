using UEconomy.Engine.Pops;

namespace UEconomy.Engine;

public struct MarketStuff
{
    public string Id { get; }
    public string Category { get; }
    public int Amount { get; }

    public double Price { get; }

    public MarketStuff(string id, string category, int amount, double price)
    {
        Id = id;
        Category = category;
        Amount = amount;
        Price = price;
    }
}

public class MarketStorage
{
    public List<MarketStuff> Items { get; }

    public MarketStorage(List<MarketStuff> items)
    {
        Items = items;
    }

    public int GetAmount(string id)
    {
        return Items
            .Where(stuff => stuff.Id == id)
            .Select(stuff => stuff.Amount)
            .FirstOrDefault();
    }

    public double GetPrice(string id)
    {
        return Items
            .Where(stuff => stuff.Id == id)
            .Select(stuff => stuff.Price)
            .FirstOrDefault();
    }

    public void AddAmount(string id, int amount)
    {
        var stuff = Items.First(item => item.Id == id);
        var index = Items.IndexOf(stuff);
        Items[index] = new MarketStuff(stuff.Id, stuff.Category, stuff.Amount + amount, stuff.Price);
    }

    public void ChangePrice(string id, double price)
    {
        var stuff = Items.First(item => item.Id == id);
        var index = Items.IndexOf(stuff);
        Items[index] = new MarketStuff(stuff.Id, stuff.Category, stuff.Amount, price);
    }
}

public enum TransactionType
{
    Buy,
    Sell,
    Transfer
}

public enum Priority
{
    Low,
    Medium,
    High
}

public class MarketTransaction
{
    public string ItemId { get; set; }
    public int Amount { get; set; }
    public double Price { get; set; }
    public string BuyerId { get; set; }
    public string SellerId { get; set; }
    public TransactionType Type { get; set; }
    public Priority Priority { get; set; }
}

public class Market
{
    private MarketStorage Storage { get; }
    public List<MarketTransaction> PendingTransactions { get; } = new();

    public Market(MarketStorage storage)
    {
        Storage = storage;
    }

    public void ProcessDailyTransactions()
    {
        try
        {
            // Sortujemy transakcje według priorytetu (najpierw wysoki)
            var sortedTransactions = PendingTransactions
                .OrderByDescending(t => t.Priority)
                .ToList();

            // Przetwarzamy każdą transakcję
            foreach (var transaction in sortedTransactions)
            {
                ProcessTransaction(transaction);
            }

            // Aktualizacja cen na podstawie podaży i popytu
            UpdatePrices();

            // Czyszczenie po zakończeniu dnia
            PendingTransactions.Clear();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Błąd podczas przetwarzania transakcji: {e.Message}");
        }
    }

    private void ProcessTransaction(MarketTransaction transaction)
    {
        try
        {
            // Sprawdzenie dostępności towaru
            var itemAmount = Storage.GetAmount(transaction.ItemId);
            var itemPrice = Storage.GetPrice(transaction.ItemId);

            switch (transaction.Type)
            {
                case TransactionType.Buy:
                    // Nie można kupić więcej niż jest dostępne
                    var actualAmount = Math.Min(transaction.Amount, itemAmount);
                    if (actualAmount > 0)
                    {
                        // Aktualizacja ilości na rynku
                        Storage.AddAmount(transaction.ItemId, -actualAmount);

                        // Tutaj można zaimplementować transfer pieniędzy między kupującym a sprzedającym
                        Console.WriteLine(
                            $"Zakupiono {actualAmount} jednostek {transaction.ItemId} za {itemPrice * actualAmount} monet");
                    }

                    break;

                case TransactionType.Sell:
                    // Dla sprzedaży po prostu dodajemy towar do rynku
                    Storage.AddAmount(transaction.ItemId, transaction.Amount);
                    Console.WriteLine(
                        $"Sprzedano {transaction.Amount} jednostek {transaction.ItemId} za {itemPrice * transaction.Amount} monet");
                    break;

                case TransactionType.Transfer:
                    // Transfer między podmiotami (np. między prowincjami)
                    // To wymaga bardziej zaawansowanej implementacji
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Błąd podczas przetwarzania pojedynczej transakcji: {e.Message}");
        }
    }

    public void AddBuyOrderFromPopulation(PopulationSegment segment, string itemId, int amount)
    {
        if (amount <= 0) return;

        var transaction = new MarketTransaction
        {
            ItemId = itemId,
            Amount = amount,
            Price = Storage.GetPrice(itemId),
            BuyerId = segment.SocialClass.ToString(),
            SellerId = "Market",
            Type = TransactionType.Buy,
            Priority = segment.SocialClass switch
            {
                SocialClass.UpperClass => Priority.High,
                SocialClass.MiddleClass => Priority.Medium,
                _ => Priority.Low
            }
        };

        PendingTransactions.Add(transaction);
    }

    public void AddBuyOrderFromBuilding(Building building, string itemId, int amount)
    {
        if (amount <= 0) return;

        var transaction = new MarketTransaction
        {
            ItemId = itemId,
            Amount = amount,
            Price = Storage.GetPrice(itemId),
            BuyerId = building.Id,
            SellerId = "Market",
            Type = TransactionType.Buy,
            Priority = Priority.High // Budynki mają zazwyczaj wyższy priorytet
        };

        PendingTransactions.Add(transaction);
    }

    public void AddSellOrder(string sellerId, string itemId, int amount, Priority priority = Priority.Medium)
    {
        if (amount <= 0) return;

        var transaction = new MarketTransaction
        {
            ItemId = itemId,
            Amount = amount,
            Price = Storage.GetPrice(itemId),
            BuyerId = "Market",
            SellerId = sellerId,
            Type = TransactionType.Sell,
            Priority = priority
        };

        PendingTransactions.Add(transaction);
    }

    public Dictionary<string, List<Dictionary<string, double>>> GetMarketStatistics()
    {
        return Storage.Items.ToDictionary(
            storage => storage.Id, storage => new List<Dictionary<string, double>>
            {
                new()
                {
                    { "Amount", storage.Amount },
                    { "Price", storage.Price }
                }
            });
    }

    private void UpdatePrices()
    {
        foreach (var item in Storage.Items)
        {
            var demand = PendingTransactions
                .Where(t => t.ItemId == item.Id && t.Type == TransactionType.Buy)
                .Sum(t => t.Amount);

            var supply = item.Amount + PendingTransactions
                .Where(t => t.ItemId == item.Id && t.Type == TransactionType.Sell)
                .Sum(t => t.Amount);

            var newPrice = item.Price;

            if (demand > supply)
            {
                newPrice = item.Price * (1.0 + (demand - supply) / (double)Math.Max(1, supply) * 0.1);
            }
            else if (supply > demand)
            {
                newPrice = item.Price * (1.0 - (supply - demand) / (double)Math.Max(1, demand) * 0.05);
            }

            newPrice = Math.Max(0.1, Math.Min(newPrice, item.Price * 1.5));
            Storage.ChangePrice(item.Id, newPrice);
        }
    }
}