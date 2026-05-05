using System;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Serialization.Json;
using MonoGame.Extended.ViewportAdapters;
using Entropy.Debugging;
using Entropy.ECS;
using Entropy.Entities;
using Entropy.Enums;
using Entropy.FileLoading;
using Entropy.Scripting;
using Entropy.Scripting.Variables;
using Entropy.Scenes;
using Entropy.Tiled;
using Entropy.UI;
using Microsoft.VisualBasic.FileIO;
using Entropy.Input;
using Entropy.Misc;

namespace Entropy;

public class Game1 : Game
{
	public static JsonSerializerOptions DeserializeOptions = null!;

    public static GraphicsDeviceManager Graphics { get; private set; }
	private SpriteBatch _spriteBatch = null!;

	public static TiledManager tiledManager = null!;

	public static World world = null!;
	public static Entity player = null!;
	private const float CHARACTER_SCALE = 0.7f;
	public static readonly Vector2 characterScale = new Vector2(CHARACTER_SCALE, CHARACTER_SCALE);

	#if DEBUG
		private readonly bool debug = true;
	#else 
		private readonly bool debug = false;
	#endif

	// the screen should pause for a moment when moving between rooms
	// to eliminate flashing
	private const float PAUSE_TIME = 0.05f;
	private static float _pauseRenderTimer = 0f;
	private static readonly System.Threading.Lock _pauseRenderTimerLock = new();

	public static SceneManager sceneManager = null!;

	public static BoxingViewportAdapter ViewportAdapter = null!;
	public static OrthographicCamera camera = null!;
	public static Vector2 CameraOffset
	{
		get
		{
			if (camera == null)
			{
				throw new NullReferenceException("Camera is null, somehow");
			}

			return camera.BoundingRectangle.TopLeft;
		}
	}

	public const int ScreenHeight = 480;
	public const int ScreenWidth = 800;

	private bool _qPressedLastFrame;

	public static InputProcessor KeybindManager = null!;

	public static Scope GlobalScope = new(null, [], 0, 0);
    
    private static Point _savedPosition = Point.Zero;
    private static Point _savedSize = Point.Zero;
    private static event EventHandler<EventArgs> FullscreenToggled;

    public static Vector2 ScreenScale => new Vector2((float)Graphics.PreferredBackBufferWidth / ScreenWidth, (float)Graphics.PreferredBackBufferHeight / ScreenHeight);
    
    public Game1()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

		DeserializeOptions = new (JsonSerializerOptions.Default)
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		DeserializeOptions.Converters.Add(new ColorJsonConverter());
		DeserializeOptions.Converters.Add(new Vector2JsonConverter());

