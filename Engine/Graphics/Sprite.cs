using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace RenyulEngine.Graphics
{
public class Sprite
{
public Texture2D Texture { get; }
public Rectangle? Source;
public Microsoft.Xna.Framework.Vector2 Origin;
public Color Color = Color.White;
public Microsoft.Xna.Framework.Vector2 Scale = Microsoft.Xna.Framework.Vector2.One;
public float Rotation = 0f;
public float Layer = 0f;


public Sprite(Texture2D tex) { Texture = tex; }
public void Draw(Renderer2D r, Microsoft.Xna.Framework.Vector2 position)
=> r.Draw(Texture, position, Source, Color, Rotation, Origin, Scale, SpriteEffects.None, Layer);
}
}