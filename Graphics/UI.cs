using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UEconomy.Graphics;

public class UI
{
    public int CellSize { get; private set; }
    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }
    public int ScreenWidth { get; private set; }
    public int ScreenHeight { get; private set; }

    private readonly Dictionary<string, Rectangle> _buttons = new();

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        ScreenWidth = graphicsDevice.Viewport.Width;
        ScreenHeight = graphicsDevice.Viewport.Height;

        // Calculate grid size based on screen size
        CalculateGridParameters();

        // Initialize UI elements
        InitializeButtons();
    }

    private void CalculateGridParameters()
    {
        // Determine a reasonable cell size based on screen size
        CellSize = ScreenWidth / 15;

        // Calculate grid size to fit the screen
        GridWidth = ScreenWidth / CellSize;
        GridHeight = ScreenHeight / CellSize;

        // Ensure minimum grid size
        if (GridWidth < 8) GridWidth = 8;
        if (GridHeight < 5) GridHeight = 5;
    }

    private void InitializeButtons()
    {
        // Main menu buttons
        _buttons["Stats"] = new Rectangle(ScreenWidth - 120, 10, 100, 40);
        _buttons["NewGame"] = new Rectangle(10, 10, 100, 40);
        _buttons["Options"] = new Rectangle(120, 10, 100, 40);
    }

    public string GetButtonAtPosition(Point position)
    {
        return (from button in _buttons where button.Value.Contains(position) select button.Key).FirstOrDefault();
    }

    public void DrawButtons(SpriteBatch spriteBatch, Texture2D texture, SpriteFont font)
    {
        foreach (var (text, value) in _buttons)
        {
            // Draw button background
            spriteBatch.Draw(texture, value, Color.DarkSlateGray);

            // Draw button text
            var textSize = font.MeasureString(text);
            var textPosition = new Vector2(
                value.X + (value.Width - textSize.X) / 2,
                value.Y + (value.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(font, text, textPosition, Color.White);
        }
    }

    public void ResizeUI(GraphicsDevice graphicsDevice)
    {
        // Update screen size
        ScreenWidth = graphicsDevice.Viewport.Width;
        ScreenHeight = graphicsDevice.Viewport.Height;

        // Recalculate grid parameters
        CalculateGridParameters();

        // Update button positions
        _buttons["Stats"] = new Rectangle(ScreenWidth - 120, 10, 100, 40);
        _buttons["NewGame"] = new Rectangle(10, 10, 100, 40);
        _buttons["Options"] = new Rectangle(120, 10, 100, 40);
    }
}