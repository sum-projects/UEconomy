using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UEconomy.Engine;

namespace UEconomy;

public class MainGame : Game
{
    private GameEngine _gameEngine;

    public MainGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _gameEngine = new GameEngine(new GraphicsDeviceManager(this));
    }

    protected override void Initialize()
    {
        _gameEngine.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _gameEngine.LoadContent(Content.Load<SpriteFont>("Arial"));

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (
            GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape)
        )
        {
            Exit();
        }


        _gameEngine.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(40, 45, 55));

        _gameEngine.Draw(gameTime);

        base.Draw(gameTime);
    }

    // protected void OnClientSizeChanged(object sender, EventArgs e)
    // {
    //     // Zaktualizuj skalowanie UI po zmianie rozmiaru okna
    //     CalculateUIScaling();
    //
    //     // Ustaw nowe pozycje prowincji
    //     for (int x = 0; x < _gridWidth; x++)
    //     {
    //         for (int y = 0; y < _gridHeight; y++)
    //         {
    //             int index = y * _gridWidth + x;
    //             if (index < _provinces.Count)
    //             {
    //                 _provinces[index].Position = new Vector2(x * _cellSize, y * _cellSize);
    //             }
    //         }
    //     }
    // }
}