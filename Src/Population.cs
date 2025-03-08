using System.Collections.Generic;

namespace UEconomy;

public class Population
{
    public int Size { get; set; }
    public Dictionary<string, float> Needs { get; set; } = new();
    public float Wealth { get; set; }
    public int ProvinceId { get; set; }
}