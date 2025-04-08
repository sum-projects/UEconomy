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
    }

    public void AddBuyOrderFromPopulation(PopulationSegment segment, string itemId, int amount)
    {
    }

    public void AddBuyOrderFromBuilding(Building building, string itemId, int amount)
    {
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