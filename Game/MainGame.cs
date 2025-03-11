using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UEconomy.Engine;
using UEconomy.Engine.Interfaces;
using UEconomy.Graphics;
using UEconomy.Graphics.DrawSystem;

namespace UEconomy.Game;

public class MainGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    private IGameEngine _gameEngine;
    private IGraphicsEngine _graphicsEngine;
    private IEventEngine _eventEngine;
    private UI _ui;

    // Track window resize events
    private bool _windowSizeChanged = false;
    private int _lastWindowWidth;
    private int _lastWindowHeight;

    public MainGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        ConfigureGraphics();

        // Enable window resizing
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    private void ConfigureGraphics()
    {
        _graphicsDeviceManager.IsFullScreen = false;
        _graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphicsDeviceManager.PreferredBackBufferHeight =
            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
        _graphicsDeviceManager.ApplyChanges();

        _lastWindowWidth = _graphicsDeviceManager.PreferredBackBufferWidth;
        _lastWindowHeight = _graphicsDeviceManager.PreferredBackBufferHeight;
    }

    private void OnClientSizeChanged(object sender, System.EventArgs e)
    {
        _windowSizeChanged = true;
    }

    protected override void Initialize()
    {
        // Create and initialize UI first as other components depend on its dimensions
        _ui = new UI();
        _ui.Initialize(GraphicsDevice);

        // Create core game components
        var stats = new Stats();
        _gameEngine = new GameEngine(stats);
        _gameEngine.Initialize(_ui.CellSize, _ui.GridWidth, _ui.GridHeight);

        _eventEngine = new EventEngine(_gameEngine, _ui);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Create basic textures
        var provinceTexture = CreateSolidTexture(Color.White);
        var uiBackground = CreateSolidTexture(Color.Black);

        // Load fonts
        var font = Content.Load<SpriteFont>("Arial");

        // Create draw systems
        var drawSystemGame = new DrawSystemGame(
            GraphicsDevice,
            provinceTexture,
            uiBackground,
            font,
            _ui,
            _eventEngine,
            _gameEngine);

        var drawSystemProvince = new DrawSystemProvince(
            GraphicsDevice,
            provinceTexture,
            uiBackground,
            font,
            _ui,
            _eventEngine,
            _gameEngine);

        var drawSystemStats = new DrawSystemStats(
            GraphicsDevice,
            provinceTexture,
            uiBackground,
            font,
            _ui,
            _eventEngine,
            _gameEngine);

        // Create graphics engine
        _graphicsEngine = new GraphicsEngine(
            GraphicsDevice,
            _gameEngine,
            _eventEngine,
            _ui,
            drawSystemGame,
            drawSystemProvince,
            drawSystemStats);

        base.LoadContent();
    }

    private Texture2D CreateSolidTexture(Color color)
    {
        var texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { color });
        return texture;
    }

    protected override void Update(GameTime gameTime)
    {
        // Check for exit condition
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // Handle window resize if needed
        if (_windowSizeChanged)
        {
            HandleWindowResize();
            _windowSizeChanged = false;
        }

        // Update game components
        _gameEngine.Update(gameTime);
        _eventEngine.Update();
        _graphicsEngine.Update(gameTime);

        base.Update(gameTime);
    }

    private void HandleWindowResize()
    {
        // Get current window size
        var currentWidth = Window.ClientBounds.Width;
        var currentHeight = Window.ClientBounds.Height;

        // Only update if size has actually changed
        if (currentWidth != _lastWindowWidth || currentHeight != _lastWindowHeight)
        {
            _lastWindowWidth = currentWidth;
            _lastWindowHeight = currentHeight;

            // Update graphics device
            _graphicsDeviceManager.PreferredBackBufferWidth = currentWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = currentHeight;
            _graphicsDeviceManager.ApplyChanges();

            // Update UI scaling
            _ui.ResizeUI(GraphicsDevice);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(40, 45, 55));

        _graphicsEngine.Draw(gameTime);

        base.Draw(gameTime);
    }
}