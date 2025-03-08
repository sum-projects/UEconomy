using System.Collections.Generic;
using System.Linq;

namespace UEconomy.Game;

public class Building
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int Level { get; set; } = 1;
    public int WorkersCapacity { get; set; } = 100;
    public int CurrentWorkers { get; set; } = 0;
    public Dictionary<string, float> ProductionRates { get; set; } = new();
    public Dictionary<string, float> ConsumptionRates { get; set; } = new();
    public int UpgradeCost { get; set; } = 1000;
    public bool IsActive { get; set; } = true;

    public void Upgrade()
    {
        Level++;
        WorkersCapacity += 100;

        // Zwiększ wskaźniki produkcji wraz z poziomem
        foreach (var resource in ProductionRates.Keys.ToList())
        {
            ProductionRates[resource] *= 1.5f;
        }

        // Zwiększ wskaźniki konsumpcji wraz z poziomem
        foreach (var resource in ConsumptionRates.Keys.ToList())
        {
            ConsumptionRates[resource] *= 1.3f;
        }

        // Koszt następnego ulepszenia rośnie eksponencjalnie
        UpgradeCost = (int)(UpgradeCost * 1.8f);
    }

    public Dictionary<string, float> CalculateProduction()
    {
        if (!IsActive || CurrentWorkers == 0)
        {
            return new Dictionary<string, float>();
        }

        var productionMultiplier = (float)CurrentWorkers / WorkersCapacity;
        var production = new Dictionary<string, float>();

        foreach (var resource in ProductionRates)
        {
            production[resource.Key] = resource.Value * productionMultiplier * Level;
        }

        return production;
    }

    public Dictionary<string, float> CalculateConsumption()
    {
        if (!IsActive || CurrentWorkers == 0)
        {
            return new Dictionary<string, float>();
        }

        var consumptionMultiplier = (float)CurrentWorkers / WorkersCapacity;
        var consumption = new Dictionary<string, float>();

        foreach (var resource in ConsumptionRates)
        {
            consumption[resource.Key] = resource.Value * consumptionMultiplier * Level;
        }

        return consumption;
    }

    public static Building CreateBuildingByType(string type, int id)
    {
        var building = new Building { Id = id, Type = type };

        switch (type)
        {
            case "FieldCrop":
                building.Name = "Pole uprawne";
                building.ProductionRates["Zboże"] = 200.0f;
                break;

            case "Sawmill":
                building.Name = "Tartak";
                building.ProductionRates["Drewno"] = 180.0f;
                break;

            case "IronMine":
                building.Name = "Kopalnia żelaza";
                building.ProductionRates["Żelazo"] = 120.0f;
                building.ConsumptionRates["Drewno"] = 20.0f;
                break;

            case "CoalMine":
                building.Name = "Kopalnia węgla";
                building.ProductionRates["Węgiel"] = 150.0f;
                building.ConsumptionRates["Drewno"] = 25.0f;
                break;

            case "FurnitureFactory":
                building.Name = "Fabryka mebli";
                building.ProductionRates["Meble"] = 80.0f;
                building.ConsumptionRates["Drewno"] = 40.0f;
                building.ConsumptionRates["Żelazo"] = 10.0f;
                building.ConsumptionRates["Węgiel"] = 20.0f;
                break;

            case "TailorShop":
                building.Name = "Szwalnia";
                building.ProductionRates["Ubrania"] = 90.0f;
                building.ConsumptionRates["Żelazo"] = 5.0f;
                building.ConsumptionRates["Węgiel"] = 15.0f;
                break;

            default:
                building.Name = "Nieznany budynek";
                break;
        }

        return building;
    }
}