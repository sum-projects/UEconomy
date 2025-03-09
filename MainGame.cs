using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UEconomy.Engine;
using UEconomy.Game;
using UEconomy.Graphics;

namespace UEconomy;

public class MainGame : Microsoft.Xna.Framework.Game
{
    private GameEngine _gameEngine;
    private GraphicsEngine _graphicsEngine;
    private EventEngine _eventEngine;
    private UI _ui;
    private GraphicsDeviceManager _graphicsDeviceManager;

    public MainGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphicsDeviceManager = new GraphicsDeviceManager(this);

        // _graphics.IsFullScreen = true;
        // _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        // _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphicsDeviceManager.IsFullScreen = false;
        _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        _gameEngine = new GameEngine(new Stats());

        _ui = new UI();
        _eventEngine = new EventEngine(_gameEngine, _ui);
        _graphicsEngine = new GraphicsEngine(_gameEngine, _eventEngine, _ui);
    }

    protected override void Initialize()
    {
        _ui.Initialize(GraphicsDevice);

        _gameEngine.Initialize(_ui.CellSize, _ui.GridWidth, _ui.GridHeight);
        _graphicsEngine.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _graphicsEngine.LoadContent(Content.Load<SpriteFont>("Arial"), GraphicsDevice);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape)
        )
        {
            Exit();
        }


        _gameEngine.Update(gameTime);
        _eventEngine.Update();
        _graphicsEngine.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(40, 45, 55));

        _graphicsEngine.Draw(gameTime);

        base.Draw(gameTime);
    }

    // protected void OnClientSizeChanged(object sender, EventArgs e)
    // {
    //     // Zaktualizuj skalowanie UI po zmianie rozmiaru okna
    //     CalculateUIScaling();
    //
    //     // Ustaw nowe pozycje prowincji
    //     for (int x = 0; x < _gridWidth; x++)
    //     {
    //         for (int y = 0; y < _gridHeight; y++)
    //         {
    //             int index = y * _gridWidth + x;
    //             if (index < _provinces.Count)
    //             {
    //                 _provinces[index].Position = new Vector2(x * _cellSize, y * _cellSize);
    //             }
    //         }
    //     }
    // }
}