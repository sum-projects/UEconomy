using System;
using System.Collections.Generic;
using System.Linq;

namespace UEconomy.Game;

public class Population
{
    public int Size { get; set; }
    public Dictionary<string, float> Needs { get; set; } = new();
    public Dictionary<string, float> Satisfaction { get; set; } = new();
    public float Wealth { get; set; }
    public int ProvinceId { get; set; }

    public void InitializeNeeds()
    {
        // Podstawowe potrzeby na osobę
        Needs["Zboże"] = 0.5f;
        Needs["Meble"] = 0.05f;
        Needs["Ubrania"] = 0.08f;

        // Inicjalizacja zadowolenia
        foreach (var need in Needs)
        {
            Satisfaction[need.Key] = 1.0f; // Początkowe zadowolenie 100%
        }
    }

    public void UpdateNeeds()
    {
        // Aktualizacja potrzeb na podstawie poziomu zadowolenia
        foreach (var resource in Satisfaction.Keys.ToList())
        {
            switch (Satisfaction[resource])
            {
                // Jeśli zadowolenie spada, potrzeby rosną (ludzie chcą więcej)
                case < 0.7f:
                {
                    var needIncreaseFactor = 1.0f + (0.7f - Satisfaction[resource]) * 0.5f;
                    Needs[resource] *= needIncreaseFactor;
                    break;
                }
                // Jeśli zadowolenie jest wysokie, potrzeby nieznacznie maleją
                case > 0.9f:
                    Needs[resource] *= 0.98f;
                    break;
            }
        }
    }

    public void ConsumptionStep(Dictionary<string, float> availableResources)
    {
        // Resetuj poziomy zadowolenia do niskich wartości
        foreach (var resource in Needs.Keys)
        {
            Satisfaction[resource] = 0.1f;
        }

        // Oblicz całkowite zapotrzebowanie
        var totalDemand = new Dictionary<string, float>();
        foreach (var need in Needs)
        {
            totalDemand[need.Key] = need.Value * Size;
        }

        // Zaspokoić potrzeby przy użyciu dostępnych zasobów
        foreach (var resource in totalDemand.Keys)
        {
            if (availableResources.ContainsKey(resource) && availableResources[resource] > 0)
            {
                var consumed = Math.Min(availableResources[resource], totalDemand[resource]);
                availableResources[resource] -= consumed;

                // Oblicz poziom zadowolenia (ile % potrzeb zostało zaspokojonych)
                var satisfactionLevel = consumed / totalDemand[resource];
                Satisfaction[resource] = satisfactionLevel;
            }
        }

        // Aktualizuj potrzeby na podstawie nowego poziomu zadowolenia
        UpdateNeeds();
    }

    public Dictionary<string, float> CalculateMaxPrices()
    {
        var maxPrices = new Dictionary<string, float>();
        var baseWealthPerCapita = Wealth / Size;

        foreach (var need in Needs)
        {
            // Podstawowa cena, którą populacja jest gotowa zapłacić
            var basePrice = baseWealthPerCapita * 0.05f;

            // Jak bardzo niezaspokojona jest potrzeba (1.0 = całkowicie, 0.0 = w pełni zaspokojona)
            var needUrgency = 1.0f - Satisfaction[need.Key];

            // Oblicz maksymalną cenę, jaką populacja jest gotowa zapłacić
            // Im niższe zadowolenie, tym więcej są gotowi zapłacić
            maxPrices[need.Key] = basePrice * (1.0f + needUrgency * 2.0f);
        }

        return maxPrices;
    }
}