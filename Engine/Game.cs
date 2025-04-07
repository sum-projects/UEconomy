namespace UEconomy.Engine;

public class Game
{
    public List<Country> Countries { get; }
    public List<BuildingStruct> ListAvailableBuildings;

    public Game()
    {
        Countries = LoaderConfig
            .GetCountries()
            .Select(
                c => new Country(c.Id, c.Provinces.Select(
                    p => new Province(p.Id, p.Population)
                ).ToList())
            ).ToList();


        ListAvailableBuildings = LoaderConfig.GetBuildings();
    }

    public void Update()
    {
        foreach (var country in Countries)
        {
            country.Update();
        }
    }

    public Province? GetProvinceById(string provinceId)
    {
        return Countries.SelectMany(country => country.Provinces)
            .FirstOrDefault(province => province.Id == provinceId);
    }

    public BuildingStruct? GetBuildingStruct(string buildingId)
    {
        return ListAvailableBuildings.FirstOrDefault(building => building.Id == buildingId);
    }
}