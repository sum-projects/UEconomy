namespace UEconomy.Engine.Pops;

public enum SocialClass
{
    LowerClass,
    MiddleClass,
    UpperClass
}

public class NeedSatisfaction
{
    public string NeedType { get; set; }
    public double SatisfactionLevel { get; set; }
    public double Priority { get; set; }
}