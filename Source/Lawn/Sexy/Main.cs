using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Lawn;
//using Microsoft.Phone.Info;
//using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sexy;

public class Main : Game
{
	private static SexyTransform2D orientationTransform;

	private static UI_ORIENTATION orientationUsed;

	private static bool newOrientation;

	public static GamerServicesComponent GamerServicesComp;

	public static bool trialModeChecked = false;

	private static bool trialModeCachedValue = true;

	internal static Graphics graphics;

	private int mFrameCnt;

	private static bool startedProfiler;

	private static bool wantToSuppressDraw;

	private GamePadState previousGamepadState = default(GamePadState);

	public static bool RunWhenLocked
	{
		get
		{
			// FIXME: Windows Phone stuff
			return true;//(int)PhoneApplicationService.Current.ApplicationIdleDetectionMode == 1;
		}
		set
		{
			try
			{
				// FIXME: Windows Phone stuff
				//PhoneApplicationService.Current.ApplicationIdleDetectionMode = (IdleDetectionMode)(value ? 1 : 0);
			}
			catch
			{
			}
		}
	}

	public static bool LOW_MEMORY_DEVICE { get; private set; }

	public static bool DO_LOW_MEMORY_OPTIONS { get; private set; }

	public static bool IsInTrialMode => trialModeCachedValue;

