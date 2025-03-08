using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace UEconomy.Engine;

public class Province
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public Dictionary<string, float> Resources { get; set; } = new();
    public List<Building> Buildings { get; set; } = new();
    public int AvailableWorkers { get; set; } = 0;

    private static int _buildingIdCounter = 0;

    public void UpdateProduction(Population population)
    {
        // Ustaw dostępną siłę roboczą
        AvailableWorkers = population.Size;

        // Najpierw przydziel pracowników do budynków
        foreach (var building in Buildings)
        {
            // Reset liczby pracowników
            building.CurrentWorkers = 0;

            // Sprawdź wymagania zasobów do działania
            var canOperate = building.ConsumptionRates.All(
                r =>
                    Resources.ContainsKey(r.Key) &&
                    !(Resources[r.Key] < r.Value)
            );

            building.IsActive = canOperate;

            if (canOperate)
            {
                // Przydziel pracowników, ale nie więcej niż dostępnych
                building.CurrentWorkers = Math.Min(building.WorkersCapacity, AvailableWorkers);
                AvailableWorkers -= building.CurrentWorkers;
            }
        }

        // Produkcja z budynków
        foreach (var building in Buildings.Where(b => b.IsActive && b.CurrentWorkers > 0))
        {
            // Najpierw oblicz, ile zasobów zostanie zużyte
            var consumption = building.CalculateConsumption();

            // Odejmij zużyte zasoby
            foreach (
                var resource in consumption.Where(
                    resource => Resources.ContainsKey(resource.Key)
                )
            )
            {
                Resources[resource.Key] = Math.Max(0, Resources[resource.Key] - resource.Value);
            }

            // Oblicz produkcję
            var production = building.CalculateProduction();

            // Dodaj wyprodukowane zasoby
            foreach (var resource in production)
            {
                Resources.TryAdd(resource.Key, 0);
                Resources[resource.Key] += resource.Value;
            }
        }

        // Produkcja od niezatrudnionych ludzi (gospodarstwa samozaopatrzeniowe)
        if (AvailableWorkers > 0)
        {
            // Niezatrudnieni ludzie produkują podstawowe zasoby
            var subsistenceFactor = AvailableWorkers / 10.0f;

            Resources.TryAdd("Zboże", 0);
            Resources["Zboże"] += 10.0f * subsistenceFactor;

            Resources.TryAdd("Meble", 0);
            Resources["Meble"] += 1.0f * subsistenceFactor;

            Resources.TryAdd("Ubrania", 0);
            Resources["Ubrania"] += 1.0f * subsistenceFactor;
        }
    }

    public void AddBuilding(string type)
    {
        var building = Building.CreateBuildingByType(type, _buildingIdCounter++);
        Buildings.Add(building);
    }


    public bool CanUpgradeBuilding(int buildingId, float availableWealth)
    {
        var building = Buildings.FirstOrDefault(b => b.Id == buildingId);
        return building != null && availableWealth >= building.UpgradeCost;
    }

    public void UpgradeBuilding(int buildingId)
    {
        var building = Buildings.FirstOrDefault(b => b.Id == buildingId);
        building?.Upgrade();
    }

    public bool DestroyBuilding(int buildingId)
    {
        var building = Buildings.FirstOrDefault(b => b.Id == buildingId);
        if (building != null)
        {
            Buildings.Remove(building);
            return true;
        }
        return false;
    }

    public Building GetBuilding(int buildingId)
    {
        return Buildings.FirstOrDefault(b => b.Id == buildingId);
    }
}