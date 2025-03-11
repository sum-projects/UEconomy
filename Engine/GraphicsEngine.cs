using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine.Interfaces;
using UEconomy.Graphics;
using UEconomy.Graphics.DrawSystem;

namespace UEconomy.Engine;

public class GraphicsEngine : IGraphicsEngine
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IGameEngine _gameEngine;
    private readonly IEventEngine _eventEngine;
    private readonly UI _ui;
    private readonly DrawSystemGame _drawSystemGame;
    private readonly DrawSystemProvince _drawSystemProvince;
    private readonly DrawSystemStats _drawSystemStats;

    private GameView _currentView = GameView.GameOverview;
    private float _updateTimer = 0;

    public GraphicsEngine(
        GraphicsDevice graphicsDevice,
        IGameEngine gameEngine,
        IEventEngine eventEngine,
        UI ui,
        DrawSystemGame drawSystemGame,
        DrawSystemProvince drawSystemProvince,
        DrawSystemStats drawSystemStats)
    {
        _graphicsDevice = graphicsDevice;
        _gameEngine = gameEngine;
        _eventEngine = eventEngine;
        _ui = ui;
        _drawSystemGame = drawSystemGame;
        _drawSystemProvince = drawSystemProvince;
        _drawSystemStats = drawSystemStats;

        // Register event handlers for view switching
        _eventEngine.RegisterButtonClickHandler(OnButtonClick);
    }

    public void Update(GameTime gameTime)
    {
        _updateTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update data for stats view every second
        if (_updateTimer >= 1.0f)
        {
            _updateTimer = 0;
            _drawSystemStats.AddPriceDataPoint(_gameEngine.GetMarket().GetAllResources());
        }
    }

    public void Draw(GameTime gameTime)
    {
        // Draw the appropriate view
        switch (_currentView)
        {
            case GameView.GameOverview:
                _drawSystemGame.Draw(gameTime);
                break;
            case GameView.ProvinceDetail:
                _drawSystemProvince.Draw(gameTime);
                break;
            case GameView.Statistics:
                _drawSystemStats.Draw(gameTime);
                break;
        }
    }

    public void SwitchView(GameView view)
    {
        _currentView = view;
    }

    private void OnButtonClick(string buttonId)
    {
        switch (buttonId)
        {
            case "Stats":
                SwitchView(GameView.Statistics);
                break;
            case "Back":
                SwitchView(GameView.GameOverview);
                break;
        }
    }
}