	public Main()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		SetupTileSchedule();
		graphics = Graphics.GetNew((Game)(object)this);
		SetLowMem();
		graphics.IsFullScreen = true;
		Guide.SimulateTrialMode = false;
		graphics.PreferredBackBufferWidth = 800;
		graphics.PreferredBackBufferHeight = 480;
		GraphicsState.mGraphicsDeviceManager.SupportedOrientations = Constants.SupportedOrientations;
		GraphicsState.mGraphicsDeviceManager.DeviceCreated += graphics_DeviceCreated;
		GraphicsState.mGraphicsDeviceManager.DeviceReset += graphics_DeviceReset;
		GraphicsState.mGraphicsDeviceManager.PreparingDeviceSettings += mGraphicsDeviceManager_PreparingDeviceSettings;
		TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30.0);
		((Game)this).Exiting += Main_Exiting;
		// FIXME: Windows Phone stuff
		/*PhoneApplicationService.Current.UserIdleDetectionMode = (IdleDetectionMode)0;
		PhoneApplicationService.Current.Launching += Game_Launching;
		PhoneApplicationService.Current.Activated += Game_Activated;
		PhoneApplicationService.Current.Closing += Current_Closing;
		PhoneApplicationService.Current.Deactivated += Current_Deactivated;*/
	}

	// FIXME: Windows Phone stuff
	/*
	private void Current_Deactivated(object sender, DeactivatedEventArgs e)
	{
		GlobalStaticVars.gSexyAppBase.Tombstoned();
	}

	private void Current_Closing(object sender, ClosingEventArgs e)
	{
		PhoneApplicationService.Current.State.Clear();
	}

	private void Game_Activated(object sender, ActivatedEventArgs e)
	{
	}

	private void Game_Launching(object sender, LaunchingEventArgs e)
	{
		PhoneApplicationService.Current.State.Clear();
	}*/

	private static void SetupTileSchedule()
	{
	}

	private void mGraphicsDeviceManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
	{
	}

	private void graphics_DeviceReset(object sender, EventArgs e)
	{
	}

	private void graphics_DeviceCreated(object sender, EventArgs e)
	{
		GraphicsDevice.PresentationParameters.PresentationInterval = (PresentInterval)3;
	}

	private void Main_Exiting(object sender, EventArgs e)
	{
		GlobalStaticVars.gSexyAppBase.AppExit();
	}

	protected override void Initialize()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Window.OrientationChanged += Window_OrientationChanged;
		GamerServicesComp = new GamerServicesComponent((Game)(object)this);
		ReportAchievement.Initialise();
		base.Initialize();
	}

	protected override void LoadContent()
	{
		GraphicsState.Init();
		SetupForResolution();
		GlobalStaticVars.initialize(this);
		GlobalStaticVars.mGlobalContent.LoadSplashScreen();
		GlobalStaticVars.gSexyAppBase.StartLoadingThread();
	}

	protected override void UnloadContent()
	{
		GlobalStaticVars.mGlobalContent.cleanUp();
	}

	protected override void BeginRun()
	{
		base.BeginRun();
	}

	public void CompensateForSlowUpdate()
	{
		base.ResetElapsedTime();
	}

	protected override void Update(GameTime gameTime)
	{
		if (!IsActive)
		{
			return;
		}
		if (GlobalStaticVars.gSexyAppBase.WantsToExit)
		{
			Exit();
		}
		HandleInput(gameTime);
		GlobalStaticVars.gSexyAppBase.UpdateApp();
		if (!trialModeChecked)
		{
			trialModeChecked = true;
			bool flag = trialModeCachedValue;
			SetLowMem();
			trialModeCachedValue = Guide.IsTrialMode;
			if (flag != trialModeCachedValue && flag)
			{
				LeftTrialMode();
			}
		}
		try
		{
			base.Update(gameTime);
		}
		catch (GameUpdateRequiredException)
		{
			GlobalStaticVars.gSexyAppBase.ShowUpdateRequiredMessage();
		}
	}

	private static void SetLowMem()
	{
		// FIXME: Windows Phone stuff
		//DeviceExtendedProperties.TryGetValue("DeviceTotalMemory", out var obj);
		DO_LOW_MEMORY_OPTIONS = false;//(LOW_MEMORY_DEVICE = (long)obj / 1024 / 1024 <= 256);
		LOW_MEMORY_DEVICE = false;
	}

	private void LeftTrialMode()
	{
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			GlobalStaticVars.gSexyAppBase.LeftTrialMode();
		}
		Window_OrientationChanged(null, null);
	}

	public static void SuppressNextDraw()
	{
		wantToSuppressDraw = true;
	}

	public static SignedInGamer GetGamer()
	{
		if (((ReadOnlyCollection<SignedInGamer>)(object)Gamer.SignedInGamers).Count == 0)
		{
			return null;
		}
		return Gamer.SignedInGamers[(PlayerIndex)0];
	}

	public static void NeedToSetUpOrientationMatrix(UI_ORIENTATION orientation)
	{
		orientationUsed = orientation;
		newOrientation = true;
	}

	private static void SetupOrientationMatrix(UI_ORIENTATION orientation)
	{
		newOrientation = false;
	}

	private void Window_OrientationChanged(object sender, EventArgs e)
	{
		SetupInterfaceOrientation();
	}

	private void SetupInterfaceOrientation()
	{
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			if (Window.CurrentOrientation == DisplayOrientation.LandscapeLeft || Window.CurrentOrientation == DisplayOrientation.LandscapeRight)
				GlobalStaticVars.gSexyAppBase.InterfaceOrientationChanged(UI_ORIENTATION.UI_ORIENTATION_LANDSCAPE_LEFT);
			else
				GlobalStaticVars.gSexyAppBase.InterfaceOrientationChanged(UI_ORIENTATION.UI_ORIENTATION_PORTRAIT);
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (newOrientation)
		{
			SetupOrientationMatrix(orientationUsed);
		}
		lock (ResourceManager.DrawLocker)
		{
			GraphicsDevice.Clear(Color.Black);
			GlobalStaticVars.gSexyAppBase.DrawGame(gameTime);
			base.Draw(gameTime);
		}
	}

	public void HandleInput(GameTime gameTime)
	{
		if (LoadingScreen.IsLoading)
			return;

		var state = GamePad.GetState(PlayerIndex.One);

		var buttons = state.Buttons;

		if (buttons.Back == ButtonState.Pressed)
		{
			var buttons2 = previousGamepadState.Buttons;

			if (buttons2.Back == ButtonState.Released)
				GlobalStaticVars.gSexyAppBase.BackButtonPress();
		}

		TouchCollection state2 = TouchPanel.GetState();
		bool flag = false;
		foreach (var current in state2)
		{
			var touch = new _Touch();
			touch.location.mX = current.Position.X;
			touch.location.mY = current.Position.Y;

			touch.previousLocation = current.TryGetPreviousLocation(out var previousLocation)
				? new CGPoint(previousLocation.Position.X, previousLocation.Position.Y)
				: touch.location;

			touch.timestamp = gameTime.TotalGameTime.TotalSeconds;
			switch (current.State)
			{
				case TouchLocationState.Pressed:
					if (!flag)
					{
						GlobalStaticVars.gSexyAppBase.TouchBegan(touch);
						flag = true;
					}
					break;
				case TouchLocationState.Moved:
					GlobalStaticVars.gSexyAppBase.TouchMoved(touch);
					break;
				case TouchLocationState.Released:
					GlobalStaticVars.gSexyAppBase.TouchEnded(touch);
					break;
				case TouchLocationState.Invalid:
					GlobalStaticVars.gSexyAppBase.TouchesCanceled();
					break;
			}
		}
		previousGamepadState = state;
	}

	protected override void OnActivated(object sender, EventArgs args)
	{
		trialModeChecked = false;
		if (GlobalStaticVars.gSexyAppBase != null)
		{
			GlobalStaticVars.gSexyAppBase.GotFocus();
			if (!GlobalStaticVars.gSexyAppBase.mMusicInterface.isStopped)
			{
				GlobalStaticVars.gSexyAppBase.mMusicInterface.ResumeMusic();
			}
		}
		base.OnActivated(sender, args);
	}

	protected override void OnDeactivated(object sender, EventArgs args)
	{
		GlobalStaticVars.gSexyAppBase.LostFocus();
		if (!GlobalStaticVars.gSexyAppBase.mMusicInterface.isStopped)
		{
			GlobalStaticVars.gSexyAppBase.mMusicInterface.PauseMusic();
		}
		GlobalStaticVars.gSexyAppBase.AppEnteredBackground();
		base.OnDeactivated(sender, args);
	}

	private void GameSpecificCheatInputCheck()
	{
	}

	private static void SetupForResolution()
	{
		Strings.Culture = CultureInfo.CurrentCulture;
		if (Strings.Culture.TwoLetterISOLanguageName == "fr")
		{
			Constants.Language = Constants.LanguageIndex.fr;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "de")
		{
			Constants.Language = Constants.LanguageIndex.de;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "es")
		{
			Constants.Language = Constants.LanguageIndex.es;
		}
		else if (Strings.Culture.TwoLetterISOLanguageName == "it")
		{
			Constants.Language = Constants.LanguageIndex.it;
		}
		else
		{
			Constants.Language = Constants.LanguageIndex.en;
		}
		if ((graphics.GraphicsDevice.PresentationParameters.BackBufferWidth == 480 && graphics.GraphicsDevice.PresentationParameters.BackBufferHeight == 800) || (graphics.GraphicsDevice.PresentationParameters.BackBufferWidth == 800 && graphics.GraphicsDevice.PresentationParameters.BackBufferHeight == 480))
		{
			AtlasResources.mAtlasResources = new AtlasResources_480x800();
			Constants.Load480x800();
			return;
		}
		throw new Exception("Unsupported Resolution");
	}
}
