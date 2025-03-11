using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine;
using UEconomy.Game;

namespace UEconomy.Graphics.DrawSystem;

public class DrawSystemGame : DrawSystem
{

    public DrawSystemGame(Texture2D provinceTexture, Texture2D uiBackground, SpriteFont font, UI ui, EventEngine eventEngine) : base(provinceTexture, uiBackground, font, ui, eventEngine)
    {
    }

    public void DrawSelectedBorder(SpriteBatch spriteBatch, int x, int y, int width, int height, float totalGameTime)
    {
        // Animowany kolor obramowania
        var pulse = (float)Math.Sin(totalGameTime * 5) * 0.5f + 0.5f;
        var borderColor = new Color(
            1.0f,
            1.0f - pulse * 0.7f,
            0.0f
        );

        const int borderThickness = 3;

        // Górna krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x - borderThickness, y - borderThickness, width + borderThickness * 2, borderThickness),
            borderColor
        );

        // Lewa krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x - borderThickness, y - borderThickness, borderThickness, height + borderThickness * 2),
            borderColor
        );

        // Prawa krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x + width, y - borderThickness, borderThickness, height + borderThickness * 2),
            borderColor
        );

        // Dolna krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x - borderThickness, y + height, width + borderThickness * 2, borderThickness),
            borderColor
        );
    }

    public void DrawGradientCell(SpriteBatch spriteBatch, int x, int y, int width, int height, Color baseColor)
    {
        // Tworzenie efektu gradientu - ciemniejszy na dole
        var bottomColor = new Color(
            (int)(baseColor.R * 0.7f),
            (int)(baseColor.G * 0.7f),
            (int)(baseColor.B * 0.7f)
        );

        // Rysuj prostokąt górny
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y, width, height / 2),
            baseColor
        );

        // Rysuj prostokąt dolny
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y + height / 2, width, height - height / 2),
            bottomColor
        );

        // Dodaj ramkę
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y, width, 1),
            Color.Black * 0.3f
        );
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y, 1, height),
            Color.Black * 0.3f
        );
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x + width - 1, y, 1, height),
            Color.Black * 0.3f
        );
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y + height - 1, width, 1),
            Color.Black * 0.3f
        );
    }

    public Color CalculateProvinceColor(Province province)
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
}