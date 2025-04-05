namespace UEconomy;

public class Pop
{
    private int id;
    private int food = 100;
    private int cloth = 100;
    private int furniture = 50;
    private readonly Random random = new();

    private const double FoodProductionBase = 1.1;
    private const double ClothProductionBase = 0.02;
    private const double FurnitureProductionBase = 0.01;
    private const double WoodProductionBase = 0.1;

    public Pop(int id)
    {
        this.id = id;
    }

    public (Stuff<FoodType>, Stuff<StuffType>, Stuff<StuffType>, Stuff<ResourceType>) ProduceSubsistence()
    {
        var foodType = (FoodType)random.Next(0, Enum.GetValues(typeof(FoodType)).Length);
        const int foodProduced = (int)(FoodProductionBase * 100);
        const int foodConsumption = 90;
        var foodSurplus = Math.Max(0, foodProduced - foodConsumption);
        food += foodProduced - foodSurplus - foodConsumption;

        const int clothProduced = (int)(ClothProductionBase * 100);
        const int clothConsumption = 2;
        var clothSurplus = Math.Max(0, clothProduced - clothConsumption);
        cloth += clothProduced - clothSurplus - clothConsumption;

        const int furnitureProduced = (int)(FurnitureProductionBase * 100);
        const int furnitureConsumption = 1;
        var furnitureSurplus = Math.Max(0, furnitureProduced - furnitureConsumption);
        furniture += furnitureProduced - furnitureSurplus - furnitureConsumption;

        const int woodProduced = (int)(WoodProductionBase * 100);
        const int woodConsumption = furnitureProduced * 2;
        var woodSurplus = Math.Max(0, woodProduced - woodConsumption);

        return (
            new Stuff<FoodType>(foodType, foodSurplus),
            new Stuff<StuffType>(StuffType.Cloth, clothSurplus),
            new Stuff<StuffType>(StuffType.Furniture, furnitureSurplus),
            new Stuff<ResourceType>(ResourceType.Wood, woodSurplus)
        );
    }

    public void Consume()
    {
        food = Math.Max(0, food - 10);
        cloth = Math.Max(0, cloth - 1);
        furniture = Math.Max(0, furniture - 1);
    }

    public bool IsHealthy()
    {
        return food > 0 && cloth > 0 && furniture > 0;
    }

    public int GetFoodLevel() => food;
    public int GetClothLevel() => cloth;
    public int GetFurnitureLevel() => furniture;
}

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

public class Province
{
    private int id;
    private string name;
    private List<Pop> pops = new();
    private Market localMarket = new();
    private List<Building<StuffType>> factories = new();
    private List<Building<FoodType>> farms = new();
    private List<Building<ResourceType>> mines = new();

    public Province(int id, string name, int pops)
    {
        this.id = id;
        this.name = name;
        for (var i = 0; i < pops; i++)
        {
            this.pops.Add(new Pop(i));
        }
    }

    public void Update()
    {
        foreach (var pop in pops)
        {
            var (foodSurplus, clothSurplus, furnitureSurplus, woodSurplus) = pop.ProduceSubsistence();

            localMarket.AddFood(foodSurplus);
            localMarket.AddProduct(clothSurplus);
            localMarket.AddProduct(furnitureSurplus);
            localMarket.AddResource(woodSurplus);

            pop.Consume();
        }

        foreach (var factory in factories)
        {
            factory.Work();
        }

        foreach (var farm in farms)
        {
            farm.Work();
        }

        foreach (var mine in mines)
        {
            mine.Work();
        }
    }

    public Dictionary<string, int> GetMarketStatistics()
    {
        return localMarket.GetMarketStatistics();
    }

    public void AddFactory(Building<StuffType> factory)
    {
        factories.Add(factory);
    }

    public void AddFarm(Building<FoodType> farm)
    {
        farms.Add(farm);
    }

    public void AddMine(Building<ResourceType> mine)
    {
        mines.Add(mine);
    }
}

public class Country
{
    private int id;
    private string name;
    private Market nationalMarket = new();
    private List<Province> provinces;


    public Country(int id, string name, List<Province> provinces)
    {
        this.id = id;
        this.name = name;
        this.provinces = provinces;
    }

    public void Update()
    {
        foreach (var province in provinces)
        {
            province.Update();
        }
    }

    public Dictionary<string, Dictionary<string, int>> GetProvinceStatistics()
    {
        var countryStats = new Dictionary<string, Dictionary<string, int>>();

        foreach (var province in provinces)
        {
            countryStats[province.ToString()] = province.GetMarketStatistics();
        }

        return countryStats;
    }
}

public class Game
{
    private int speed = 1;
    private List<Country> countries = new();
    private int currentDay = 0;

    public Game(List<Country> countries)
    {
        this.countries = countries;
    }

    public void Update()
    {
        currentDay++;
        Console.WriteLine($"Day {currentDay}");

        foreach (var country in countries)
        {
            country.Update();
        }
    }
}