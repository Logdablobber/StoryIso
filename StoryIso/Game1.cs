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
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.FileLoading;
using StoryIso.Scripting;
using StoryIso.Scripting.Variables;
using StoryIso.Scenes;
using StoryIso.Tiled;
using StoryIso.UI;

namespace StoryIso;

public class Game1 : Game
{
	public static JsonSerializerOptions DeserializeOptions = null!;

    private readonly GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch = null!;

	public static TiledManager tiledManager = null!;

	private World _world = null!;
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

	public static OrthographicCamera camera = null!;
	public static Vector2 cameraOffset
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

	private bool qPressedPreviousFrame = false;

	public static Scope GlobalScope = new(null, [], 0, 0);

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

		DeserializeOptions = new (JsonSerializerOptions.Default)
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		DeserializeOptions.Converters.Add(new ColorJsonConverter());
		DeserializeOptions.Converters.Add(new Vector2JsonConverter());
    }

    protected override void Initialize()
    {
		VariableManager.Initialize();
		var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, ScreenWidth, ScreenHeight);
		camera = new OrthographicCamera(viewportAdapter)
		{
			Zoom = 4f
		};
		FunctionProcessor.Initialize();

		base.Initialize();
    }

    protected override void LoadContent()
    {
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		FileLoadingManager.LoadAll(GraphicsDevice);

		tiledManager = new TiledManager(GraphicsDevice, Content, "Map1");

		DebugConsole.Font = FontLoader.GetFont("arial");
		DebugConsole.Scale = 0.125f;

		sceneManager = new SceneManager(TextureLoader.GetTexture("dialogue-box")!,
										TextureLoader.GetTexture("name-box")!,
										FontLoader.GetFont("ibmbios")!,
										"./Content/Scenes/");

		_world = new WorldBuilder()
					.AddSystem(new RenderSystem(_spriteBatch))
					.AddSystem(new PlayerSystem())
					.AddSystem(new AnimationSystem())
					.AddSystem(new CharacterSystem())
					.AddSystem(new UISystem())
					.Build();

		UIManager.LoadAll("./Content/UI/", _world);

		CharacterManager.LoadCharacters("./Content/Characters/", _world);

		Animation? player_animation = AsepriteLoader.GetAnimation("player-animation");

		if (player_animation == null)
		{
			DebugConsole.Raise(new MissingAssetError(new Source(0, null, "Player"), "player-animation"));
		}

		player = _world.CreateEntity();
		player.Attach(player_animation);
		player.Attach(new Player(speed:100f));
		player.Attach(new Character("Player", Direction.Down, room:"#any#"));
		player.Attach(new Transform2(new Vector2(190, 150), 0, characterScale));
		player.Attach(new RenderAttributes(true, Color.White));

		sceneManager.RunScene("startup", new Source(0, null, "startup"));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

		if (Keyboard.GetState().IsKeyDown(Keys.Q))
		{
			if (!qPressedPreviousFrame)
			{
				SoundEffectInstance sfx = AudioLoader.GetSound("tx0_fire1") ?? throw new NullReferenceException();

				sfx.Play();
			}
			
			qPressedPreviousFrame = true;
		}
		else
		{
			qPressedPreviousFrame = false;
		}

		_world.Update(gameTime);

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

		_world.Draw(gameTime);
	
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

		sceneManager.Draw(_spriteBatch);

		if (debug)
		{
			DebugConsole.Draw(_spriteBatch);
		}

		_spriteBatch.End();

		base.Draw(gameTime);
    }
}
