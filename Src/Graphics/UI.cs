using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UEconomy.Graphics;

public class UI
{
    public int GridWidth { get; private set; } = 8;
    public int GridHeight { get; private set; } = 6;
    public int CellSize { get; private set; } = 80;

    public int GridMarginX { get; private set; }
    public int GridMarginY { get; private set; }

    public int ScreenWidth { get; private set; }
    public int ScreenHeight { get; private set; }

    public int PanelWidth { get; private set; }
    public int PanelHeight { get; private set; }
    public int PanelX { get; private set; }
    public int PanelY { get; private set; }

    public int DetailsPanelX { get; private set; }
    public int DetailsPanelY { get; private set; }
    public int DetailsPanelWidth = 250;
    public int DetailsPanelMargin = 15;


    public int ChartMargin { get; private set; } = 15;

    public float FontScale { get; private set; } = 1.0f;

    public UI()
    {

    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        ScreenWidth = graphicsDevice.Viewport.Width;
        ScreenHeight = graphicsDevice.Viewport.Height;

        CalculateUIScaling();

        PanelX = GridMarginX + GridWidth * CellSize + 20;
        PanelY = GridMarginY;
        PanelWidth = (int)(ScreenWidth * 0.24f);
        PanelHeight = GridHeight * CellSize;

        DetailsPanelY = PanelX + PanelWidth + 10;
        DetailsPanelY = PanelY;
    }

    private void CalculateUIScaling()
    {
        // Oblicz liczbę prowincji w poziomie i pionie dla wypełnienia ekranu
        // Pozostawiamy miejsce na panel po prawej stronie
        var panelWidth = (int)(ScreenWidth * 0.25f); // 25% szerokości ekranu na panel statystyk
        var availableWidth = ScreenWidth - panelWidth;

        // Oblicz optymalny rozmiar komórki i liczbę komórek
        CellSize = Math.Min(availableWidth / 10, ScreenHeight / 8); // Maksymalnie 10x8 prowincji
        GridWidth = availableWidth / CellSize;
        GridHeight = ScreenHeight / CellSize;

        // Ustaw marginesy, aby siatka była wycentrowana
        GridMarginX = (availableWidth - (GridWidth * CellSize)) / 2;
        GridMarginY = (ScreenHeight - (GridHeight * CellSize)) / 2;

        // Skaluj rozmiar czcionki w zależności od rozdzielczości
        FontScale = ScreenHeight / 1080f; // 1.0 dla 1080p, proporcjonalnie dla innych rozdzielczości
    }
}