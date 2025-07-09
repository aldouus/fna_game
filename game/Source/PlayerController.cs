using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public static class PlayerController
{
    public static void UpdatePlayer(
        GameTime gameTime,
        Player player,
        TileMap tileMap,
        ref bool jumpPressedLastFrame
    )
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.A))
            player.Velocity.X = -120f;
        else if (state.IsKeyDown(Keys.D))
            player.Velocity.X = 120f;
        else
            player.Velocity.X = 0f;

        bool jumpPressed = state.IsKeyDown(Keys.Space);
        if (jumpPressed && !jumpPressedLastFrame && player.OnGround)
        {
            player.Velocity.Y = -350f;
            player.OnGround = false;
        }
        jumpPressedLastFrame = jumpPressed;

        player.Velocity.Y += 800f * dt;
        if (player.Velocity.Y > 400f)
            player.Velocity.Y = 400f;

        Vector2 nextPos = player.Position;

        nextPos.X += player.Velocity.X * dt;
        Rectangle horizRect = new Rectangle(
            (int)nextPos.X, (int)player.Position.Y,
            (int)player.Width, (int)player.Height
        );
        Collision.ResolveCollisions(tileMap, player, ref nextPos, horizRect, true);

        nextPos.Y += player.Velocity.Y * dt;
        Rectangle vertRect = new Rectangle(
            (int)nextPos.X, (int)nextPos.Y,
            (int)player.Width, (int)player.Height
        );
        player.OnGround = false;
        Collision.ResolveCollisions(tileMap, player, ref nextPos, vertRect, false);

        player.Position = nextPos;

        Rectangle feetRect = new Rectangle(
            (int)player.Position.X,
            (int)(player.Position.Y + player.Height),
            (int)player.Width,
            2
        );

        player.OnGround = false;
        int leftTile = feetRect.Left / tileMap.TileSize;
        int rightTile = (feetRect.Right - 1) / tileMap.TileSize;
        int y = feetRect.Top / tileMap.TileSize;

        for (int x = leftTile; x <= rightTile; x++)
        {
            if (x < 0 || y < 0 || x >= tileMap.Tiles.GetLength(0) || y >= tileMap.Tiles.GetLength(1))
                continue;

            if (tileMap.Tiles[x, y] != TileType.Air)
            {
                player.OnGround = true;
                break;
            }
        }
    }
}
