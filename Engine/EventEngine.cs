using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UEconomy.Engine.Interfaces;
using UEconomy.Graphics;

namespace UEconomy.Engine;

public class EventEngine : IEventEngine
{
    private readonly IGameEngine _gameEngine;
    private readonly UI _ui;

    private MouseState _currentMouseState;
    private MouseState _previousMouseState;
    private KeyboardState _currentKeyboardState;
    private KeyboardState _previousKeyboardState;

    private readonly List<ProvinceClickHandler> _provinceClickHandlers = new();
    private readonly List<BuildingClickHandler> _buildingClickHandlers = new();
    private readonly List<ButtonClickHandler> _buttonClickHandlers = new();

    public EventEngine(IGameEngine gameEngine, UI ui)
    {
        _gameEngine = gameEngine;
        _ui = ui;
    }

    public void Update()
    {
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        HandleMouseInput();
        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        if (_currentMouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            // Mouse click detected
            var mousePosition = new Point(_currentMouseState.X, _currentMouseState.Y);

            // Check for UI element clicks first
            var clickedButton = _ui.GetButtonAtPosition(mousePosition);
            if (clickedButton != null)
            {
                NotifyButtonClickHandlers(clickedButton);
                return;
            }

            // Check if a province was clicked
            var clickedProvinceId = GetProvinceIdAtPosition(mousePosition);
            if (clickedProvinceId >= 0)
            {
                NotifyProvinceClickHandlers(clickedProvinceId);

                // Check if a building within the province was clicked
                var clickedBuildingId = GetBuildingIdAtPosition(clickedProvinceId, mousePosition);
                if (clickedBuildingId >= 0)
                {
                    NotifyBuildingClickHandlers(clickedBuildingId);
                }
            }
        }
    }

    private void HandleKeyboardInput()
    {
        // Add keyboard handling logic here
    }

    private int GetProvinceIdAtPosition(Point position)
    {
        // Check each province to see if the position is within its bounds
        foreach (
            var province in from province in _gameEngine.GetProvinces()
            let rect = new Rectangle(
                (int)province.Position.X,
                (int)province.Position.Y,
                _ui.CellSize,
                _ui.CellSize
            )
            where rect.Contains(position)
            select province
        )
        {
            return province.Id;
        }

        return -1;
    }

    private int GetBuildingIdAtPosition(int provinceId, Point position)
    {
        // This would need detailed UI positioning information for buildings
        // For now just return -1 as a placeholder
        return -1;
    }

    private void NotifyProvinceClickHandlers(int provinceId)
    {
        foreach (var handler in _provinceClickHandlers)
        {
            handler(provinceId);
        }
    }

    private void NotifyBuildingClickHandlers(int buildingId)
    {
        foreach (var handler in _buildingClickHandlers)
        {
            handler(buildingId);
        }
    }

    private void NotifyButtonClickHandlers(string buttonId)
    {
        foreach (var handler in _buttonClickHandlers)
        {
            handler(buttonId);
        }
    }

    public void RegisterProvinceClickHandler(ProvinceClickHandler handler)
    {
        _provinceClickHandlers.Add(handler);
    }

    public void RegisterBuildingClickHandler(BuildingClickHandler handler)
    {
        _buildingClickHandlers.Add(handler);
    }

    public void RegisterButtonClickHandler(ButtonClickHandler handler)
    {
        _buttonClickHandlers.Add(handler);
    }
}