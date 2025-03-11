using System.Collections.Generic;
using System.Linq;
using UEconomy.Game;

namespace UEconomy.Game;

public class Stats
{
    public Dictionary<string, float> GlobalStats { get; } = new();

    public void UpdateGlobalStats(string[] resources, List<Population> populations, List<Province> provinces)
    {
        // Całkowita populacja
        GlobalStats["Total Population"] = populations.Sum(p => p.Size);

        // Całkowite zasoby
        foreach (var resource in resources)
        {
            GlobalStats[$"Total {resource}"] = provinces.Sum(p =>
                p.Resources.TryGetValue(resource, out var pResource) ? pResource : 0);

            // Globalna produkcja
            var totalProduction = 0.0f;
            foreach (var province in provinces)
            {
                totalProduction += province.Buildings
                    .Where(building => building.IsActive && building.ProductionRates.ContainsKey(resource))
                    .Sum(building => building.ProductionRates[resource] * building.CurrentWorkers /
                        building.WorkersCapacity * building.Level);

                // Dodaj produkcję niezatrudnionych
                if (resource == "Zboże" || resource == "Meble" || resource == "Ubrania")
                {
                    var subsistenceFactor = province.AvailableWorkers / 10.0f;
                    if (resource == "Zboże")
                        totalProduction += 10.0f * subsistenceFactor;
                    else
                        totalProduction += 1.0f * subsistenceFactor;
                }
            }

            GlobalStats[$"Production {resource}"] = totalProduction;

            // Średnie zapotrzebowanie na zasoby
            var totalDemand = populations
                .Where(population => population.Needs.ContainsKey(resource))
                .Sum(population => population.Needs[resource] * population.Size);

            GlobalStats[$"{resource} Demand"] = totalDemand;

            // Średnie zadowolenie z zaspokojenia potrzeb
            var totalSatisfaction = 0.0f;
            var populationsWithNeed = 0;
            foreach (
                var population in populations.Where(population => population.Satisfaction.ContainsKey(resource))
            )
            {
                totalSatisfaction += population.Satisfaction[resource];
                populationsWithNeed++;
            }

            if (populationsWithNeed > 0)
            {
                GlobalStats[$"Satisfaction {resource}"] = totalSatisfaction / populationsWithNeed;
            }
        }

        // Całkowita zamożność
        GlobalStats["Total Wealth"] = populations.Sum(p => p.Wealth);

        // Suma pracowników w budynkach
        GlobalStats["Employed Workers"] = provinces.Sum(p => p.Buildings.Sum(b => b.CurrentWorkers));

        // Całkowita liczba dostępnych pracowników
        GlobalStats["Available Workers"] = provinces.Sum(p => p.AvailableWorkers);
    }
}