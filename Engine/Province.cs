namespace UEconomy.Engine;

public class Province
{
    public string Id { get; }
    public int Population { get; }
    public Market LocalMarket { get; } = new();
    public List<Building> Buildings { get; } = new();

    public Province(string id, int population)
    {
        Id = id;
        Population = population;
    }

    public void Update()
    {
        foreach (var building in Buildings)
        {
            building.Work();
        }
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