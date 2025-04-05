namespace UEconomy;

public class Storage<T>
{
    private Dictionary<T, int> items = new();

    public void Store(Stuff<T> stuff)
    {
        if (items.ContainsKey(stuff.Type))
        {
            items[stuff.Type] += stuff.Amount;
        }
        else
        {
            items[stuff.Type] = stuff.Amount;
        }
    }

    public bool TryTake(T type, int amount, out Stuff<T> takenStuff)
    {
        if (items.ContainsKey(type) && items[type] >= amount)
        {
            items[type] -= amount;
            takenStuff = new Stuff<T>(type, amount);
            return true;
        }

        takenStuff = default;
        return false;
    }

    public int GetAvailableAmount(T type)
    {
        return items.ContainsKey(type) ? items[type] : 0;
    }
}