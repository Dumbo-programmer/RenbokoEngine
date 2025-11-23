using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;


namespace RenbokoEngine.Graphics
{
public class Renderer2D : Core.IService
{
private readonly GraphicsDevice _gd;
private readonly SpriteBatch _batch;
private readonly ContentManager _content;
public Camera2D Camera { get; } = new();


public Renderer2D(GraphicsDevice gd, ContentManager content)
{
_gd = gd;
_content = content;
_batch = new SpriteBatch(_gd);
}


public SpriteFont LoadFont(string assetName) => _content.Load<SpriteFont>(assetName);
public Texture2D LoadTexture(string assetName) => _content.Load<Texture2D>(assetName);


public void Begin()
{
_batch.Begin(transformMatrix: Camera.GetViewMatrix());
}


public void Draw(Texture2D tex, Microsoft.Xna.Framework.Vector2 pos, Rectangle? src = null, Color? color = null, float rotation = 0f, Microsoft.Xna.Framework.Vector2? origin = null, Microsoft.Xna.Framework.Vector2? scale = null, SpriteEffects fx = SpriteEffects.None, float layer = 0f)
{
_batch.Draw(tex, pos, src, color ?? Color.White, rotation, origin ?? Microsoft.Xna.Framework.Vector2.Zero, scale ?? Microsoft.Xna.Framework.Vector2.One, fx, layer);
}


public void DrawString(SpriteFont font, string text, Microsoft.Xna.Framework.Vector2 pos, Color? color = null)
{
_batch.DrawString(font, text, pos, color ?? Color.White);
}


public void End() { _batch.End(); }
}
}