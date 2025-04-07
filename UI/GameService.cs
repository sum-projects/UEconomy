using Microsoft.AspNetCore.SignalR;
using UEconomy.Engine;

namespace UI;

public class GameService
{
    private Game game;
    private bool isRunning = false;
    private int speedMultiplier = 1;
    private Timer? gameTimer;

    private readonly IHubContext<GameHub> HubContext;

    public GameService(IHubContext<GameHub> hubContext)
    {
        HubContext = hubContext;

        game = new Game();
    }

    public Game GetGame() => game;

    public void StartGame()
    {
        Console.WriteLine("StartGame called in GameService");
        if (isRunning)
        {
            Console.WriteLine("Game already running, ignoring start request");
            return;
        }

        Console.WriteLine("Starting game simulation");
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

    public int GetSpeed() => speedMultiplier;

    private void UpdateGame(object? state)
    {
        Console.WriteLine($"Updating game to day: {game.GetCurrentDay() + 1}");
        game.Update();
        Console.WriteLine($"Game updated to day: {game.GetCurrentDay()}");

        var gameData = new
        {
            CurrentDay = game.GetCurrentDay(),
            Countries = game.GetCountries().Select(c => new
            {
                Id = c.GetId(),
                Name = c.GetName(),
                Provinces = c.GetProvinces().Select(p => new
                {
                    Id = p.GetId(),
                    Name = p.GetName(),
                    PopCount = p.GetPopCount(),
                    Buildings = p.GetAllBuildings(),
                    MarketStats = p.GetMarketStatistics()
                })
            })
        };

        HubContext.Clients.All.SendAsync("ReceiveGameUpdate", gameData);
    }

    public void ConstructBuilding(string provinceId, string buildingId)
    {
        var province = game.GetProvinceById(provinceId);
        if (province == null) return;

        var buildingStruct = game.GetBuildingStruct(buildingId);
        if (buildingStruct == null) return;


        switch (buildingType)
        {
            case "Workshop":
                province.Add
                var workshop = new Workshop(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFactory(workshop);
                break;
            case "TextileMill":
                var textileMill = new TextileMill(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFactory(textileMill);
                break;
            case "LumberMill":
                var lumberMill = new LumberMill(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFactory(lumberMill);
                break;
            case "FurnitureFactory":
                var furnitureFactory = new FurnitureFactory(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFactory(furnitureFactory);
                break;
            case "Glassworks":
                var glassworks = new Glassworks(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFactory(glassworks);
                break;
            case "WheatFarm":
                var wheatFarm = new WheatFarm(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFarm(wheatFarm);
                break;
            case "RiceFarm":
                var riceFarm = new RiceFarm(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFarm(riceFarm);
                break;
            case "MaizeFarm":
                var maizeFarm = new MaizeFarm(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddFarm(maizeFarm);
                break;
            case "IronMine":
                var ironMine = new IronMine(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddMine(ironMine);
                break;
            case "CoalMine":
                var coalMine = new CoalMine(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddMine(coalMine);
                break;
            case "LoggingCamp":
                var loggingCamp = new LoggingCamp(
                    GetNextBuildingId(), level, 0,
                    new List<Stuff<ResourceType>> { new(ResourceType.Wood, 10), new(ResourceType.Iron, 5) },
                    new List<Stuff<StuffType>> { }
                );
                province.AddMine(loggingCamp);
                break;

        }
    }

    private int GetNextBuildingId()
    {
        return new Random().Next(1000, 9999);
    }
}