using UEconomy.Engine.Pops;

namespace UEconomy.Engine;

public class Province
{
    public string Id { get; }
    public Market LocalMarket { get; }
    public List<Building> Buildings { get; } = new();
    public Population Population { get; private set; }

    public Province(string id, int population, Market localMarket)
    {
        Id = id;
        LocalMarket = localMarket;
        Population = new Population(population);

        // Utworzenie początkowych gospodarstw subsystencyjnych
        var subsistenceFarms = population / 10;

        for (var i = 0; subsistenceFarms > i; i++)
        {
            Buildings.Add(new Building(
                "subsistenceFarm",
                1,
                1,
                10,
                0,
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
        try
        {
            // Obliczenie potrzeb populacji i budynków
            var populationNeeds = Population.CalculateDailyConsumption();
            var buildingNeeds = new Dictionary<string, int>();

            // Zbieramy potrzeby wszystkich budynków
            foreach (var need in Buildings.Select(building => building.CalculateNeeds())
                         .SelectMany(buildingNeedsDict => buildingNeedsDict))
            {
                if (buildingNeeds.ContainsKey(need.Key))
                {
                    buildingNeeds[need.Key] += need.Value;
                }
                else
                {
                    buildingNeeds[need.Key] = need.Value;
                }
            }

            // Dodanie zamówień na rynek dla populacji
            foreach (var need in populationNeeds)
            {
                var populationSegment = Population.GetSegmentForNeed(need.Key);
                try
                {
                    LocalMarket.AddBuyOrderFromPopulation(populationSegment, need.Key, need.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd przy dodawaniu zamówienia od populacji: {ex.Message}");
                }
            }

            // Dodanie zamówień na rynek dla budynków
            foreach (var need in buildingNeeds.Where(need => Buildings.Count > 0))
            {
                try
                {
                    LocalMarket.AddBuyOrderFromBuilding(Buildings[0], need.Key, need.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd przy dodawaniu zamówienia od budynku: {ex.Message}");
                }
            }

            // Przetworzenie transakcji rynkowych
            try
            {
                LocalMarket.ProcessDailyTransactions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd przy przetwarzaniu transakcji: {ex.Message}");
            }

            // Praca wszystkich budynków
            foreach (var building in Buildings)
            {
                try
                {
                    building.Work();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd w pracy budynku {building.Id}: {ex.Message}");
                }
            }

            // Alokacja pracowników do budynków
            try
            {
                Population.AllocateWorkers(Buildings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd przy alokacji pracowników: {ex.Message}");
            }

            // Aktualizacja mobilności społecznej
            try
            {
                Population.UpdateSocialMobility();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd przy aktualizacji mobilności społecznej: {ex.Message}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Błąd w metodzie Update prowincji {Id}: {e.Message}");
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