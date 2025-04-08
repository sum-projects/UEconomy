namespace UEconomy.Engine;

public class BuildingStorage
{
    public Dictionary<string, int> Items { get; } = new();
}

public class Building
{
    public string Id { get; }
    public int Level { get; }
    public int OutputMultiplayer { get; }
    public int MaxEmployeesPerLevel { get; }
    public int CurrentEmployees { get; set; }

    public List<Dictionary<string, int>> InputStuff { get; }
    public List<Dictionary<string, int>> OutputStuff { get; }

    public BuildingStorage InputStorage { get; }
    public BuildingStorage OutputStorage { get; }

    public Building(
        string id,
        int level,
        int outputMultiplayer,
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
        OutputMultiplayer = outputMultiplayer;
        MaxEmployeesPerLevel = maxEmployeesPerLevel;
        CurrentEmployees = currentEmployees;

        InputStuff = inputStuff;
        OutputStuff = outputStuff;

        InputStorage = inputStorage;
        OutputStorage = outputStorage;
    }

    public void Work()
    {
        var maxEmployees = MaxEmployeesPerLevel * Level;
        var employeeEfficiency = maxEmployees > 0 ? Math.Min((double)CurrentEmployees / maxEmployees, 1.0) : 0.0;
        if (employeeEfficiency <= 0.0)
        {
            return;
        }

        var resourceEfficiency = 1.0;
        if (InputStuff.Count > 0)
        {
            var totalResourcePercentage = 0.0;

            foreach (var neededResourcesDict in InputStuff)
            {
                foreach (var (resourceId, neededAmount) in neededResourcesDict)
                {
                    var availableAmount = 0;
                    if (InputStorage.Items.TryGetValue(resourceId, out var item))
                    {
                        availableAmount = item;
                    }

                    totalResourcePercentage += Math.Min(1.0, (double)availableAmount / neededAmount);
                }
            }

            resourceEfficiency = InputStuff.Count > 0 ? totalResourcePercentage / InputStuff.Count : 1.0;
        }

        var totalEfficiency = Math.Min(employeeEfficiency, resourceEfficiency);

        if (totalEfficiency <= 0.0)
        {
            return;
        }

        var baseOutput = Level * OutputMultiplayer;
        var actualOutput = (int)(baseOutput * totalEfficiency);

        if (actualOutput <= 0)
        {
            return;
        }

        ConsumeInputs(actualOutput);
        ProduceOutputs(actualOutput);
    }

    private void ConsumeInputs(int outputMultiplier)
    {
        if (InputStuff.Count == 0)
        {
            return;
        }

        foreach (var inputDict in InputStuff)
        {
            foreach (var (resourceId, value) in inputDict)
            {
                var amountNeeded = value * outputMultiplier;

                if (InputStorage.Items.ContainsKey(resourceId) && InputStorage.Items[resourceId] >= amountNeeded)
                {
                    InputStorage.Items[resourceId] -= amountNeeded;
                }
            }
        }
    }

    private void ProduceOutputs(int outputMultiplier)
    {
        if (OutputStuff.Count == 0)
        {
            return;
        }

        foreach (var outputDict in OutputStuff)
        {
            foreach (var (resourceId, value) in outputDict)
            {
                var amountProduced = value * outputMultiplier;

                OutputStorage.Items.TryAdd(resourceId, 0);

                OutputStorage.Items[resourceId] += amountProduced;
            }
        }
    }
}