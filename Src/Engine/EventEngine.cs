using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UEconomy.Game;

namespace UEconomy.Engine;

public class EventEngine
{
    private GameEngine _gameEngine;
    public GraphicsEngine _graphicsEngine { private get; set; }

    public Province SelectedProvince { get; private set; }
    public string SelectedResource { get; private set; } = "Zboże";
    public Building SelectedBuilding { get; private set; }
    public int SelectedBuildingIndex { get; set; } = -1;

    private MouseState _previousMouseState;


    public EventEngine(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void Update()
    {
        var currentMouseState = Mouse.GetState();
        // Obsługa kliknięcia lewym przyciskiem myszy
        if (currentMouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            HandleMouseClick(currentMouseState.Position);
        }

        _previousMouseState = currentMouseState;
    }

    private void HandleResourceButtonClick(Point mousePosition)
    {
        var panelPos = _graphicsEngine.PanelPos();
        var panelX = panelPos["X"];
        var panelY = panelPos["Y"];
        var panelWidth = panelPos["Width"];
        var panelHeight = panelPos["Height"];

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
            SelectedResource = resources[resourceIndex];
        }
    }

    private void HandleMouseClick(Point mousePosition)
    {
        // Sprawdź kliknięcie na prowincje
        SelectedProvince = null;
        SelectedBuilding = null;

        // Sprawdź kliknięcie na przyciski wyboru zasobu do wykresu (zawsze dostępne)
        HandleResourceButtonClick(mousePosition);

        // Sprawdź kliknięcie na siatkę prowincji
        foreach (var province in _gameEngine._provinces)
        {
            Rectangle provinceRect = new Rectangle(
                (int)province.Position.X + _graphicsEngine.GridMarginX,
                (int)province.Position.Y + _graphicsEngine.GridMarginY,
                _graphicsEngine.CellSize - 2,
                _graphicsEngine.CellSize - 2
            );

            if (provinceRect.Contains(mousePosition))
            {
                SelectedProvince = province;
                break;
            }
        }

        // Jeśli wybrano prowincję, sprawdź kliknięcia na szczegóły
        if (SelectedProvince != null)
        {
            // Pozycja i rozmiar panelu szczegółów prowincji
            int screenWidth = _graphicsEngine.ScreenWidth;
            int screenHeight = _graphicsEngine.ScreenHeight;
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
                SelectedProvince = null;
                return;
            }

            int margin = 20;

            // Sprawdź kliknięcia na budynki
            if (SelectedBuildingIndex >= 0 && SelectedBuildingIndex < SelectedProvince.Buildings.Count)
            {
                int buildingsContainerX = panelX + margin + 10;
                int buildingsContainerY = panelY + 250; // Przybliżona pozycja sekcji budynków
                int buildingsContainerWidth = panelWidth - margin * 2 - 20;

                for (int i = 0; i < SelectedProvince.Buildings.Count; i++)
                {
                    int buildingY = buildingsContainerY + 5 + i * (int)(30 * _graphicsEngine.FontScale);

                    // Przycisk wyboru budynku
                    Rectangle buildingRect = new Rectangle(
                        buildingsContainerX,
                        buildingY,
                        buildingsContainerWidth - 30,
                        (int)(25 * _graphicsEngine.FontScale)
                    );

                    // Przycisk ulepszenia budynku
                    Rectangle upgradeButtonRect = new Rectangle(
                        buildingsContainerX + buildingsContainerWidth - 25,
                        buildingY + 2,
                        (int)(20 * _graphicsEngine.FontScale),
                        (int)(20 * _graphicsEngine.FontScale)
                    );

                    if (buildingRect.Contains(mousePosition))
                    {
                        SelectedBuilding = SelectedProvince.Buildings[i];
                    }
                    else if (upgradeButtonRect.Contains(mousePosition))
                    {
                        var building = SelectedProvince.Buildings[i];
                        var population = _gameEngine._populations.Find(p => p.ProvinceId == SelectedProvince.Id);

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
            int buttonHeight = (int)(35 * _graphicsEngine.FontScale);
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
                    var population = _gameEngine._populations.Find(p => p.ProvinceId == SelectedProvince.Id);

                    if (population != null && population.Wealth >= buildCost)
                    {
                        population.Wealth -= buildCost;
                        SelectedProvince.AddBuilding(buildingTypes[i]);
                    }
                }
            }

            // Sprawdź kliknięcie na przycisk ulepszenia w panelu szczegółów budynku
            if (SelectedBuilding != null)
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
                    (int)(35 * _graphicsEngine.FontScale)
                );

                if (upgradeButtonRect.Contains(mousePosition))
                {
                    var population = _gameEngine._populations.Find(p => p.ProvinceId == SelectedProvince.Id);

                    if (population != null && population.Wealth >= SelectedBuilding.UpgradeCost)
                    {
                        population.Wealth -= SelectedBuilding.UpgradeCost;
                        SelectedBuilding.Upgrade();
                    }
                }
            }
        }
    }
}