using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sexy;

internal class GraphicsState
{
	public int mTransX;

	public int mTransY;

	public float mScaleX;

	public float mScaleY;

	public float mScaleOrigX;

	public float mScaleOrigY;

	public bool mFastStretch;

	public bool mWriteColoredString;

	public bool mLinearBlend;

	public TRect mClipRect;

	protected Font mFont;

	public Graphics.DrawMode mDrawMode;

	public bool mColorizeImages;

	protected static Image dummy;

	public float WorldRotation;

	public bool NeedToSetWorldRotation;

	public static GraphicsDeviceManager mGraphicsDeviceManager;

	public Color mColor { get; protected set; }

	public int mScreenWidth
	{
		get
		{
			if (mGraphicsDeviceManager.GraphicsDevice == null)
			{
				return mGraphicsDeviceManager.PreferredBackBufferWidth;
			}
			return mGraphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
		}
	}

	public int mScreenHeight
	{
		get
		{
			if (mGraphicsDeviceManager.GraphicsDevice == null)
			{
				return mGraphicsDeviceManager.PreferredBackBufferHeight;
			}
			return mGraphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
		}
	}

	public GraphicsState(Game game)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		mGraphicsDeviceManager = new GraphicsDeviceManager(game);
		Reset();
	}

	public GraphicsState()
	{
		Reset();
	}

	private void Reset()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		mTransX = 0;
		mTransY = 0;
		mFastStretch = false;
		mWriteColoredString = false;
		mLinearBlend = false;
		mScaleX = 1f;
		mScaleY = 1f;
		mScaleOrigX = 0f;
		mScaleOrigY = 0f;
		mFont = null;
		mColor = default(SexyColor);
		mColorizeImages = false;
		WorldRotation = 0f;
		mDrawMode = Graphics.DrawMode.DRAWMODE_NORMAL;
		mClipRect = new TRect(0, 0, mScreenWidth, mScreenHeight);
	}

	public static void Init()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(mGraphicsDeviceManager.GraphicsDevice, 1, 1);
		val.SetData<Color>((Color[])(object)new Color[1] { Color.White });
		dummy = new Image(val);
	}

	public void CopyStateFrom(GraphicsState theState)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		mTransX = theState.mTransX;
		mTransY = theState.mTransY;
		mFastStretch = theState.mFastStretch;
		mWriteColoredString = theState.mWriteColoredString;
		mLinearBlend = theState.mLinearBlend;
		mScaleX = theState.mScaleX;
		mScaleY = theState.mScaleY;
		mScaleOrigX = theState.mScaleOrigX;
		mScaleOrigY = theState.mScaleOrigY;
		mClipRect = theState.mClipRect;
		mFont = theState.mFont;
		mColor = theState.mColor;
		mDrawMode = theState.mDrawMode;
		mColorizeImages = theState.mColorizeImages;
		WorldRotation = theState.WorldRotation;
	}

	public void SetWorldRotation(float theRotation)
	{
		WorldRotation = theRotation;
		NeedToSetWorldRotation = true;
	}

	public void ApplyWorldRotation()
	{
	}

	public static int GetClosestPowerOf2Above(int theNum)
	{
		int num;
		for (num = 1; num < theNum; num <<= 1)
		{
		}
		return num;
	}
}
