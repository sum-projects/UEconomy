using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UEconomy.Game;

namespace UEconomy.Engine;

public class GraphicsEngine
{
    public int GridWidth { get; private set; } = 8;
    public int GridHeight { get; private set; } = 6;
    public int CellSize { get; private set; } = 80;

    private float _fontScale = 1.0f;
    private int _gridMarginX = 0;
    private int _gridMarginY = 0;

    private GraphicsDeviceManager _graphics;
    private Texture2D _uiBackground;
    private Texture2D _provinceTexture;
    private Texture2D _backgroundTexture;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    private Province _selectedProvince;
    private Building _selectedBuilding;
    private int _selectedBuildingIndex = -1;

    private MouseState _previousMouseState;

    private float _totalGameTime = 0;
    private string _selectedResource = "Zboże";

    private int _screenWidth;
    private int _screenHeight;

    private Rectangle[] _resourceButtonRects = new Rectangle[6]; // Zakładając, że mamy 6 zasobów

    private readonly GameEngine _gameEngine;

    public GraphicsEngine(GraphicsDeviceManager graphics, GameEngine gameEngine)
    {
        _gameEngine = gameEngine;

        _graphics = graphics;
        // _graphics.IsFullScreen = true;
        // _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        // _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        CalculateUIScaling();
    }

    public void Initialize()
    {
        _screenWidth = _graphics.GraphicsDevice.Viewport.Width;
        _screenHeight = _graphics.GraphicsDevice.Viewport.Height;
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        // Rysuj tło dla całej planszy
        _spriteBatch.Draw(
            _backgroundTexture,
            new Rectangle(0, 0, _screenWidth, _screenHeight),
            new Color(30, 35, 45)
        );

        // Rysuj tło siatki
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(
                _gridMarginX,
                _gridMarginY,
                GridWidth * CellSize,
                GridHeight * CellSize
            ),
            new Color(50, 55, 65, 220)
        );

