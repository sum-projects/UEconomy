namespace UEconomy;

public enum StuffType
{
    Tool,
    Glass,
    Cloth,
    Plank,
    Furniture,
}

public enum FoodType
{
    Wheat,
    Rice,
    Maize,
}

public enum ResourceType
{
    Iron,
    Coal,
    Wood,
}

public struct Stuff<T>
{
    public T Type { get; }
    public int Amount { get; }

    public Stuff(T type, int amount)
    {
        Type = type;
        Amount = amount;
    }

    public Stuff<T> WithAmount(int newAmount)
    {
        return new Stuff<T>(Type, newAmount);
    }
}

public class Storage<T>
{
    private Dictionary<T, int> items = new();

    public void Store(Stuff<T> stuff)
    {
        if (items.ContainsKey(stuff.Type))
        {
            items[stuff.Type] += stuff.Amount;
        }
        else
        {
            items[stuff.Type] = stuff.Amount;
        }
    }

    public bool TryTake(T type, int amount, out Stuff<T> takenStuff)
    {
        if (items.ContainsKey(type) && items[type] >= amount)
        {
            items[type] -= amount;
            takenStuff = new Stuff<T>(type, amount);
            return true;
        }

        takenStuff = default;
        return false;
    }

    public int GetAvailableAmount(T type)
    {
        return items.ContainsKey(type) ? items[type] : 0;
    }
}




public abstract class Building<T> where T : Enum
{
    protected int id;
    protected int level;
    protected int basicOutputPerLevel;
    protected int maxEmployeesPerLevel;
    protected int currentEmployees;

    protected List<Stuff<ResourceType>> neededResources;
    protected List<Stuff<StuffType>> neededProducts;

    protected Storage<ResourceType> resourceStorage = new();
    protected Storage<StuffType> productStorage = new();
    protected Storage<T> outputStorage = new();

    protected Building(
        int id,
        int level,
        int basicOutputPerLevel,
        int maxEmployeesPerLevel,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    )
    {
        this.id = id;
        this.level = level;
        this.basicOutputPerLevel = basicOutputPerLevel;
        this.maxEmployeesPerLevel = maxEmployeesPerLevel;
        this.currentEmployees = currentEmployees;
        this.neededResources = neededResources;
        this.neededProducts = neededProducts;
    }

    public void BuyNeeds(
        List<Stuff<ResourceType>> resources,
        List<Stuff<StuffType>> products
    )
    {
        foreach (var resource in resources)
        {
            resourceStorage.Store(resource);
        }

        foreach (var product in products)
        {
            productStorage.Store(product);
        }
    }

    public Stuff<T> TakeOutput(T type, int amount)
    {
        return outputStorage.TryTake(type, amount, out var output) ? output : new Stuff<T>(type, 0);
    }

    public abstract Stuff<T> Work();

    protected double CalculateOutputValue()
    {
        var baseOutput = this.level * this.basicOutputPerLevel;
        var maxEmployees = this.maxEmployeesPerLevel * this.level;

        var employeeEfficiency = maxEmployees > 0 ? Math.Min((double)currentEmployees / maxEmployees, 1.0) : 0.0;

        if (employeeEfficiency <= 0.0)
        {
            return 0;
        }

        var resourceEfficiency = 1.0;
        if (neededResources.Count > 0)
        {
            var totalResourcePercentage = (
                from neededResource
                    in neededResources
                let availableAmount = resourceStorage.GetAvailableAmount(neededResource.Type)
                select Math.Min(1.0, (double)availableAmount / neededResource.Amount)
            ).Sum();

            resourceEfficiency = totalResourcePercentage / neededResources.Count;
        }


        var productEfficiency = 1.0;
        if (neededProducts.Count > 0)
        {
            var totalProductPercentage = (
                from neededProduct
                    in neededProducts
                let availableAmount = productStorage.GetAvailableAmount(neededProduct.Type)
                select Math.Min(1.0, (double)availableAmount / neededProduct.Amount)
            ).Sum();

            productEfficiency = totalProductPercentage / neededProducts.Count;
        }

        var totalEfficiency = Math.Min(employeeEfficiency, Math.Min(resourceEfficiency, productEfficiency));

        return baseOutput * totalEfficiency;
    }

    protected void ConsumeNeeds()
    {
        foreach (var resource in neededResources)
        {
            resourceStorage.TryTake(resource.Type, resource.Amount, out _);
        }

        foreach (var product in neededProducts)
        {
            productStorage.TryTake(product.Type, product.Amount, out _);
        }
    }
}

public abstract class Factory : Building<StuffType>
{
    protected Factory(
        int id,
        int level,
        int basicOutputPerLevel,
        int maxEmployeesPerLevel,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, basicOutputPerLevel, maxEmployeesPerLevel, currentEmployees, neededResources, neededProducts)
    {
    }
}

