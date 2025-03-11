using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine.Interfaces;
using UEconomy.Game;

namespace UEconomy.Graphics.DrawSystem;

public class DrawSystemProvince : DrawSystemBase
{
    private int _selectedProvinceId = -1;

    public DrawSystemProvince(
        GraphicsDevice graphicsDevice,
        Texture2D provinceTexture,
        Texture2D uiBackground,
        SpriteFont font,
        UI ui,
        IEventEngine eventEngine,
        IGameEngine gameEngine)
        : base(graphicsDevice, provinceTexture, uiBackground, font, ui, eventEngine, gameEngine)
    {
        // Register for province click events
        eventEngine.RegisterProvinceClickHandler(OnProvinceClick);
    }

    private void OnProvinceClick(int provinceId)
    {
        _selectedProvinceId = provinceId;
    }

    public override void Draw(GameTime gameTime)
    {
        DrawBackground();

        if (_selectedProvinceId >= 0)
        {
            var province = GameEngine.GetProvinceById(_selectedProvinceId);
            var population = GameEngine.GetPopulationById(_selectedProvinceId);

            if (province != null)
            {
                DrawProvinceDetails(province, population);
                DrawResources(province);
                DrawBuildings(province);

                if (population != null)
                {
                    DrawPopulationDetails(population);
                }
            }
        }
        else
        {
            DrawText(
                "No province selected. Click on a province to view details.",
                new Vector2(20, 20),
                Color.White
            );
        }
    }

