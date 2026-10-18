using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private readonly TileMap _tileMap;

    private Player _player;
    private Texture2D _playerTexture;
    private bool _jumpPressedLastFrame;

    private Texture2D _dirtTexture;
    private Texture2D _grassTexture;
    private Texture2D _stoneTexture;
    private Texture2D _bedRockTexture;

    private GameAudio _audio = null!;
    private bool _testPressedLastFrame;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _tileMap = new TileMap(50, 30);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = _tileMap.TileSize * _tileMap.Tiles.GetLength(0);
        _graphics.PreferredBackBufferHeight = _tileMap.TileSize * _tileMap.Tiles.GetLength(1);
    }

    protected override void Initialize()
    {
        _audio = new GameAudio(GameAudio.FindTestWav());

        base.Initialize();

        _player = new Player
        {
            Position = new Vector2(100, 100)
        };

        int width = _tileMap.Tiles.GetLength(0);
        int height = _tileMap.Tiles.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int fromBottom = height - 1 - y;

                if (fromBottom < 1)
                    _tileMap.Tiles[x, y] = TileType.BedRock;
                else if (fromBottom < 3)
                    _tileMap.Tiles[x, y] = TileType.Stone;
                else if (fromBottom < 5)
                    _tileMap.Tiles[x, y] = TileType.Dirt;
                else if (fromBottom == 5)
                    _tileMap.Tiles[x, y] = TileType.Grass;
                else
                    _tileMap.Tiles[x, y] = TileType.Air;
            }
        }
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _playerTexture = new Texture2D(GraphicsDevice, 1, 1);
        _playerTexture.SetData([Color.Red]);

        _dirtTexture = new Texture2D(GraphicsDevice, 1, 1);
        _dirtTexture.SetData([Color.Brown]);

        _grassTexture = new Texture2D(GraphicsDevice, 1, 1);
        _grassTexture.SetData([Color.Green]);

        _stoneTexture = new Texture2D(GraphicsDevice, 1, 1);
        _stoneTexture.SetData([Color.Gray]);

        _bedRockTexture = new Texture2D(GraphicsDevice, 1, 1);
        _bedRockTexture.SetData([Color.DarkSlateGray]);
    }

    const int debounceTime = 300;
    readonly Debouncer leftDebouncer = new(debounceTime);
    readonly Debouncer rightDebouncer = new(debounceTime);

    protected override void Update(GameTime gameTime)
    {
        PlayerController.UpdatePlayer(gameTime, _player, _tileMap, ref _jumpPressedLastFrame);

        MouseState currentMouseState = Mouse.GetState();

        double now = gameTime.TotalGameTime.TotalMilliseconds;

        if (currentMouseState.RightButton == ButtonState.Pressed && leftDebouncer.CanTrigger(now))
        {
            HandleTileInteraction(
                new Point(currentMouseState.X, currentMouseState.Y),
                TileType.Dirt
            );
        }

        if (currentMouseState.LeftButton == ButtonState.Pressed && rightDebouncer.CanTrigger(now))
        {
            HandleTileInteraction(
                new Point(currentMouseState.X, currentMouseState.Y),
                TileType.Air
            );
        }

        var state = Keyboard.GetState();
        if (state.IsKeyDown(Keys.Q))
        {
            Exit();
        }

        var testDown = state.IsKeyDown(Keys.T);
        if (testDown && !_testPressedLastFrame)
            _audio.PlayTest();
        _testPressedLastFrame = testDown;

        base.Update(gameTime);
    }

    protected override void UnloadContent()
    {
        _audio.Dispose();

        base.UnloadContent();
    }

    private static bool IsSolid(TileType type)
    {
        return type != TileType.Air;
    }

    private bool HasAdjacentSolidBlock(int x, int y)
    {
        int width = _tileMap.Tiles.GetLength(0);
        int height = _tileMap.Tiles.GetLength(1);

        if (x > 0 && Game1.IsSolid(_tileMap.Tiles[x - 1, y])) return true;
        if (x < width - 1 && Game1.IsSolid(_tileMap.Tiles[x + 1, y])) return true;
        if (y > 0 && Game1.IsSolid(_tileMap.Tiles[x, y - 1])) return true;
        if (y < height - 1 && Game1.IsSolid(_tileMap.Tiles[x, y + 1])) return true;

        return false;
    }

    private void HandleTileInteraction(Point mousePosition, TileType newType)
    {
        int tileX = mousePosition.X / _tileMap.TileSize;
        int tileY = mousePosition.Y / _tileMap.TileSize;

        if (tileX < 0 || tileX >= _tileMap.Tiles.GetLength(0) ||
            tileY < 0 || tileY >= _tileMap.Tiles.GetLength(1))
            return;

        int playerTileX = (int)(_player.Position.X / _tileMap.TileSize);
        int playerTileY = (int)(_player.Position.Y / _tileMap.TileSize);

        int maxRange = 3;

        int dx = Math.Abs(tileX - playerTileX);
        int dy = Math.Abs(tileY - playerTileY);

        if (dx > maxRange)
            return;

        Rectangle tileRect = new Rectangle(
            tileX * _tileMap.TileSize,
            tileY * _tileMap.TileSize,
            _tileMap.TileSize,
            _tileMap.TileSize
        );

        if (_player.BoundingBox.Intersects(tileRect))
            return;

        var current = _tileMap.Tiles[tileX, tileY];

        if (newType == TileType.Air)
        {
            if (current != TileType.Air && current != TileType.BedRock)
                _tileMap.Tiles[tileX, tileY] = TileType.Air;
            return;
        }

        if (current == TileType.Air)
        {
            if (HasAdjacentSolidBlock(tileX, tileY))
                _tileMap.Tiles[tileX, tileY] = newType;
        }

    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        _spriteBatch.Draw(
            _playerTexture,
            new Rectangle(
                (int)_player.Position.X,
                (int)_player.Position.Y,
                (int)_player.Width,
                (int)_player.Height
            ),
            Color.White
        );

        for (int x = 0; x < _tileMap.Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < _tileMap.Tiles.GetLength(1); y++)
            {
                Rectangle tileRect = new Rectangle(
                    x * _tileMap.TileSize,
                    y * _tileMap.TileSize,
                    _tileMap.TileSize,
                    _tileMap.TileSize
                );

                Texture2D? tileTexture = GetTextureForTileType(_tileMap.Tiles[x, y]);
                if (tileTexture != null)
                {
                    _spriteBatch.Draw(tileTexture, tileRect, Color.White);
                }
            }
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Texture2D? GetTextureForTileType(TileType type)
    {
        return type switch
        {
            TileType.Dirt => _dirtTexture,
            TileType.Grass => _grassTexture,
            TileType.Stone => _stoneTexture,
            TileType.BedRock => _bedRockTexture,
            TileType.Air => null,
            _ => null,
        };
    }
}
