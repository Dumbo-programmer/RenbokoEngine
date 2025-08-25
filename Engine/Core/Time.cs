using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace RenyulEngine.Core
{
public class Time : IService
{
public float DeltaTime { get; private set; }
public double FixedDelta { get; set; } = 1.0 / 60.0;
public double TotalTime { get; private set; }


public void Update(GameTime gt)
{
DeltaTime = (float)gt.ElapsedGameTime.TotalSeconds;
TotalTime = gt.TotalGameTime.TotalSeconds;
}
}
}