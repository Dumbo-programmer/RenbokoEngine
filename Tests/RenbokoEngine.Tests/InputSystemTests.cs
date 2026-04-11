using RenbokoEngine.Input;
using Xunit;

namespace RenbokoEngine.Tests;

public class InputSystemTests
{
    private sealed class FakeDevice : IInputDevice
    {
        public int UpdateCount { get; private set; }
        public int PostUpdateCount { get; private set; }

        public void Update() => UpdateCount++;
        public void PostUpdate() => PostUpdateCount++;
    }

    [Fact]
    public void Register_Then_GetDevice_ReturnsRegisteredInstance()
    {
        var input = new InputSystem();
        var device = new FakeDevice();

        input.RegisterDevice(device);

        var resolved = input.GetDevice<FakeDevice>();
        Assert.Same(device, resolved);
    }

    [Fact]
    public void Update_And_PostUpdate_CallAllDevices()
    {
        var input = new InputSystem();
        var d1 = new FakeDevice();
        var d2 = new FakeDevice();
        input.RegisterDevice(d1);
        input.RegisterDevice(d2);

        input.Update();
        input.PostUpdate();

        Assert.Equal(1, d1.UpdateCount);
        Assert.Equal(1, d2.UpdateCount);
        Assert.Equal(1, d1.PostUpdateCount);
        Assert.Equal(1, d2.PostUpdateCount);
    }
}
