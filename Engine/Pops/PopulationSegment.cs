namespace UEconomy.Engine.Pops;

public class PopulationSegment
{
    public SocialClass SocialClass { get; private set; }
    public int Amount { get; private set; }
    public double Money { get; private set; }
    public Dictionary<string, NeedSatisfaction> Needs { get; private set; } = new();
    public Dictionary<string, double> Consumption { get; private set; } = new();

    public PopulationSegment(SocialClass socialClass = SocialClass.LowerClass, int amount = 0, double money = 0.0)
    {
        SocialClass = socialClass;
        Amount = amount;
        Money = money;
        InitializeNeeds();
    }

    private void InitializeNeeds()
    {
        Needs["food"] = new NeedSatisfaction { NeedType = "food", SatisfactionLevel = 0.5, Priority = 1.0 };
        Needs["clothing"] = new NeedSatisfaction { NeedType = "clothing", SatisfactionLevel = 0.5, Priority = 0.8 };
        Needs["furniture"] = new NeedSatisfaction { NeedType = "furniture", SatisfactionLevel = 0.5, Priority = 0.6 };

        switch (SocialClass)
        {
            case SocialClass.MiddleClass:
                Needs["luxury"] = new NeedSatisfaction { NeedType = "luxury", SatisfactionLevel = 0.3, Priority = 0.5 };
                break;
            case SocialClass.UpperClass:
                Needs["luxury"] = new NeedSatisfaction { NeedType = "luxury", SatisfactionLevel = 0.6, Priority = 0.7 };
                Needs["art"] = new NeedSatisfaction { NeedType = "art", SatisfactionLevel = 0.4, Priority = 0.6 };
                break;
        }
    }

    public double LifeQualityIndex => CalculateLifeQualityIndex();

    private double CalculateLifeQualityIndex()
    {
        return Needs.Values.Average(n => n.SatisfactionLevel);
    }

    public void AdjustAmount(int change)
    {
        Amount = Math.Max(0, Amount + change);
    }

    public void UpdateNeedSatisfaction(string needType, double satisfactionLevel)
    {
        if (Needs.TryGetValue(needType, out var need))
        {
            need.SatisfactionLevel = Math.Clamp(need.SatisfactionLevel + satisfactionLevel, 0.0, 1.0);
        }
    }

    public void UpdateMoney(double amount)
    {
        Money = Math.Max(0, Money + amount);
    }

    public Dictionary<string, int> CalculateSegmentConsumption()
    {
        var result = new Dictionary<string, int>();

        var foodFactor = SocialClass switch
        {
            SocialClass.LowerClass => 0.1,
            SocialClass.MiddleClass => 0.15,
            SocialClass.UpperClass => 0.2,
            _ => 0.1
        };

        var clothingFactor = SocialClass switch
        {
            SocialClass.LowerClass => 0.05,
            SocialClass.MiddleClass => 0.1,
            SocialClass.UpperClass => 0.15,
            _ => 0.05
        };

        var furnitureFactor = SocialClass switch
        {
            SocialClass.LowerClass => 0.01,
            SocialClass.MiddleClass => 0.02,
            SocialClass.UpperClass => 0.05,
            _ => 0.01
        };

        result["wheat"] = (int)(Amount * foodFactor);
        result["rice"] = (int)(Amount * foodFactor * 0.5);
        result["maize"] = (int)(Amount * foodFactor * 0.3);
        result["cloth"] = (int)(Amount * clothingFactor);
        result["furniture"] = (int)(Amount * furnitureFactor);

        if (SocialClass is SocialClass.MiddleClass or SocialClass.UpperClass)
        {
            result["glass"] = (int)(Amount * 0.02);
        }

        if (SocialClass == SocialClass.UpperClass)
        {
            result["tool"] = (int)(Amount * 0.05);
        }

        foreach (var key in result.Keys.ToList().Where(key => result[key] <= 0))
        {
            result.Remove(key);
        }

        return result;
    }
}