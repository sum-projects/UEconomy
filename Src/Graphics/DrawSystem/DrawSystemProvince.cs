using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UEconomy.Engine;
using UEconomy.Game;

namespace UEconomy.Graphics.DrawSystem;

public class DrawSystemProvince : DrawSystem
{

    public DrawSystemProvince(Texture2D provinceTexture, Texture2D uiBackground, SpriteFont font, UI ui, EventEngine eventEngine) : base(provinceTexture, uiBackground, font, ui, eventEngine)
    {
    }

    public void DrawProvinceIcons(SpriteBatch spriteBatch, Province province, Population population,  int cellX, int cellY)
    {
        var iconSize = Ui.CellSize / 10;
        const int margin = 2;
        var iconX = cellX + Ui.CellSize - iconSize - margin;
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
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(iconX, iconY, iconSize, iconSize),
                iconColor
            );

            // Dodaj obramowanie ikony
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(iconX, iconY, iconSize, 1),
                Color.Black * 0.5f
            );
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(iconX, iconY, 1, iconSize),
                Color.Black * 0.5f
            );
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(iconX + iconSize - 1, iconY, 1, iconSize),
                Color.Black * 0.5f
            );
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(iconX, iconY + iconSize - 1, iconSize, 1),
                Color.Black * 0.5f
            );

            iconY += iconSize + margin;
        }

        // Rysuj liczbę mieszkańców
        spriteBatch.DrawString(
            Font,
            $"{population.Size}",
            new Vector2(cellX + 5, cellY + 5),
            Color.White,
            0f,
            Vector2.Zero,
            Ui.FontScale * 0.8f,
            SpriteEffects.None,
            0f
        );
    }

    public void DrawProvinceDetailsPanel(SpriteBatch spriteBatch, Population population)
    {
        // Tło panelu z efektem gradientu
        var topColor = new Color(20, 20, 40, 230);
        var bottomColor = new Color(30, 30, 60, 230);

        // Górna część panelu
        spriteBatch.Draw(
            UiBackground,
            new Rectangle(Ui.PanelX, Ui.PanelY, Ui.PanelWidth, Ui.PanelHeight / 2),
            topColor
        );

        // Dolna część panelu
        spriteBatch.Draw(
            UiBackground,
            new Rectangle(Ui.PanelX, Ui.PanelY + Ui.PanelHeight / 2, Ui.PanelWidth,
                Ui.PanelHeight - Ui.PanelHeight / 2),
            bottomColor
        );

        // Obramowanie panelu
        DrawPanelBorder(spriteBatch, Ui.PanelX, Ui.PanelY, Ui.PanelWidth, Ui.PanelHeight,
            new Color(80, 80, 120, 200));

        // Nagłówek z nazwą prowincji
        DrawHeaderBar(spriteBatch, Ui.PanelX, Ui.PanelY, Ui.PanelWidth,
            $"PROWINCJA: {EventEngine.SelectedProvince.Name}", Color.Gold);

        const int sectionMargin = 20;
        var yPos = Ui.PanelY + 50;

        // Sekcja podstawowych informacji
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Informacje podstawowe:", sectionMargin,
            () =>
            {
                // Populacja
                spriteBatch.DrawString(
                    Font,
                    $"Populacja: {population?.Size ?? 0} osób",
                    new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale * 1.1f,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(25 * Ui.FontScale);

                // Zamożność
                spriteBatch.DrawString(
                    Font,
                    $"Zamożność: {population?.Wealth.ToString("F2") ?? "0"} $",
                    new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale * 1.1f,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(25 * Ui.FontScale);

                // Dostępni pracownicy
                spriteBatch.DrawString(
                    Font,
                    $"Dostępni pracownicy: {EventEngine.SelectedProvince.AvailableWorkers}",
                    new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale * 1.1f,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(25 * Ui.FontScale);
            });

        // Sekcja zasobów
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Dostępne zasoby:", sectionMargin, () =>
        {
            // Utworzenie złożonego widoku zasobów jako paska postępu
            foreach (var resource in EventEngine.SelectedProvince.Resources)
            {
                // Tło paska
                var barWidth = Ui.PanelWidth - sectionMargin * 2 - 20;
                var barHeight = (int)(20 * Ui.FontScale);

                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(Ui.PanelX + sectionMargin + 10, yPos, barWidth, barHeight),
                    new Color(40, 40, 60)
                );

                // Wypełnienie paska proporcjonalne do ilości zasobu
                var fillRatio = Math.Min(1.0f, resource.Value / 1000f); // Maksimum przy 1000 jednostek

                // Kolor paska zależny od typu zasobu
                var resourceColor = GetResourceColor(resource.Key);

                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(Ui.PanelX + sectionMargin + 10, yPos, (int)(barWidth * fillRatio), barHeight),
                    resourceColor
                );

                // Tekst nazwy zasobu
                spriteBatch.DrawString(
                    Font,
                    $"{resource.Key}:",
                    new Vector2(Ui.PanelX + sectionMargin + 15, yPos + 2),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale,
                    SpriteEffects.None,
                    0f
                );

                // Tekst ilości zasobu
                var resourceAmount = $"{resource.Value:F1}";
                var textSize = Font.MeasureString(resourceAmount) * Ui.FontScale;

                spriteBatch.DrawString(
                    Font,
                    resourceAmount,
                    new Vector2(Ui.PanelX + sectionMargin + barWidth - textSize.X - 5, yPos + 2),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale,
                    SpriteEffects.None,
                    0f
                );

                yPos += barHeight + 5;
            }
        });

        // Sekcja potrzeb populacji
        if (population != null)
        {
            DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Potrzeby populacji (na osobę):",
                sectionMargin,
                () =>
                {
                    foreach (var need in population.Needs)
                    {
                        // Pasek potrzeby
                        var barWidth = Ui.PanelWidth - sectionMargin * 2 - 20;
                        var barHeight = (int)(16 * Ui.FontScale);

                        // Tło paska
                        spriteBatch.Draw(
                            ProvinceTexture,
                            new Rectangle(Ui.PanelX + sectionMargin + 10, yPos, barWidth, barHeight),
                            new Color(40, 40, 60)
                        );

                        // Wypełnienie paska proporcjonalne do poziomu zadowolenia
                        var satisfactionLevel = population.Satisfaction.TryGetValue(need.Key, out var value)
                            ? value
                            : 0.1f;

                        // Kolor zależny od poziomu zadowolenia
                        var satisfactionColor = satisfactionLevel > 0.8f ? new Color(0.2f, 0.8f, 0.2f) :
                            satisfactionLevel > 0.5f ? new Color(0.8f, 0.8f, 0.2f) :
                            new Color(0.8f, 0.2f, 0.2f);

                        spriteBatch.Draw(
                            ProvinceTexture,
                            new Rectangle(Ui.PanelX + sectionMargin + 10, yPos, (int)(barWidth * satisfactionLevel),
                                barHeight),
                            satisfactionColor
                        );

                        // Tekst nazwy potrzeby i wartości
                        spriteBatch.DrawString(
                            Font,
                            $"{need.Key}: {need.Value:F2} jedn./os. ({satisfactionLevel:P0})",
                            new Vector2(Ui.PanelX + sectionMargin + 15, yPos),
                            Color.White,
                            0f,
                            Vector2.Zero,
                            Ui.FontScale,
                            SpriteEffects.None,
                            0f
                        );

                        yPos += barHeight + 5;
                    }

                    // Całkowite zapotrzebowanie
                    yPos += (int)(10 * Ui.FontScale);

                    spriteBatch.DrawString(
                        Font,
                        "Całkowite zapotrzebowanie:",
                        new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        Ui.FontScale,
                        SpriteEffects.None,
                        0f
                    );

                    yPos += (int)(20 * Ui.FontScale);

                    foreach (var need in population.Needs)
                    {
                        var totalNeed = need.Value * population.Size;
                        spriteBatch.DrawString(
                            Font,
                            $"{need.Key}: {totalNeed:F1} jedn.",
                            new Vector2(Ui.PanelX + sectionMargin + 20, yPos),
                            Color.White,
                            0f,
                            Vector2.Zero,
                            Ui.FontScale,
                            SpriteEffects.None,
                            0f
                        );
                        yPos += (int)(20 * Ui.FontScale);
                    }
                });
        }

        // Sekcja budynków
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Budynki:", sectionMargin, () =>
        {
            // Reset indeksu wybranego budynku
            EventEngine.SelectedBuildingIndex = -1;

            // Kontener na listę budynków
            var buildingsContainerX = Ui.PanelX + sectionMargin + 10;
            var buildingsContainerY = yPos;
            var buildingsContainerWidth = Ui.PanelWidth - sectionMargin * 2 - 20;
            var buildingsContainerHeight =
                (int)(Math.Min(6, EventEngine.SelectedProvince.Buildings.Count) * (30 * Ui.FontScale) + 10);

            // Tło kontenera
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(buildingsContainerX, buildingsContainerY, buildingsContainerWidth,
                    buildingsContainerHeight),
                new Color(30, 30, 50)
            );

            // Rysuj listę budynków
            var buildingY = buildingsContainerY + 5;

            for (var i = 0; i < EventEngine.SelectedProvince.Buildings.Count; i++)
            {
                var building = EventEngine.SelectedProvince.Buildings[i];

                // Określ kolor budynku w zależności od jego stanu
                var buildingColor = building.IsActive ? Color.White : new Color(150, 150, 150);

                // Jeśli budynek jest wybrany, podświetl go
                if (EventEngine.SelectedBuilding != null && EventEngine.SelectedBuilding.Id == building.Id)
                {
                    // Tło wybranego budynku
                    spriteBatch.Draw(
                        ProvinceTexture,
                        new Rectangle(buildingsContainerX + 2, buildingY, buildingsContainerWidth - 4,
                            (int)(25 * Ui.FontScale)),
                        new Color(80, 80, 120)
                    );

                    buildingColor = Color.Yellow;
                }

                // Ikona typu budynku
                DrawBuildingIcon(spriteBatch, buildingsContainerX + 5, buildingY + 2, (int)(20 * Ui.FontScale),
                    building.Type);

                // Nazwa budynku i poziom
                spriteBatch.DrawString(
                    Font,
                    $"{building.Name} (Lv.{building.Level})",
                    new Vector2(buildingsContainerX + (int)(30 * Ui.FontScale), buildingY + 2),
                    buildingColor,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale,
                    SpriteEffects.None,
                    0f
                );

                // Informacja o pracownikach
                var workersInfo = $"{building.CurrentWorkers}/{building.WorkersCapacity}";
                var workersSize = Font.MeasureString(workersInfo) * Ui.FontScale;

                spriteBatch.DrawString(
                    Font,
                    workersInfo,
                    new Vector2(buildingsContainerX + buildingsContainerWidth - workersSize.X - 30, buildingY + 2),
                    buildingColor,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale,
                    SpriteEffects.None,
                    0f
                );

                // Przycisk ulepszenia
                DrawUpgradeButton(
                    spriteBatch,
                    buildingsContainerX + buildingsContainerWidth - 25,
                    buildingY + 2,
                    (int)(20 * Ui.FontScale),
                    (int)(20 * Ui.FontScale),
                    building,
                    population
                );

                // Zapisz indeks budynku pod kursorem
                var buildingRect = new Rectangle(
                    buildingsContainerX,
                    buildingY,
                    buildingsContainerWidth - 30,
                    (int)(25 * Ui.FontScale)
                );

                if (buildingRect.Contains(Mouse.GetState().Position))
                {
                    EventEngine.SelectedBuildingIndex = i;
                }

                buildingY += (int)(30 * Ui.FontScale);
            }

            yPos += buildingsContainerHeight + 10;

            // Przyciski budowy nowych budynków
            DrawBuildButtons(population, spriteBatch, ref yPos, sectionMargin);
        });

        // Przycisk zamknięcia panelu
        DrawCloseButton(spriteBatch, Ui.PanelX + Ui.PanelWidth - 40, Ui.PanelY + 5, 30, 30);

        // Jeśli budynek jest wybrany, pokaż szczegóły
        if (EventEngine.SelectedBuilding != null)
        {
            DrawBuildingDetailsPanel(spriteBatch, Ui.PanelX + Ui.PanelWidth + 10, Ui.PanelY, population);
        }
    }


    private void DrawCloseButton(SpriteBatch spriteBatch, int x, int y, int width, int height)
    {
        // Sprawdź, czy myszka jest nad przyciskiem
        var buttonRect = new Rectangle(x, y, width, height);
        var isHovered = buttonRect.Contains(Mouse.GetState().Position);

        // Tło przycisku
        var buttonColor = isHovered ? new Color(180, 60, 60) : new Color(120, 40, 40);

        spriteBatch.Draw(
            ProvinceTexture,
            buttonRect,
            buttonColor
        );

        // Symbol X - narysowany za pomocą dwóch prostokątów obróconych względem siebie
        var padding = width / 4;
        var lineThickness = Math.Max(2, width / 10);

        // Alternatywne podejście: rysujemy dwa prostokąty zamiast obracać tekstury
        // Pierwsza linia (ukośna lewa-góra do prawa-dół)
        for (var i = 0; i < lineThickness; i++)
        {
            // Narysuj linię po przekątnej punkt po punkcie
            for (var j = 0; j < width - padding * 2; j++)
            {
                var pointX = x + padding + j;
                var pointY = y + padding + j;
                if (pointX < x + width - padding && pointY < y + height - padding)
                {
                    spriteBatch.Draw(
                        ProvinceTexture,
                        new Rectangle(pointX, pointY, 1, 1),
                        Color.White
                    );
                }
            }
        }

        // Druga linia (ukośna prawa-góra do lewa-dół)
        for (var i = 0; i < lineThickness; i++)
        {
            // Narysuj linię po przekątnej punkt po punkcie
            for (var j = 0; j < width - padding * 2; j++)
            {
                var pointX = x + width - padding - j;
                var pointY = y + padding + j;
                if (pointX >= x + padding && pointY < y + height - padding)
                {
                    spriteBatch.Draw(
                        ProvinceTexture,
                        new Rectangle(pointX, pointY, 1, 1),
                        Color.White
                    );
                }
            }
        }
    }

    private void DrawBuildButtons(Population population, SpriteBatch spriteBatch, ref int yPos, int margin)
    {
        spriteBatch.DrawString(
            Font,
            "Zbuduj nowy budynek:",
            new Vector2(Ui.PanelX + margin + 10, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );

        yPos += (int)(25 * Ui.FontScale);

        // Definicje typów budynków
        string[] buildingTypes = { "FieldCrop", "Sawmill", "IronMine", "CoalMine", "FurnitureFactory", "TailorShop" };
        string[] buildingNames = { "Pole uprawne", "Tartak", "Kopalnia Fe", "Kopalnia C", "Fabryka mebli", "Szwalnia" };

        // Układamy przyciski w 2 rzędy po 3
        var buttonWidth = (Ui.PanelWidth - margin * 2 - 40) / 3;
        var buttonHeight = (int)(35 * Ui.FontScale);
        var buttonSpacing = 10;

        for (var i = 0; i < buildingTypes.Length; i++)
        {
            var row = i / 3;
            var col = i % 3;

            var buttonX = Ui.PanelX + margin + 10 + col * (buttonWidth + buttonSpacing);
            var buttonY = yPos + row * (buttonHeight + buttonSpacing);

            // Sprawdź, czy myszka jest nad przyciskiem
            var buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
            var isHovered = buttonRect.Contains(Mouse.GetState().Position);

            // Sprawdź, czy gracz może sobie pozwolić na budynek
            const int buildCost = 500;
            var canAfford = population is { Wealth: >= buildCost };

            // Tło przycisku
            var buttonColor = !canAfford ? new Color(80, 80, 80) :
                isHovered ? new Color(100, 100, 160) :
                new Color(80, 80, 120);

            spriteBatch.Draw(
                ProvinceTexture,
                buttonRect,
                buttonColor
            );

            // Obramowanie przycisku
            DrawPanelBorder(spriteBatch, buttonX, buttonY, buttonWidth, buttonHeight, new Color(120, 120, 150, 200));

            // Ikona budynku
            DrawBuildingIcon(spriteBatch, buttonX + 5, buttonY + 5, buttonHeight - 10, buildingTypes[i]);

            // Tekst przycisku
            spriteBatch.DrawString(
                Font,
                buildingNames[i],
                new Vector2(buttonX + buttonHeight, buttonY + buttonHeight / 2 - 10),
                canAfford ? Color.White : new Color(150, 150, 150),
                0f,
                Vector2.Zero,
                Ui.FontScale * 0.9f,
                SpriteEffects.None,
                0f
            );

            // Koszt budowy
            spriteBatch.DrawString(
                Font,
                $"{buildCost}$",
                new Vector2(buttonX + buttonHeight, buttonY + buttonHeight / 2 + 5),
                canAfford ? new Color(200, 255, 200) : new Color(150, 150, 150),
                0f,
                Vector2.Zero,
                Ui.FontScale * 0.8f,
                SpriteEffects.None,
                0f
            );
        }

        yPos += 2 * (buttonHeight + buttonSpacing);
    }

    private void DrawBuildingIcon(SpriteBatch spriteBatch, int x, int y, int size, string buildingType)
    {
        // Tło ikony
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y, size, size),
            new Color(40, 40, 60)
        );

        // Symbol budynku zależny od typu
        switch (buildingType)
        {
            case "FieldCrop":
                // Prosty symbol pola
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 4, y + size / 2, size / 2, size / 3),
                    new Color(0, 200, 0)
                );
                break;

            case "Sawmill":
                // Symbol tartaku (drzewo)
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 2 - size / 10, y + size / 3, size / 5, size / 2),
                    new Color(150, 75, 0)
                );
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 4, y + size / 6, size / 2, size / 3),
                    new Color(0, 150, 0)
                );
                break;

            case "IronMine":
                // Symbol kopalni żelaza
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 4, y + size / 4, size / 2, size / 2),
                    new Color(180, 180, 180)
                );
                break;

            case "CoalMine":
                // Symbol kopalni węgla
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 4, y + size / 4, size / 2, size / 2),
                    new Color(50, 50, 50)
                );
                break;

            case "FurnitureFactory":
                // Symbol fabryki mebli
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 6, y + size / 3, size * 2 / 3, size / 2),
                    new Color(180, 120, 60)
                );
                break;

            case "TailorShop":
                // Symbol szwalni
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 4, y + size / 4, size / 2, size / 6),
                    new Color(150, 150, 200)
                );
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + size / 4, y + size / 4 + size / 6, size / 6, size / 3),
                    new Color(150, 150, 200)
                );
                break;
        }

        // Obramowanie ikony
        DrawPanelBorder(spriteBatch, x, y, size, size, new Color(80, 80, 100, 150));
    }

    private void DrawUpgradeButton(SpriteBatch spriteBatch, int x, int y, int width, int height, Building building,
        Population population)
    {
        // Sprawdź, czy stać gracza na ulepszenie
        var canAfford = population != null && population.Wealth >= building.UpgradeCost;

        // Sprawdź, czy myszka jest nad przyciskiem
        var buttonRect = new Rectangle(x, y, width, height);
        var isHovered = buttonRect.Contains(Mouse.GetState().Position);

        // Tło przycisku
        var buttonColor = !canAfford ? new Color(80, 80, 80) :
            isHovered ? new Color(100, 180, 100) :
            new Color(60, 140, 60);

        spriteBatch.Draw(
            ProvinceTexture,
            buttonRect,
            buttonColor
        );

        // Symbol +
        var padding = width / 4;
        var lineThickness = Math.Max(2, height / 5);

        // Linia pozioma
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x + padding, y + height / 2 - lineThickness / 2, width - padding * 2, lineThickness),
            Color.White
        );

        // Linia pionowa
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x + width / 2 - lineThickness / 2, y + padding, lineThickness, height - padding * 2),
            Color.White
        );

        // Obramowanie przycisku
        DrawPanelBorder(spriteBatch, x, y, width, height, new Color(100, 100, 100, 150));
    }

    private void DrawBuildingDetailsPanel(SpriteBatch spriteBatch, int x, int y, Population population)
    {
        // Określ rozmiar panelu
        const int panelWidth = 250;
        const int panelHeight = 350;

        // Tło panelu
        spriteBatch.Draw(
            UiBackground,
            new Rectangle(x, y, panelWidth, panelHeight),
            new Color(25, 25, 45, 230)
        );

        // Obramowanie panelu
        DrawPanelBorder(spriteBatch, x, y, panelWidth, panelHeight, new Color(80, 80, 120, 200));

        // Nagłówek z nazwą budynku
        DrawHeaderBar(spriteBatch, x, y, panelWidth, EventEngine.SelectedBuilding.Name, Color.Gold);

        const int margin = 15;
        var yPos = y + 50;

        // Informacja o poziomie
        spriteBatch.DrawString(
            Font,
            $"Poziom: {EventEngine.SelectedBuilding.Level}",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(25 * Ui.FontScale);

        // Informacja o pracownikach
        spriteBatch.DrawString(
            Font,
            $"Pracownicy: {EventEngine.SelectedBuilding.CurrentWorkers}/{EventEngine.SelectedBuilding.WorkersCapacity}",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(25 * Ui.FontScale);

        // Status
        spriteBatch.DrawString(
            Font,
            $"Status: {(EventEngine.SelectedBuilding.IsActive ? "Aktywny" : "Nieaktywny")}",
            new Vector2(x + margin, yPos),
            EventEngine.SelectedBuilding.IsActive ? new Color(100, 255, 100) : new Color(255, 100, 100),
            0f,
            Vector2.Zero,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(30 * Ui.FontScale);

        // Produkcja
        spriteBatch.DrawString(
            Font,
            "Produkcja:",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );
        yPos += (int)(20 * Ui.FontScale);

        foreach (var resource in EventEngine.SelectedBuilding.ProductionRates)
        {
            // Oblicz faktyczną produkcję
            var actualProduction = resource.Value * EventEngine.SelectedBuilding.CurrentWorkers /
                EventEngine.SelectedBuilding.WorkersCapacity * EventEngine.SelectedBuilding.Level;

            // Pasek produkcji
            var barWidth = panelWidth - margin * 2 - 10;
            var barHeight = (int)(15 * Ui.FontScale);

            // Tło paska
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(x + margin, yPos, barWidth, barHeight),
                new Color(40, 40, 60)
            );

            // Wypełnienie paska
            var maxProduction = resource.Value * EventEngine.SelectedBuilding.Level; // Maksymalna możliwa produkcja
            var fillRatio = EventEngine.SelectedBuilding.CurrentWorkers /
                            (float)EventEngine.SelectedBuilding.WorkersCapacity;

            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(x + margin, yPos, (int)(barWidth * fillRatio), barHeight),
                GetResourceColor(resource.Key)
            );

            // Tekst produkcji
            spriteBatch.DrawString(
                Font,
                $"{resource.Key}: {actualProduction:F1}/tura",
                new Vector2(x + margin + 5, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                Ui.FontScale,
                SpriteEffects.None,
                0f
            );

            yPos += barHeight + 5;
        }

        // Zużycie
        if (EventEngine.SelectedBuilding.ConsumptionRates.Count > 0)
        {
            yPos += (int)(10 * Ui.FontScale);

            spriteBatch.DrawString(
                Font,
                "Zużycie:",
                new Vector2(x + margin, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                Ui.FontScale,
                SpriteEffects.None,
                0f
            );
            yPos += (int)(20 * Ui.FontScale);

            foreach (var resource in EventEngine.SelectedBuilding.ConsumptionRates)
            {
                // Oblicz faktyczne zużycie
                var actualConsumption = resource.Value * EventEngine.SelectedBuilding.CurrentWorkers /
                    EventEngine.SelectedBuilding.WorkersCapacity * EventEngine.SelectedBuilding.Level;

                // Pasek zużycia
                var barWidth = panelWidth - margin * 2 - 10;
                var barHeight = (int)(15 * Ui.FontScale);

                // Tło paska
                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + margin, yPos, barWidth, barHeight),
                    new Color(40, 40, 60)
                );

                // Wypełnienie paska
                var maxConsumption = resource.Value * EventEngine.SelectedBuilding.Level;
                var fillRatio = EventEngine.SelectedBuilding.CurrentWorkers /
                                (float)EventEngine.SelectedBuilding.WorkersCapacity;

                spriteBatch.Draw(
                    ProvinceTexture,
                    new Rectangle(x + margin, yPos, (int)(barWidth * fillRatio), barHeight),
                    new Color(180, 100, 100) // Czerwonawy kolor dla zużycia
                );

                // Tekst zużycia
                spriteBatch.DrawString(
                    Font,
                    $"{resource.Key}: {actualConsumption:F1}/tura",
                    new Vector2(x + margin + 5, yPos),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale,
                    SpriteEffects.None,
                    0f
                );

                yPos += barHeight + 5;
            }
        }

        // Koszt ulepszenia
        yPos += (int)(20 * Ui.FontScale);

        spriteBatch.DrawString(
            Font,
            $"Koszt ulepszenia: {EventEngine.SelectedBuilding.UpgradeCost}$",
            new Vector2(x + margin, yPos),
            Color.White,
            0f,
            Vector2.Zero,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );

        // Przycisk ulepszenia
        yPos += (int)(30 * Ui.FontScale);

        // Sprawdź, czy gracz może sobie pozwolić na ulepszenie
        var canAfford = population != null && population.Wealth >= EventEngine.SelectedBuilding.UpgradeCost;

        // Rysuj przycisk
        var buttonWidth = panelWidth - margin * 2;
        var buttonHeight = (int)(35 * Ui.FontScale);

        // Sprawdź, czy kursor jest nad przyciskiem
        var upgradeButtonRect = new Rectangle(x + margin, yPos, buttonWidth, buttonHeight);
        var isHovered = upgradeButtonRect.Contains(Mouse.GetState().Position);

        // Tło przycisku
        var buttonColor = !canAfford ? new Color(80, 80, 80) :
            isHovered ? new Color(80, 120, 80) :
            new Color(60, 100, 60);

        spriteBatch.Draw(
            ProvinceTexture,
            upgradeButtonRect,
            buttonColor
        );

        // Tekst przycisku
        spriteBatch.DrawString(
            Font,
            "Ulepsz budynek",
            new Vector2(x + margin + buttonWidth / 2, yPos + buttonHeight / 2),
            canAfford ? Color.White : new Color(150, 150, 150),
            0f,
            Font.MeasureString("Ulepsz budynek") * Ui.FontScale / 2,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );

        // Obramowanie przycisku
        DrawPanelBorder(spriteBatch, x + margin, yPos, buttonWidth, buttonHeight, new Color(100, 120, 100, 200));
    }
}