using RenbokoEngine.Assets;
using Xunit;

namespace RenbokoEngine.Tests;

public class AssetManifestTests
{
    [Fact]
    public void ToJson_And_FromJson_RoundTripsFields()
    {
        var manifest = new AssetManifest
        {
            Name = "CoreAssets",
            Textures = new[] { "player.png", "coin.png" },
            Fonts = new[] { "DefaultFont" },
            Audio = new[] { "jump.wav" },
            Misc = new[] { "levels/level1.json" }
        };

        var json = manifest.ToJson();
        var parsed = AssetManifest.FromJson(json);

        Assert.Equal("CoreAssets", parsed.Name);
        Assert.Equal(2, parsed.Textures.Length);
        Assert.Contains("player.png", parsed.Textures);
        Assert.Single(parsed.Fonts);
        Assert.Single(parsed.Audio);
        Assert.Single(parsed.Misc);
    }

    [Fact]
    public void FromJson_WithInvalidJson_ReturnsDefaultManifest()
    {
        var parsed = AssetManifest.FromJson("not-json");

        Assert.NotNull(parsed);
        Assert.Equal("UnnamedManifest", parsed.Name);
        Assert.Empty(parsed.Textures);
        Assert.Empty(parsed.Fonts);
        Assert.Empty(parsed.Audio);
        Assert.Empty(parsed.Misc);
    }
}
