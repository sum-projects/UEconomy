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
            CurrentDay = gameService.GetCurrentDay(),
            Countries = game.Countries.Select(c => new
            {
                Id = c.Id,
                Provinces = c.Provinces.Select(p => new
                {
                    Id = p.Id,
                    Name = p.Id,
                    PopCount = p.Population,
                    Buildings = p.GetAllBuildings(),
                    MarketStats = p.LocalMarket.GetMarketStatistics()
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

    public async Task ConstructBuilding(string provinceId, string buildingType)
    {
        gameService.ConstructBuilding(provinceId, buildingType);
        await SendGameUpdate();
    }
}