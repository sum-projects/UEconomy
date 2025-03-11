using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine;
using UEconomy.Engine.Interfaces;
using UEconomy.Game;

namespace UEconomy.Graphics.DrawSystem;

public class DrawSystemGame : DrawSystemBase
{
        public DrawSystemGame(
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

        public override void Draw(GameTime gameTime)
        {
            DrawBackground();
            DrawProvinces();
            DrawTopPanel();
            DrawResourcePanel();
        }

        private void DrawProvinces()
        {
            SpriteBatch.Begin();

            foreach (var province in GameEngine.GetProvinces())
            {
                var rect = new Rectangle(
                    (int)province.Position.X,
                    (int)province.Position.Y,
                    Ui.CellSize,
                    Ui.CellSize
                );

                // Base province color based on prosperity
                var population = GameEngine.GetPopulationById(province.Id);
                var prosperityFactor = population != null ?
                    MathHelper.Clamp((float)population.Size / 1000f, 0.2f, 1.0f) : 0.5f;

                var color = new Color(
                    (byte)(200 * prosperityFactor),
                    (byte)(220 * prosperityFactor),
                    (byte)(180 * prosperityFactor)
                );

                SpriteBatch.Draw(ProvinceTexture, rect, color);

                // Draw province name
                var namePosition = new Vector2(
                    province.Position.X + 5,
                    province.Position.Y + 5
                );
                SpriteBatch.DrawString(Font, province.Name, namePosition, Color.Black);

                // Draw number of buildings
                var buildingText = $"Buildings: {province.Buildings.Count}";
                var buildingPosition = new Vector2(
                    province.Position.X + 5,
                    province.Position.Y + 25
                );
                SpriteBatch.DrawString(Font, buildingText, buildingPosition, Color.Black);

                // Draw population
                if (population != null)
                {
                    var popText = $"Pop: {population.Size}";
                    var popPosition = new Vector2(
                        province.Position.X + 5,
                        province.Position.Y + 45
                    );
                    SpriteBatch.DrawString(Font, popText, popPosition, Color.Black);
                }
            }

            SpriteBatch.End();
        }

        private void DrawTopPanel()
        {
            var stats = GameEngine.GetStats();

            // Draw top panel background
            var topPanelRect = new Rectangle(0, 0, Ui.ScreenWidth, 60);
            DrawPanel(topPanelRect, Color.DarkSlateGray);

            // Draw key statistics
            SpriteBatch.Begin();

            // Total population
            var totalPop = stats.GlobalStats.TryGetValue("Total Population", out var popValue) ?
                (int)popValue : 0;
            SpriteBatch.DrawString(
                Font,
                $"Population: {totalPop}",
                new Vector2(20, 20),
                Color.White
            );

            // Employed workers
            var employedWorkers = stats.GlobalStats.TryGetValue("Employed Workers", out var employedValue) ?
                (int)employedValue : 0;
            SpriteBatch.DrawString(
                Font,
                $"Employed: {employedWorkers}",
                new Vector2(220, 20),
                Color.White
            );

            // Available workers
            var availableWorkers = stats.GlobalStats.TryGetValue("Available Workers", out var availableValue) ?
                (int)availableValue : 0;
            SpriteBatch.DrawString(
                Font,
                $"Unemployed: {availableWorkers}",
                new Vector2(420, 20),
                Color.White
            );

            // Total wealth
            var totalWealth = stats.GlobalStats.TryGetValue("Total Wealth", out var wealthValue) ?
                (int)wealthValue : 0;
            SpriteBatch.DrawString(
                Font,
                $"Wealth: {totalWealth}",
                new Vector2(620, 20),
                Color.White
            );

            SpriteBatch.End();
        }

        private void DrawResourcePanel()
        {
            var market = GameEngine.GetMarket();
            var resources = market.GetAllResources();

            // Draw resource panel background
            var panelRect = new Rectangle(
                Ui.ScreenWidth - 250,
                70,
                240,
                70 + resources.Length * 30
            );
            DrawPanel(panelRect, Color.DarkSlateBlue, 0.9f);

            // Draw panel title
            DrawText(
                "Resource Prices",
                new Vector2(Ui.ScreenWidth - 230, 80),
                Color.White
            );

            // Draw resources and prices
            for (int i = 0; i < resources.Length; i++)
            {
                var resource = resources[i];
                var price = market.GetPrice(resource);
                var ratio = market.GetSupplyDemandRatio(resource);

                // Determine price trend color
                Color priceColor;
                if (ratio < 0.8f) priceColor = Color.LightGreen;      // Demand > Supply, prices rising
                else if (ratio > 1.2f) priceColor = Color.LightCoral; // Supply > Demand, prices falling
                else priceColor = Color.White;                        // Balanced

                var y = 110 + i * 30;

                // Draw resource name
                DrawText(
                    resource,
                    new Vector2(Ui.ScreenWidth - 230, y),
                    Color.White
                );

                // Draw price
                DrawText(
                    $"{price:F1}",
                    new Vector2(Ui.ScreenWidth - 100, y),
                    priceColor
                );
            }
        }
    }