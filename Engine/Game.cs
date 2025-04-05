namespace UEconomy;

public class Game
{
    private int speed = 1;
    private List<Country> countries;
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

    public int GetCurrentDay()
    {
        return currentDay;
    }

    public List<Country> GetCountries()
    {
        return countries;
    }

    public Province? GetProvinceById(int provinceId)
    {
        return countries.SelectMany(country => country.GetProvinces()).FirstOrDefault(province => province.GetId() == provinceId);
    }
}