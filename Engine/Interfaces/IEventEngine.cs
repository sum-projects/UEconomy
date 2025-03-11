namespace UEconomy.Engine.Interfaces;


public delegate void ProvinceClickHandler(int provinceId);
public delegate void BuildingClickHandler(int buildingId);
public delegate void ButtonClickHandler(string buttonId);

public interface IEventEngine
{
    void Update();
    void RegisterProvinceClickHandler(ProvinceClickHandler handler);
    void RegisterBuildingClickHandler(BuildingClickHandler handler);
    void RegisterButtonClickHandler(ButtonClickHandler handler);
}