namespace UEconomy.Engine;

public enum SocialClass
{
    LowerClass,
    MiddleClass,
    UpperClass
}

public class NeedSatisfaction
{
    public string NeedType { get; set; }
    public double SatisfactionLevel { get; set; }
    public double Priority { get; set; }
}

public class PopulationSegment
{
    public SocialClass SocialClass { get; private set; }
    public int Amount { get; private set; }
    public double Money { get; private set; }
    public Dictionary<string, NeedSatisfaction> Needs { get; private set; } = new();
    public Dictionary<string, double> Consumption { get; private set; } = new();

    public double LifeQualityIndex => CalculateLifeQualityIndex();

    private double CalculateLifeQualityIndex()
    {
        return Needs.Values.Average(n => n.SatisfactionLevel);
    }
}

public class Population
{
    public int TotalAmount => LowerClass.Amount + MiddleClass.Amount + UpperClass.Amount;

    public PopulationSegment LowerClass { get; private set; } = new();
    public PopulationSegment MiddleClass { get; private set; } = new();
    public PopulationSegment UpperClass { get; private set; } = new();

    public Dictionary<string, int> Employment { get; private set; } = new();

    public Dictionary<string, int> CalculateDailyConsumption()
    {
        return new Dictionary<string, int>();
    }

    public void AllocateWorkers(List<Building> buildings)
    {
        foreach (var building in buildings)
        {
            building.CurrentEmployees = 0;
        }

        var sortedBuildings = buildings.OrderByDescending(b => GetBuildingEmploymentPriority(b.Id)).ToList();

        var totalDemand = sortedBuildings.Sum(b => b.MaxEmployeesPerLevel * b.Level);

        var availableWorkers = LowerClass.Amount + (int)(MiddleClass.Amount * 0.7);

        var employmentRatio = Math.Min(1.0, availableWorkers / (double)Math.Max(1, totalDemand));

        var remainingWorkers = availableWorkers;

        foreach (var building in sortedBuildings)
        {
            var desiredWorkers = building.MaxEmployeesPerLevel * building.Level;
            var allocatedWorkers = (int)(desiredWorkers * employmentRatio);

            allocatedWorkers = Math.Min(allocatedWorkers, remainingWorkers);

            building.CurrentEmployees = allocatedWorkers;
            remainingWorkers -= allocatedWorkers;

            if (remainingWorkers <= 0) break;
        }

        Employment["unemployed"] = availableWorkers - (availableWorkers - remainingWorkers);

        foreach (var building in buildings)
        {
            var jobType = building.Id;
            Employment.TryAdd(jobType, 0);

            Employment[jobType] += building.CurrentEmployees;
        }
    }

    public void UpdateSocialMobility()
    {
        const double promotionThreshold = 0.8;
        const double degradationThreshold = 0.3;

        var lowerToMiddle = (int)(
            LowerClass.Amount *
            Math.Max(0, (LowerClass.LifeQualityIndex - promotionThreshold) / (1 - promotionThreshold)) * 0.01
        );

        var middleToUpper = (int)(
            MiddleClass.Amount *
            Math.Max(0, (MiddleClass.LifeQualityIndex - promotionThreshold) / (1 - promotionThreshold)) * 0.005
        );

        var upperToMiddle = (int)(
            UpperClass.Amount *
            Math.Max(0, (degradationThreshold - UpperClass.LifeQualityIndex) / degradationThreshold) * 0.01
        );

        LowerClass.AdjustAmount(lowerToMiddle * -1 + middleToLower);
        MiddleClass.AdjustAmount(lowerToMiddle + upperToMiddle - middleToLower - middleToUpper);
        UpperClass.AdjustAmount(middleToUpper - upperToMiddle);
    }
}