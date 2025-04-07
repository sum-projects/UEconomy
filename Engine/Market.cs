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
    public List<MarketStuff> Items { get; } = new();

    public void Store()
    {
        var configStuffs = LoaderConfig.GetStuff();

        foreach (var configStuff in configStuffs)
        {
            Items.Add(new MarketStuff(configStuff.Id, configStuff.Category, 0, 1));
        }
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

public class Market
{
    private MarketStorage marketStorage = new();

    public Dictionary<string, List<Dictionary<string, double>>> GetMarketStatistics()
    {
        return marketStorage.Items.ToDictionary(
            storage => storage.Id, storage => new List<Dictionary<string, double>>
            {
                new()
                {
                    { "Amount", storage.Amount },
                    { "Price", storage.Price }
                }
            });
    }
}