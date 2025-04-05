using UEconomy;

namespace UI;

public class GameService
{
    private Game game;
    private bool isRunning = false;
    private int speedMultiplier = 1;
    private Timer? gameTimer;

    public GameService()
    {
        var province = new Province(1, "Province", 50);
        var country = new Country(1, "Country", new List<Province> { province });
        game = new Game(new List<Country> { country });
    }

    public Game GetGame() => game;

    public void Start()
    {
        if (isRunning) return;

        isRunning = true;
        gameTimer = new Timer(UpdateGame, null, 0, 1000 / speedMultiplier);
    }

    public void Stop()
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
        game.Update();
    }

    public void ConstructBuilding(int provinceId, string buildingType, int level = 1)
    {
        var province = game.GetProvinceById(provinceId);
        if (province == null) return;

        switch (buildingType)
        {
            case "Workshop":
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