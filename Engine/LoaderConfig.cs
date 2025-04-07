using System.Reflection;
using Newtonsoft.Json;

namespace UEconomy.Engine;

public struct ProductionMethodStruct
{
    [JsonProperty("id")] public string Id { get; }
    [JsonProperty("input")] public Dictionary<string, int> Input { get; }
    [JsonProperty("requireTech")] public List<string> RequireTech { get; }
    [JsonProperty("employeesNeeded")] public int EmployeesNeeded { get; }
    [JsonProperty("outputMultiplier")] public int OutputMultiplier { get; }

    public ProductionMethodStruct(
        string id,
        Dictionary<string, int> input,
        List<string> requireTech,
        int employeesNeeded,
        int outputMultiplier
    )
    {
        Id = id;
        Input = input;
        RequireTech = requireTech;
        EmployeesNeeded = employeesNeeded;
        OutputMultiplier = outputMultiplier;
    }
}

public struct ProductionTypeStruct
{
    [JsonProperty("id")] public string Id { get; }
    [JsonProperty("input")] public Dictionary<string, int> Input { get; }
    [JsonProperty("output")] public Dictionary<string, int> Output { get; }
    [JsonProperty("requireTech")] public List<string> RequireTech { get; }
    [JsonProperty("requiredMethod")] public List<string> RequiredMethod { get; }

    public ProductionTypeStruct(
        string id,
        Dictionary<string, int> input,
        Dictionary<string, int> output,
        List<string> requireTech
    )
    {
        Id = id;
        Input = input;
        Output = output;
        RequireTech = requireTech;
        RequiredMethod = requireTech;
    }
}

public struct BuildingStruct
{
    [JsonProperty("id")] public string Id { get; }
    [JsonProperty("productionMethods")] public Dictionary<string, ProductionMethodStruct> ProductionMethods { get; }
    [JsonProperty("productionTypes")] public Dictionary<string, ProductionTypeStruct> ProductionTypes { get; }

    public BuildingStruct(
        string id,
        Dictionary<string, ProductionMethodStruct> productionMethods,
        Dictionary<string, ProductionTypeStruct> productionTypes
    )
    {
        Id = id;
        ProductionMethods = productionMethods;
        ProductionTypes = productionTypes;
    }
}

public struct TechStruct
{
    [JsonProperty("id")] public string Id { get; }
    [JsonProperty("description")] public string Description { get; }
    [JsonProperty("category")] public string Category { get; }
    [JsonProperty("requireTech")] public List<string> RequireTech { get; }
    [JsonProperty("cost")] public int Cost { get; }

    public TechStruct(
        string id,
        string description,
        string category,
        List<string> requireTech,
        int cost
    )
    {
        Id = id;
        Description = description;
        Category = category;
        RequireTech = requireTech;
        Cost = cost;
    }
}

public struct StuffStruct
{
    [JsonProperty("id")] public string Id { get; }

    [JsonProperty("category")] public string Category { get; }

    public StuffStruct
    (
        string id,
        string category
    )
    {
        Id = id;
        Category = category;
    }
}

public struct ProvinceStruct
{
    [JsonProperty("id")] public string Id { get; }

    [JsonProperty("population")] public int Population { get; }

    public ProvinceStruct(
        string id,
        int population
    )
    {
        Id = id;
        Population = population;
    }
}

public struct CountryStruct
{
    [JsonProperty("id")] public string Id { get; }

    [JsonProperty("provinces")] public List<ProvinceStruct> Provinces { get; }

    public CountryStruct(
        string id,
        List<ProvinceStruct> provinces
    )
    {
        Id = id;
        Provinces = provinces;
    }
}

public static class LoaderConfig
{
    public static List<CountryStruct> GetCountries()
    {
        return LoadJson<CountryStruct>("countries.json");
    }

    public static List<TechStruct> GetTechnologies()
    {
        return LoadJson<TechStruct>("technologies.json");
    }

    public static List<BuildingStruct> GetBuildings()
    {
        return LoadJson<BuildingStruct>("buildings.json");
    }

    public static List<StuffStruct> GetStuff()
    {
        return LoadJson<StuffStruct>("stuffs.json");
    }

    private static List<T> LoadJson<T>(string fileName)
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var path = currentPath + "/config/" + fileName;
        using var r = new StreamReader(path);
        var json = r.ReadToEnd();
        return JsonConvert.DeserializeObject<List<T>>(json);
    }
}