using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UEconomy.Game;

namespace UEconomy.Engine.Interfaces;

public interface IGameEngine
{
    void Initialize(int cellSize, int gridWidth, int gridHeight);
    void Update(GameTime gameTime);
    List<Province> GetProvinces();
    List<Population> GetPopulations();
    Market GetMarket();
    Stats GetStats();
    Province GetProvinceById(int id);
    Population GetPopulationById(int id);
}