namespace UI;

using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    private readonly GameService gameService;

    public GameHub(GameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task SendGameUpdate()
    {
        var game = gameService.GetGame();
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

        await Clients.All.SendAsync("ReceiveGameUpdate", gameData);
    }

    public async Task GameUpdated()
    {
        await SendGameUpdate();
    }

    public async Task StartGame()
    {
        Console.WriteLine("StartGame called in GameHub");
        gameService.StartGame();
        await Clients.All.SendAsync("GameStarted");
    }

    public async Task StopGame()
    {
        gameService.StopGame();
        await Clients.All.SendAsync("GameStopped");
    }

    public async Task SetSpeed(int speed)
    {
        gameService.SetSpeed(speed);
        await Clients.All.SendAsync("SpeedChanged", speed);
    }

    public async Task ConstructBuilding(int provinceId, string buildingType)
    {
        gameService.ConstructBuilding(provinceId, buildingType);
        await SendGameUpdate();
    }
}