using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace RenbokoEngine.Input
{
    /// <summary>
    /// Handles keyboard input.
    /// </summary>
    public class KeyboardDevice : IInputDevice
    {
        private KeyboardState _currentState;
        private KeyboardState _previousState;

        private readonly HashSet<Keys> _keysDown = new HashSet<Keys>();
        private readonly HashSet<Keys> _keysPressed = new HashSet<Keys>();
        private readonly HashSet<Keys> _keysReleased = new HashSet<Keys>();

        public void Update()
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();

            _keysPressed.Clear();
            _keysReleased.Clear();

            foreach (var key in _currentState.GetPressedKeys())
            {
                if (!_keysDown.Contains(key))
                {
                    _keysPressed.Add(key);
                    _keysDown.Add(key);
                }
            }

            foreach (var key in _keysDown)
            {
                if (!_currentState.IsKeyDown(key))
                {
                    _keysReleased.Add(key);
                }
            }

            foreach (var key in _keysReleased)
                _keysDown.Remove(key);
        }

        public void PostUpdate()
        {
            _keysPressed.Clear();
            _keysReleased.Clear();
        }

        public bool GetKey(Keys key) => _keysDown.Contains(key);
        public bool GetKeyDown(Keys key) => _keysPressed.Contains(key);
        public bool GetKeyUp(Keys key) => _keysReleased.Contains(key);
    }
}
