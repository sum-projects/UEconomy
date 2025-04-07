using Microsoft.AspNetCore.SignalR;
using UEconomy.Engine;
using UI;

public class GameService
{
    private Game game;
    private bool isRunning;
    private int speedMultiplier = 1;
    private Timer? gameTimer;
    private int currentDay = 1;

    private readonly IHubContext<GameHub> HubContext;

    public GameService(IHubContext<GameHub> hubContext)
    {
        HubContext = hubContext;

        game = new Game();
    }

    public Game GetGame() => game;

    public int GetCurrentDay() => currentDay;

    public void StartGame()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        gameTimer = new Timer(UpdateGame, null, 0, 1000 / speedMultiplier);
    }

    public void StopGame()
    {
        isRunning = false;
        gameTimer?.Dispose();
        gameTimer = null;
    }

    public void SetSpeed(int speed)
    {
        if (!isRunning) return;

        speedMultiplier = Math.Clamp(speed, 1, 10);
        gameTimer?.Dispose();
        gameTimer = new Timer(UpdateGame, null, 0, 1000 / speedMultiplier);
    }

    private void UpdateGame(object? state)
    {
        game.Update();
        currentDay++;

        SendGameUpdateToClients();
    }

    private void SendGameUpdateToClients()
    {
        var gameData = new
        {
            CurrentDay = currentDay,
            Countries = game.Countries.Select(c => new
            {
                Id = c.Id,
                Provinces = c.Provinces.Select(p => new
                {
                    Id = p.Id,
                    Name = p.Id, // Używamy Id jako nazwy
                    PopCount = p.Population,
                    Buildings = p.GetAllBuildings(),
                    MarketStats = GetMarketStats(p.LocalMarket)
                })
            })
        };

        HubContext.Clients.All.SendAsync("ReceiveGameUpdate", gameData);
    }

    private object GetMarketStats(Market market)
    {
        var marketStats = market.GetMarketStatistics();

        var categoryTotals = new Dictionary<string, double>();

        foreach (var item in marketStats)
        {
            var stuffInfo = GetStuffInfo(item.Key);
            var category = stuffInfo?.Category ?? "unknown";
            var amount = item.Value[0]["Amount"];

            categoryTotals.TryAdd(category, 0);

            categoryTotals[category] += amount;
        }

        var detailedStats = marketStats.ToDictionary(
            kvp => kvp.Key,
            kvp => new {
                Amount = kvp.Value[0]["Amount"],
                Price = kvp.Value[0]["Price"],
                Category = GetStuffInfo(kvp.Key)?.Category ?? "unknown"
            }
        );

        return new {
            CategoryTotals = categoryTotals,
            DetailedStats = detailedStats
        };
    }

    private StuffStruct? GetStuffInfo(string stuffId)
    {
        var allStuff = LoaderConfig.GetStuff();
        return allStuff.FirstOrDefault(s => s.Id == stuffId);
    }

    public void ConstructBuilding(string provinceId, string buildingId)
    {
        var province = game.GetProvinceById(provinceId);
        if (province == null) return;

        var buildingStruct = game.GetBuildingStruct(buildingId);

        var inputStorage = new BuildingStorage();
        var outputStorage = new BuildingStorage();

        var inputStuff = new List<Dictionary<string, int>>();
        var outputStuff = new List<Dictionary<string, int>>();

        if (buildingStruct.ProductionTypes.TryGetValue("level0", out var type))
        {
            inputStuff.Add(type.Input);
            outputStuff.Add(type.Output);
        }

        var level = 1;
        var currentEmployees = 0;

        var prodMethod = buildingStruct.ProductionMethods["level0"];
        var maxEmployeesPerLevel = prodMethod.EmployeesNeeded;
        var outputMultiplayer = prodMethod.OutputMultiplier;

        var building = new Building(
            buildingId,
            level,
            outputMultiplayer,
            maxEmployeesPerLevel,
            currentEmployees,
            inputStuff,
            outputStuff,
            inputStorage,
            outputStorage
        );

        province.AddBuilding(building);

        SendGameUpdateToClients();
    }
}