using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Graphics;
using UEconomy.Graphics.DrawSystem;

namespace UEconomy.Engine;

public class GraphicsEngine
{
    private Texture2D _backgroundTexture;
    private EventEngine _eventEngine;
    private SpriteBatch _spriteBatch;

    private UI _ui;

    private readonly GameEngine _gameEngine;

    private DrawSystemGame _drawSystemGame;
    private DrawSystemProvince _drawSystemProvince;
    private DrawSystemStats _drawSystemStats;

    private float _totalGameTime = 0;

    public GraphicsEngine(
        GraphicsDevice graphics,
        GameEngine gameEngine,
        EventEngine eventEngine,
        UI ui,
        DrawSystemGame drawSystemGame,
        DrawSystemProvince drawSystemProvince,
        DrawSystemStats drawSystemStats
    )
    {
        _gameEngine = gameEngine;
        _eventEngine = eventEngine;
        _ui = ui;
        _drawSystemGame = drawSystemGame;
        _drawSystemProvince = drawSystemProvince;
        _drawSystemStats = drawSystemStats;

        _spriteBatch = new SpriteBatch(graphics);

        _backgroundTexture = new Texture2D(graphics, 1, 1);
        _backgroundTexture.SetData(new[] { Color.White });
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        // Rysuj tło dla całej planszy
        _spriteBatch.Draw(
            _backgroundTexture,
            new Rectangle(0, 0, _ui.ScreenWidth, _ui.ScreenHeight),
            new Color(30, 35, 45)
        );

        // Rysuj tło siatki
        _spriteBatch.Draw(
            _drawSystemGame.UiBackground,
            new Rectangle(
                _ui.GridMarginX,
                _ui.GridMarginY,
                _ui.GridWidth * _ui.CellSize,
                _ui.GridHeight * _ui.CellSize
            ),
            new Color(50, 55, 65, 220)
        );

        // Rysuj siatkę prowincji
        foreach (var province in _gameEngine._provinces)
        {
            // Oblicz pozycję prowincji na ekranie z uwzględnieniem marginesów
            var cellX = (int)province.Position.X + _ui.GridMarginX;
            var cellY = (int)province.Position.Y + _ui.GridMarginY;

            // Oblicz kolor prowincji na podstawie jej zasobów i typu
            var provinceColor = _drawSystemGame.CalculateProvinceColor(province);

            // Dodaj efekt gradientu do komórek
            _drawSystemGame.DrawGradientCell(_spriteBatch, cellX, cellY, _ui.CellSize - 2, _ui.CellSize - 2,
                provinceColor);

            // Jeśli prowincja jest wybrana, dodaj efektowne obramowanie
            if (_eventEngine.SelectedProvince != null && _eventEngine.SelectedProvince.Id == province.Id)
            {
                _drawSystemGame.DrawSelectedBorder(_spriteBatch, cellX, cellY, _ui.CellSize, _ui.CellSize,
                    _totalGameTime);
            }

            // Rysuj informacje o prowincji
            var population = _gameEngine._populations.Find(p => p.ProvinceId == province.Id);
            if (population != null)
            {
                // Dodaj ikony budynków jako małe symbole
                _drawSystemProvince.DrawProvinceIcons(_spriteBatch, province, population, cellX, cellY);
            }
        }

        // Rysuj panel boczny ze statystykami globalnymi
        _drawSystemStats.DrawGlobalStatsPanel(_gameEngine, _spriteBatch,
            _gameEngine._market.GetAllResources().ToList());

        // Rysuj szczegółowe informacje o wybranej prowincji
        if (_eventEngine.SelectedProvince != null)
        {
            var population = _gameEngine._populations.Find(p => p.ProvinceId == _eventEngine.SelectedProvince.Id);

            _drawSystemProvince.DrawProvinceDetailsPanel(_spriteBatch, population);
        }

        _spriteBatch.End();
    }
    public void Update(GameTime gameTime)
    {
        // Aktualizuj całkowity czas gry (do animacji)
        _totalGameTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}