    private void DrawProvinceDetails(Province province, Population population)
    {
        var panelRect = new Rectangle(20, 20, 400, 120);
        DrawPanel(panelRect, Color.DarkSlateGray, 0.9f);

        SpriteBatch.Begin();

        // Province name and ID
        SpriteBatch.DrawString(
            Font,
            $"{province.Name} (ID: {province.Id})",
            new Vector2(30, 30),
            Color.White
        );

        // Population
        if (population != null)
        {
            SpriteBatch.DrawString(
                Font,
                $"Population: {population.Size}",
                new Vector2(30, 60),
                Color.White
            );
        }

        // Buildings count
        SpriteBatch.DrawString(
            Font,
            $"Buildings: {province.Buildings.Count}",
            new Vector2(30, 90),
            Color.White
        );

        // Back button
        var backButton = new Rectangle(340, 30, 60, 30);
        SpriteBatch.Draw(UiBackground, backButton, Color.DarkRed);

        var backText = "Back";
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

    private void DrawResources(Province province)
    {
        var panelRect = new Rectangle(20, 160, 400, 320);
        DrawPanel(panelRect, Color.DarkSlateBlue, 0.9f);

        SpriteBatch.Begin();
        SpriteBatch.DrawString(
            Font,
            "Resources",
            new Vector2(30, 170),
            Color.White
        );
        SpriteBatch.End();

        int y = 210;
        foreach (var resource in province.Resources)
        {
            // Draw resource name
            DrawText(
                resource.Key,
                new Vector2(40, y),
                Color.White
            );

            // Draw resource amount
            DrawText(
                resource.Value.ToString("F1"),
                new Vector2(180, y),
                Color.LightGreen
            );

            // Draw production/consumption indicators
            float production = 0;
            float consumption = 0;

            foreach (var building in province.Buildings)
            {
                if (building.IsActive)
                {
                    if (building.ProductionRates.TryGetValue(resource.Key, out var prodRate))
                    {
                        production += prodRate * building.CurrentWorkers / building.WorkersCapacity;
                    }

                    if (building.ConsumptionRates.TryGetValue(resource.Key, out var consRate))
                    {
                        consumption += consRate * building.CurrentWorkers / building.WorkersCapacity;
                    }
                }
            }

            var netProduction = production - consumption;
            string trendText;
            Color trendColor;

            if (netProduction > 0.1f)
            {
                trendText = $"+{netProduction:F1}";
                trendColor = Color.LightGreen;
            }
            else if (netProduction < -0.1f)
            {
                trendText = netProduction.ToString("F1");
                trendColor = Color.LightCoral;
            }
            else
            {
                trendText = "±0.0";
                trendColor = Color.White;
            }

            DrawText(trendText, new Vector2(280, y), trendColor);

            y += 30;
        }
    }

    private void DrawBuildings(Province province)
    {
        var panelRect = new Rectangle(450, 20, 550, 460);
        DrawPanel(panelRect, Color.DarkSlateGray, 0.9f);

        SpriteBatch.Begin();
        SpriteBatch.DrawString(
            Font,
            "Buildings",
            new Vector2(470, 30),
            Color.White
        );

        // Add building button
        var addButton = new Rectangle(900, 30, 80, 30);
        SpriteBatch.Draw(UiBackground, addButton, Color.DarkGreen);

        var addText = "Add New";
        var textSize = Font.MeasureString(addText);
        SpriteBatch.DrawString(
            Font,
            addText,
            new Vector2(
                addButton.X + (addButton.Width - textSize.X) / 2,
                addButton.Y + (addButton.Height - textSize.Y) / 2
            ),
            Color.White
        );

        SpriteBatch.End();

        int y = 70;
        foreach (var building in province.Buildings)
        {
            var buildingRect = new Rectangle(460, y, 530, 90);
            DrawPanel(buildingRect, Color.DarkSlateBlue, 0.8f);

            SpriteBatch.Begin();

            // Building name and type
            SpriteBatch.DrawString(
                Font,
                $"{building.Name} (Level {building.Level})",
                new Vector2(470, y + 10),
                Color.White
            );

            // Workers
            SpriteBatch.DrawString(
                Font,
                $"Workers: {building.CurrentWorkers}/{building.WorkersCapacity}",
                new Vector2(470, y + 35),
                building.IsActive ? Color.White : Color.Gray
            );

            // Status
            SpriteBatch.DrawString(
                Font,
                building.IsActive ? "Active" : "Inactive",
                new Vector2(470, y + 60),
                building.IsActive ? Color.LightGreen : Color.LightCoral
            );

            // Upgrade button
            var upgradeButton = new Rectangle(820, y + 10, 60, 25);
            SpriteBatch.Draw(UiBackground, upgradeButton, Color.DarkBlue);

            var upgradeText = "Upgrade";
            textSize = Font.MeasureString(upgradeText);
            SpriteBatch.DrawString(
                Font,
                upgradeText,
                new Vector2(
                    upgradeButton.X + (upgradeButton.Width - textSize.X) / 2,
                    upgradeButton.Y + (upgradeButton.Height - textSize.Y) / 2
                ),
                Color.White
            );

            // Delete button
            var deleteButton = new Rectangle(890, y + 10, 60, 25);
            SpriteBatch.Draw(UiBackground, deleteButton, Color.DarkRed);

            var deleteText = "Delete";
            textSize = Font.MeasureString(deleteText);
            SpriteBatch.DrawString(
                Font,
                deleteText,
                new Vector2(
                    deleteButton.X + (deleteButton.Width - textSize.X) / 2,
                    deleteButton.Y + (deleteButton.Height - textSize.Y) / 2
                ),
                Color.White
            );

            SpriteBatch.End();

            // Production and consumption rates
            if (building.ProductionRates.Any())
            {
                SpriteBatch.Begin();
                SpriteBatch.DrawString(
                    Font,
                    "Produces:",
                    new Vector2(640, y + 35),
                    building.IsActive ? Color.White : Color.Gray
                );
                SpriteBatch.End();

                int px = 720;
                foreach (var prod in building.ProductionRates)
                {
                    DrawText(
                        $"{prod.Key}: +{prod.Value:F1}",
                        new Vector2(px, y + 35),
                        building.IsActive ? Color.LightGreen : Color.Gray
                    );
                    px += 140;
                }
            }

            if (building.ConsumptionRates.Any())
            {
                SpriteBatch.Begin();
                SpriteBatch.DrawString(
                    Font,
                    "Consumes:",
                    new Vector2(640, y + 60),
                    building.IsActive ? Color.White : Color.Gray
                );
                SpriteBatch.End();

                int cx = 720;
                foreach (var cons in building.ConsumptionRates)
                {
                    DrawText(
                        $"{cons.Key}: -{cons.Value:F1}",
                        new Vector2(cx, y + 60),
                        building.IsActive ? Color.LightCoral : Color.Gray
                    );
                    cx += 140;
                }
            }

            y += 100;
        }
    }

    private void DrawPopulationDetails(Population population)
    {
        var panelRect = new Rectangle(450, 500, 550, 200);
        DrawPanel(panelRect, Color.DarkSlateGray, 0.9f);

        SpriteBatch.Begin();
        SpriteBatch.DrawString(
            Font,
            "Population Needs",
            new Vector2(470, 510),
            Color.White
        );

        SpriteBatch.DrawString(
            Font,
            $"Wealth: {population.Wealth:F0}",
            new Vector2(700, 510),
            Color.White
        );
        SpriteBatch.End();

        int y = 550;
        foreach (var need in population.Needs)
        {
            // Need name
            DrawText(
                need.Key,
                new Vector2(470, y),
                Color.White
            );

            // Per person need
            DrawText(
                $"{need.Value:F2} per person",
                new Vector2(570, y),
                Color.White
            );

            // Total need
            DrawText(
                $"Total: {need.Value * population.Size:F0}",
                new Vector2(700, y),
                Color.White
            );

            // Satisfaction
            float satisfaction = 0;
            if (population.Satisfaction.TryGetValue(need.Key, out var sat))
            {
                satisfaction = sat;
            }

            DrawText(
                $"Satisfaction: {satisfaction * 100:F0}%",
                new Vector2(830, y),
                GetSatisfactionColor(satisfaction)
            );

            y += 30;
        }
    }

    private Color GetSatisfactionColor(float satisfaction)
    {
        if (satisfaction < 0.3f) return Color.Red;
        if (satisfaction < 0.7f) return Color.Yellow;
        return Color.LightGreen;
    }
}