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
    private Building _selectedBuilding;
    private int _selectedBuildingIndex = -1;

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
        // Inicjalizacja prowincji
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
                        { "Zboże", 50 },
                        { "Drewno", 50 },
                        { "Żelazo", 30 },
                        { "Węgiel", 30 },
                        { "Meble", 20 },
                        { "Ubrania", 20 }
                    }
                };

                // Dodaj przykładowe budynki w zależności od lokalizacji
                if (x % 3 == 0)
                    province.AddBuilding("FieldCrop");
                if (y % 3 == 0)
                    province.AddBuilding("Sawmill");
                if ((x + y) % 4 == 0)
                    province.AddBuilding("IronMine");
                if ((x + y) % 5 == 0)
                    province.AddBuilding("CoalMine");
                if (x % 4 == 0 && y % 2 == 0)
                    province.AddBuilding("FurnitureFactory");
                if (x % 2 == 0 && y % 4 == 0)
                    province.AddBuilding("TailorShop");

                _provinces.Add(province);
            }
        }

        // Inicjalizacja populacji
        _populations = new List<Population>();
        Random random = new();

        foreach (
            var population in _provinces.Select(province => new Population
            {
                Size = random.Next(100, 1000),
                Wealth = random.Next(500, 2000),
                ProvinceId = province.Id
            }))
        {
            population.InitializeNeeds();
            _populations.Add(population);
        }

        _market = new Market();
        _market.Initialize(new[] { "Zboże", "Drewno", "Żelazo", "Węgiel", "Meble", "Ubrania" });

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

            var panelX = _gridWidth * _cellSize + 10;
            const int panelY = 10;
            const int panelWidth = 200;
            const int panelHeight = 400;

            if (_selectedProvince != null && _selectedBuildingIndex >= 0 &&
                _selectedBuildingIndex < _selectedProvince.Buildings.Count)
            {
                var buildingBox = new Rectangle(
                    panelX + 20,
                    panelY + 200 + (_selectedBuildingIndex * 30),
                    200,
                    25
                );

                if (buildingBox.Contains(currentMouseState.Position))
                {
                    _selectedBuilding = _selectedProvince.Buildings[_selectedBuildingIndex];
                }

                if (_selectedBuilding != null)
                {
                    var upgradeButton = new Rectangle(
                        panelX + 180,
                        panelY + 200 + (_selectedBuildingIndex * 30),
                        20,
                        20
                    );

                    if (upgradeButton.Contains(currentMouseState.Position))
                    {
                        var population = _populations.Find(p => p.ProvinceId == _selectedProvince.Id);
                        if (population != null && population.Wealth >= _selectedBuilding.UpgradeCost)
                        {
                            population.Wealth -= _selectedBuilding.UpgradeCost;
                            _selectedBuilding.Upgrade();
                        }
                    }
                }
            }

            if (_selectedProvince != null)
            {
                string[] buildingTypes =
                    { "FieldCrop", "Sawmill", "IronMine", "CoalMine", "FurnitureFactory", "TailorShop" };

                for (var i = 0; i < buildingTypes.Length; i++)
                {
                    var buildButton = new Rectangle(
                        panelX + 10 + (i % 3) * 90,
                        panelY + panelHeight - 80 + (i / 3) * 35,
                        80,
                        30
                    );

                    if (buildButton.Contains(currentMouseState.Position))
                    {
                        var buildCost = 500;
                        var population = _populations.Find(p => p.ProvinceId == _selectedProvince.Id);
                        if (population != null && population.Wealth >= buildCost)
                        {
                            population.Wealth -= buildCost;
                            _selectedProvince.AddBuilding(buildingTypes[i]);
                        }
                    }
                }
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
        // Rysuj tło panelu
        var panelX = _gridWidth * _cellSize + 10;
        var panelY = 10;
        var panelWidth = 220;
        var panelHeight = 550;

        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX, panelY, panelWidth, panelHeight),
            new Color(0, 0, 0, 180)
        );

        // Nagłówek
        _spriteBatch.DrawString(
            _font,
            "STATYSTYKI GLOBALNE",
            new Vector2(panelX + 10, panelY + 10),
            Color.Yellow
        );

        // Ceny rynkowe
        int yPos = panelY + 40;
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

        // Całkowita populacja
        yPos += 10;
        _spriteBatch.DrawString(
            _font,
            $"Populacja: {_globalStats.GetValueOrDefault("Total Population"):F0}",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        // Całkowite zasoby
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

        // Globalna produkcja
        yPos += 10;
        _spriteBatch.DrawString(
            _font,
            "Globalna produkcja (na turę):",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 20;
        foreach (var resource in _market.GetAllResources())
        {
            _spriteBatch.DrawString(
                _font,
                $"{resource}: {_globalStats.GetValueOrDefault($"Production {resource}"):F0}",
                new Vector2(panelX + 20, yPos),
                Color.White
            );
            yPos += 20;
        }

        // Wymiana handlowa
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

        // Średnie zadowolenie potrzeb
        yPos += 10;
        _spriteBatch.DrawString(
            _font,
            "Średnie zadowolenie potrzeb:",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 20;
        foreach (var resource in _market.GetAllResources())
        {
            if (_globalStats.ContainsKey($"Satisfaction {resource}"))
            {
                var satisfaction = _globalStats[$"Satisfaction {resource}"];
                var satColor = satisfaction > 0.8f ? Color.Green : (satisfaction > 0.5f ? Color.Yellow : Color.Red);

                _spriteBatch.DrawString(
                    _font,
                    $"{resource}: {(satisfaction * 100):F0}%",
                    new Vector2(panelX + 20, yPos),
                    satColor
                );
                yPos += 20;
            }
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

        // Budynki w prowincji
        yPos += 20;
        _spriteBatch.DrawString(
            _font,
            "Budynki:",
            new Vector2(panelX + 10, yPos),
            Color.White
        );

        yPos += 25;
        _selectedBuildingIndex = -1;

        for (var i = 0; i < _selectedProvince.Buildings.Count; i++)
        {
            var building = _selectedProvince.Buildings[i];
            var buildingColor = building.IsActive ? Color.White : Color.Gray;

            // Podświetl, jeśli to wybrany budynek
            if (_selectedBuilding != null && _selectedBuilding.Id == building.Id)
            {
                buildingColor = Color.Yellow;
            }

            _spriteBatch.DrawString(
                _font,
                $"{building.Name} (Lv.{building.Level}) [{building.CurrentWorkers}/{building.WorkersCapacity}]",
                new Vector2(panelX + 20, yPos),
                buildingColor
            );

            // Rysuj przycisk ulepszenia
            _spriteBatch.DrawString(
                _font,
                "+",
                new Vector2(panelX + 180, yPos),
                Color.Green
            );

            // Zapisz indeks budynku pod kursorem
            if (new Rectangle(panelX + 20, yPos, 150, 20).Contains(Mouse.GetState().Position))
            {
                _selectedBuildingIndex = i;
            }

            yPos += 30;
        }

        // Rysuj przyciski do budowy nowych budynków
        yPos = panelY + panelHeight - 80;
        _spriteBatch.DrawString(
            _font,
            "Zbuduj nowy budynek:",
            new Vector2(panelX + 10, yPos - 25),
            Color.White
        );

        string[] buildingNames = { "Pole", "Tartak", "Kopalnia Fe", "Kopalnia C", "Fabryka", "Szwalnia" };
        for (var i = 0; i < buildingNames.Length; i++)
        {
            var buttonX = panelX + 10 + (i % 3) * 90;
            var buttonY = yPos + (i / 3) * 35;

            // Tło przycisku
            _spriteBatch.Draw(
                _uiBackground,
                new Rectangle(buttonX, buttonY, 80, 30),
                new Color(80, 80, 100, 220)
            );

            // Tekst przycisku
            _spriteBatch.DrawString(
                _font,
                buildingNames[i],
                new Vector2(buttonX + 5, buttonY + 5),
                Color.White
            );
        }

// Jeśli budynek jest wybrany, pokaż szczegóły
        if (_selectedBuilding != null)
        {
            // Tło dla szczegółów budynku
            _spriteBatch.Draw(
                _uiBackground,
                new Rectangle(panelX + panelWidth + 10, panelY, 200, 280),
                new Color(0, 0, 50, 230)
            );

            var detailsX = panelX + panelWidth + 20;
            var detailsY = panelY + 10;

            // Nagłówek
            _spriteBatch.DrawString(
                _font,
                $"{_selectedBuilding.Name} (Poziom {_selectedBuilding.Level})",
                new Vector2(detailsX, detailsY),
                Color.Yellow
            );

            // Pracownicy
            detailsY += 30;
            _spriteBatch.DrawString(
                _font,
                $"Pracownicy: {_selectedBuilding.CurrentWorkers}/{_selectedBuilding.WorkersCapacity}",
                new Vector2(detailsX, detailsY),
                Color.White
            );

            // Status
            detailsY += 20;
            _spriteBatch.DrawString(
                _font,
                $"Status: {(_selectedBuilding.IsActive ? "Aktywny" : "Nieaktywny")}",
                new Vector2(detailsX, detailsY),
                _selectedBuilding.IsActive ? Color.Green : Color.Red
            );

            // Produkcja
            detailsY += 30;
            _spriteBatch.DrawString(
                _font,
                "Produkcja:",
                new Vector2(detailsX, detailsY),
                Color.White
            );

            detailsY += 20;
            foreach (var resource in _selectedBuilding.ProductionRates)
            {
                var actualProduction = resource.Value * _selectedBuilding.CurrentWorkers /
                    _selectedBuilding.WorkersCapacity * _selectedBuilding.Level;
                _spriteBatch.DrawString(
                    _font,
                    $"{resource.Key}: {actualProduction:F1}/tura",
                    new Vector2(detailsX + 10, detailsY),
                    Color.White
                );
                detailsY += 20;
            }

            // Zużycie
            if (_selectedBuilding.ConsumptionRates.Count > 0)
            {
                detailsY += 10;
                _spriteBatch.DrawString(
                    _font,
                    "Zużycie:",
                    new Vector2(detailsX, detailsY),
                    Color.White
                );

                detailsY += 20;
                foreach (var resource in _selectedBuilding.ConsumptionRates)
                {
                    var actualConsumption = resource.Value * _selectedBuilding.CurrentWorkers /
                        _selectedBuilding.WorkersCapacity * _selectedBuilding.Level;
                    _spriteBatch.DrawString(
                        _font,
                        $"{resource.Key}: {actualConsumption:F1}/tura",
                        new Vector2(detailsX + 10, detailsY),
                        Color.White
                    );
                    detailsY += 20;
                }
            }

            // Koszt ulepszenia
            detailsY += 30;
            _spriteBatch.DrawString(
                _font,
                $"Koszt ulepszenia: {_selectedBuilding.UpgradeCost}$",
                new Vector2(detailsX, detailsY),
                Color.White
            );

            // Przycisk ulepszenia
            detailsY += 30;
            var canAffordUpgrade = false;
            if (population != null)
            {
                canAffordUpgrade = population.Wealth >= _selectedBuilding.UpgradeCost;
            }

            var upgradeColor = canAffordUpgrade ? Color.Green : Color.Gray;

            // Tło przycisku
            _spriteBatch.Draw(
                _uiBackground,
                new Rectangle(detailsX, detailsY, 180, 30),
                new Color(60, 60, 80, 220)
            );

            // Tekst przycisku
            _spriteBatch.DrawString(
                _font,
                "Ulepsz budynek",
                new Vector2(detailsX + 10, detailsY + 5),
                upgradeColor
            );
        }
    }

    private void SimulateEconomy()
    {
        // Aktualizuj produkcję w każdej prowincji
        foreach (var province in _provinces)
        {
            var population = _populations.Find(p => p.ProvinceId == province.Id);
            if (population != null)
            {
                province.UpdateProduction(population);
            }
        }

        // Aktualizuj produkcję w każdej prowincji
        _market.ResetOffers();

        // Resetujemy statystyki handlowe
        foreach (var resource in _market.GetAllResources())
        {
            _totalTraded[resource] = 0;
        }

        // Każda populacja próbuje zaspokoić swoje potrzeby
        foreach (var population in _populations)
        {
            // Znajdź prowincję, w której znajduje się populacja
            var province = _provinces.Find(p => p.Id == population.ProvinceId);

            // Pobierz zasoby z prowincji i oblicz poziom zadowolenia
            var availableResources = new Dictionary<string, float>(province.Resources);
            population.ConsumptionStep(availableResources);

            // Aktualizuj zasoby prowincji po konsumpcji
            province.Resources = availableResources;

            // Oblicz maksymalne ceny, jakie populacja jest gotowa zapłacić
            var maxPrices = population.CalculateMaxPrices();

            foreach (var (resourceName, demandPerCapita) in population.Needs)
            {
                var totalDemand = demandPerCapita * population.Size;

                // Sprawdź, ile zasobów jest dostępnych w prowincji
                if (province.Resources.TryGetValue(resourceName, out var availableResource) && availableResource > 0)
                {
                    // Używaj lokalnych zasobów najpierw
                    var usedResource = Math.Min(availableResource, totalDemand);
                    province.Resources[resourceName] -= usedResource;
                    totalDemand -= usedResource;
                }

                // Jeśli nadal są niezaspokojone potrzeby, stwórz popyt na rynku
                if (totalDemand > 0)
                {
                    var maxPrice = maxPrices.TryGetValue(resourceName, out var price)
                        ? price
                        : population.Wealth / (population.Size * 2);
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
        // Całkowita populacja
        _globalStats["Total Population"] = _populations.Sum(p => p.Size);

        // Całkowite zasoby
        foreach (var resource in _market.GetAllResources())
        {
            _globalStats[$"Total {resource}"] = _provinces.Sum(p =>
                p.Resources.TryGetValue(resource, out var pResource) ? pResource : 0);

            // Globalna produkcja
            var totalProduction = 0.0f;
            foreach (var province in _provinces)
            {
                totalProduction += province.Buildings
                    .Where(building => building.IsActive && building.ProductionRates.ContainsKey(resource))
                    .Sum(building => building.ProductionRates[resource] * building.CurrentWorkers /
                        building.WorkersCapacity * building.Level);

                // Dodaj produkcję niezatrudnionych
                if (resource == "Zboże" || resource == "Meble" || resource == "Ubrania")
                {
                    var subsistenceFactor = province.AvailableWorkers / 10.0f;
                    if (resource == "Zboże")
                        totalProduction += 10.0f * subsistenceFactor;
                    else
                        totalProduction += 1.0f * subsistenceFactor;
                }
            }

            _globalStats[$"Production {resource}"] = totalProduction;

            // Średnie zapotrzebowanie na zasoby
            var totalDemand = _populations
                .Where(population => population.Needs.ContainsKey(resource))
                .Sum(population => population.Needs[resource] * population.Size);

            _globalStats[$"{resource} Demand"] = totalDemand;

            // Średnie zadowolenie z zaspokojenia potrzeb
            var totalSatisfaction = 0.0f;
            var populationsWithNeed = 0;
            foreach (
                var population in _populations.Where(population => population.Satisfaction.ContainsKey(resource))
            )
            {
                totalSatisfaction += population.Satisfaction[resource];
                populationsWithNeed++;
            }

            if (populationsWithNeed > 0)
            {
                _globalStats[$"Satisfaction {resource}"] = totalSatisfaction / populationsWithNeed;
            }
        }

        // Całkowita zamożność
        _globalStats["Total Wealth"] = _populations.Sum(p => p.Wealth);

        // Suma pracowników w budynkach
        _globalStats["Employed Workers"] = _provinces.Sum(p => p.Buildings.Sum(b => b.CurrentWorkers));

        // Całkowita liczba dostępnych pracowników
        _globalStats["Available Workers"] = _provinces.Sum(p => p.AvailableWorkers);
    }
}