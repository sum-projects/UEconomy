using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine;

namespace UEconomy.Graphics.DrawSystem;

public class DrawSystemStats : DrawSystem
{
    public DrawSystemStats(Texture2D provinceTexture, Texture2D uiBackground, SpriteFont font, UI ui, EventEngine eventEngine) : base(provinceTexture, uiBackground, font, ui, eventEngine)
    {
    }

    public void DrawGlobalStatsPanel(GameEngine gameEngine, SpriteBatch spriteBatch, List<string> resources)
    {
        // Tło panelu z gradientem
        var topColor = new Color(20, 25, 40, 230);
        var bottomColor = new Color(40, 45, 65, 230);

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
            new Color(80, 100, 120, 200));

        // Nagłówek
        DrawHeaderBar(spriteBatch, Ui.PanelX, Ui.PanelY, Ui.PanelWidth, "STATYSTYKI GLOBALNE", Color.Gold);

        // Obszary sekcji statystyk
        var sectionMargin = 15;
        var yPos = Ui.PanelY + 50;

        // Sekcja cen rynkowych
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Ceny rynkowe:", sectionMargin, () =>
        {
            foreach (var resource in resources)
            {
                // Wskaźnik podaży/popytu określa kolor (czerwony dla niedoboru, zielony dla nadwyżki)
                var ratio = gameEngine._market.GetSupplyDemandRatio(resource);
                var priceColor = ratio < 0.8f ? new Color(1.0f, 0.7f, 0.7f) :
                    ratio > 1.2f ? new Color(0.7f, 1.0f, 0.7f) :
                    Color.White;

                spriteBatch.DrawString(
                    Font,
                    $"{resource}: {gameEngine._market.GetPrice(resource):F2}$ ({ratio:F2})",
                    new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                    priceColor,
                    0f,
                    Vector2.Zero,
                    Ui.FontScale,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(22 * Ui.FontScale);
            }
        });

