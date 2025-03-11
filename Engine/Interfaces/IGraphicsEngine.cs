using Microsoft.Xna.Framework;

namespace UEconomy.Engine.Interfaces;

public enum GameView
{
    GameOverview,
    ProvinceDetail,
    Statistics
}

public interface IGraphicsEngine
{
    void Update(GameTime gameTime);
    void Draw(GameTime gameTime);
    void SwitchView(GameView view);
}