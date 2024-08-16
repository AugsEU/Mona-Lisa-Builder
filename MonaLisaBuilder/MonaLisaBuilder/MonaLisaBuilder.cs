using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MonaLisaBuilder
{
	public class MonaLisaBuilder : Game
	{
		const int GAME_SCALE = 2;
		const int MONA_LISA_WIDTH = 180;
		const int MONA_LISA_HEIGHT = 269;

		private GraphicsDeviceManager mGraphics;
		private SpriteBatch mSpriteBatch;
		RenderTarget2D mGameTarget;
		Texture2D mMonaLisa;
		Texture2D mWhitePixel;

		bool[] mPixelCovered;

		MouseState mPrevState;

		Point mCursorPos;

		bool mHasWon = false;
		float mFlashEffectTime = -1.0f;
		SoundEffect mWinSound;
		SoundEffect mLoseSound;
		Song mMainMusic;

		int mCheatIndex = -1;

		public MonaLisaBuilder()
		{
			mGraphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			Window.AllowUserResizing = false;

			mGraphics.IsFullScreen = false;
			mGraphics.PreferredBackBufferWidth = MONA_LISA_WIDTH * GAME_SCALE;
			mGraphics.PreferredBackBufferHeight = MONA_LISA_HEIGHT * GAME_SCALE;
			IsMouseVisible = false;
			mGraphics.ApplyChanges();

			mPixelCovered = new bool[MONA_LISA_WIDTH * MONA_LISA_HEIGHT];
			for(int i = 0; i < mPixelCovered.Length; i++)
			{
				mPixelCovered[i] = true;
			}

			mPrevState = new MouseState();
			mCursorPos = new Point(-1, -1);
		}

		protected override void Initialize()
		{
			base.Initialize();


			Window.Title = "Mona Lisa Builder";
		}

		protected override void LoadContent()
		{
			mSpriteBatch = new SpriteBatch(GraphicsDevice);

			mGameTarget = new RenderTarget2D(GraphicsDevice, MONA_LISA_WIDTH, MONA_LISA_HEIGHT);

			mMonaLisa = Content.Load<Texture2D>("MonaLisa");

			mWinSound = Content.Load<SoundEffect>("Win");
			mLoseSound = Content.Load<SoundEffect>("Failure");
			mMainMusic = Content.Load<Song>("MainTheme");

			MediaPlayer.Volume = 0.4f;
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Play(mMainMusic);

			mWhitePixel = new Texture2D(GraphicsDevice, 1, 1);
			Color[] colData = { Color.White };
			mWhitePixel.SetData(colData);
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			if(mCheatIndex >= 0 && mCheatIndex < mPixelCovered.Length)
			{
				for (int i = 0; i < 60 && mCheatIndex < mPixelCovered.Length; i++)
				{
					mPixelCovered[mCheatIndex] = false;
					mCheatIndex++;
				}
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.M) &&
					Keyboard.GetState().IsKeyDown(Keys.O) &&
					Keyboard.GetState().IsKeyDown(Keys.N) &&
					Keyboard.GetState().IsKeyDown(Keys.A))
			{
				// Make cheat
				mCheatIndex = 0;
			}

			MouseState mouseState = Mouse.GetState();

			// Mouse pos
			mCursorPos = new Point(mouseState.X / GAME_SCALE, mouseState.Y / GAME_SCALE);
			if(mCursorPos.X >= MONA_LISA_WIDTH || mCursorPos.Y >= MONA_LISA_HEIGHT)
			{
				mCursorPos = new Point(-1, -1);
			}

			if(!mHasWon && CursorValid() && mouseState.LeftButton == ButtonState.Pressed && !(mPrevState.LeftButton == ButtonState.Pressed))
			{
				int index = mCursorPos.X + mCursorPos.Y * MONA_LISA_WIDTH;

				if (mPixelCovered[index] == false)
				{
					LoseGame();
				}
				else
				{
					// Uncover a pixel
					mPixelCovered[index] = false;
				}
			}

			// Check win
			if (!mHasWon)
			{
				mHasWon = true;
				for (int i = 0; i < mPixelCovered.Length; i++)
				{
					if (mPixelCovered[i])
					{
						mHasWon = false;
						break;
					}
				}

				if (mHasWon)
				{
					mWinSound.Play(0.4f, 0.0f, 0.0f);
				}


				mFlashEffectTime = 0.0f;
			}
			else if(mFlashEffectTime >= 0.0f)
			{
				mFlashEffectTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

				if(mFlashEffectTime > 3.0f)
				{
					mFlashEffectTime = -1.0f;
				}
			}

			mPrevState = mouseState;

			base.Update(gameTime);
		}

		void LoseGame()
		{
			mLoseSound.Play(0.4f, 0.0f, 0.0f);
			for (int i = 0; i < mPixelCovered.Length; i++)
			{
				mPixelCovered[i] = true;
			}
		}

		bool CursorValid()
		{
			return mCursorPos.X >= 0 && mCursorPos.Y >= 0;
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// Draw to target...
			GraphicsDevice.SetRenderTarget(mGameTarget);

			mSpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default);
			
			
			Color monaLisaColor = Color.White;

			if(mFlashEffectTime > 0.0f)
			{
				int colIdx = (int)(mFlashEffectTime * 3.0f);
				if(colIdx % 2 == 0)
				{
					monaLisaColor = Color.Pink;
				}
			}
			
			mSpriteBatch.Draw(mMonaLisa, Vector2.Zero, monaLisaColor);

			for (int x = 0; x < MONA_LISA_WIDTH; x++)
			{
				for (int y = 0; y < MONA_LISA_HEIGHT; y++)
				{
					int index = x + y * MONA_LISA_WIDTH;
					if (mPixelCovered[index])
					{
						mSpriteBatch.Draw(mWhitePixel, new Vector2(x, y), Color.Black);
					}
				}
			}


			Color cursorCol = Color.White;
			if(CursorValid() && !mHasWon)
			{
				int index = mCursorPos.X + mCursorPos.Y * MONA_LISA_WIDTH;
				cursorCol = mPixelCovered[index] ? Color.White : Color.Red;
			}

			mSpriteBatch.Draw(mWhitePixel, new Vector2(mCursorPos.X, mCursorPos.Y), cursorCol);

			mSpriteBatch.End();

			// Draw to screen at twice size.
			GraphicsDevice.SetRenderTarget(null);

			mSpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default);

			mSpriteBatch.Draw(mGameTarget, new Rectangle(0, 0, MONA_LISA_WIDTH * GAME_SCALE, MONA_LISA_HEIGHT * GAME_SCALE), Color.White);

			mSpriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
