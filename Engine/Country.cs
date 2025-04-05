namespace UEconomy;

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

    public int GetId()
    {
        return id;
    }

    public string GetName()
    {
        return name;
    }

    public List<Province> GetProvinces()
    {
        return provinces;
    }
}