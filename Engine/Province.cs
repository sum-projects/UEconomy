namespace UEconomy.Engine;

public class Province
{
    public string Id { get; }
    public int Population { get; }
    public Market LocalMarket { get; }
    public List<Building> Buildings { get; } = new();

    public Province(string id, int population, Market localMarket)
    {
        Id = id;
        Population = population;
        LocalMarket = localMarket;

        var subsistenceFarms = population / 10;

        for (var i = 0; subsistenceFarms > i; i++)
        {
            Buildings.Add(new Building(
                "subsistenceFarm",
                1,
                1,
                1,
                1,
                new List<Dictionary<string, int>>(),
                new List<Dictionary<string, int>>
                {
                    new()
                    {
                        { "wheat", 5 },
                        { "clothe", 1 },
                        { "furniture", 1 },
                        { "wood", 2 }
                    }
                },
                new BuildingStorage(),
                new BuildingStorage()
            ));
        }
    }

    public void Update()
    {
        var populationNeeds = Population.CalculateDailyConsumption();
        var buildingNeeds = Buildings.SelectMany(b => b.CalculateNeeds()).ToList();

        foreach (var need in populationNeeds)
        {
            LocalMarket.AddBuyOrderFromPopulation(Population.GetSegmentForNeed(need.Key), need.Key, need.Value);
        }

        foreach (var building in Buildings)
        {
            foreach (var need in building.CalculateNeeds())
            {
                LocalMarket.AddBuyOrderFromBuilding(building, need.Key, need.Value);
            }
        }

        LocalMarket.ProcessDailyTransactions();

        foreach (var building in Buildings)
        {
            building.Work();
        }

        Population.AllocateWorkers(Buildings);

        Population.UpdateSocialMobility();
    }

    public void AddBuilding(Building building)
    {
        Buildings.Add(building);
    }

    public List<object> GetAllBuildings()
    {
        var allBuildings = new List<object>();

        foreach (var building in Buildings)
        {
            allBuildings.Add(new
            {
                id = building.Id,
                level = building.Level,
                currentEmployees = building.CurrentEmployees,
                maxEmployees = building.MaxEmployeesPerLevel,
            });
        }

        return allBuildings;
    }
}