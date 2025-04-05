namespace UEconomy;

public enum StuffType
{
    Tool,
    Glass,
    Cloth,
    Plank,
    Furniture,
}

public enum FoodType
{
    Wheat,
    Rice,
    Maize,
}

public enum ResourceType
{
    Iron,
    Coal,
    Wood,
}

public struct Stuff<T>
{
    public T Type { get; }
    public int Amount { get; }

    public Stuff(T type, int amount)
    {
        Type = type;
        Amount = amount;
    }

    public Stuff<T> WithAmount(int newAmount)
    {
        return new Stuff<T>(Type, newAmount);
    }
}