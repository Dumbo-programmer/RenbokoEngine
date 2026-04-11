using RenbokoEngine.Core;
using Xunit;

namespace RenbokoEngine.Tests;

public class ServiceLocatorTests
{
    private sealed class TestService : IService
    {
        public int Value { get; set; }
    }

    [Fact]
    public void Register_Then_Get_ReturnsSameInstance()
    {
        ServiceLocator.Unregister<TestService>();

        var service = new TestService { Value = 42 };
        ServiceLocator.Register(service);

        var resolved = ServiceLocator.Get<TestService>();

        Assert.Same(service, resolved);
        Assert.Equal(42, resolved.Value);

        ServiceLocator.Unregister<TestService>();
    }

    [Fact]
    public void TryGet_WhenMissing_ReturnsFalse()
    {
        ServiceLocator.Unregister<TestService>();

        var ok = ServiceLocator.TryGet<TestService>(out var resolved);

        Assert.False(ok);
        Assert.Null(resolved);
    }

    [Fact]
    public void Unregister_RemovesService()
    {
        ServiceLocator.Unregister<TestService>();

        ServiceLocator.Register(new TestService());
        var removed = ServiceLocator.Unregister<TestService>();

        Assert.True(removed);
        Assert.False(ServiceLocator.Has<TestService>());
    }
}