public class Workshop : Factory
{
    public Workshop(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, 100, 20, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<StuffType> Work()
    {
        var outputValue = CalculateOutputValue();
        var toolsProduced = (int)Math.Round(outputValue);

        if (toolsProduced <= 0) return new Stuff<StuffType>(StuffType.Tool, 0);

        ConsumeNeeds();

        var output = new Stuff<StuffType>(StuffType.Tool, toolsProduced);
        outputStorage.Store(output);
        return output;

    }
}

public class TextileMill : Factory
{
    public TextileMill(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, 120, 25, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<StuffType> Work()
    {
        var outputValue = CalculateOutputValue();
        var clothProduced = (int)Math.Round(outputValue);

        if (clothProduced <= 0) return new Stuff<StuffType>(StuffType.Cloth, 0);

        ConsumeNeeds();

        var output = new Stuff<StuffType>(StuffType.Cloth, clothProduced);
        outputStorage.Store(output);
        return output;

    }
}

public class LumberMill : Factory
{
    public LumberMill(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, 150, 30, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<StuffType> Work()
    {
        var outputValue = CalculateOutputValue();
        var planksProduced = (int)Math.Round(outputValue);

        if (planksProduced <= 0) return new Stuff<StuffType>(StuffType.Plank, 0);

        ConsumeNeeds();

        var output = new Stuff<StuffType>(StuffType.Plank, planksProduced);
        outputStorage.Store(output);
        return output;
    }
}

public class FurnitureFactory : Factory
{
    public FurnitureFactory(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, 200, 35, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<StuffType> Work()
    {
        var outputValue = CalculateOutputValue();
        var furnitureProduced = (int)Math.Round(outputValue);

        if (furnitureProduced <= 0) return new Stuff<StuffType>(StuffType.Furniture, 0);

        ConsumeNeeds();

        var output = new Stuff<StuffType>(StuffType.Furniture, furnitureProduced);
        outputStorage.Store(output);
        return output;

    }
}

public class Glassworks : Factory
{
    public Glassworks(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, 80, 15, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<StuffType> Work()
    {
        var outputValue = CalculateOutputValue();
        var glassProduced = (int)Math.Round(outputValue);

        if (glassProduced <= 0) return new Stuff<StuffType>(StuffType.Glass, 0);

        ConsumeNeeds();

        var output = new Stuff<StuffType>(StuffType.Glass, glassProduced);
        outputStorage.Store(output);
        return output;

    }
}

public abstract class Farm : Building<FoodType>
{
    protected Farm(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, 1000, 50, currentEmployees, neededResources, neededProducts)
    {
    }
}

public class WheatFarm : Farm
{
    public WheatFarm(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<FoodType> Work()
    {
        var outputValue = CalculateOutputValue();
        var wheatProduced = (int)Math.Round(outputValue);

        if (wheatProduced <= 0) return new Stuff<FoodType>(FoodType.Wheat, 0);

        ConsumeNeeds();

        var output = new Stuff<FoodType>(FoodType.Wheat, wheatProduced);
        outputStorage.Store(output);
        return output;
    }
}

public class RiceFarm : Farm
{
    public RiceFarm(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<FoodType> Work()
    {
        var outputValue = CalculateOutputValue();
        var riceProduced = (int)Math.Round(outputValue);

        if (riceProduced <= 0) return new Stuff<FoodType>(FoodType.Rice, 0);

        ConsumeNeeds();

        var output = new Stuff<FoodType>(FoodType.Rice, riceProduced);
        outputStorage.Store(output);
        return output;

    }
}

public class MaizeFarm : Farm
{
    public MaizeFarm(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<FoodType> Work()
    {
        var outputValue = CalculateOutputValue();
        var maizeProduced = (int)Math.Round(outputValue);

        if (maizeProduced <= 0) return new Stuff<FoodType>(FoodType.Maize, 0);

        ConsumeNeeds();

        var output = new Stuff<FoodType>(FoodType.Maize, maizeProduced);
        outputStorage.Store(output);
        return output;

    }
}

public abstract class Mine : Building<ResourceType>
{
    protected Mine(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, 500, 40, currentEmployees, neededResources, neededProducts)
    {
    }
}

public class IronMine : Mine
{
    public IronMine(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<ResourceType> Work()
    {
        var outputValue = CalculateOutputValue();
        var ironProduced = (int)Math.Round(outputValue);

        if (ironProduced <= 0) return new Stuff<ResourceType>(ResourceType.Iron, 0);

        ConsumeNeeds();

        var output = new Stuff<ResourceType>(ResourceType.Iron, ironProduced);
        outputStorage.Store(output);
        return output;
    }
}

public class CoalMine : Mine
{
    public CoalMine(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<ResourceType> Work()
    {
        var outputValue = CalculateOutputValue();
        var coalProduced = (int)Math.Round(outputValue);

        if (coalProduced <= 0) return new Stuff<ResourceType>(ResourceType.Coal, 0);

        ConsumeNeeds();

        var output = new Stuff<ResourceType>(ResourceType.Coal, coalProduced);
        outputStorage.Store(output);
        return output;
    }
}

public class LoggingCamp : Mine
{
    public LoggingCamp(
        int id,
        int level,
        int currentEmployees,
        List<Stuff<ResourceType>> neededResources,
        List<Stuff<StuffType>> neededProducts
    ) : base(id, level, currentEmployees, neededResources, neededProducts)
    {
    }

    public override Stuff<ResourceType> Work()
    {
        var outputValue = CalculateOutputValue();
        var woodProduced = (int)Math.Round(outputValue);

        if (woodProduced <= 0) return new Stuff<ResourceType>(ResourceType.Wood, 0);

        ConsumeNeeds();

        var output = new Stuff<ResourceType>(ResourceType.Wood, woodProduced);
        outputStorage.Store(output);
        return output;
    }
}