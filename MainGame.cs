using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UEconomy;

public class MainGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private List<Province> _provinces;
    private Texture2D _provinceTexture;
    private int _gridWidth = 8;
    private int _gridHeight = 6;
    private int _cellSize = 80;

    private Market _market;
    private List<Population> _populations;

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
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (gameTime.TotalGameTime.TotalSeconds % 3 < 0.016)
        {
            SimulateEconomy();
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

            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle((int)province.Position.X, (int)province.Position.Y, _cellSize - 2, _cellSize - 2),
                provinceColor
            );

            var population = _populations.Find(p => p.ProvinceId == province.Id);
            if (population != null)
            {
                _spriteBatch.DrawString(
                    Content.Load<SpriteFont>("Arial"),
                    $"{population.Size}",
                    new Vector2(province.Position.X + 5, province.Position.Y + 5),
                    Color.White
                );
            }
        }

        _spriteBatch.DrawString(
            Content.Load<SpriteFont>("Arial"),
            GetMarketInfo(),
            new Vector2(_gridWidth * _cellSize + 20, 20),
            Color.White
        );

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private string GetMarketInfo()
    {
        var info = "Rynek:\n";
        foreach (var resource in new[] { "Zboże", "Drewno", "Żelazo" })
        {
            info += $"{resource}: Cena {_market.GetPrice(resource):F2}$ | Popyt {_market.GetDemand(resource):F0} | Podaż {_market.GetSupply(resource):F0}\n";
        }

        return info;
    }

    private void SimulateEconomy()
    {
        _market.ResetOffers();

        foreach (var population in _populations)
        {
            var province = _provinces.First(p => p.Id == population.ProvinceId);
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
    }
}