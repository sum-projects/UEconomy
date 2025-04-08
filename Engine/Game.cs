namespace UEconomy.Engine;

public class Game
{
    public int CurrentDay { get; private set; } = 1;

    public List<Country> Countries { get; }
    public List<BuildingStruct> ListAvailableBuildings;

    public Game()
    {
        var marketStorage = new MarketStorage(
            LoaderConfig.GetStuff().Select(s => new MarketStuff(s.Id, s.Category, 0, 1)).ToList()
        );
        Countries = LoaderConfig
            .GetCountries()
            .Select(
                c => new Country(
                    c.Id,
                    c.Provinces.Select(p => new Province(p.Id, p.Population, new Market(marketStorage))).ToList(),
                    new Market(marketStorage)
                )
            ).ToList();


        ListAvailableBuildings = LoaderConfig.GetBuildings();
    }

    public void Update()
    {
        foreach (var country in Countries)
        {
            country.Update();
        }

        CurrentDay++;
    }

    public Province? GetProvinceById(string provinceId)
    {
        return Countries.SelectMany(country => country.Provinces)
            .FirstOrDefault(province => province.Id == provinceId);
    }

    public BuildingStruct GetBuildingStruct(string buildingId)
    {
        return ListAvailableBuildings.First(building => building.Id == buildingId);
    }
}