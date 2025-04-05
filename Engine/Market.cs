namespace UEconomy;

public class Market
{
    private Storage<FoodType> foodStorage = new();
    private Storage<StuffType> productStorage = new();
    private Storage<ResourceType> resourceStorage = new();

    private Dictionary<FoodType, double> foodPrices = new()
    {
        { FoodType.Wheat, 1.0 },
        { FoodType.Rice, 1.2 },
        { FoodType.Maize, 0.9 }
    };

    private Dictionary<StuffType, double> productPrices = new()
    {
        { StuffType.Tool, 5.0 },
        { StuffType.Glass, 3.0 },
        { StuffType.Cloth, 2.0 },
        { StuffType.Plank, 1.5 },
        { StuffType.Furniture, 8.0 }
    };

    private Dictionary<ResourceType, double> resourcePrices = new()
    {
        { ResourceType.Iron, 2.0 },
        { ResourceType.Coal, 1.5 },
        { ResourceType.Wood, 1.0 }
    };

    public void AddFood(Stuff<FoodType> food)
    {
        if (food.Amount > 0)
            foodStorage.Store(food);
    }

    public void AddProduct(Stuff<StuffType> product)
    {
        if (product.Amount > 0)
            productStorage.Store(product);
    }

    public void AddResource(Stuff<ResourceType> resource)
    {
        if (resource.Amount > 0)
            resourceStorage.Store(resource);
    }

    public bool BuyFood(FoodType type, int amount, out Stuff<FoodType> boughtFood)
    {
        return foodStorage.TryTake(type, amount, out boughtFood);
    }

    public bool BuyProduct(StuffType type, int amount, out Stuff<StuffType> boughtProduct)
    {
        return productStorage.TryTake(type, amount, out boughtProduct);
    }

    public bool BuyResource(ResourceType type, int amount, out Stuff<ResourceType> boughtResource)
    {
        return resourceStorage.TryTake(type, amount, out boughtResource);
    }

    public int GetAvailableFood(FoodType type)
    {
        return foodStorage.GetAvailableAmount(type);
    }

    public int GetAvailableProduct(StuffType type)
    {
        return productStorage.GetAvailableAmount(type);
    }

    public int GetAvailableResource(ResourceType type)
    {
        return resourceStorage.GetAvailableAmount(type);
    }

    public Dictionary<string, int> GetMarketStatistics()
    {
        var stats = new Dictionary<string, int>();

        var totalFood = Enum.GetValues(typeof(FoodType)).Cast<FoodType>().Sum(GetAvailableFood);
        stats["TotalFood"] = totalFood;

        var totalProducts = Enum.GetValues(typeof(StuffType)).Cast<StuffType>().Sum(GetAvailableProduct);
        stats["TotalProducts"] = totalProducts;

        var totalResources = Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>().Sum(GetAvailableResource);
        stats["TotalResources"] = totalResources;

        return stats;
    }
}