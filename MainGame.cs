using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UEconomy.Engine;
using UEconomy.Game;
using UEconomy.Graphics;
using UEconomy.Graphics.DrawSystem;

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
        _graphicsDeviceManager.PreferredBackBufferHeight =
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        _gameEngine = new GameEngine(new Stats());

        _ui = new UI();
        _eventEngine = new EventEngine(_gameEngine, _ui);

        //DrawSystemGame(Texture2D provinceTexture, Texture2D uiBackground, SpriteFont font, UI ui, EventEngine eventEngine)
    }

    protected override void Initialize()
    {
        _ui.Initialize(GraphicsDevice);

        _gameEngine.Initialize(_ui.CellSize, _ui.GridWidth, _ui.GridHeight);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        var provinceTexture = new Texture2D(GraphicsDevice, 1, 1);
        provinceTexture.SetData(new[] { Color.White });


        var uiBackground = new Texture2D(GraphicsDevice, 1, 1);
        uiBackground.SetData(new[] { Color.Black });

        var font = Content.Load<SpriteFont>("Arial");

        _graphicsEngine = new GraphicsEngine(
            GraphicsDevice,
            _gameEngine, _eventEngine, _ui,
            new DrawSystemGame(provinceTexture, uiBackground, font, _ui, _eventEngine),
            new DrawSystemProvince(provinceTexture, uiBackground, font, _ui, _eventEngine),
            new DrawSystemStats(provinceTexture, uiBackground, font, _ui, _eventEngine)
        );


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