using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine.Interfaces;

namespace UEconomy.Graphics.DrawSystem;

public class DrawSystemStats : DrawSystemBase
{
    private readonly List<string> _resourcesForGraphs = new();
    private readonly Dictionary<string, List<float>> _priceHistory = new();
    private const int MaxHistoryPoints = 30;

    public DrawSystemStats(
        GraphicsDevice graphicsDevice,
        Texture2D provinceTexture,
        Texture2D uiBackground,
        SpriteFont font,
        UI ui,
        IEventEngine eventEngine,
        IGameEngine gameEngine)
        : base(graphicsDevice, provinceTexture, uiBackground, font, ui, eventEngine, gameEngine)
    {
    }

    public void AddPriceDataPoint(IEnumerable<string> resources)
    {
        var market = GameEngine.GetMarket();

        foreach (var resource in resources)
        {
            if (!_priceHistory.ContainsKey(resource))
            {
                _priceHistory[resource] = new List<float>();
                _resourcesForGraphs.Add(resource);
            }

            var price = market.GetPrice(resource);
            _priceHistory[resource].Add(price);

            if (_priceHistory[resource].Count > MaxHistoryPoints)
            {
                _priceHistory[resource].RemoveAt(0);
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        DrawBackground();
        DrawHeader();
        DrawGlobalStats();
        DrawPriceGraphs();
        DrawProductionConsumptionStats();
    }

    private void DrawHeader()
    {
        var headerRect = new Rectangle(0, 0, Ui.ScreenWidth, 60);
        DrawPanel(headerRect, Color.DarkSlateGray);

        SpriteBatch.Begin();
        SpriteBatch.DrawString(
            Font,
            "Global Statistics",
            new Vector2(20, 20),
            Color.White
        );

        // Back button
        var backButton = new Rectangle(Ui.ScreenWidth - 100, 15, 80, 30);
        SpriteBatch.Draw(UiBackground, backButton, Color.DarkRed);

        const string backText = "Back";
        var textSize = Font.MeasureString(backText);
        SpriteBatch.DrawString(
            Font,
            backText,
            new Vector2(
                backButton.X + (backButton.Width - textSize.X) / 2,
                backButton.Y + (backButton.Height - textSize.Y) / 2
            ),
            Color.White
        );

        SpriteBatch.End();
    }

    private void DrawGlobalStats()
    {
        var stats = GameEngine.GetStats();
        var panelRect = new Rectangle(20, 70, 400, 300);
        DrawPanel(panelRect, Color.DarkSlateBlue, 0.9f);

        SpriteBatch.Begin();
        SpriteBatch.DrawString(
            Font,
            "Key Indicators",
            new Vector2(30, 80),
            Color.White
        );
        SpriteBatch.End();

        var y = 120;

        // Display key global stats
        var statsToShow = new string[]
        {
            "Total Population",
            "Total Wealth",
            "Employed Workers",
            "Available Workers"
        };

        foreach (var statName in statsToShow)
        {
            if (stats.GlobalStats.TryGetValue(statName, out var value))
            {
                DrawText(
                    statName,
                    new Vector2(40, y),
                    Color.White
                );

                DrawText(
                    $"{value:F0}",
                    new Vector2(300, y),
                    Color.LightGreen
                );

                y += 30;
            }
        }

        y += 20;

        // Display resource satisfaction stats
        DrawText("Resource Satisfaction", new Vector2(40, y), Color.White);
        y += 30;

        foreach (var key in stats.GlobalStats.Keys.Where(k => k.StartsWith("Satisfaction ")))
        {
            var resourceName = key.Substring("Satisfaction ".Length);
            var satisfactionValue = stats.GlobalStats[key];

            DrawText(
                resourceName,
                new Vector2(60, y),
                Color.White
            );

            // Draw satisfaction bar
            var barRect = new Rectangle(200, y, 180, 20);
            DrawProgressBar(
                barRect,
                satisfactionValue,
                Color.DarkGray,
                GetSatisfactionColor(satisfactionValue),
                $"{satisfactionValue * 100:F0}%"
            );

            y += 30;
        }
    }

    private void DrawPriceGraphs()
    {
        var market = GameEngine.GetMarket();
        var resources = market.GetAllResources();

        // Add current prices to history if not already tracking
        foreach (var resource in resources)
        {
            if (!_priceHistory.ContainsKey(resource))
            {
                _priceHistory[resource] = new List<float>();
                _resourcesForGraphs.Add(resource);
            }
        }

        // Draw price history panel
        var panelRect = new Rectangle(430, 70, Ui.ScreenWidth - 450, 300);
        DrawPanel(panelRect, Color.DarkSlateBlue, 0.9f);

        SpriteBatch.Begin();
        SpriteBatch.DrawString(
            Font,
            "Resource Price Trends",
            new Vector2(440, 80),
            Color.White
        );
        SpriteBatch.End();

        // Draw small graphs for each resource
        const int graphsPerRow = 3;
        var graphWidth = (panelRect.Width - 20) / graphsPerRow;
        const int graphHeight = 80;
        const int margin = 10;

        var x = panelRect.X + margin;
        var y = panelRect.Y + 40;
        var count = 0;

        foreach (var resource in _resourcesForGraphs)
        {
            // Draw graph background
            var graphRect = new Rectangle(x, y, graphWidth - margin, graphHeight);
            DrawPanel(graphRect, Color.Black, 0.6f);

            // Draw resource name
            DrawText(
                resource,
                new Vector2(x + 5, y + 5),
                Color.White
            );

            // Draw current price
            var currentPrice = market.GetPrice(resource);
            DrawText(
                $"{currentPrice:F1}",
                new Vector2(x + graphWidth - 50, y + 5),
                Color.LightGreen
            );

            // Draw price line graph
            if (_priceHistory.TryGetValue(resource, out var history) && history.Count > 1)
            {
                SpriteBatch.Begin();

                var minPrice = history.Min();
                var maxPrice = history.Max();
                var range = maxPrice - minPrice;

                // Ensure some minimum range to avoid division by zero
                if (range < 0.01f) range = 1.0f;

                for (var i = 1; i < history.Count; i++)
                {
                    // Calculate points for line segment
                    var x1 = x + 5 + (i - 1) * (graphWidth - 10 - margin) / MaxHistoryPoints;
                    var x2 = x + 5 + i * (graphWidth - 10 - margin) / MaxHistoryPoints;

                    var y1 = y + graphHeight - 10 - (history[i - 1] - minPrice) * (graphHeight - 20) / range;
                    var y2 = y + graphHeight - 10 - (history[i] - minPrice) * (graphHeight - 20) / range;

                    // Draw line segment
                    DrawLine(SpriteBatch, ProvinceTexture,
                        new Vector2(x1, y1),
                        new Vector2(x2, y2),
                        Color.Yellow,
                        1);
                }

                SpriteBatch.End();
            }

            // Move to next position
            count++;
            if (count % graphsPerRow == 0)
            {
                x = panelRect.X + margin;
                y += graphHeight + margin;
            }
            else
            {
                x += graphWidth;
            }
        }
    }

    private void DrawProductionConsumptionStats()
    {
        var stats = GameEngine.GetStats();
        var panelRect = new Rectangle(20, 380, Ui.ScreenWidth - 40, 300);
        DrawPanel(panelRect, Color.DarkSlateBlue, 0.9f);

        SpriteBatch.Begin();
        SpriteBatch.DrawString(
            Font,
            "Production & Demand",
            new Vector2(30, 390),
            Color.White
        );
        SpriteBatch.End();

        var resources = GameEngine.GetMarket().GetAllResources();
        var y = 430;

        // Draw column headers
        DrawText("Resource", new Vector2(40, y), Color.White);
        DrawText("Total Stock", new Vector2(200, y), Color.White);
        DrawText("Production", new Vector2(350, y), Color.White);
        DrawText("Demand", new Vector2(500, y), Color.White);
        DrawText("Balance", new Vector2(650, y), Color.White);
        DrawText("Market Price", new Vector2(800, y), Color.White);

        y += 30;

        foreach (var resource in resources)
        {
            // Resource name
            DrawText(resource, new Vector2(40, y), Color.White);

            // Total stock
            float totalStock = 0;
            if (stats.GlobalStats.TryGetValue($"Total {resource}", out var stock))
            {
                totalStock = stock;
            }

            DrawText($"{totalStock:F0}", new Vector2(200, y), Color.White);

            // Production
            float production = 0;
            if (stats.GlobalStats.TryGetValue($"Production {resource}", out var prod))
            {
                production = prod;
            }

            DrawText($"{production:F1}/day", new Vector2(350, y), Color.LightGreen);

            // Demand
            float demand = 0;
            if (stats.GlobalStats.TryGetValue($"{resource} Demand", out var dem))
            {
                demand = dem;
            }

            DrawText($"{demand:F1}/day", new Vector2(500, y), Color.LightCoral);

            // Balance
            var balance = production - demand;
            var balanceColor = balance >= 0 ? Color.LightGreen : Color.LightCoral;
            DrawText($"{balance:F1}/day", new Vector2(650, y), balanceColor);

            // Market price
            var price = GameEngine.GetMarket().GetPrice(resource);
            DrawText($"{price:F1}", new Vector2(800, y), Color.Yellow);

            y += 30;
        }
    }

    private Color GetSatisfactionColor(float satisfaction)
    {
        if (satisfaction < 0.3f) return Color.Red;
        if (satisfaction < 0.7f) return Color.Yellow;
        return Color.LightGreen;
    }

    private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color,
        float thickness)
    {
        var length = Vector2.Distance(start, end);
        var angle = (float)System.Math.Atan2(end.Y - start.Y, end.X - start.X);

        spriteBatch.Draw(
            texture,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0);
    }
}