        // Rysuj siatkę prowincji
        foreach (var province in _gameEngine._provinces)
        {
            // Oblicz pozycję prowincji na ekranie z uwzględnieniem marginesów
            var cellX = (int)province.Position.X + _gridMarginX;
            var cellY = (int)province.Position.Y + _gridMarginY;

            // Oblicz kolor prowincji na podstawie jej zasobów i typu
            Color provinceColor = CalculateProvinceColor(province);

            // Dodaj efekt gradientu do komórek
            DrawGradientCell(cellX, cellY, CellSize - 2, CellSize - 2, provinceColor);

            // Jeśli prowincja jest wybrana, dodaj efektowne obramowanie
            if (_selectedProvince != null && _selectedProvince.Id == province.Id)
            {
                DrawSelectedBorder(cellX, cellY, CellSize, CellSize);
            }

            // Rysuj informacje o prowincji
            var population = _gameEngine._populations.Find(p => p.ProvinceId == province.Id);
            if (population != null)
            {
                // Dodaj ikony budynków jako małe symbole
                DrawProvinceIcons(province, cellX, cellY);

                // Rysuj liczbę mieszkańców
                _spriteBatch.DrawString(
                    _font,
                    $"{population.Size}",
                    new Vector2(cellX + 5, cellY + 5),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _fontScale * 0.8f,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        // Rysuj panel boczny ze statystykami globalnymi
        DrawGlobalStatsPanel();

        // Rysuj szczegółowe informacje o wybranej prowincji
        if (_selectedProvince != null)
        {
            DrawProvinceDetailsPanel();
        }

        _spriteBatch.End();
    }

    public void LoadContent(SpriteFont font)
    {
        _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

        // Stwórz podstawowe tekstury
        _provinceTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _provinceTexture.SetData(new[] { Color.White });

        _uiBackground = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _uiBackground.SetData(new[] { Color.Black });

        _backgroundTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _backgroundTexture.SetData(new[] { Color.White });

        // Załaduj czcionkę
        _font = font;
    }

    public void Update(GameTime gameTime)
    {
        // Aktualizuj całkowity czas gry (do animacji)
        _totalGameTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        var currentMouseState = Mouse.GetState();
        // Obsługa kliknięcia lewym przyciskiem myszy
        if (currentMouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            HandleMouseClick(currentMouseState.Position);
        }

        _previousMouseState = currentMouseState;
    }


    private void CalculateUIScaling()
    {
        // Pobierz rozmiar ekranu
        int screenWidth = _graphics.PreferredBackBufferWidth;
        int screenHeight = _graphics.PreferredBackBufferHeight;

        // Oblicz liczbę prowincji w poziomie i pionie dla wypełnienia ekranu
        // Pozostawiamy miejsce na panel po prawej stronie
        int panelWidth = (int)(screenWidth * 0.25f); // 25% szerokości ekranu na panel statystyk
        int availableWidth = screenWidth - panelWidth;

        // Oblicz optymalny rozmiar komórki i liczbę komórek
        CellSize = Math.Min(availableWidth / 10, screenHeight / 8); // Maksymalnie 10x8 prowincji
        GridWidth = availableWidth / CellSize;
        GridHeight = screenHeight / CellSize;

        // Ustaw marginesy, aby siatka była wycentrowana
        _gridMarginX = (availableWidth - (GridWidth * CellSize)) / 2;
        _gridMarginY = (screenHeight - (GridHeight * CellSize)) / 2;

        // Skaluj rozmiar czcionki w zależności od rozdzielczości
        _fontScale = screenHeight / 1080f; // 1.0 dla 1080p, proporcjonalnie dla innych rozdzielczości
    }

    private Color CalculateProvinceColor(Province province)
    {
        // Bazowa intensywność kolorów na podstawie różnych zasobów
        var red = 0.2f;
        var green = 0.2f;
        var blue = 0.2f;

        // Zwiększ intensywność koloru na podstawie posiadanych zasobów
        if (province.Resources.ContainsKey("Zboże") && province.Resources["Zboże"] > 50)
            green += Math.Min(0.6f, province.Resources["Zboże"] / 500f);

        if (province.Resources.ContainsKey("Drewno") && province.Resources["Drewno"] > 40)
            green += Math.Min(0.4f, province.Resources["Drewno"] / 400f);

        if (province.Resources.ContainsKey("Żelazo") && province.Resources["Żelazo"] > 30)
            red += Math.Min(0.5f, province.Resources["Żelazo"] / 300f);

        if (province.Resources.ContainsKey("Węgiel") && province.Resources["Węgiel"] > 30)
            blue += Math.Min(0.5f, province.Resources["Węgiel"] / 300f);

        if (province.Resources.ContainsKey("Meble") && province.Resources["Meble"] > 20)
            red += Math.Min(0.3f, province.Resources["Meble"] / 200f);

        if (province.Resources.ContainsKey("Ubrania") && province.Resources["Ubrania"] > 20)
            blue += Math.Min(0.3f, province.Resources["Ubrania"] / 200f);

        // Dodaj wpływ budynków
        foreach (var building in province.Buildings)
        {
            switch (building.Type)
            {
                case "FieldCrop":
                    green += 0.1f;
                    break;
                case "Sawmill":
                    green += 0.05f;
                    red += 0.05f;
                    break;
                case "IronMine":
                    red += 0.1f;
                    break;
                case "CoalMine":
                    blue += 0.1f;
                    break;
                case "FurnitureFactory":
                    red += 0.05f;
                    green += 0.05f;
                    break;
                case "TailorShop":
                    blue += 0.05f;
                    red += 0.05f;
                    break;
            }
        }

        // Upewnij się, że wartości kolorów są w zakresie [0, 1]
        red = Math.Min(1.0f, red);
        green = Math.Min(1.0f, green);
        blue = Math.Min(1.0f, blue);

        return new Color(red, green, blue);
    }

    private Color GetResourceColor(string resourceName)
    {
        switch (resourceName)
        {
            case "Zboże":
                return new Color(0.8f, 0.9f, 0.2f);
            case "Drewno":
                return new Color(0.6f, 0.4f, 0.2f);
            case "Żelazo":
                return new Color(0.7f, 0.7f, 0.7f);
            case "Węgiel":
                return new Color(0.3f, 0.3f, 0.3f);
            case "Meble":
                return new Color(0.8f, 0.6f, 0.4f);
            case "Ubrania":
                return new Color(0.5f, 0.5f, 0.9f);
            default:
                return Color.White;
        }
    }

    private void DrawGradientCell(int x, int y, int width, int height, Color baseColor)
    {
        // Tworzenie efektu gradientu - ciemniejszy na dole
        Color topColor = baseColor;
        Color bottomColor = new Color(
            (int)(baseColor.R * 0.7f),
            (int)(baseColor.G * 0.7f),
            (int)(baseColor.B * 0.7f)
        );

        // Rysuj prostokąt górny
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y, width, height / 2),
            topColor
        );

        // Rysuj prostokąt dolny
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y + height / 2, width, height - height / 2),
            bottomColor
        );

        // Dodaj ramkę
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y, width, 1),
            Color.Black * 0.3f
        );
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y, 1, height),
            Color.Black * 0.3f
        );
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x + width - 1, y, 1, height),
            Color.Black * 0.3f
        );
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y + height - 1, width, 1),
            Color.Black * 0.3f
        );
    }

    private void DrawSelectedBorder(int x, int y, int width, int height)
    {
        // Animowany kolor obramowania
        float pulse = (float)Math.Sin(_totalGameTime * 5) * 0.5f + 0.5f;
        Color borderColor = new Color(
            1.0f,
            1.0f - pulse * 0.7f,
            0.0f
        );

        int borderThickness = 3;

        // Górna krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x - borderThickness, y - borderThickness, width + borderThickness * 2, borderThickness),
            borderColor
        );

        // Lewa krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x - borderThickness, y - borderThickness, borderThickness, height + borderThickness * 2),
            borderColor
        );

        // Prawa krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x + width, y - borderThickness, borderThickness, height + borderThickness * 2),
            borderColor
        );

        // Dolna krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x - borderThickness, y + height, width + borderThickness * 2, borderThickness),
            borderColor
        );
    }

    private void DrawProvinceIcons(Province province, int cellX, int cellY)
    {
        var iconSize = CellSize / 10;
        var margin = 2;
        var iconX = cellX + CellSize - iconSize - margin;
        var iconY = cellY + margin;

        // Słownik kolorów dla różnych typów budynków
        var buildingColors = new Dictionary<string, Color>
        {
            { "FieldCrop", new Color(0.0f, 0.8f, 0.0f) },
            { "Sawmill", new Color(0.5f, 0.3f, 0.1f) },
            { "IronMine", new Color(0.6f, 0.6f, 0.6f) },
            { "CoalMine", new Color(0.2f, 0.2f, 0.2f) },
            { "FurnitureFactory", new Color(0.8f, 0.4f, 0.0f) },
            { "TailorShop", new Color(0.3f, 0.3f, 0.8f) }
        };

        // Narysuj ikony budynków
        foreach (
            var iconColor in province.Buildings
                .Select(building => buildingColors.TryGetValue(building.Type, out var color)
                    ? color
                    : Color.White)
        )
        {
            // Rysuj ikonę budynku jako mały kwadrat z odpowiednim kolorem
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(iconX, iconY, iconSize, iconSize),
                iconColor
            );

            // Dodaj obramowanie ikony
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(iconX, iconY, iconSize, 1),
                Color.Black * 0.5f
            );
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(iconX, iconY, 1, iconSize),
                Color.Black * 0.5f
            );
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(iconX + iconSize - 1, iconY, 1, iconSize),
                Color.Black * 0.5f
            );
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(iconX, iconY + iconSize - 1, iconSize, 1),
                Color.Black * 0.5f
            );

            iconY += iconSize + margin;
        }
    }

    private void DrawHeaderBar(int x, int y, int width, string title, Color textColor)
    {
        int headerHeight = (int)(30 * _fontScale);

        // Tło nagłówka
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(x, y, width, headerHeight),
            new Color(50, 60, 80, 180)
        );

        // Tekst nagłówka
        _spriteBatch.DrawString(
            _font,
            title,
            new Vector2(x + width / 2, y + headerHeight / 2),
            textColor,
            0f,
            new Vector2(_font.MeasureString(title).X / 2, _font.MeasureString(title).Y / 2),
            _fontScale * 1.2f,
            SpriteEffects.None,
            0f
        );
    }

    private void DrawProvinceDetailsPanel()
    {
        int panelWidth = (int)(_screenWidth * 0.35f);
        int panelHeight = (int)(_screenHeight * 0.7f);
        int panelX = (_screenWidth - panelWidth) / 2;
        int panelY = (_screenHeight - panelHeight) / 2;

        // Tło panelu z efektem gradientu
        Color topColor = new Color(20, 20, 40, 230);
        Color bottomColor = new Color(30, 30, 60, 230);

        // Górna część panelu
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX, panelY, panelWidth, panelHeight / 2),
            topColor
        );

        // Dolna część panelu
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX, panelY + panelHeight / 2, panelWidth, panelHeight - panelHeight / 2),
            bottomColor
        );

        // Obramowanie panelu
        DrawPanelBorder(panelX, panelY, panelWidth, panelHeight, new Color(80, 80, 120, 200));

        // Pobierz populację prowincji
        var population = _gameEngine._populations.Find(p => p.ProvinceId == _selectedProvince.Id);

        // Nagłówek z nazwą prowincji
        DrawHeaderBar(panelX, panelY, panelWidth, $"PROWINCJA: {_selectedProvince.Name}", Color.Gold);

        int sectionMargin = 20;
        int yPos = panelY + 50;

        // Sekcja podstawowych informacji
        DrawStatsSection(panelX, ref yPos, panelWidth, "Informacje podstawowe:", sectionMargin, () =>
        {
            // Populacja
            _spriteBatch.DrawString(
                _font,
                $"Populacja: {population?.Size ?? 0} osób",
                new Vector2(panelX + sectionMargin + 10, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                _fontScale * 1.1f,
                SpriteEffects.None,
                0f
            );
            yPos += (int)(25 * _fontScale);

            // Zamożność
            _spriteBatch.DrawString(
                _font,
                $"Zamożność: {population?.Wealth.ToString("F2") ?? "0"} $",
                new Vector2(panelX + sectionMargin + 10, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                _fontScale * 1.1f,
                SpriteEffects.None,
                0f
            );
            yPos += (int)(25 * _fontScale);

            // Dostępni pracownicy
            _spriteBatch.DrawString(
                _font,
                $"Dostępni pracownicy: {_selectedProvince.AvailableWorkers}",
                new Vector2(panelX + sectionMargin + 10, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                _fontScale * 1.1f,
                SpriteEffects.None,
                0f
            );
            yPos += (int)(25 * _fontScale);
        });

        // Sekcja zasobów
        DrawStatsSection(panelX, ref yPos, panelWidth, "Dostępne zasoby:", sectionMargin, () =>
        {
            // Utworzenie złożonego widoku zasobów jako paska postępu
            foreach (var resource in _selectedProvince.Resources)
            {
                // Tło paska
                int barWidth = panelWidth - sectionMargin * 2 - 20;
                int barHeight = (int)(20 * _fontScale);

                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(panelX + sectionMargin + 10, yPos, barWidth, barHeight),
                    new Color(40, 40, 60)
                );

                // Wypełnienie paska proporcjonalne do ilości zasobu
                float fillRatio = Math.Min(1.0f, resource.Value / 1000f); // Maksimum przy 1000 jednostek

                // Kolor paska zależny od typu zasobu
                Color resourceColor = GetResourceColor(resource.Key);

                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(panelX + sectionMargin + 10, yPos, (int)(barWidth * fillRatio), barHeight),
                    resourceColor
                );

                // Tekst nazwy zasobu
                _spriteBatch.DrawString(
                    _font,
                    $"{resource.Key}:",
                    new Vector2(panelX + sectionMargin + 15, yPos + 2),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );

                // Tekst ilości zasobu
                string resourceAmount = $"{resource.Value:F1}";
                Vector2 textSize = _font.MeasureString(resourceAmount) * _fontScale;

                _spriteBatch.DrawString(
                    _font,
                    resourceAmount,
                    new Vector2(panelX + sectionMargin + barWidth - textSize.X - 5, yPos + 2),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );

                yPos += barHeight + 5;
            }
        });

        // Sekcja potrzeb populacji
        if (population != null)
        {
            DrawStatsSection(panelX, ref yPos, panelWidth, "Potrzeby populacji (na osobę):", sectionMargin, () =>
            {
                foreach (var need in population.Needs)
                {
                    // Pasek potrzeby
                    int barWidth = panelWidth - sectionMargin * 2 - 20;
                    int barHeight = (int)(16 * _fontScale);

                    // Tło paska
                    _spriteBatch.Draw(
                        _provinceTexture,
                        new Rectangle(panelX + sectionMargin + 10, yPos, barWidth, barHeight),
                        new Color(40, 40, 60)
                    );

                    // Wypełnienie paska proporcjonalne do poziomu zadowolenia
                    float satisfactionLevel = population.Satisfaction.ContainsKey(need.Key)
                        ? population.Satisfaction[need.Key]
                        : 0.1f;

                    // Kolor zależny od poziomu zadowolenia
                    Color satisfactionColor = satisfactionLevel > 0.8f ? new Color(0.2f, 0.8f, 0.2f) :
                        satisfactionLevel > 0.5f ? new Color(0.8f, 0.8f, 0.2f) :
                        new Color(0.8f, 0.2f, 0.2f);

                    _spriteBatch.Draw(
                        _provinceTexture,
                        new Rectangle(panelX + sectionMargin + 10, yPos, (int)(barWidth * satisfactionLevel),
                            barHeight),
                        satisfactionColor
                    );

                    // Tekst nazwy potrzeby i wartości
                    _spriteBatch.DrawString(
                        _font,
                        $"{need.Key}: {need.Value:F2} jedn./os. ({satisfactionLevel:P0})",
                        new Vector2(panelX + sectionMargin + 15, yPos),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        _fontScale,
                        SpriteEffects.None,
                        0f
                    );

                    yPos += barHeight + 5;
                }

                // Całkowite zapotrzebowanie
                yPos += (int)(10 * _fontScale);

                _spriteBatch.DrawString(
                    _font,
                    "Całkowite zapotrzebowanie:",
                    new Vector2(panelX + sectionMargin + 10, yPos),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );

                yPos += (int)(20 * _fontScale);

                foreach (var need in population.Needs)
                {
                    var totalNeed = need.Value * population.Size;
                    _spriteBatch.DrawString(
                        _font,
                        $"{need.Key}: {totalNeed:F1} jedn.",
                        new Vector2(panelX + sectionMargin + 20, yPos),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        _fontScale,
                        SpriteEffects.None,
                        0f
                    );
                    yPos += (int)(20 * _fontScale);
                }
            });
        }

        // Sekcja budynków
        DrawStatsSection(panelX, ref yPos, panelWidth, "Budynki:", sectionMargin, () =>
        {
            // Reset indeksu wybranego budynku
            _selectedBuildingIndex = -1;

            // Kontener na listę budynków
            int buildingsContainerX = panelX + sectionMargin + 10;
            int buildingsContainerY = yPos;
            int buildingsContainerWidth = panelWidth - sectionMargin * 2 - 20;
            int buildingsContainerHeight =
                (int)(Math.Min(6, _selectedProvince.Buildings.Count) * (30 * _fontScale) + 10);

            // Tło kontenera
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(buildingsContainerX, buildingsContainerY, buildingsContainerWidth,
                    buildingsContainerHeight),
                new Color(30, 30, 50)
            );

            // Rysuj listę budynków
            int buildingY = buildingsContainerY + 5;

            for (int i = 0; i < _selectedProvince.Buildings.Count; i++)
            {
                var building = _selectedProvince.Buildings[i];

                // Określ kolor budynku w zależności od jego stanu
                Color buildingColor = building.IsActive ? Color.White : new Color(150, 150, 150);

                // Jeśli budynek jest wybrany, podświetl go
                if (_selectedBuilding != null && _selectedBuilding.Id == building.Id)
                {
                    // Tło wybranego budynku
                    _spriteBatch.Draw(
                        _provinceTexture,
                        new Rectangle(buildingsContainerX + 2, buildingY, buildingsContainerWidth - 4,
                            (int)(25 * _fontScale)),
                        new Color(80, 80, 120)
                    );

                    buildingColor = Color.Yellow;
                }

                // Ikona typu budynku
                DrawBuildingIcon(buildingsContainerX + 5, buildingY + 2, (int)(20 * _fontScale), building.Type);

                // Nazwa budynku i poziom
                _spriteBatch.DrawString(
                    _font,
                    $"{building.Name} (Lv.{building.Level})",
                    new Vector2(buildingsContainerX + (int)(30 * _fontScale), buildingY + 2),
                    buildingColor,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );

                // Informacja o pracownikach
                string workersInfo = $"{building.CurrentWorkers}/{building.WorkersCapacity}";
                Vector2 workersSize = _font.MeasureString(workersInfo) * _fontScale;

                _spriteBatch.DrawString(
                    _font,
                    workersInfo,
                    new Vector2(buildingsContainerX + buildingsContainerWidth - workersSize.X - 30, buildingY + 2),
                    buildingColor,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );

                // Przycisk ulepszenia
                DrawUpgradeButton(
                    buildingsContainerX + buildingsContainerWidth - 25,
                    buildingY + 2,
                    (int)(20 * _fontScale),
                    (int)(20 * _fontScale),
                    building,
                    population
                );

                // Zapisz indeks budynku pod kursorem
                Rectangle buildingRect = new Rectangle(
                    buildingsContainerX,
                    buildingY,
                    buildingsContainerWidth - 30,
                    (int)(25 * _fontScale)
                );

                if (buildingRect.Contains(Mouse.GetState().Position))
                {
                    _selectedBuildingIndex = i;
                }

                buildingY += (int)(30 * _fontScale);
            }

            yPos += buildingsContainerHeight + 10;

            // Przyciski budowy nowych budynków
            DrawBuildButtons(panelX, ref yPos, panelWidth, sectionMargin);
        });

        // Przycisk zamknięcia panelu
        DrawCloseButton(panelX + panelWidth - 40, panelY + 5, 30, 30);

        // Jeśli budynek jest wybrany, pokaż szczegóły
        if (_selectedBuilding != null)
        {
            DrawBuildingDetailsPanel(panelX + panelWidth + 10, panelY, population);
        }
    }

    private void DrawCloseButton(int x, int y, int width, int height)
    {
        // Sprawdź, czy myszka jest nad przyciskiem
        Rectangle buttonRect = new Rectangle(x, y, width, height);
        bool isHovered = buttonRect.Contains(Mouse.GetState().Position);

        // Tło przycisku
        Color buttonColor = isHovered ? new Color(180, 60, 60) : new Color(120, 40, 40);

        _spriteBatch.Draw(
            _provinceTexture,
            buttonRect,
            buttonColor
        );

        // Symbol X - narysowany za pomocą dwóch prostokątów obróconych względem siebie
        int padding = width / 4;
        int lineThickness = Math.Max(2, width / 10);

        // Alternatywne podejście: rysujemy dwa prostokąty zamiast obracać tekstury
        // Pierwsza linia (ukośna lewa-góra do prawa-dół)
        for (int i = 0; i < lineThickness; i++)
        {
            // Narysuj linię po przekątnej punkt po punkcie
            for (int j = 0; j < width - padding * 2; j++)
            {
                int pointX = x + padding + j;
                int pointY = y + padding + j;
                if (pointX < x + width - padding && pointY < y + height - padding)
                {
                    _spriteBatch.Draw(
                        _provinceTexture,
                        new Rectangle(pointX, pointY, 1, 1),
                        Color.White
                    );
                }
            }
        }

        // Druga linia (ukośna prawa-góra do lewa-dół)
        for (int i = 0; i < lineThickness; i++)
        {
            // Narysuj linię po przekątnej punkt po punkcie
            for (int j = 0; j < width - padding * 2; j++)
            {
                int pointX = x + width - padding - j;
                int pointY = y + padding + j;
                if (pointX >= x + padding && pointY < y + height - padding)
                {
                    _spriteBatch.Draw(
                        _provinceTexture,
                        new Rectangle(pointX, pointY, 1, 1),
                        Color.White
                    );
                }
            }
        }
    }

    private void DrawBuildButtons(int panelX, ref int yPos, int panelWidth, int margin)
    {
        _spriteBatch.DrawString(
            _font,
            "Zbuduj nowy budynek:",
            new Vector2(panelX + margin + 10, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            _fontScale,
            SpriteEffects.None,
            0f
        );

        yPos += (int)(25 * _fontScale);

        // Definicje typów budynków
        string[] buildingTypes = { "FieldCrop", "Sawmill", "IronMine", "CoalMine", "FurnitureFactory", "TailorShop" };
        string[] buildingNames = { "Pole uprawne", "Tartak", "Kopalnia Fe", "Kopalnia C", "Fabryka mebli", "Szwalnia" };

        // Układamy przyciski w 2 rzędy po 3
        int buttonWidth = (panelWidth - margin * 2 - 40) / 3;
        int buttonHeight = (int)(35 * _fontScale);
        int buttonSpacing = 10;

        for (int i = 0; i < buildingTypes.Length; i++)
        {
            int row = i / 3;
            int col = i % 3;

            int buttonX = panelX + margin + 10 + col * (buttonWidth + buttonSpacing);
            int buttonY = yPos + row * (buttonHeight + buttonSpacing);

            // Sprawdź, czy myszka jest nad przyciskiem
            Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            bool isHovered = buttonRect.Contains(Mouse.GetState().Position);

            // Sprawdź, czy gracz może sobie pozwolić na budynek
            var population = _gameEngine._populations.Find(p => p.ProvinceId == _selectedProvince.Id);
            int buildCost = 500;
            bool canAfford = population != null && population.Wealth >= buildCost;

            // Tło przycisku
            Color buttonColor = !canAfford ? new Color(80, 80, 80) :
                isHovered ? new Color(100, 100, 160) :
                new Color(80, 80, 120);

            _spriteBatch.Draw(
                _provinceTexture,
                buttonRect,
                buttonColor
            );

            // Obramowanie przycisku
            DrawPanelBorder(buttonX, buttonY, buttonWidth, buttonHeight, new Color(120, 120, 150, 200));

            // Ikona budynku
            DrawBuildingIcon(buttonX + 5, buttonY + 5, buttonHeight - 10, buildingTypes[i]);

            // Tekst przycisku
            _spriteBatch.DrawString(
                _font,
                buildingNames[i],
                new Vector2(buttonX + buttonHeight, buttonY + buttonHeight / 2 - 10),
                canAfford ? Color.White : new Color(150, 150, 150),
                0f,
                Vector2.Zero,
                _fontScale * 0.9f,
                SpriteEffects.None,
                0f
            );

            // Koszt budowy
            _spriteBatch.DrawString(
                _font,
                $"{buildCost}$",
                new Vector2(buttonX + buttonHeight, buttonY + buttonHeight / 2 + 5),
                canAfford ? new Color(200, 255, 200) : new Color(150, 150, 150),
                0f,
                Vector2.Zero,
                _fontScale * 0.8f,
                SpriteEffects.None,
                0f
            );
        }

        yPos += 2 * (buttonHeight + buttonSpacing);
    }

    private void DrawBuildingIcon(int x, int y, int size, string buildingType)
    {
        // Tło ikony
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y, size, size),
            new Color(40, 40, 60)
        );

        // Symbol budynku zależny od typu
        switch (buildingType)
        {
            case "FieldCrop":
                // Prosty symbol pola
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 4, y + size / 2, size / 2, size / 3),
                    new Color(0, 200, 0)
                );
                break;

            case "Sawmill":
                // Symbol tartaku (drzewo)
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 2 - size / 10, y + size / 3, size / 5, size / 2),
                    new Color(150, 75, 0)
                );
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 4, y + size / 6, size / 2, size / 3),
                    new Color(0, 150, 0)
                );
                break;

            case "IronMine":
                // Symbol kopalni żelaza
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 4, y + size / 4, size / 2, size / 2),
                    new Color(180, 180, 180)
                );
                break;

            case "CoalMine":
                // Symbol kopalni węgla
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 4, y + size / 4, size / 2, size / 2),
                    new Color(50, 50, 50)
                );
                break;

            case "FurnitureFactory":
                // Symbol fabryki mebli
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 6, y + size / 3, size * 2 / 3, size / 2),
                    new Color(180, 120, 60)
                );
                break;

            case "TailorShop":
                // Symbol szwalni
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 4, y + size / 4, size / 2, size / 6),
                    new Color(150, 150, 200)
                );
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + size / 4, y + size / 4 + size / 6, size / 6, size / 3),
                    new Color(150, 150, 200)
                );
                break;
        }

        // Obramowanie ikony
        DrawPanelBorder(x, y, size, size, new Color(80, 80, 100, 150));
    }

    private void DrawUpgradeButton(int x, int y, int width, int height, Building building, Population population)
    {
        // Sprawdź, czy stać gracza na ulepszenie
        bool canAfford = population != null && population.Wealth >= building.UpgradeCost;

        // Sprawdź, czy myszka jest nad przyciskiem
        Rectangle buttonRect = new Rectangle(x, y, width, height);
        bool isHovered = buttonRect.Contains(Mouse.GetState().Position);

        // Tło przycisku
        Color buttonColor = !canAfford ? new Color(80, 80, 80) :
            isHovered ? new Color(100, 180, 100) :
            new Color(60, 140, 60);

        _spriteBatch.Draw(
            _provinceTexture,
            buttonRect,
            buttonColor
        );

        // Symbol +
        int padding = width / 4;
        int lineThickness = Math.Max(2, height / 5);

        // Linia pozioma
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x + padding, y + height / 2 - lineThickness / 2, width - padding * 2, lineThickness),
            Color.White
        );

        // Linia pionowa
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x + width / 2 - lineThickness / 2, y + padding, lineThickness, height - padding * 2),
            Color.White
        );

        // Obramowanie przycisku
        DrawPanelBorder(x, y, width, height, new Color(100, 100, 100, 150));
    }

    private void DrawBuildingDetailsPanel(int x, int y, Population population)
    {
        // Określ rozmiar panelu
        int panelWidth = 250;
        int panelHeight = 350;

        // Tło panelu
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(x, y, panelWidth, panelHeight),
            new Color(25, 25, 45, 230)
        );

        // Obramowanie panelu
        DrawPanelBorder(x, y, panelWidth, panelHeight, new Color(80, 80, 120, 200));

        // Nagłówek z nazwą budynku
        DrawHeaderBar(x, y, panelWidth, _selectedBuilding.Name, Color.Gold);

        int margin = 15;
        int yPos = y + 50;

        // Informacja o poziomie
        _spriteBatch.DrawString(
            _font,
            $"Poziom: {_selectedBuilding.Level}",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            _fontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(25 * _fontScale);

        // Informacja o pracownikach
        _spriteBatch.DrawString(
            _font,
            $"Pracownicy: {_selectedBuilding.CurrentWorkers}/{_selectedBuilding.WorkersCapacity}",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            _fontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(25 * _fontScale);

        // Status
        _spriteBatch.DrawString(
            _font,
            $"Status: {(_selectedBuilding.IsActive ? "Aktywny" : "Nieaktywny")}",
            new Vector2(x + margin, yPos),
            _selectedBuilding.IsActive ? new Color(100, 255, 100) : new Color(255, 100, 100),
            0f,
            Vector2.Zero,
            _fontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(30 * _fontScale);

        // Produkcja
        _spriteBatch.DrawString(
            _font,
            "Produkcja:",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            _fontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(20 * _fontScale);

        foreach (var resource in _selectedBuilding.ProductionRates)
        {
            // Oblicz faktyczną produkcję
            float actualProduction = resource.Value * _selectedBuilding.CurrentWorkers /
                _selectedBuilding.WorkersCapacity * _selectedBuilding.Level;

            // Pasek produkcji
            int barWidth = panelWidth - margin * 2 - 10;
            int barHeight = (int)(15 * _fontScale);

            // Tło paska
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(x + margin, yPos, barWidth, barHeight),
                new Color(40, 40, 60)
            );

            // Wypełnienie paska
            float maxProduction = resource.Value * _selectedBuilding.Level; // Maksymalna możliwa produkcja
            float fillRatio = _selectedBuilding.CurrentWorkers / (float)_selectedBuilding.WorkersCapacity;

            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(x + margin, yPos, (int)(barWidth * fillRatio), barHeight),
                GetResourceColor(resource.Key)
            );

            // Tekst produkcji
            _spriteBatch.DrawString(
                _font,
                $"{resource.Key}: {actualProduction:F1}/tura",
                new Vector2(x + margin + 5, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                _fontScale,
                SpriteEffects.None,
                0f
            );

            yPos += barHeight + 5;
        }

        // Zużycie
        if (_selectedBuilding.ConsumptionRates.Count > 0)
        {
            yPos += (int)(10 * _fontScale);

            _spriteBatch.DrawString(
                _font,
                "Zużycie:",
                new Vector2(x + margin, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                _fontScale,
                SpriteEffects.None,
                0f
            );
            yPos += (int)(20 * _fontScale);

            foreach (var resource in _selectedBuilding.ConsumptionRates)
            {
                // Oblicz faktyczne zużycie
                float actualConsumption = resource.Value * _selectedBuilding.CurrentWorkers /
                    _selectedBuilding.WorkersCapacity * _selectedBuilding.Level;

                // Pasek zużycia
                int barWidth = panelWidth - margin * 2 - 10;
                int barHeight = (int)(15 * _fontScale);

                // Tło paska
                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + margin, yPos, barWidth, barHeight),
                    new Color(40, 40, 60)
                );

                // Wypełnienie paska
                float maxConsumption = resource.Value * _selectedBuilding.Level;
                float fillRatio = _selectedBuilding.CurrentWorkers / (float)_selectedBuilding.WorkersCapacity;

                _spriteBatch.Draw(
                    _provinceTexture,
                    new Rectangle(x + margin, yPos, (int)(barWidth * fillRatio), barHeight),
                    new Color(180, 100, 100) // Czerwonawy kolor dla zużycia
                );

                // Tekst zużycia
                _spriteBatch.DrawString(
                    _font,
                    $"{resource.Key}: {actualConsumption:F1}/tura",
                    new Vector2(x + margin + 5, yPos),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );

                yPos += barHeight + 5;
            }
        }

        // Koszt ulepszenia
        yPos += (int)(20 * _fontScale);

        _spriteBatch.DrawString(
            _font,
            $"Koszt ulepszenia: {_selectedBuilding.UpgradeCost}$",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            _fontScale,
            SpriteEffects.None,
            0f
        );

        // Przycisk ulepszenia
        yPos += (int)(30 * _fontScale);

        // Sprawdź, czy gracz może sobie pozwolić na ulepszenie
        bool canAfford = population != null && population.Wealth >= _selectedBuilding.UpgradeCost;

        // Rysuj przycisk
        int buttonWidth = panelWidth - margin * 2;
        int buttonHeight = (int)(35 * _fontScale);

        // Sprawdź, czy kursor jest nad przyciskiem
        Rectangle upgradeButtonRect = new Rectangle(x + margin, yPos, buttonWidth, buttonHeight);
        bool isHovered = upgradeButtonRect.Contains(Mouse.GetState().Position);

        // Tło przycisku
        Color buttonColor = !canAfford ? new Color(80, 80, 80) :
            isHovered ? new Color(80, 120, 80) :
            new Color(60, 100, 60);

        _spriteBatch.Draw(
            _provinceTexture,
            upgradeButtonRect,
            buttonColor
        );

        // Tekst przycisku
        _spriteBatch.DrawString(
            _font,
            "Ulepsz budynek",
            new Vector2(x + margin + buttonWidth / 2, yPos + buttonHeight / 2),
            canAfford ? Color.White : new Color(150, 150, 150),
            0f,
            _font.MeasureString("Ulepsz budynek") * _fontScale / 2,
            _fontScale,
            SpriteEffects.None,
            0f
        );

        // Obramowanie przycisku
        DrawPanelBorder(x + margin, yPos, buttonWidth, buttonHeight, new Color(100, 120, 100, 200));
    }

    private void DrawPriceChart(int x, int y, int width, int height, string resource)
    {
        // Tło wykresu
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y, width, height),
            new Color(20, 20, 40)
        );

        // Obramowanie wykresu
        DrawPanelBorder(x, y, width, height, new Color(80, 80, 120, 200));

        var history = _gameEngine._priceHistory[resource];
        if (history.Count <= 1) return;

        // Znajdź min i max dla skalowania
        float min = history.Min();
        float max = history.Max();
        if (max == min) max = min + 1; // Unikaj dzielenia przez zero

        // Narysuj siatkę pomocniczą
        for (int i = 1; i < 4; i++)
        {
            float yPos = y + height - (i * height / 4);

            // Linia pomocnicza
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle(x, (int)yPos, width, 1),
                new Color(100, 100, 100, 100)
            );

            // Wartość na osi Y
            float value = min + (i * (max - min) / 4);
            _spriteBatch.DrawString(
                _font,
                $"{value:F1}",
                new Vector2(x + 5, yPos - 15),
                new Color(150, 150, 150),
                0f,
                Vector2.Zero,
                _fontScale * 0.7f,
                SpriteEffects.None,
                0f
            );
        }

        // Narysuj linię trendu
        for (int i = 1; i < history.Count; i++)
        {
            // Pozycje punktów
            float x1 = x + (i - 1) * width / (history.Count - 1);
            float y1 = y + height - ((history[i - 1] - min) / (max - min)) * height;
            float x2 = x + i * width / (history.Count - 1);
            float y2 = y + height - ((history[i] - min) / (max - min)) * height;

            // Linia między punktami
            DrawLine(_spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), GetResourceColor(resource), 2);

            // Kropka na końcach linii
            _spriteBatch.Draw(
                _provinceTexture,
                new Rectangle((int)x2 - 2, (int)y2 - 2, 4, 4),
                GetResourceColor(resource)
            );
        }

        // Tytuł wykresu
        _spriteBatch.DrawString(
            _font,
            $"Trend ceny: {resource}",
            new Vector2(x + width / 2, y + 10),
            Color.White,
            0f,
            new Vector2(_font.MeasureString($"Trend ceny: {resource}").X / 2, 0),
            _fontScale * 0.8f,
            SpriteEffects.None,
            0f
        );

        // Aktualna cena
        _spriteBatch.DrawString(
            _font,
            $"Obecna: {_gameEngine._market.GetPrice(resource):F2}$",
            new Vector2(x + width - 10, y + height - 20),
            GetResourceColor(resource),
            0f,
            new Vector2(_font.MeasureString($"Obecna: {_gameEngine._market.GetPrice(resource):F2}$").X, 0),
            _fontScale * 0.8f,
            SpriteEffects.None,
            0f
        );
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 1)
    {
        Vector2 delta = end - start;
        float angle = (float)Math.Atan2(delta.Y, delta.X);
        float length = delta.Length();

        spriteBatch.Draw(
            _provinceTexture,
            new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
            null,
            color,
            angle,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );
    }


    private void DrawGlobalStatsPanel()
    {
        // Oblicz pozycję i rozmiar panelu
        int panelX = _gridMarginX + GridWidth * CellSize + 20;
        int panelY = _gridMarginY;
        int panelWidth = (int)(_screenWidth * 0.24f); // ~24% szerokości ekranu
        int panelHeight = GridHeight * CellSize;

        // Tło panelu z gradientem
        Color topColor = new Color(20, 25, 40, 230);
        Color bottomColor = new Color(40, 45, 65, 230);

        // Górna część panelu
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX, panelY, panelWidth, panelHeight / 2),
            topColor
        );

        // Dolna część panelu
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX, panelY + panelHeight / 2, panelWidth, panelHeight - panelHeight / 2),
            bottomColor
        );

        // Obramowanie panelu
        DrawPanelBorder(panelX, panelY, panelWidth, panelHeight, new Color(80, 100, 120, 200));

        // Nagłówek
        DrawHeaderBar(panelX, panelY, panelWidth, "STATYSTYKI GLOBALNE", Color.Gold);

        // Obszary sekcji statystyk
        int sectionMargin = 15;
        int yPos = panelY + 50;

        // Sekcja cen rynkowych
        DrawStatsSection(panelX, ref yPos, panelWidth, "Ceny rynkowe:", sectionMargin, () =>
        {
            foreach (var resource in _gameEngine._market.GetAllResources())
            {
                // Wskaźnik podaży/popytu określa kolor (czerwony dla niedoboru, zielony dla nadwyżki)
                float ratio = _gameEngine._market.GetSupplyDemandRatio(resource);
                Color priceColor = ratio < 0.8f ? new Color(1.0f, 0.7f, 0.7f) :
                    ratio > 1.2f ? new Color(0.7f, 1.0f, 0.7f) :
                    Color.White;

                _spriteBatch.DrawString(
                    _font,
                    $"{resource}: {_gameEngine._market.GetPrice(resource):F2}$ ({ratio:F2})",
                    new Vector2(panelX + sectionMargin + 10, yPos),
                    priceColor,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(22 * _fontScale);
            }
        });

        // Sekcja populacji
        DrawStatsSection(panelX, ref yPos, panelWidth, "Populacja:", sectionMargin, () =>
        {
            _spriteBatch.DrawString(
                _font,
                $"{_gameEngine._globalStats.GetValueOrDefault("Total Population"):F0}",
                new Vector2(panelX + sectionMargin + 10, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                _fontScale * 1.2f, // Większa czcionka dla ważnej statystyki
                SpriteEffects.None,
                0f
            );
            yPos += (int)(30 * _fontScale);
        });

        // Sekcja całkowitych zasobów
        DrawStatsSection(panelX, ref yPos, panelWidth, "Całkowite zasoby:", sectionMargin, () =>
        {
            foreach (var resource in _gameEngine._market.GetAllResources())
            {
                // Kolor tekstu zależny od ilości zasobu (im więcej, tym jaśniejszy)
                float amount = _gameEngine._globalStats.GetValueOrDefault($"Total {resource}");
                float brightness = Math.Min(1.0f, 0.7f + amount / 10000f);

                _spriteBatch.DrawString(
                    _font,
                    $"{resource}: {amount:F0}",
                    new Vector2(panelX + sectionMargin + 10, yPos),
                    new Color(brightness, brightness, brightness),
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(22 * _fontScale);
            }
        });

        // Sekcja produkcji
        DrawStatsSection(panelX, ref yPos, panelWidth, "Globalna produkcja (na turę):", sectionMargin, () =>
        {
            foreach (var resource in _gameEngine._market.GetAllResources())
            {
                float production = _gameEngine._globalStats.GetValueOrDefault($"Production {resource}");
                // Kolor zależny od wydajności (zielony dla wysokiej produkcji)
                Color productionColor = production > 1000 ? new Color(0.7f, 1.0f, 0.7f) :
                    production > 500 ? Color.White :
                    new Color(1.0f, 1.0f, 0.8f);

                _spriteBatch.DrawString(
                    _font,
                    $"{resource}: {production:F0}",
                    new Vector2(panelX + sectionMargin + 10, yPos),
                    productionColor,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(22 * _fontScale);
            }
        });

        // Sekcja wymiany handlowej
        DrawStatsSection(panelX, ref yPos, panelWidth, "Ostatnia wymiana handlowa:", sectionMargin, () =>
        {
            foreach (var resource in _gameEngine._totalTraded.Keys)
            {
                float amount = _gameEngine._totalTraded[resource];
                // Kolor zależny od ilości wymienionego towaru
                Color tradeColor = amount > 100 ? new Color(0.8f, 1.0f, 0.8f) :
                    amount > 0 ? Color.White :
                    new Color(0.8f, 0.8f, 0.8f);

                _spriteBatch.DrawString(
                    _font,
                    $"{resource}: {amount:F0} jedn.",
                    new Vector2(panelX + sectionMargin + 10, yPos),
                    tradeColor,
                    0f,
                    Vector2.Zero,
                    _fontScale,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(22 * _fontScale);
            }
        });

        // Sekcja zadowolenia potrzeb
        DrawStatsSection(panelX, ref yPos, panelWidth, "Średnie zadowolenie potrzeb:", sectionMargin, () =>
        {
            foreach (var resource in _gameEngine._market.GetAllResources())
            {
                if (_gameEngine._globalStats.ContainsKey($"Satisfaction {resource}"))
                {
                    float satisfaction = _gameEngine._globalStats[$"Satisfaction {resource}"];
                    Color satColor = satisfaction > 0.8f
                        ? new Color(0.5f, 1.0f, 0.5f)
                        : (satisfaction > 0.5f ? new Color(1.0f, 1.0f, 0.5f) : new Color(1.0f, 0.5f, 0.5f));

                    _spriteBatch.DrawString(
                        _font,
                        $"{resource}: {(satisfaction * 100):F0}%",
                        new Vector2(panelX + sectionMargin + 10, yPos),
                        satColor,
                        0f,
                        Vector2.Zero,
                        _fontScale,
                        SpriteEffects.None,
                        0f
                    );
                    yPos += (int)(22 * _fontScale);
                }
            }
        });

        DrawChartAndResourceButtonsToPanel(ref yPos, panelX, panelWidth);
    }

    private void DrawStatsSection(int panelX, ref int yPos, int panelWidth, string title, int margin,
        Action drawContent)
    {
        // Tło nagłówka sekcji
        _spriteBatch.Draw(
            _uiBackground,
            new Rectangle(panelX + margin, yPos, panelWidth - margin * 2, (int)(25 * _fontScale)),
            new Color(60, 80, 100, 150)
        );

        // Tekst nagłówka
        _spriteBatch.DrawString(
            _font,
            title,
            new Vector2(panelX + margin + 5, yPos + 2),
            Color.White,
            0f,
            Vector2.Zero,
            _fontScale,
            SpriteEffects.None,
            0f
        );

        yPos += (int)(30 * _fontScale);

        // Rysuj zawartość sekcji
        drawContent();

        // Dodaj odstęp na końcu sekcji
        yPos += (int)(10 * _fontScale);
    }

    private void DrawPanelBorder(int x, int y, int width, int height, Color color)
    {
        int borderThickness = 2;

        // Górna krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y, width, borderThickness),
            color
        );

        // Lewa krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y, borderThickness, height),
            color
        );

        // Prawa krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x + width - borderThickness, y, borderThickness, height),
            color
        );

        // Dolna krawędź
        _spriteBatch.Draw(
            _provinceTexture,
            new Rectangle(x, y + height - borderThickness, width, borderThickness),
            color
        );
    }

    private void DrawChartAndResourceButtonsToPanel(ref int yPos, int panelX, int panelWidth)
    {
        // Po wyświetleniu wszystkich statystyk, dodaj wykres cen
        if (_gameEngine._priceHistory.ContainsKey(_selectedResource))
        {
            int chartMargin = 15;
            int chartX = panelX + chartMargin;
            int chartY = yPos;
            int chartWidth = panelWidth - chartMargin * 2;
            int chartHeight = 150;

            DrawPriceChart(chartX, chartY, chartWidth, chartHeight, _selectedResource);

            yPos += chartHeight + 20;

            // Przyciski wyboru zasobu do wykresu
            int buttonSize = (panelWidth - chartMargin * 2 - 10) / 3; // 3 przyciski w rzędzie, 5px odstępu
            int buttonY = yPos;
            int buttonX = chartX;
            int resourceButtonsSectionY = buttonY; // Zapisz tę pozycję do użycia w HandleResourceButtonClick

            string[] resources = _gameEngine._market.GetAllResources();
            for (int i = 0; i < resources.Length; i++)
            {
                string resource = resources[i];

                // Tło przycisku
                Color buttonColor = (_selectedResource == resource)
                    ? GetResourceColor(resource) * 0.7f
                    : new Color(60, 60, 80);

                Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonSize, 25);

                // Zapisz tę pozycję przycisku do debugowania
                _resourceButtonRects[i] = buttonRect;

                _spriteBatch.Draw(
                    _provinceTexture,
                    buttonRect,
                    buttonColor
                );

                // Tekst przycisku
                _spriteBatch.DrawString(
                    _font,
                    resource,
                    new Vector2(buttonX + buttonSize / 2, buttonY + 12),
                    Color.White,
                    0f,
                    _font.MeasureString(resource) * _fontScale / 2,
                    _fontScale * 0.8f,
                    SpriteEffects.None,
                    0f
                );

                // Ustaw pozycję dla następnego przycisku
                buttonX += buttonSize + 5;
                if (buttonX + buttonSize > panelX + panelWidth - chartMargin)
                {
                    buttonX = chartX;
                    buttonY += 30;
                }
            }

            yPos = buttonY + 30; // Aktualizuj pozycję Y po narysowaniu wszystkich przycisków
        }
    }

        private void HandleResourceButtonClick(Point mousePosition)
    {
        // Oblicz te same pozycje co w metodzie DrawGlobalStatsPanel
        int panelX = _gridMarginX + GridWidth * CellSize + 20;
        int panelY = _gridMarginY;
        int panelWidth = (int)(_screenWidth * 0.24f);
        int panelHeight = GridHeight * CellSize;

        // Sprawdź, czy kliknięcie jest w ogóle w obszarze panelu statystyk
        Rectangle panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);
        if (!panelRect.Contains(mousePosition))
        {
            return; // Kliknięcie poza panelem, nie sprawdzaj dalej
        }

        // Obliczenie pozycji przycisków zasobów na podstawie faktycznej zawartości panelu
        int resourceButtonsSectionY = panelY + panelHeight - 55; // Przyciski są około 200px od dołu panelu

        // Sprawdź, czy kliknięcie jest w obszarze przycisków zasobów
        if (mousePosition.Y < resourceButtonsSectionY || mousePosition.Y > resourceButtonsSectionY + 60)
        {
            return; // Kliknięcie nie jest w obszarze przycisków
        }

        int chartMargin = 15;
        int buttonSize = (panelWidth - chartMargin * 2 - 10) / 3; // 3 przyciski w rzędzie, 5px odstępu
        int buttonSpacing = 5;

        int row = (mousePosition.Y - resourceButtonsSectionY) / 30; // Przyciski mają 25px wysokości + 5px odstępu
        int col = (mousePosition.X - (panelX + chartMargin)) / (buttonSize + buttonSpacing);

        // Ograniczenie do prawidłowych zakresów
        row = Math.Max(0, Math.Min(1, row)); // Maksymalnie 2 rzędy
        col = Math.Max(0, Math.Min(2, col)); // Maksymalnie 3 kolumny

        // Oblicz indeks zasobu
        int resourceIndex = row * 3 + col;
        string[] resources = _gameEngine._market.GetAllResources();

        // Upewnij się, że indeks jest w zakresie
        if (resourceIndex >= 0 && resourceIndex < resources.Length)
        {
            _selectedResource = resources[resourceIndex];
        }
    }

    private void HandleMouseClick(Point mousePosition)
    {
        // Sprawdź kliknięcie na prowincje
        _selectedProvince = null;
        _selectedBuilding = null;

        // Sprawdź kliknięcie na przyciski wyboru zasobu do wykresu (zawsze dostępne)
        HandleResourceButtonClick(mousePosition);

        // Sprawdź kliknięcie na siatkę prowincji
        foreach (var province in _gameEngine._provinces)
        {
            Rectangle provinceRect = new Rectangle(
                (int)province.Position.X + _gridMarginX,
                (int)province.Position.Y + _gridMarginY,
                CellSize - 2,
                CellSize - 2
            );

            if (provinceRect.Contains(mousePosition))
            {
                _selectedProvince = province;
                break;
            }
        }

        // Jeśli wybrano prowincję, sprawdź kliknięcia na szczegóły
        if (_selectedProvince != null)
        {
            // Pozycja i rozmiar panelu szczegółów prowincji
            int screenWidth = _screenWidth;
            int screenHeight = _screenHeight;
            int panelWidth = (int)(screenWidth * 0.35f);
            int panelHeight = (int)(screenHeight * 0.7f);
            int panelX = (screenWidth - panelWidth) / 2;
            int panelY = (screenHeight - panelHeight) / 2;

            // Sprawdź kliknięcie przycisku zamknięcia
            Rectangle closeButtonRect = new Rectangle(
                panelX + panelWidth - 40,
                panelY + 5,
                30,
                30
            );

            if (closeButtonRect.Contains(mousePosition))
            {
                _selectedProvince = null;
                return;
            }

            int margin = 20;

            // Sprawdź kliknięcia na budynki
            if (_selectedBuildingIndex >= 0 && _selectedBuildingIndex < _selectedProvince.Buildings.Count)
            {
                int buildingsContainerX = panelX + margin + 10;
                int buildingsContainerY = panelY + 250; // Przybliżona pozycja sekcji budynków
                int buildingsContainerWidth = panelWidth - margin * 2 - 20;

                for (int i = 0; i < _selectedProvince.Buildings.Count; i++)
                {
                    int buildingY = buildingsContainerY + 5 + i * (int)(30 * _fontScale);

                    // Przycisk wyboru budynku
                    Rectangle buildingRect = new Rectangle(
                        buildingsContainerX,
                        buildingY,
                        buildingsContainerWidth - 30,
                        (int)(25 * _fontScale)
                    );

                    // Przycisk ulepszenia budynku
                    Rectangle upgradeButtonRect = new Rectangle(
                        buildingsContainerX + buildingsContainerWidth - 25,
                        buildingY + 2,
                        (int)(20 * _fontScale),
                        (int)(20 * _fontScale)
                    );

                    if (buildingRect.Contains(mousePosition))
                    {
                        _selectedBuilding = _selectedProvince.Buildings[i];
                    }
                    else if (upgradeButtonRect.Contains(mousePosition))
                    {
                        var building = _selectedProvince.Buildings[i];
                        var population = _gameEngine._populations.Find(p => p.ProvinceId == _selectedProvince.Id);

                        if (population != null && population.Wealth >= building.UpgradeCost)
                        {
                            population.Wealth -= building.UpgradeCost;
                            building.Upgrade();
                        }
                    }
                }
            }

            // Sprawdź kliknięcia na przyciski budowy
            string[] buildingTypes =
                { "FieldCrop", "Sawmill", "IronMine", "CoalMine", "FurnitureFactory", "TailorShop" };
            int buildButtonsY = panelY + panelHeight - 150; // Przybliżona pozycja sekcji przycisków budowy
            int buttonWidth = (panelWidth - margin * 2 - 40) / 3;
            int buttonHeight = (int)(35 * _fontScale);
            int buttonSpacing = 10;

            for (int i = 0; i < buildingTypes.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;

                int buttonX = panelX + margin + 10 + col * (buttonWidth + buttonSpacing);
                int buttonY = buildButtonsY + row * (buttonHeight + buttonSpacing);

                Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

                if (buttonRect.Contains(mousePosition))
                {
                    int buildCost = 500;
                    var population = _gameEngine._populations.Find(p => p.ProvinceId == _selectedProvince.Id);

                    if (population != null && population.Wealth >= buildCost)
                    {
                        population.Wealth -= buildCost;
                        _selectedProvince.AddBuilding(buildingTypes[i]);
                    }
                }
            }

            // Sprawdź kliknięcie na przycisk ulepszenia w panelu szczegółów budynku
            if (_selectedBuilding != null)
            {
                int detailsPanelX = panelX + panelWidth + 10;
                int detailsPanelY = panelY;
                int detailsPanelWidth = 250;
                int detailsPanelMargin = 15;

                // Przycisk ulepszenia w panelu szczegółów
                Rectangle upgradeButtonRect = new Rectangle(
                    detailsPanelX + detailsPanelMargin,
                    detailsPanelY + 300, // Przybliżona pozycja przycisku
                    detailsPanelWidth - detailsPanelMargin * 2,
                    (int)(35 * _fontScale)
                );

                if (upgradeButtonRect.Contains(mousePosition))
                {
                    var population = _gameEngine._populations.Find(p => p.ProvinceId == _selectedProvince.Id);

                    if (population != null && population.Wealth >= _selectedBuilding.UpgradeCost)
                    {
                        population.Wealth -= _selectedBuilding.UpgradeCost;
                        _selectedBuilding.Upgrade();
                    }
                }
            }
        }
    }
}