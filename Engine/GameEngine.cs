using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UEconomy.Engine.Interfaces;
using UEconomy.Game;

namespace UEconomy.Engine;

public class GameEngine : IGameEngine
{
    private readonly Stats _stats;
    private readonly List<Province> _provinces = new();
    private readonly List<Population> _populations = new();
    private readonly Market _market = new();

    // Game simulation parameters
    private const float SimulationTimeStep = 1.0f; // seconds
    private float _elapsedTime = 0;

    public GameEngine(Stats stats)
    {
        _stats = stats;
    }

    public void Initialize(int cellSize, int gridWidth, int gridHeight)
    {
        InitializeProvinces(cellSize, gridWidth, gridHeight);
        InitializePopulations();
        InitializeMarket();
    }

    private void InitializeProvinces(int cellSize, int gridWidth, int gridHeight)
    {
        // Create a grid of provinces
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                var province = new Province
                {
                    Id = y * gridWidth + x,
                    Name = $"Province {y * gridWidth + x}",
                    Position = new Vector2(x * cellSize, y * cellSize),
                    Resources =
                    {
                        // Initialize resources
                        ["Zboże"] = 200,
                        ["Drewno"] = 150,
                        ["Żelazo"] = 100,
                        ["Węgiel"] = 100,
                        ["Meble"] = 50,
                        ["Ubrania"] = 50
                    }
                };

                // Add some random buildings
                if ((x + y) % 3 == 0) province.AddBuilding("FieldCrop");
                if ((x + y) % 3 == 1) province.AddBuilding("Sawmill");
                if ((x + y) % 3 == 2) province.AddBuilding("IronMine");

                _provinces.Add(province);
            }
        }
    }

    private void InitializePopulations()
    {
        // Create populations for each province
        foreach (
            var population in _provinces.Select(province => new Population
            {
                Size = 500 + province.Id * 25, // Some variability
                Wealth = 10000,
                ProvinceId = province.Id
            })
        )
        {
            population.InitializeNeeds();
            _populations.Add(population);
        }
    }

    private void InitializeMarket()
    {
        // Get all resource types from provinces
        var allResources = _provinces
            .SelectMany(p => p.Resources.Keys)
            .Distinct()
            .ToArray();

        _market.Initialize(allResources);
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Run simulation steps at fixed intervals
        if (_elapsedTime >= SimulationTimeStep)
        {
            _elapsedTime -= SimulationTimeStep;

            UpdateProduction();
            UpdateConsumption();
            UpdateMarket();
            UpdateStats();
        }
    }

    private void UpdateProduction()
    {
        // Update production in each province
        foreach (var province in _provinces)
        {
            var province1 = province;
            var population = _populations.FirstOrDefault(p => p.ProvinceId == province1.Id);

            if (population != null)
            {
                province.UpdateProduction(population);
            }
        }
    }

    private void UpdateConsumption()
    {
        // Handle population consumption and satisfaction
        foreach (var population in _populations)
        {
            var province = _provinces.FirstOrDefault(p => p.Id == population.ProvinceId);

            if (province != null)
            {
                // Create a copy of resources for consumption
                var availableResources = new Dictionary<string, float>(province.Resources);

                // Population consumes resources
                population.ConsumptionStep(availableResources);

                // Update province resources after consumption
                province.Resources = availableResources;
            }
        }
    }

    private void UpdateMarket()
    {
        _market.ResetOffers();

        // Provinces sell excess resources
        foreach (var province in _provinces)
        {
            foreach (var resource in province.Resources)
            {
                // Sell half of available resources
                if (resource.Value > 20)
                {
                    var amount = resource.Value * 0.5f;
                    var minPrice = _market.GetPrice(resource.Key) * 0.9f;

                    _market.AddSupply(resource.Key, amount, minPrice, province);
                }
            }
        }

        // Populations buy needed resources
        foreach (var population in _populations)
        {
            var maxPrices = population.CalculateMaxPrices();

            foreach (var need in population.Needs)
            {
                var resource = need.Key;
                var amount = need.Value * population.Size;
                var maxPrice = maxPrices.GetValueOrDefault(resource, 20.0f);

                _market.AddDemand(resource, amount, maxPrice, population);
            }
        }

        // Resolve market trades
        _market.ResolveTrades();
    }

    private void UpdateStats()
    {
        var resources = _market.GetAllResources();
        _stats.UpdateGlobalStats(resources, _populations, _provinces);
    }

    // Interface implementation methods
    public List<Province> GetProvinces() => _provinces;
    public List<Population> GetPopulations() => _populations;
    public Market GetMarket() => _market;
    public Stats GetStats() => _stats;
    public Province GetProvinceById(int id) => _provinces.FirstOrDefault(p => p.Id == id);
    public Population GetPopulationById(int id) => _populations.FirstOrDefault(p => p.ProvinceId == id);
}