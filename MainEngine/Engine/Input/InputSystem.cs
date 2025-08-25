using System.Collections.Generic;

namespace RenyulEngine.Input
{
    /// <summary>
    /// Centralized input system that manages all input devices.
    /// </summary>
    public class InputSystem : Core.IService
    {
        private readonly List<IInputDevice> _devices = new List<IInputDevice>();

        public void RegisterDevice(IInputDevice device)
        {
            if (!_devices.Contains(device))
                _devices.Add(device);
        }

        public T GetDevice<T>() where T : class, IInputDevice
        {
            foreach (var d in _devices)
                if (d is T typed) return typed;

            return null;
        }

        /// <summary>
        /// Updates all registered devices.
        /// </summary>
        public void Update()
        {
            foreach (var device in _devices)
                device.Update();
        }

        /// <summary>
        /// Resets transient states after Update.
        /// </summary>
        public void PostUpdate()
        {
            foreach (var device in _devices)
                device.PostUpdate();
        }
    }
}
