namespace UEconomy.Engine;

public class BuildingStorage
{
    public Dictionary<string, int> Items { get; } = new();
}

public class Building
{
    public string Id { get; }
    public int Level { get; }
    public int BasicOutputPerLevel { get; }
    public int MaxEmployeesPerLevel { get; }
    public int CurrentEmployees { get; }

    public List<Dictionary<string, int>> InputStuff { get; }
    public List<Dictionary<string, int>> OutputStuff { get; }

    public BuildingStorage InputStorage { get; }
    public BuildingStorage OutputStorage { get; }

    public Building(
        string id,
        int level,
        int basicOutputPerLevel,
        int maxEmployeesPerLevel,
        int currentEmployees,
        List<Dictionary<string, int>> inputStuff,
        List<Dictionary<string, int>> outputStuff,
        BuildingStorage inputStorage,
        BuildingStorage outputStorage
    )
    {
        Id = id;
        Level = level;
        BasicOutputPerLevel = basicOutputPerLevel;
        MaxEmployeesPerLevel = maxEmployeesPerLevel;
        CurrentEmployees = currentEmployees;

        InputStuff = inputStuff;
        OutputStuff = outputStuff;

        InputStorage = inputStorage;
        OutputStorage = outputStorage;
    }

    public void Work()
    {

    }

    // public void BuyNeeds(
    //     List<Stuff<ResourceType>> resources,
    //     List<Stuff<StuffType>> products
    // )
    // {
    //     foreach (var resource in resources)
    //     {
    //         resourceStorage.Store(resource);
    //     }
    //
    //     foreach (var product in products)
    //     {
    //         productStorage.Store(product);
    //     }
    // }
    //
    // public Stuff<T> TakeOutput(T type, int amount)
    // {
    //     return outputStorage.TryTake(type, amount, out var output) ? output : new Stuff<T>(type, 0);
    // }
    //
    // public abstract Stuff<T> Work();
    //
    // protected double CalculateOutputValue()
    // {
    //     var baseOutput = this.level * this.basicOutputPerLevel;
    //     var maxEmployees = this.maxEmployeesPerLevel * this.level;
    //
    //     var employeeEfficiency = maxEmployees > 0 ? Math.Min((double)currentEmployees / maxEmployees, 1.0) : 0.0;
    //
    //     if (employeeEfficiency <= 0.0)
    //     {
    //         return 0;
    //     }
    //
    //     var resourceEfficiency = 1.0;
    //     if (neededResources.Count > 0)
    //     {
    //         var totalResourcePercentage = (
    //             from neededResource
    //                 in neededResources
    //             let availableAmount = resourceStorage.GetAvailableAmount(neededResource.Type)
    //             select Math.Min(1.0, (double)availableAmount / neededResource.Amount)
    //         ).Sum();
    //
    //         resourceEfficiency = totalResourcePercentage / neededResources.Count;
    //     }
    //
    //
    //     var productEfficiency = 1.0;
    //     if (neededProducts.Count > 0)
    //     {
    //         var totalProductPercentage = (
    //             from neededProduct
    //                 in neededProducts
    //             let availableAmount = productStorage.GetAvailableAmount(neededProduct.Type)
    //             select Math.Min(1.0, (double)availableAmount / neededProduct.Amount)
    //         ).Sum();
    //
    //         productEfficiency = totalProductPercentage / neededProducts.Count;
    //     }
    //
    //     var totalEfficiency = Math.Min(employeeEfficiency, Math.Min(resourceEfficiency, productEfficiency));
    //
    //     return baseOutput * totalEfficiency;
    // }
    //
    // protected void ConsumeNeeds()
    // {
    //     foreach (var resource in neededResources)
    //     {
    //         resourceStorage.TryTake(resource.Type, resource.Amount, out _);
    //     }
    //
    //     foreach (var product in neededProducts)
    //     {
    //         productStorage.TryTake(product.Type, product.Amount, out _);
    //     }
    // }
    //
    // public int GetLevel()
    // {
    //     return level;
    // }
    //
    // public int GetCurrentEmployees()
    // {
    //     return currentEmployees;
    // }
    //
    // public int GetMaxEmployees()
    // {
    //     return maxEmployeesPerLevel * level;
    // }
}