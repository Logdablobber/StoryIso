using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ViewportAdapters;
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.Functions;
using StoryIso.Misc;
using StoryIso.Scenes;
using StoryIso.Tiled;

namespace StoryIso;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
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

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
		var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 800, 480);
		camera = new OrthographicCamera(viewportAdapter);
		camera.Zoom = 4f;
		FunctionProcessor.Initialize();

		base.Initialize();
    }

    protected override void LoadContent()
    {
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		AsepriteLoader.LoadAsefiles(GraphicsDevice, "./Content/Aseprite/");

		tiledManager = new TiledManager(GraphicsDevice, Content, "Map1");

		DebugConsole.Font = Content.Load<BitmapFont>("Fonts/arial");
		DebugConsole.Scale = 0.125f;

		sceneManager = new SceneManager(Content.Load<Texture2D>("Textures/dialogue-box"),
										Content.Load<Texture2D>("Textures/name-box"),
										Content.Load<BitmapFont>("Fonts/ibmbios"),
										"./Content/Scenes/");

		_world = new WorldBuilder()
					.AddSystem(new RenderSystem(_spriteBatch))
					.AddSystem(new PlayerSystem())
					.AddSystem(new AnimationSystem())
					.AddSystem(new CharacterSystem())
					.Build();

		CharacterManager.LoadCharacters("./Content/Characters/", _world);

		Animation? player_animation = AsepriteLoader.GetAnimation("player-animation");

		if (player_animation == null)
		{
			DebugConsole.Raise(new MissingAssetError(new Source(0, null, "Player"), "player-animation"));
		}

		player = _world.CreateEntity();
		player.Attach(player_animation);
		player.Attach(new Player(speed:100f));
		player.Attach(new Character("Player", Direction.Down, visibility:true));
		player.Attach(new Transform2(new Vector2(190, 150), 0, characterScale));

		sceneManager.RunScene("startup", new Source(0, null, "startup"));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

		_world.Update(gameTime);

		sceneManager.Update(gameTime);
		tiledManager.Update(gameTime);

		DebugConsole.Update(gameTime);

        base.Update(gameTime);
    }

	public static void PauseRendering()
	{
		_pauseRenderTimer = PAUSE_TIME;
	}

    protected override void Draw(GameTime gameTime)
	{
		if (_pauseRenderTimer > 0)
		{
			_pauseRenderTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			return;
		}

		Transform2 playerTransform = player.Get<Transform2>();

		Vector2 playerCenter = new RectangleF(playerTransform.Position, player.Get<Animation>().GetFrame().Bounds.Size.ToVector2() * playerTransform.Scale).Center;

		camera.LookAt(playerCenter);

		GraphicsDevice.Clear(Color.Black);

		BlendState previous_blend_state = GraphicsDevice.BlendState;
		GraphicsDevice.BlendState = BlendState.AlphaBlend;

		tiledManager.Draw();

		GraphicsDevice.BlendState = previous_blend_state;

		_world.Draw(gameTime);
	
		if (debug)
		{
			_spriteBatch!.Begin(samplerState: SamplerState.PointClamp, blendState:BlendState.AlphaBlend, transformMatrix: camera.GetViewMatrix());

			foreach (var trigger in tiledManager.currentRoom.triggers)
			{
				trigger.Draw(_spriteBatch);
			}

			_spriteBatch.End();
		}

		_spriteBatch!.Begin(samplerState: SamplerState.PointWrap, transformMatrix:camera.GetViewMatrix());

		sceneManager.Draw(_spriteBatch);

		if (debug)
		{
			DebugConsole.Draw(_spriteBatch);
		}

		_spriteBatch.End();

		base.Draw(gameTime);
    }
}
