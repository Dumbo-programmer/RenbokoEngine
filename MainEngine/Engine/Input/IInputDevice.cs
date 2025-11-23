namespace RenbokoEngine.Input
{
    /// <summary>
    /// Base interface for all input devices.
    /// </summary>
    public interface IInputDevice
    {
        /// <summary>
        /// Called once per frame to update device state.
        /// </summary>
        void Update();

        /// <summary>
        /// Called to reset transient states (e.g., keyDown events).
        /// </summary>
        void PostUpdate();
    }
}
