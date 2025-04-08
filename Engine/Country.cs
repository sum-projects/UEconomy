namespace UEconomy.Engine;

public class Country
{
    public string Id { get; }
    public Market NationalMarket { get; }
    public List<Province> Provinces { get; }

    public Country(string id, List<Province> provinces, Market market)
    {
        Id = id;
        Provinces = provinces;
        NationalMarket = market;
    }

    public void Update()
    {
        foreach (var province in Provinces)
        {
            province.Update();
        }
    }
}