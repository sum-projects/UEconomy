namespace UEconomy;

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

    public int GetId()
    {
        return id;
    }

    public string GetName()
    {
        return name;
    }

    public int GetPopCount()
    {
        return pops.Count;
    }

    public List<object> GetAllBuildings()
    {
        var allBuildings = new List<object>();

        foreach (var factory in factories)
        {
            allBuildings.Add(new {
                type = factory.GetType().Name,
                level = factory.GetLevel(),
                currentEmployees = factory.GetCurrentEmployees(),
                maxEmployees = factory.GetMaxEmployees()
            });
        }

        foreach (var farm in farms)
        {
            allBuildings.Add(new {
                type = farm.GetType().Name,
                level = farm.GetLevel(),
                currentEmployees = farm.GetCurrentEmployees(),
                maxEmployees = farm.GetMaxEmployees()
            });
        }

        foreach (var mine in mines)
        {
            allBuildings.Add(new {
                type = mine.GetType().Name,
                level = mine.GetLevel(),
                currentEmployees = mine.GetCurrentEmployees(),
                maxEmployees = mine.GetMaxEmployees()
            });
        }

        return allBuildings;
    }
}