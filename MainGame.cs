using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UEconomy.Engine;

namespace UEconomy;

public class MainGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private List<Province> _provinces;
    private Texture2D _provinceTexture;
    private Texture2D _uiBackground;
    private int _gridWidth = 8;
    private int _gridHeight = 6;
    private int _cellSize = 80;

    private Market _market;
    private List<Population> _populations;

    private Province _selectedProvince;
    private MouseState _previousMouseState;
    private SpriteFont _font;

    private Dictionary<string, float> _globalStats = new();
    private Dictionary<string, List<float>> _priceHistory = new();
    private Dictionary<string, float> _totalTraded = new();

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = _gridWidth * _cellSize + 400;
        _graphics.PreferredBackBufferHeight = _gridHeight * _cellSize + 200;
    }

    protected override void Initialize()
    {
        _provinces = new List<Province>();
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                var province = new Province
                {
                    Id = _provinces.Count,
                    Name = $"Province {x}, {y}",
                    Position = new Vector2(x * _cellSize, y * _cellSize),
                    Resources = new Dictionary<string, float>
                    {
                        { "Zboże", x % 3 == 0 ? 100 : 20 },
                        { "Drewno", y % 2 == 0 ? 80 : 10 },
                        { "Żelazo", (x + y) % 4 == 0 ? 50 : 0 }
                    }
                };
                _provinces.Add(province);
            }
        }

        _populations = new List<Population>();
        Random random = new();

        foreach (
            var population in _provinces.Select(
                province => new Population
                {
                    Size = random.Next(100, 1000),
                    Needs = new Dictionary<string, float>
                    {
                        { "Zboże", 0.5f },
                        { "Drewno", 0.3f },
                        { "Żelazo", 0.1f }
                    },
                    Wealth = random.Next(500, 2000),
                    ProvinceId = province.Id
                }))
        {
            _populations.Add(population);
        }

        _market = new Market();
        _market.Initialize(new[] { "Zboże", "Drewno", "Żelazo" });

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _provinceTexture = new Texture2D(GraphicsDevice, 1, 1);
        _provinceTexture.SetData(new[] { Color.White });

        _uiBackground = new Texture2D(GraphicsDevice, 1, 1);
        _uiBackground.SetData(new[] { Color.Black });

        _font = Content.Load<SpriteFont>("Arial");


        foreach (var resource in _market.GetAllResources())
        {
            _priceHistory[resource] = new List<float> { _market.GetPrice(resource) };
            _totalTraded[resource] = 0;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var currentMouseState = Mouse.GetState();
        if (currentMouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            _selectedProvince = null;
            foreach (
                var province in from province in _provinces
                let provinceRect = new Rectangle(
                    (int)province.Position.X,
                    (int)province.Position.Y,
                    _cellSize - 2,
                    _cellSize - 2
                )
                where provinceRect.Contains(currentMouseState.Position)
                select province
            )
            {
                _selectedProvince = province;
                break;
            }
        }

        _previousMouseState = currentMouseState;

        if (gameTime.TotalGameTime.TotalSeconds % 3 < 0.016)
        {
            SimulateEconomy();

            foreach (var resource in _market.GetAllResources())
            {
                _priceHistory[resource].Add(_market.GetPrice(resource));
                if (_priceHistory[resource].Count > 20)
                {
                    _priceHistory[resource].RemoveAt(0);
                }
            }

            UpdateGlobalStats();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        foreach (var province in _provinces)
        {
            var provinceColor = new Color(
                province.Resources.GetValueOrDefault("Żelazo") > 0 ? 0.5f : 0.2f,
                province.Resources.GetValueOrDefault("Drewno") > 40 ? 0.8f : 0.2f,
                province.Resources.GetValueOrDefault("Zboże") > 50 ? 0.6f : 0.2f
            );

            if (_selectedProvince != null && _selectedProvince.Id == province.Id)
            {
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(
                        (int)province.Position.X - 2,
                        (int)province.Position.Y - 2,
                        _cellSize + 2,
                        _cellSize + 2
                    ),
                    Color.Yellow
                );
            }

            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle((int)province.Position.X, (int)province.Position.Y, _cellSize - 2, _cellSize - 2),
                provinceColor
            );

            var population = _populations.Find(p => p.ProvinceId == province.Id);
            if (population != null)
            {
                _spriteBatch.DrawString(
                    _font,
                    $"{population.Size}",
                    new Vector2(province.Position.X + 5, province.Position.Y + 5),
                    Color.White
                );
            }
        }

        DrawGlobalStatsPanel();

        if (_selectedProvince != null)
        {
            DrawProvinceDetailsPanel();
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawGlobalStatsPanel()
    {
        var panelX = _gridWidth * _cellSize + 10;
        const int panelY = 10;
        const int panelWidth = 200;
        const int panelHeight = 400;

        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX, panelY, panelWidth, panelHeight),
            new Color(0, 0, 0, 180)
        );

        _spriteBatch.DrawString(
            _font,
            "STATYSTYKI GLOBALNE",
            new Vector2(panelX + 10, panelY + 10),
            Color.Yellow
        );

        var yPos = panelY + 40;
        _spriteBatch.DrawString(
            _font,
            "Ceny rynkowe:",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 25;
        foreach (var resource in _market.GetAllResources())
        {
            _spriteBatch.DrawString(
                _font,
                $"{resource}: {_market.GetPrice(resource):F2}$ ({_market.GetSupplyDemandRatio(resource):F2})",
                new Vector2(panelX + 20, yPos),
                Color.White
            );
            yPos += 20;
        }

        yPos += 10;
        _spriteBatch.DrawString(
            _font,
            $"Populacja: {_globalStats.GetValueOrDefault("Total Population"):F0}",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 25;
        _spriteBatch.DrawString(
            _font,
            "Całkowite zasoby:",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 20;
        foreach (var resource in _market.GetAllResources())
        {
            _spriteBatch.DrawString(
                _font,
                $"{resource}: {_globalStats.GetValueOrDefault($"Total {resource}"):F0}",
                new Vector2(panelX + 20, yPos),
                Color.White
            );
            yPos += 20;
        }

        yPos += 10;
        _spriteBatch.DrawString(
            _font,
            "Ostatnia wymiana handlowa:",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 20;
        foreach (var resource in _totalTraded.Keys)
        {
            _spriteBatch.DrawString(
                _font,
                $"{resource}: {_totalTraded[resource]:F0} jedn.",
                new Vector2(panelX + 20, yPos),
                Color.White
            );
            yPos += 20;
        }
    }

    private void DrawProvinceDetailsPanel()
    {
        var panelX = (_gridWidth * _cellSize) / 2 - 150;
        var panelY = (_gridHeight * _cellSize) / 2 - 150;
        const int panelWidth = 300;
        const int panelHeight = 400;

        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX, panelY, panelWidth, panelHeight),
            new Color(0, 0, 50, 230)
        );

        var population = _populations.Find(p => p.ProvinceId == _selectedProvince.Id);

        _spriteBatch.DrawString(
            _font,
            $"PROWINCJA: {_selectedProvince.Name}",
            new Vector2(panelX + 10, panelY + 10),
            Color.Yellow
        );

        var yPos = panelY + 40;
        _spriteBatch.DrawString(
            _font,
            $"Populacja: {population?.Size ?? 0} osób",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 20;
        _spriteBatch.DrawString(
            _font,
            $"Zamożność: {population?.Wealth.ToString("F2") ?? "0"} $",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 30;
        _spriteBatch.DrawString(
            _font,
            "Dostępne zasoby:",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 20;
        foreach (var resource in _selectedProvince.Resources)
        {
            _spriteBatch.DrawString(
                _font,
                $"{resource.Key}: {resource.Value:F1} jednostek",
                new Vector2(panelX + 20, yPos),
                Color.White
            );
            yPos += 20;
        }

        if (population != null)
        {
            yPos += 10;
            _spriteBatch.DrawString(
                _font,
                "Potrzeby populacji (na osobę):",
                new Vector2(panelX + 10, yPos),
                Color.White
            );

            yPos += 20;
            foreach (var need in population.Needs)
            {
                _spriteBatch.DrawString(
                    _font,
                    $"{need.Key}: {need.Value:F2} jedn./os.",
                    new Vector2(panelX + 20, yPos),
                    Color.White
                );
                yPos += 20;
            }

            yPos += 10;
            _spriteBatch.DrawString(
                _font,
                "Całkowite zapotrzebowanie:",
                new Vector2(panelX + 10, yPos),
                Color.White
            );

            yPos += 20;
            foreach (var need in population.Needs)
            {
                var totalNeed = need.Value * population.Size;
                _spriteBatch.DrawString(
                    _font,
                    $"{need.Key}: {totalNeed:F1} jedn.",
                    new Vector2(panelX + 20, yPos),
                    Color.White
                );
                yPos += 20;
            }
        }

        // Przycisk zamknicia
        _spriteBatch.DrawString(
            _font,
            "X",
            new Vector2(panelX + panelWidth - 20, panelY + 10),
            Color.Red
        );
    }

    private void SimulateEconomy()
    {
        _market.ResetOffers();

        foreach (var resource in _market.GetAllResources())
        {
            _totalTraded[resource] = 0;
        }

        foreach (var population in _populations)
        {
            var province = _provinces.Find(p => p.Id == population.ProvinceId);
            foreach (var (resourceName, demandAmount) in population.Needs)
            {
                var totalDemand = demandAmount * population.Size;

                if (province.Resources.TryGetValue(resourceName, out var availableResource) && availableResource > 0)
                {
                    var usedResource = Math.Min(availableResource, totalDemand);
                    province.Resources[resourceName] -= usedResource;
                    totalDemand -= usedResource;
                }

                if (totalDemand > 0)
                {
                    var maxPrice = population.Wealth / (population.Size * 2);
                    _market.AddDemand(resourceName, totalDemand, maxPrice, population);
                }
            }

            foreach (var resource in province.Resources)
            {
                if (resource.Value > 50)
                {
                    var surplusToSell = (resource.Value - 50) / 2;
                    const int minPrice = 10;
                    _market.AddSupply(resource.Key, surplusToSell, minPrice, province);
                }
            }
        }

        _market.ResolveTrades();

        foreach (var resource in _market.GetAllResources())
        {
            _totalTraded[resource] = _market.GetLastTradedAmount(resource);
        }
    }

    private void UpdateGlobalStats()
    {
        _globalStats["Total Population"] = _populations.Sum(p => p.Size);

        foreach (var resource in _market.GetAllResources())
        {
            _globalStats[$"Total {resource}"] = _provinces.Sum(p =>
                p.Resources.TryGetValue(resource, out var pResource) ? pResource : 0);

            float totalDemand = 0;
            foreach (var population in _populations)
            {
                if (population.Needs.TryGetValue(resource, out var need))
                {
                    totalDemand += need * population.Size;
                }
            }

            _globalStats[$"{resource} Demand"] = totalDemand;
        }

        _globalStats["Total Wealth"] = _populations.Sum(p => p.Wealth);
    }
}