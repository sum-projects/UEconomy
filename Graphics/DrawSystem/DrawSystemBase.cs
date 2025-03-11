using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine;
using UEconomy.Engine.Interfaces;

namespace UEconomy.Graphics.DrawSystem;

public abstract class DrawSystemBase
    {
        protected readonly SpriteBatch SpriteBatch;
        protected readonly Texture2D ProvinceTexture;
        protected readonly Texture2D UiBackground;
        protected readonly SpriteFont Font;
        protected readonly UI Ui;
        protected readonly IEventEngine EventEngine;
        protected readonly IGameEngine GameEngine;

        protected DrawSystemBase(
            GraphicsDevice graphicsDevice,
            Texture2D provinceTexture,
            Texture2D uiBackground,
            SpriteFont font,
            UI ui,
            IEventEngine eventEngine,
            IGameEngine gameEngine)
        {
            SpriteBatch = new SpriteBatch(graphicsDevice);
            ProvinceTexture = provinceTexture;
            UiBackground = uiBackground;
            Font = font;
            Ui = ui;
            EventEngine = eventEngine;
            GameEngine = gameEngine;
        }

        public abstract void Draw(GameTime gameTime);

        protected void DrawBackground()
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(
                UiBackground,
                new Rectangle(0, 0, Ui.ScreenWidth, Ui.ScreenHeight),
                new Color(40, 45, 55));
            SpriteBatch.End();
        }

        protected void DrawText(string text, Vector2 position, Color color)
        {
            SpriteBatch.Begin();
            SpriteBatch.DrawString(Font, text, position, color);
            SpriteBatch.End();
        }

        protected void DrawProgressBar(
            Rectangle bounds,
            float fillPercentage,
            Color backgroundColor,
            Color fillColor,
            string label = null)
        {
            SpriteBatch.Begin();

            // Draw background
            SpriteBatch.Draw(ProvinceTexture, bounds, backgroundColor);

            // Draw fill
            var fillWidth = (int)(bounds.Width * fillPercentage);
            if (fillWidth > 0)
            {
                var fillRect = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);
                SpriteBatch.Draw(ProvinceTexture, fillRect, fillColor);
            }

            // Draw label if provided
            if (!string.IsNullOrEmpty(label))
            {
                Vector2 textSize = Font.MeasureString(label);
                Vector2 textPosition = new Vector2(
                    bounds.X + (bounds.Width - textSize.X) / 2,
                    bounds.Y + (bounds.Height - textSize.Y) / 2
                );
                SpriteBatch.DrawString(Font, label, textPosition, Color.White);
            }

            SpriteBatch.End();
        }

        protected void DrawPanel(Rectangle bounds, Color color, float opacity = 0.8f)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(UiBackground, bounds, color * opacity);
            SpriteBatch.End();
        }
    }