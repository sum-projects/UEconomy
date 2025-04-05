namespace UEconomy;

public class Pop
{
    private int id;
    private int food = 100;
    private int cloth = 100;
    private int furniture = 50;
    private readonly Random random = new();

    private const double FoodProductionBase = 1.1;
    private const double ClothProductionBase = 0.02;
    private const double FurnitureProductionBase = 0.01;
    private const double WoodProductionBase = 0.1;

    public Pop(int id)
    {
        this.id = id;
    }

    public (Stuff<FoodType>, Stuff<StuffType>, Stuff<StuffType>, Stuff<ResourceType>) ProduceSubsistence()
    {
        var foodType = (FoodType)random.Next(0, Enum.GetValues(typeof(FoodType)).Length);
        const int foodProduced = (int)(FoodProductionBase * 100);
        const int foodConsumption = 90;
        var foodSurplus = Math.Max(0, foodProduced - foodConsumption);
        food += foodProduced - foodSurplus - foodConsumption;

        const int clothProduced = (int)(ClothProductionBase * 100);
        const int clothConsumption = 2;
        var clothSurplus = Math.Max(0, clothProduced - clothConsumption);
        cloth += clothProduced - clothSurplus - clothConsumption;

        const int furnitureProduced = (int)(FurnitureProductionBase * 100);
        const int furnitureConsumption = 1;
        var furnitureSurplus = Math.Max(0, furnitureProduced - furnitureConsumption);
        furniture += furnitureProduced - furnitureSurplus - furnitureConsumption;

        const int woodProduced = (int)(WoodProductionBase * 100);
        const int woodConsumption = furnitureProduced * 2;
        var woodSurplus = Math.Max(0, woodProduced - woodConsumption);

        return (
            new Stuff<FoodType>(foodType, foodSurplus),
            new Stuff<StuffType>(StuffType.Cloth, clothSurplus),
            new Stuff<StuffType>(StuffType.Furniture, furnitureSurplus),
            new Stuff<ResourceType>(ResourceType.Wood, woodSurplus)
        );
    }

    public void Consume()
    {
        food = Math.Max(0, food - 10);
        cloth = Math.Max(0, cloth - 1);
        furniture = Math.Max(0, furniture - 1);
    }

    public bool IsHealthy()
    {
        return food > 0 && cloth > 0 && furniture > 0;
    }

    public int GetFoodLevel() => food;
    public int GetClothLevel() => cloth;
    public int GetFurnitureLevel() => furniture;
}