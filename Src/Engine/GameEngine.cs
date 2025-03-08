using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UEconomy.Game;

namespace UEconomy.Engine;

public class GameEngine
{
    public Market _market { get; private set; }
    public List<Population> _populations { get; private set; }
    public List<Province> _provinces { get; private set; }

    public Dictionary<string, List<float>> _priceHistory { get; private set; } = new();
    public Dictionary<string, float> _totalTraded { get; private set; } = new();

    public Stats Stats;

    public GameEngine(Stats stats)
    {
        Stats = stats;
    }

    public void Initialize(int cellSize, int gridWidth, int gridHeight)
    {
        // Inicjalizacja prowincji
        _provinces = new List<Province>();
        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                var province = new Province
                {
                    Id = _provinces.Count,
                    Name = $"Province {x}, {y}",
                    Position = new Vector2(x * cellSize, y * cellSize),
                    Resources = new Dictionary<string, float>
                    {
                        { "Zboże", 50 },
                        { "Drewno", 50 },
                        { "Żelazo", 30 },
                        { "Węgiel", 30 },
                        { "Meble", 20 },
                        { "Ubrania", 20 }
                    }
                };

                // Dodaj przykładowe budynki w zależności od lokalizacji
                if (x % 3 == 0)
                    province.AddBuilding("FieldCrop");
                if (y % 3 == 0)
                    province.AddBuilding("Sawmill");
                if ((x + y) % 4 == 0)
                    province.AddBuilding("IronMine");
                if ((x + y) % 5 == 0)
                    province.AddBuilding("CoalMine");
                if (x % 4 == 0 && y % 2 == 0)
                    province.AddBuilding("FurnitureFactory");
                if (x % 2 == 0 && y % 4 == 0)
                    province.AddBuilding("TailorShop");

                _provinces.Add(province);
            }
        }

        // Inicjalizacja populacji
        _populations = new List<Population>();
        Random random = new();

        foreach (
            var population in _provinces.Select(province => new Population
            {
                Size = random.Next(100, 1000),
                Wealth = random.Next(500, 2000),
                ProvinceId = province.Id
            }))
        {
            population.InitializeNeeds();
            _populations.Add(population);
        }

        _market = new Market();
        _market.Initialize(new[] { "Zboże", "Drewno", "Żelazo", "Węgiel", "Meble", "Ubrania" });

        // Inicjalizuj historię cen
        foreach (var resource in _market.GetAllResources())
        {
            _priceHistory[resource] = new List<float> { _market.GetPrice(resource) };
            _totalTraded[resource] = 0;
        }
    }

    public void Update(GameTime gameTime)
    {
        // Symulacja ekonomii co kilka sekund
        if (gameTime.TotalGameTime.TotalSeconds % 3 < 0.016)
        {
            SimulateEconomy();

            foreach (var resource in _market.GetAllResources())
            {
                _priceHistory[resource].Add(_market.GetPrice(resource));
                if (_priceHistory[resource].Count > 20)
                {
                    _priceHistory[resource].RemoveAt(0);
                }
            }

            Stats.UpdateGlobalStats(_market.GetAllResources(), _populations, _provinces);
        }
    }

    private void SimulateEconomy()
    {
        // Aktualizuj produkcję w każdej prowincji
        foreach (var province in _provinces)
        {
            var population = _populations.Find(p => p.ProvinceId == province.Id);
            if (population != null)
            {
                province.UpdateProduction(population);
            }
        }

        // Aktualizuj produkcję w każdej prowincji
        _market.ResetOffers();

        // Resetujemy statystyki handlowe
        foreach (var resource in _market.GetAllResources())
        {
            _totalTraded[resource] = 0;
        }

        // Każda populacja próbuje zaspokoić swoje potrzeby
        foreach (var population in _populations)
        {
            // Znajdź prowincję, w której znajduje się populacja
            var province = _provinces.Find(p => p.Id == population.ProvinceId);

            // Pobierz zasoby z prowincji i oblicz poziom zadowolenia
            var availableResources = new Dictionary<string, float>(province.Resources);
            population.ConsumptionStep(availableResources);

            // Aktualizuj zasoby prowincji po konsumpcji
            province.Resources = availableResources;

            // Oblicz maksymalne ceny, jakie populacja jest gotowa zapłacić
            var maxPrices = population.CalculateMaxPrices();

            foreach (var (resourceName, demandPerCapita) in population.Needs)
            {
                var totalDemand = demandPerCapita * population.Size;

                // Sprawdź, ile zasobów jest dostępnych w prowincji
                if (province.Resources.TryGetValue(resourceName, out var availableResource) && availableResource > 0)
                {
                    // Używaj lokalnych zasobów najpierw
                    var usedResource = Math.Min(availableResource, totalDemand);
                    province.Resources[resourceName] -= usedResource;
                    totalDemand -= usedResource;
                }

                // Jeśli nadal są niezaspokojone potrzeby, stwórz popyt na rynku
                if (totalDemand > 0)
                {
                    var maxPrice = maxPrices.TryGetValue(resourceName, out var price)
                        ? price
                        : population.Wealth / (population.Size * 2);
                    _market.AddDemand(resourceName, totalDemand, maxPrice, population);
                }
            }

            foreach (var resource in province.Resources)
            {
                if (resource.Value > 50)
                {
                    var surplusToSell = (resource.Value - 50) / 2;
                    const int minPrice = 10;
                    _market.AddSupply(resource.Key, surplusToSell, minPrice, province);
                }
            }
        }

        _market.ResolveTrades();

        foreach (var resource in _market.GetAllResources())
        {
            _totalTraded[resource] = _market.GetLastTradedAmount(resource);
        }
    }
}