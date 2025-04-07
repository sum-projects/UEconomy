namespace UEconomy.Engine;

public class Game
{
    public int CurrentDay { get; private set; } = 1;

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

        foreach (var province in Countries.SelectMany(country => country.Provinces))
        {
            InitializeMarketForProvince(province);
        }
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

    private void InitializeMarketForProvince(Province province)
    {
        var allStuffs = LoaderConfig.GetStuff();

        var marketStorage = new MarketStorage();
        marketStorage.Store();

        foreach (var stuff in allStuffs)
        {
            var initialAmount = stuff.Category switch
            {
                "material" => new Random().Next(50, 200),
                "goods" => new Random().Next(20, 100),
                "construction" => new Random().Next(10, 50),
                _ => 0
            };

            if (initialAmount > 0)
            {
                marketStorage.AddAmount(stuff.Id, initialAmount);
            }
        }
    }
}