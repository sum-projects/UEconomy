using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace UEconomy;

public class Province
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public Dictionary<string, float> Resources { get; set; } = new();
}