		KeybindManager = new InputProcessor("./Content/System/");
    }

    protected override void Initialize()
    {
		VariableManager.Initialize();
		ViewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, ScreenWidth, ScreenHeight);
		camera = new OrthographicCamera(ViewportAdapter)
		{
			Zoom = 4f
		};

		FullscreenToggled += (_, _) => ToggleBorderlessFullscreen();

		FunctionProcessor.Initialize();
		TextFormatter.Initialize();

		base.Initialize();
    }

    protected override void LoadContent()
    {
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		FileLoadingManager.LoadAll(GraphicsDevice);

		tiledManager = new TiledManager(GraphicsDevice, Content, "Map1");

		DebugConsole.Font = FontLoader.GetFont("arial")?.Font;
		DebugConsole.Scale = 0.125f;

		world = new WorldBuilder()
					.AddSystem(new RenderSystem(_spriteBatch))
					.AddSystem(new UIRenderSystem(_spriteBatch))
					.AddSystem(new PlayerSystem())
					.AddSystem(new AnimationSystem())
					.AddSystem(new CharacterSystem())
					.AddSystem(new UISystem())
					.AddSystem(new ButtonSystem())
					.Build();
        
        sceneManager = new SceneManager(TextureLoader.GetTexture("dialogue-box")!,
	        TextureLoader.GetTexture("name-box")!,
	        TextureLoader.GetTexture("option-box")!,
	        FontLoader.GetFont("ibmbios") ?? throw new NullReferenceException(),
	        "./Content/Scenes/");

        UIManager.LoadAll(GraphicsDevice, "./Content/UI/", world);

		CharacterManager.LoadCharacters("./Content/Characters/", world);

		Animation? player_animation = AsepriteLoader.GetAnimation("player-animation");

		if (player_animation == null)
		{
			DebugConsole.Raise(new MissingAssetError(new Source(0, null, "Player"), "player-animation"));
		}

		player = world.CreateEntity();
		player.Attach(player_animation);
		player.Attach(new Player(speed:100f));
		player.Attach(new Character("Player", Direction.Down, room:"#any#"));
		player.Attach(new Transform2(new Vector2(190, 150), 0, characterScale));
		player.Attach(new RenderAttributes("Player", true, Color.White, RenderLayer.Player));

		world.Update(new GameTime());
        sceneManager.RunScene("startup", new Source(0, null, "startup"));
    }

    protected override void Update(GameTime gameTime)
    {
	    var keystate = Keyboard.GetState();
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keystate.IsKeyDown(Keys.Escape))
            Exit();

		if (keystate.IsKeyDown(Keys.Q))
		{
			if (!_qPressedLastFrame)
			{
				var sfx = AudioLoader.GetSound("tx0_fire1") ?? throw new NullReferenceException();
                ToggleBorderlessFullscreen();

				sfx.Play();
			}
			
			_qPressedLastFrame = true;
		}
		else
		{
			_qPressedLastFrame = false;
		}

		KeybindManager.Process(keystate);

		world.Update(gameTime);

		sceneManager.Update(gameTime);
		tiledManager.Update(gameTime);

		DebugConsole.Update(gameTime);

        base.Update(gameTime);
    }

	public static void PauseRendering()
	{
		lock (_pauseRenderTimerLock)
		{
			_pauseRenderTimer = PAUSE_TIME;
		}
	}

    protected override void Draw(GameTime gameTime)
	{
		lock (_pauseRenderTimerLock)
		{
			if (_pauseRenderTimer > 0)
			{
				_pauseRenderTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
				return;
			}
		}

		Transform2 playerTransform = player.Get<Transform2>();

		Vector2 playerCenter = new RectangleF(playerTransform.Position, player.Get<Animation>().GetFrame().Bounds.Size.ToVector2() * playerTransform.Scale).Center;

		camera.LookAt(playerCenter);

		GraphicsDevice.Clear(Color.Black);

		_spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix());

		tiledManager.Draw(_spriteBatch);

		_spriteBatch.End();

		world.Draw(gameTime);
	
		if (debug && tiledManager.currentRoom != null)
		{
			_spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState:BlendState.AlphaBlend, transformMatrix: camera.GetViewMatrix());

			foreach (var trigger in tiledManager.currentRoom.Triggers)
			{
				trigger.Draw(_spriteBatch);
			}

			_spriteBatch.End();
		}

		_spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix:camera.GetViewMatrix());

		if (debug)
		{
			DebugConsole.Draw(_spriteBatch);
		}

		_spriteBatch.End();

		base.Draw(gameTime);
    }

	public void ToggleBorderlessFullscreen()
	{
		if (Window.IsBorderless)
		{
			Window.IsBorderless = false;
            Window.Position = _savedPosition;
            Graphics.PreferredBackBufferWidth = _savedSize.X;
            Graphics.PreferredBackBufferHeight = _savedSize.Y;
            Graphics.ApplyChanges();
            return;
		}

		_savedPosition = Window.Position;
		_savedSize = new Point(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
		Window.IsBorderless = true;
		Window.Position = Point.Zero;
        Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        Graphics.ApplyChanges();
	}
    
    public static void ToggleFullscreen()
    {
	    FullscreenToggled(null, EventArgs.Empty);
    }
    
    public static bool ContainsRectangle(Rectangle rect, Vector2 position, Vector2 origin, Vector2 scale)
    {
	    var rectF = new RectangleF((rect.Location.ToVector2() - origin) * scale + position, rect.Size.ToVector2() * scale);

	    return rectF.Intersects(camera.BoundingRectangle);
    }
}