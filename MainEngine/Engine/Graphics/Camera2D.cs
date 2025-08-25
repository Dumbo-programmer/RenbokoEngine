using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace RenyulEngine.Graphics
{
public class Camera2D
{
public Microsoft.Xna.Framework.Vector2 Position;
public float Zoom = 1f;
public float Rotation = 0f;
public Matrix GetViewMatrix()
{
return Matrix.CreateTranslation(new Microsoft.Xna.Framework.Vector3(-Position, 0)) *
Matrix.CreateRotationZ(Rotation) *
Matrix.CreateScale(Zoom, Zoom, 1f);
}
}
}