        // Sekcja populacji
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Populacja:", sectionMargin, () =>
        {
            spriteBatch.DrawString(
                Font,
                $"{gameEngine.Stats.GlobalStats.GetValueOrDefault("Total Population"):F0}",
                new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                Color.White,
                0f,
                Vector2.Zero,
                Ui.FontScale * 1.2f, // Większa czcionka dla ważnej statystyki
                SpriteEffects.None,
                0f
            );
            yPos += (int)(30 * Ui.FontScale);
        });

        // Sekcja całkowitych zasobów
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Całkowite zasoby:", sectionMargin, () =>
        {
            foreach (var resource in gameEngine._market.GetAllResources())
            {
                // Kolor tekstu zależny od ilości zasobu (im więcej, tym jaśniejszy)
                var amount = gameEngine.Stats.GlobalStats.GetValueOrDefault($"Total {resource}");
                var brightness = Math.Min(1.0f, 0.7f + amount / 10000f);

                spriteBatch.DrawString(
                    Font,
                    $"{resource}: {amount:F0}",
                    new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                    new Color(brightness, brightness, brightness),
                    0f,
                    Vector2.Zero,
                    Ui.FontScale,
                    SpriteEffects.None,
                    0f
                );
                yPos += (int)(22 * Ui.FontScale);
            }
        });

        // Sekcja produkcji
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Globalna produkcja (na turę):",
            sectionMargin, () =>
            {
                foreach (var resource in gameEngine._market.GetAllResources())
                {
                    var production = gameEngine.Stats.GlobalStats.GetValueOrDefault($"Production {resource}");
                    // Kolor zależny od wydajności (zielony dla wysokiej produkcji)
                    var productionColor = production > 1000 ? new Color(0.7f, 1.0f, 0.7f) :
                        production > 500 ? Color.White :
                        new Color(1.0f, 1.0f, 0.8f);

                    spriteBatch.DrawString(
                        Font,
                        $"{resource}: {production:F0}",
                        new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                        productionColor,
                        0f,
                        Vector2.Zero,
                        Ui.FontScale,
                        SpriteEffects.None,
                        0f
                    );
                    yPos += (int)(22 * Ui.FontScale);
                }
            });

        // Sekcja wymiany handlowej
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Ostatnia wymiana handlowa:", sectionMargin,
            () =>
            {
                foreach (var resource in gameEngine._totalTraded.Keys)
                {
                    var amount = gameEngine._totalTraded[resource];
                    // Kolor zależny od ilości wymienionego towaru
                    var tradeColor = amount > 100 ? new Color(0.8f, 1.0f, 0.8f) :
                        amount > 0 ? Color.White :
                        new Color(0.8f, 0.8f, 0.8f);

                    spriteBatch.DrawString(
                        Font,
                        $"{resource}: {amount:F0} jedn.",
                        new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                        tradeColor,
                        0f,
                        Vector2.Zero,
                        Ui.FontScale,
                        SpriteEffects.None,
                        0f
                    );
                    yPos += (int)(22 * Ui.FontScale);
                }
            });

        // Sekcja zadowolenia potrzeb
        DrawStatsSection(spriteBatch, Ui.PanelX, ref yPos, Ui.PanelWidth, "Średnie zadowolenie potrzeb:",
            sectionMargin, () =>
            {
                foreach (var resource in gameEngine._market.GetAllResources())
                {
                    if (gameEngine.Stats.GlobalStats.ContainsKey($"Satisfaction {resource}"))
                    {
                        var satisfaction = gameEngine.Stats.GlobalStats[$"Satisfaction {resource}"];
                        var satColor = satisfaction > 0.8f
                            ? new Color(0.5f, 1.0f, 0.5f)
                            : (satisfaction > 0.5f ? new Color(1.0f, 1.0f, 0.5f) : new Color(1.0f, 0.5f, 0.5f));

                        spriteBatch.DrawString(
                            Font,
                            $"{resource}: {(satisfaction * 100):F0}%",
                            new Vector2(Ui.PanelX + sectionMargin + 10, yPos),
                            satColor,
                            0f,
                            Vector2.Zero,
                            Ui.FontScale,
                            SpriteEffects.None,
                            0f
                        );
                        yPos += (int)(22 * Ui.FontScale);
                    }
                }
            });

        DrawChartAndResourceButtonsToPanel(gameEngine, spriteBatch, ref yPos);
    }

    private void DrawChartAndResourceButtonsToPanel(GameEngine gameEngine, SpriteBatch spriteBatch, ref int yPos)
    {
        const int chartMargin = 15;
        var chartX = Ui.PanelX + chartMargin;
        var chartY = yPos;
        var chartWidth = Ui.PanelWidth - chartMargin * 2;
        const int chartHeight = 150;

        DrawPriceChart(gameEngine, spriteBatch, chartX, chartY, chartWidth, chartHeight, EventEngine.SelectedResource);

        yPos += chartHeight + 20;

        // Przyciski wyboru zasobu do wykresu
        var buttonSize = (Ui.PanelWidth - chartMargin * 2 - 10) / 3; // 3 przyciski w rzędzie, 5px odstępu
        var buttonY = yPos;
        var buttonX = chartX;
        var resourceButtonsSectionY = buttonY; // Zapisz tę pozycję do użycia w HandleResourceButtonClick

        var resources = gameEngine._market.GetAllResources();
        for (var i = 0; i < resources.Length; i++)
        {
            var resource = resources[i];

            // Tło przycisku
            var buttonColor = (EventEngine.SelectedResource == resource)
                ? GetResourceColor(resource) * 0.7f
                : new Color(60, 60, 80);

            var buttonRect = new Rectangle(buttonX, buttonY, buttonSize, 25);

            // Zapisz tę pozycję przycisku do debugowania
            ResourceButtonRects[i] = buttonRect;

            spriteBatch.Draw(
                ProvinceTexture,
                buttonRect,
                buttonColor
            );

            // Tekst przycisku
            spriteBatch.DrawString(
                Font,
                resource,
                new Vector2(buttonX + buttonSize / 2, buttonY + 12),
                Color.White,
                0f,
                Font.MeasureString(resource) * Ui.FontScale / 2,
                Ui.FontScale * 0.8f,
                SpriteEffects.None,
                0f
            );

            // Ustaw pozycję dla następnego przycisku
            buttonX += buttonSize + 5;
            if (buttonX + buttonSize > Ui.PanelX + Ui.PanelWidth - chartMargin)
            {
                buttonX = chartX;
                buttonY += 30;
            }
        }

        yPos = buttonY + 30; // Aktualizuj pozycję Y po narysowaniu wszystkich przycisków
    }

    private void DrawPriceChart(
        GameEngine gameEngine,
        SpriteBatch spriteBatch,
        int x, int y,
        int width, int height,
        string resource
    )
    {
        // Tło wykresu
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y, width, height),
            new Color(20, 20, 40)
        );

        // Obramowanie wykresu
        DrawPanelBorder(spriteBatch, x, y, width, height, new Color(80, 80, 120, 200));

        var history = gameEngine._priceHistory[resource];
        if (history.Count <= 1) return;

        // Znajdź min i max dla skalowania
        var min = history.Min();
        var max = history.Max();
        if (max == min) max = min + 1; // Unikaj dzielenia przez zero

        // Narysuj siatkę pomocniczą
        for (var i = 1; i < 4; i++)
        {
            float yPos = y + height - (i * height / 4);

            // Linia pomocnicza
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle(x, (int)yPos, width, 1),
                new Color(100, 100, 100, 100)
            );

            // Wartość na osi Y
            var value = min + (i * (max - min) / 4);
            spriteBatch.DrawString(
                Font,
                $"{value:F1}",
                new Vector2(x + 5, yPos - 15),
                new Color(150, 150, 150),
                0f,
                Vector2.Zero,
                Ui.FontScale * 0.7f,
                SpriteEffects.None,
                0f
            );
        }

        // Narysuj linię trendu
        for (var i = 1; i < history.Count; i++)
        {
            // Pozycje punktów
            float x1 = x + (i - 1) * width / (history.Count - 1);
            var y1 = y + height - ((history[i - 1] - min) / (max - min)) * height;
            float x2 = x + i * width / (history.Count - 1);
            var y2 = y + height - ((history[i] - min) / (max - min)) * height;

            // Linia między punktami
            DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), GetResourceColor(resource), 2);

            // Kropka na końcach linii
            spriteBatch.Draw(
                ProvinceTexture,
                new Rectangle((int)x2 - 2, (int)y2 - 2, 4, 4),
                GetResourceColor(resource)
            );
        }

        // Tytuł wykresu
        spriteBatch.DrawString(
            Font,
            $"Trend ceny: {resource}",
            new Vector2(x + width / 2, y + 10),
            Color.White,
            0f,
            new Vector2(Font.MeasureString($"Trend ceny: {resource}").X / 2, 0),
            Ui.FontScale * 0.8f,
            SpriteEffects.None,
            0f
        );

        // Aktualna cena
        spriteBatch.DrawString(
            Font,
            $"Obecna: {gameEngine._market.GetPrice(resource):F2}$",
            new Vector2(x + width - 10, y + height - 20),
            GetResourceColor(resource),
            0f,
            new Vector2(Font.MeasureString($"Obecna: {gameEngine._market.GetPrice(resource):F2}$").X, 0),
            Ui.FontScale * 0.8f,
            SpriteEffects.None,
            0f
        );
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 1)
    {
        var delta = end - start;
        var angle = (float)Math.Atan2(delta.Y, delta.X);
        var length = delta.Length();

        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
            null,
            color,
            angle,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );
    }
}