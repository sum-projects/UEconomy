using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UEconomy.Game;
using UEconomy.Graphics;

namespace UEconomy.Engine;

public class EventEngine
{
    private GameEngine _gameEngine;
    private UI _ui;
    public Province SelectedProvince { get; private set; }
    public string SelectedResource { get; private set; } = "Zboże";
    public Building SelectedBuilding { get; private set; }
    public int SelectedBuildingIndex { get; set; } = -1;

    private MouseState _previousMouseState;


    public EventEngine(GameEngine gameEngine, UI ui)
    {
        _gameEngine = gameEngine;
        _ui = ui;
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
        // Sprawdź, czy kliknięcie jest w ogóle w obszarze panelu statystyk
        var panelRect = new Rectangle(_ui.PanelX, _ui.PanelY, _ui.PanelWidth, _ui.PanelHeight);
        if (!panelRect.Contains(mousePosition))
        {
            return; // Kliknięcie poza panelem, nie sprawdzaj dalej
        }

        // Obliczenie pozycji przycisków zasobów na podstawie faktycznej zawartości panelu
        var resourceButtonsSectionY = _ui.PanelY + _ui.PanelHeight - 55; // Przyciski są około 200px od dołu panelu

        // Sprawdź, czy kliknięcie jest w obszarze przycisków zasobów
        if (mousePosition.Y < resourceButtonsSectionY || mousePosition.Y > resourceButtonsSectionY + 60)
        {
            return; // Kliknięcie nie jest w obszarze przycisków
        }

        var buttonSize = (_ui.PanelWidth - _ui.ChartMargin * 2 - 10) / 3; // 3 przyciski w rzędzie, 5px odstępu
        const int buttonSpacing = 5;

        var row = (mousePosition.Y - resourceButtonsSectionY) / 30; // Przyciski mają 25px wysokości + 5px odstępu
        var col = (mousePosition.X - (_ui.PanelX + _ui.ChartMargin)) / (buttonSize + buttonSpacing);

        // Ograniczenie do prawidłowych zakresów
        row = Math.Max(0, Math.Min(1, row)); // Maksymalnie 2 rzędy
        col = Math.Max(0, Math.Min(2, col)); // Maksymalnie 3 kolumny

        // Oblicz indeks zasobu
        var resourceIndex = row * 3 + col;
        var resources = _gameEngine._market.GetAllResources();

        // Upewnij się, że indeks jest w zakresie
        if (resourceIndex < resources.Length)
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
            var provinceRect = new Rectangle(
                (int)province.Position.X + _ui.GridMarginX,
                (int)province.Position.Y + _ui.GridMarginY,
                _ui.CellSize - 2,
                _ui.CellSize - 2
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
            // Sprawdź kliknięcie przycisku zamknięcia
            var closeButtonRect = new Rectangle(
                _ui.PanelX + _ui.PanelWidth - 40,
                _ui.PanelY + 5,
                30,
                30
            );

            if (closeButtonRect.Contains(mousePosition))
            {
                SelectedProvince = null;
                return;
            }

            const int margin = 20;

            // Sprawdź kliknięcia na budynki
            if (SelectedBuildingIndex >= 0 && SelectedBuildingIndex < SelectedProvince.Buildings.Count)
            {
                var buildingsContainerX = _ui.PanelX + margin + 10;
                var buildingsContainerY = _ui.PanelY + 250; // Przybliżona pozycja sekcji budynków
                var buildingsContainerWidth = _ui.PanelWidth - margin * 2 - 20;

                for (var i = 0; i < SelectedProvince.Buildings.Count; i++)
                {
                    var buildingY = buildingsContainerY + 5 + i * (int)(30 * _ui.FontScale);

                    // Przycisk wyboru budynku
                    var buildingRect = new Rectangle(
                        buildingsContainerX,
                        buildingY,
                        buildingsContainerWidth - 30,
                        (int)(25 * _ui.FontScale)
                    );

                    // Przycisk ulepszenia budynku
                    var upgradeButtonRect = new Rectangle(
                        buildingsContainerX + buildingsContainerWidth - 25,
                        buildingY + 2,
                        (int)(20 * _ui.FontScale),
                        (int)(20 * _ui.FontScale)
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
            var buildButtonsY = _ui.PanelY + _ui.PanelHeight - 150; // Przybliżona pozycja sekcji przycisków budowy
            var buttonWidth = (_ui.PanelWidth - margin * 2 - 40) / 3;
            var buttonHeight = (int)(35 * _ui.FontScale);
            const int buttonSpacing = 10;

            for (var i = 0; i < buildingTypes.Length; i++)
            {
                var row = i / 3;
                var col = i % 3;

                var buttonX = _ui.PanelX + margin + 10 + col * (buttonWidth + buttonSpacing);
                var buttonY = buildButtonsY + row * (buttonHeight + buttonSpacing);

                var buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);

                if (buttonRect.Contains(mousePosition))
                {
                    const int buildCost = 500;
                    var population = _gameEngine._populations.Find(p => p.ProvinceId == SelectedProvince.Id);

                    if (population is { Wealth: >= buildCost })
                    {
                        population.Wealth -= buildCost;
                        SelectedProvince.AddBuilding(buildingTypes[i]);
                    }
                }
            }

            // Sprawdź kliknięcie na przycisk ulepszenia w panelu szczegółów budynku
            if (SelectedBuilding != null)
            {
                // Przycisk ulepszenia w panelu szczegółów
                var upgradeButtonRect = new Rectangle(
                    _ui.DetailsPanelX + _ui.DetailsPanelMargin,
                    _ui.DetailsPanelY + 300, // Przybliżona pozycja przycisku
                    _ui.DetailsPanelWidth - _ui.DetailsPanelMargin * 2,
                    (int)(35 * _ui.FontScale)
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