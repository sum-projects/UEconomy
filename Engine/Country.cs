namespace UEconomy.Engine;

public class Country
{
    public string Id { get; }
    public Market NationalMarket { get; } = new();
    public List<Province> Provinces { get; }

    public Country(string id, List<Province> provinces)
    {
        Id = id;
        Provinces = provinces;
    }

    public void Update()
    {
        foreach (var province in Provinces)
        {
            province.Update();
        }
    }

    // public Dictionary<string, Dictionary<string, int>> GetProvinceStatistics()
    // {
    //     var countryStats = new Dictionary<string, Dictionary<string, int>>();
    //
    //     foreach (var province in Provinces)
    //     {
    //         countryStats[province.Id] = province.GetMarketStatistics();
    //     }
    //
    //     return countryStats;
    // }
}