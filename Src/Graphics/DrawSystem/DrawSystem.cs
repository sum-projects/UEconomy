using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UEconomy.Engine;

namespace UEconomy.Graphics.DrawSystem;

public abstract class DrawSystem
{
    protected readonly Texture2D ProvinceTexture;
    public Texture2D UiBackground { get; protected set; }
    protected readonly SpriteFont Font;

    protected readonly UI Ui;
    protected readonly EventEngine EventEngine;

    protected readonly Rectangle[] ResourceButtonRects = new Rectangle[6]; // Zakładając, że mamy 6 zasobów

    protected DrawSystem(Texture2D provinceTexture, Texture2D uiBackground, SpriteFont font, UI ui, EventEngine eventEngine)
    {
        ProvinceTexture = provinceTexture;
        UiBackground = uiBackground;
        Font = font;
        Ui = ui;
        EventEngine = eventEngine;
    }

    protected void DrawPanelBorder(SpriteBatch spriteBatch, int x, int y, int width, int height, Color color)
    {
        const int borderThickness = 2;

        // Górna krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y, width, borderThickness),
            color
        );

        // Lewa krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y, borderThickness, height),
            color
        );

        // Prawa krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x + width - borderThickness, y, borderThickness, height),
            color
        );

        // Dolna krawędź
        spriteBatch.Draw(
            ProvinceTexture,
            new Rectangle(x, y + height - borderThickness, width, borderThickness),
            color
        );
    }

    protected void DrawHeaderBar(SpriteBatch spriteBatch, int x, int y, int width, string title, Color textColor)
    {
        var headerHeight = (int)(30 * Ui.FontScale);

        // Tło nagłówka
        spriteBatch.Draw(
            UiBackground,
            new Rectangle(x, y, width, headerHeight),
            new Color(50, 60, 80, 180)
        );

        // Tekst nagłówka
        spriteBatch.DrawString(
            Font,
            title,
            new Vector2(x + width / 2, y + headerHeight / 2),
            textColor,
            0f,
            new Vector2(Font.MeasureString(title).X / 2, Font.MeasureString(title).Y / 2),
            Ui.FontScale * 1.2f,
            SpriteEffects.None,
            0f
        );
    }

    protected Color GetResourceColor(string resourceName)
    {
        return resourceName switch
        {
            "Zboże" => new Color(0.8f, 0.9f, 0.2f),
            "Drewno" => new Color(0.6f, 0.4f, 0.2f),
            "Żelazo" => new Color(0.7f, 0.7f, 0.7f),
            "Węgiel" => new Color(0.3f, 0.3f, 0.3f),
            "Meble" => new Color(0.8f, 0.6f, 0.4f),
            "Ubrania" => new Color(0.5f, 0.5f, 0.9f),
            _ => Color.White
        };
    }

    protected void DrawStatsSection(SpriteBatch spriteBatch, int panelX, ref int yPos, int panelWidth, string title,
        int margin,
        Action drawContent)
    {
        // Tło nagłówka sekcji
        spriteBatch.Draw(
            UiBackground,
            new Rectangle(panelX + margin, yPos, panelWidth - margin * 2, (int)(25 * Ui.FontScale)),
            new Color(60, 80, 100, 150)
        );

        // Tekst nagłówka
        spriteBatch.DrawString(
            Font,
            title,
            new Vector2(panelX + margin + 5, yPos + 2),
            Color.White,
            0f,
            Vector2.Zero,
            Ui.FontScale,
            SpriteEffects.None,
            0f
        );

        yPos += (int)(30 * Ui.FontScale);

        // Rysuj zawartość sekcji
        drawContent();

        // Dodaj odstęp na końcu sekcji
        yPos += (int)(10 * Ui.FontScale);
    }
}