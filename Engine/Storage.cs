namespace UEconomy.Engine;

public struct Stuff
{
    public string Id { get; }
    public string Category { get; }
    public int Amount { get; }
    public bool IsAvailable { get; }

    public Stuff(string id, string category, int amount, bool isAvailable)
    {
        Id = id;
        Category = category;
        Amount = amount;
        IsAvailable = isAvailable;
    }
}

public class Storage
{
    public List<Stuff> Items { get; } = new();

    public void Store()
    {
        var configStuffs = LoaderConfig.GetStuff();

        foreach (var configStuff in configStuffs)
        {
            Items.Add(new Stuff(configStuff.Id, configStuff.Category, 0, true));
        }
    }

    public int GetAmount(string id)
    {
        return Items
            .Where(stuff => stuff.Id == id)
            .Select(stuff => stuff.Amount)
            .FirstOrDefault();
    }

    public void AddAmount(string id, int amount)
    {
        var stuff = Items.First(item => item.Id == id);
        var index = Items.IndexOf(stuff);
        Items[index] = new Stuff(stuff.Id, stuff.Category, stuff.Amount + amount, true);
    }
}