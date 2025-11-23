using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace RenbokoEngine.Input
{
    /// <summary>
    /// Handles mouse input.
    /// </summary>
    public class MouseDevice : IInputDevice
    {
        private MouseState _currentState;
        private MouseState _previousState;

        public Point Position { get; private set; }
        public Point Delta { get; private set; }
        public int ScrollWheelValue { get; private set; }
        public int ScrollDelta { get; private set; }

        public void Update()
        {
            _previousState = _currentState;
            _currentState = Mouse.GetState();

            Position = _currentState.Position;
            Delta = _currentState.Position - _previousState.Position;

            ScrollWheelValue = _currentState.ScrollWheelValue;
            ScrollDelta = _currentState.ScrollWheelValue - _previousState.ScrollWheelValue;
        }

        public void PostUpdate()
        {
            // Mouse doesn't need transient resets, but could be used for one-frame events
        }

        public bool GetButton(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _currentState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _currentState.MiddleButton == ButtonState.Pressed,
                MouseButton.XButton1 => _currentState.XButton1 == ButtonState.Pressed,
                MouseButton.XButton2 => _currentState.XButton2 == ButtonState.Pressed,
                _ => false
            };
        }

        public bool GetButtonDown(MouseButton button)
        {
            return GetButton(button) && !WasButtonDown(button);
        }

        public bool GetButtonUp(MouseButton button)
        {
            return !GetButton(button) && WasButtonDown(button);
        }

        private bool WasButtonDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _previousState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _previousState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _previousState.MiddleButton == ButtonState.Pressed,
                MouseButton.XButton1 => _previousState.XButton1 == ButtonState.Pressed,
                MouseButton.XButton2 => _previousState.XButton2 == ButtonState.Pressed,
                _ => false
            };
        }
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }
}
