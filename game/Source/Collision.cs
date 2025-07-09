using Microsoft.Xna.Framework;

public static class Collision
{
    public static void ResolveCollisions(
        TileMap tileMap,
        Player player,
        ref Vector2 pos,
        Rectangle playerRect,
        bool horizontal
    )
    {
        int leftTile = playerRect.Left / tileMap.TileSize;
        int rightTile = (playerRect.Right - 1) / tileMap.TileSize;
        int topTile = playerRect.Top / tileMap.TileSize;
        int bottomTile = (playerRect.Bottom - 1) / tileMap.TileSize;

        for (int x = leftTile; x <= rightTile; x++)
        {
            for (int y = topTile; y <= bottomTile; y++)
            {
                if (x < 0 || y < 0 ||
                    x >= tileMap.Tiles.GetLength(0) ||
                    y >= tileMap.Tiles.GetLength(1))
                    continue;

                if (tileMap.Tiles[x, y] == TileType.Air)
                    continue;

                Rectangle tileRect = new Rectangle(
                    x * tileMap.TileSize,
                    y * tileMap.TileSize,
                    tileMap.TileSize,
                    tileMap.TileSize
                );

                if (!playerRect.Intersects(tileRect))
                    continue;

                if (horizontal)
                {
                    if (player.Velocity.X > 0)
                        pos.X = tileRect.Left - player.Width;
                    else if (player.Velocity.X < 0)
                        pos.X = tileRect.Right;
                    player.Velocity.X = 0;
                    continue;
                }

                if (player.Velocity.Y > 0)
                {
                    pos.Y = tileRect.Top - player.Height;
                    player.Velocity.Y = 0;
                    player.OnGround = true;
                    continue;
                }

                if (player.Velocity.Y < 0)
                {
                    pos.Y = tileRect.Bottom;
                    player.Velocity.Y = 0;
                }
            }
        }
    }
}
