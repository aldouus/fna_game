using Microsoft.Xna.Framework;

public class Player
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Width = 24f;
    public float Height = 56f;
    public bool OnGround = false;
    public bool WantsToJump = false;

    public Rectangle BoundingBox =>
        new(
            (int)Position.X,
            (int)Position.Y,
            (int)Width,
            (int)Height
        );
}
