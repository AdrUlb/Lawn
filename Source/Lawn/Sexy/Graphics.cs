using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sexy;

internal class Graphics : GraphicsState
{
	public enum DrawMode
	{
		DRAWMODE_NORMAL,
		DRAWMODE_ADDITIVE
	}

	private static BlendState additiveState = BlendState.AlphaBlend;

	public static readonly SamplerState WrapSamplerState;

	private static readonly SamplerState NormalSamplerState;

	private static bool hardwareClippingEnabled;

	private static TRect hardwareClippedRectangle;

	private static Stack<Graphics> unusedObjects;

	public static SpriteBatch gSpriteBatch;

	public static PrimitiveBatch gPrimitiveBatch;

	public RenderTarget2D mDestImage;

	public Game mGame;

	protected static Stack<SexyTransform2D> gTransformStack;

	internal static SpriteBatch spriteBatch;

	protected static PrimitiveBatch primitiveBatch;

	public static Stack<Graphics> mStateStack;

	private static BasicEffect quadEffect;

	private bool add;

	private bool polyFillBegun;

	private static Image temp;

	private TriVertex[,] tempTriangles = new TriVertex[1, 3];

	private Image triangleBatchTexture;

	private static RasterizerState hardwareClipState;

	protected DrawMode currentlyActiveDrawMode
	{
		get
		{
			if (GraphicsDevice.BlendState != BlendState.AlphaBlend)
			{
				return DrawMode.DRAWMODE_ADDITIVE;
			}
			return DrawMode.DRAWMODE_NORMAL;
		}
		set
		{
			GraphicsDevice.BlendState = ((value == DrawMode.DRAWMODE_NORMAL) ? BlendState.AlphaBlend : additiveState);
		}
	}

	protected static bool spritebatchBegan { get; private set; }

	public GraphicsDevice GraphicsDevice => GraphicsState.mGraphicsDeviceManager.GraphicsDevice;

	public GraphicsDeviceManager GraphicsDeviceManager => GraphicsState.mGraphicsDeviceManager;

	public int PreferredBackBufferWidth
	{
		get
		{
			return GraphicsState.mGraphicsDeviceManager.PreferredBackBufferWidth;
		}
		set
		{
			GraphicsState.mGraphicsDeviceManager.PreferredBackBufferWidth = value;
		}
	}

	public int PreferredBackBufferHeight
	{
		get
		{
			return GraphicsState.mGraphicsDeviceManager.PreferredBackBufferHeight;
		}
		set
		{
			GraphicsState.mGraphicsDeviceManager.PreferredBackBufferHeight = value;
		}
	}

	public bool IsFullScreen
	{
		get
		{
			return GraphicsState.mGraphicsDeviceManager.IsFullScreen;
		}
		set
		{
			GraphicsState.mGraphicsDeviceManager.IsFullScreen = value;
		}
	}

	private Vector2 scale => new Vector2(mScaleX, mScaleY);

	public static void PreAllocateMemory()
	{
		for (int i = 0; i < 10; i++)
		{
			new Graphics().PrepareForReuse();
		}
	}

	public bool IsHardWareClipping()
	{
		return hardwareClippingEnabled;
	}

	public static Graphics GetNew()
	{
		if (unusedObjects.Count > 0)
		{
			Graphics graphics = unusedObjects.Pop();
			graphics.Reset();
			return graphics;
		}
		return new Graphics();
	}

	public static Graphics GetNew(Graphics theGraphics)
	{
		if (unusedObjects.Count > 0)
		{
			Graphics graphics = unusedObjects.Pop();
			graphics.Reset();
			graphics.CopyStateFrom(theGraphics);
			return graphics;
		}
		return new Graphics(theGraphics);
	}

	public static Graphics GetNew(Game theGame)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if (unusedObjects.Count > 0)
		{
			Graphics graphics = unusedObjects.Pop();
			graphics.Reset();
			graphics.mGame = theGame;
			GraphicsState.mGraphicsDeviceManager = new GraphicsDeviceManager(theGame);
			return graphics;
		}
		return new Graphics(theGame);
	}

	public static Graphics GetNew(MemoryImage theDestImage)
	{
		if (unusedObjects.Count > 0)
		{
			Graphics graphics = unusedObjects.Pop();
			graphics.Reset();
			graphics.mDestImage = theDestImage.RenderTarget;
			graphics.mClipRect = new TRect(0, 0, ((Texture2D)graphics.mDestImage).Width, ((Texture2D)graphics.mDestImage).Height);
			graphics.SetRenderTarget(graphics.mDestImage);
			return graphics;
		}
		return new Graphics(theDestImage);
	}

	public void PrepareForReuse()
	{
		unusedObjects.Push(this);
	}

	private void ResetForReuse()
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
		base.mColor = default(SexyColor);
		mColorizeImages = false;
		WorldRotation = 0f;
		mDrawMode = DrawMode.DRAWMODE_NORMAL;
		mClipRect = new TRect(0, 0, base.mScreenWidth, base.mScreenHeight);
	}

	private Graphics()
	{
		spriteBatch = gSpriteBatch;
		primitiveBatch = gPrimitiveBatch;
	}

	private Graphics(Graphics theGraphics)
	{
		CopyStateFrom(theGraphics);
		spriteBatch = gSpriteBatch;
		primitiveBatch = gPrimitiveBatch;
	}

	private Graphics(Game theGame)
		: base(theGame)
	{
		mGame = theGame;
		PreAllocateMemory();
	}

	private Graphics(MemoryImage theDestImage)
	{
		mDestImage = theDestImage.RenderTarget;
		mClipRect = new TRect(0, 0, ((Texture2D)mDestImage).Width, ((Texture2D)mDestImage).Height);
		spriteBatch = gSpriteBatch;
		primitiveBatch = gPrimitiveBatch;
		SetRenderTarget(mDestImage);
	}

	public new void Init()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		gSpriteBatch = new SpriteBatch(GraphicsState.mGraphicsDeviceManager.GraphicsDevice);
		gPrimitiveBatch = new PrimitiveBatch(GraphicsState.mGraphicsDeviceManager.GraphicsDevice);
		spriteBatch = gSpriteBatch;
		primitiveBatch = gPrimitiveBatch;
		quadEffect = new BasicEffect(GraphicsState.mGraphicsDeviceManager.GraphicsDevice);
		quadEffect.LightingEnabled = false;
	}

	public virtual void Dispose()
	{
		_ = mDestImage;
	}

	public void BeginFrame()
	{
		BeginFrame((SpriteSortMode)0);
	}

	public void BeginFrame(SpriteSortMode sortmode)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		BeginFrame(hardwareClippingEnabled ? hardwareClipState : null, sortmode);
	}

	public void BeginFrame(RasterizerState rasterState)
	{
		BeginFrame(rasterState, (SpriteSortMode)0);
	}

	public void BeginFrame(RasterizerState rasterState, SpriteSortMode sortmode)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (NeedToSetWorldRotation)
		{
			ApplyWorldRotation();
			NeedToSetWorldRotation = false;
		}
		BlendState val = ((mDrawMode == DrawMode.DRAWMODE_ADDITIVE) ? additiveState : BlendState.AlphaBlend);
		GraphicsDevice.BlendState = val;
		if (gTransformStack.empty())
		{
			spriteBatch.Begin(sortmode, val, NormalSamplerState, (DepthStencilState)null, rasterState);
		}
		else
		{
			spriteBatch.Begin(sortmode, val, NormalSamplerState, (DepthStencilState)null, rasterState, (Effect)null, gTransformStack.Peek().mMatrix);
		}
		spritebatchBegan = true;
	}

	public void EndFrame()
	{
		EndDrawImageTransformed();
		spriteBatch.End();
		spritebatchBegan = false;
	}

	public static void OrientationChanged()
	{
		primitiveBatch.SetupMatrices();
	}

	protected void SetupDrawMode(DrawMode theDrawingMode)
	{
		add = theDrawingMode == DrawMode.DRAWMODE_ADDITIVE;
		if (spritebatchBegan)
		{
			if (theDrawingMode != currentlyActiveDrawMode)
			{
				currentlyActiveDrawMode = (mDrawMode = theDrawingMode);
				EndFrame();
				BeginFrame();
			}
		}
		else if (primitiveBatch.HasBegun)
		{
			_ = currentlyActiveDrawMode;
			if (hardwareClippingEnabled)
			{
				GraphicsDevice.RasterizerState = hardwareClipState;
			}
		}
		else
		{
			if (theDrawingMode == DrawMode.DRAWMODE_ADDITIVE)
			{
				GraphicsDevice.BlendState = additiveState;
			}
			else
			{
				GraphicsDevice.BlendState = BlendState.AlphaBlend;
			}
			if (hardwareClippingEnabled)
			{
				GraphicsDevice.RasterizerState = hardwareClipState;
			}
		}
		DrawMode drawMode2 = (currentlyActiveDrawMode = theDrawingMode);
		mDrawMode = drawMode2;
	}

	public void SetRenderTarget(RenderTarget2D renderTarget)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		bool flag = spritebatchBegan;
		if (spritebatchBegan)
		{
			EndFrame();
		}
		if (renderTarget == null && gTransformStack.Count > 0)
		{
			gTransformStack.Pop();
		}
		else if (gTransformStack.Count > 0)
		{
			gTransformStack.Push(new SexyTransform2D(Matrix.Identity));
		}
		mDestImage = renderTarget;
		GraphicsDevice.SetRenderTarget(mDestImage);
		ClearClipRect();
		if (flag)
		{
			BeginFrame();
		}
	}

	public void Clear()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Clear(Color.Black);
	}

	public void Clear(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		GraphicsDevice.Clear(color);
	}

	public Graphics Create()
	{
		return new Graphics(this);
	}

	public void SetFont(Font theFont)
	{
		mFont = theFont;
		if (mFont != null)
		{
			mFont.mScaleX = mScaleX;
			mFont.mScaleY = mScaleY;
		}
	}

	public Font GetFont()
	{
		return mFont;
	}

	public static void PremultiplyColour(ref Color c)
	{
		float num = c.A / 255f;
		c *= num;
	}

	public static void PremultiplyColour(ref SexyColor c)
	{
		float num = (float)c.mAlpha / 255f;
		c.mRed = (int)((float)c.mRed * num);
		c.mGreen = (int)((float)c.mGreen * num);
		c.mBlue = (int)((float)c.mBlue * num);
	}

	public void SetColor(Color theColor)
	{
		SetColor(theColor, premultiply: true);
	}

	public void SetColor(Color theColor, bool premultiply)
	{
		if (mDrawMode == DrawMode.DRAWMODE_NORMAL)
		{
			if (premultiply)
				PremultiplyColour(ref theColor);
		}
		else
			theColor.A = 0;

		mColor = theColor;
	}

	public Color GetColor()
	{
		return mColor;
	}

	public void SetDrawMode(DrawMode theDrawMode)
	{
		SetupDrawMode(theDrawMode);
	}

	public void SetDrawMode(int theDrawMode)
	{
		SetupDrawMode((DrawMode)theDrawMode);
	}

	public DrawMode GetDrawMode()
	{
		return mDrawMode;
	}

	public void SetColorizeImages(bool colorizeImages)
	{
		mColorizeImages = colorizeImages;
	}

	public bool GetColorizeImages()
	{
		return mColorizeImages;
	}

	public void SetScaleX(float scaleX)
	{
		mScaleX = scaleX;
		if (mFont != null)
		{
			mFont.mScaleX = mScaleX;
		}
		mScaleOrigX = 0.5f;
	}

	public void SetScaleY(float scaleY)
	{
		mScaleY = scaleY;
		if (mFont != null)
		{
			mFont.mScaleY = scaleY;
		}
		mScaleOrigY = 0.5f;
	}

	public void SetScale(float scale)
	{
		SetScale(scale, scale);
	}

	public void SetScale(float scaleX, float scaleY)
	{
		SetScaleX(scaleX);
		SetScaleY(scaleY);
	}

	public void SetScale(float theScaleX, float theScaleY, float theOrigX, float theOrigY)
	{
		mScaleX = theScaleX;
		mScaleY = theScaleY;
		mScaleOrigX = theOrigX;
		mScaleOrigY = theOrigY;
	}

	public void SetFastStretch(bool fastStretch)
	{
		mFastStretch = fastStretch;
	}

	public bool GetFastStretch()
	{
		return mFastStretch;
	}

	public void SetLinearBlend(bool linear)
	{
		mLinearBlend = linear;
	}

	public bool GetLinearBlend()
	{
		return mLinearBlend;
	}

	public void FillRect(TRect theRect)
	{
		FillRect(theRect.mX, theRect.mY, theRect.mWidth, theRect.mHeight);
	}

	public void FillRect(int theX, int theY, int theWidth, int theHeight)
	{
		bool colorizeImages = mColorizeImages;
		SetColorizeImages(colorizeImages: true);
		DrawImage(GraphicsState.dummy, theX, theY, theWidth, theHeight);
		SetColorizeImages(colorizeImages);
	}

	public void DrawRect(TRect theRect)
	{
		DrawRect(theRect.mX, theRect.mY, theRect.mWidth, theRect.mHeight);
	}

	public void DrawRect(int theX, int theY, int theWidth, int theHeight)
	{
		Color val = mColor;
		if (val.A != 0)
		{
			FillRect(theX, theY, theWidth + 1, 1);
			FillRect(theX, theY + theHeight, theWidth + 1, 1);
			FillRect(theX, theY + 1, 1, theHeight - 1);
			FillRect(theX + theWidth, theY + 1, 1, theHeight - 1);
		}
	}

	public void DrawStringLayer(string theString, int theX, int theY, int theLayer)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		EndDrawImageTransformed();
		if (mFont != null)
		{
			mFont.DrawStringLayer(this, theX + mTransX, theY + mTransY, theString, mColorizeImages ? base.mColor : Color.White, theLayer);
		}
	}

	public void DrawStringLayer(string theString, int theX, int theY, int theLayer, int maxWidth)
	{
		float num = 1f;
		float num2 = mFont.StringWidth(theString);
		if (num2 > (float)maxWidth)
		{
			num = (float)maxWidth / num2;
		}
		SetScale(num);
		DrawStringLayer(theString, theX, theY - (int)((num - 1f) * (float)mFont.GetHeight() / 2f), theLayer);
		SetScale(1f);
	}

	public void DrawString(string theString, int theX, int theY)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (mFont != null)
		{
			mFont.DrawString(this, theX + mTransX, theY + mTransY, theString, base.mColor);
		}
	}

	public void DrawString(StringBuilder theString, int theX, int theY)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (mFont != null)
		{
			mFont.DrawString(this, theX + mTransX, theY + mTransY, theString, mColorizeImages ? base.mColor : Color.White);
		}
	}

	public void DrawLine(int theStartX, int theStartY, int theEndX, int theEndY)
	{
	}

	public void BeginPolyFill()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		EndFrame();
		Matrix? transform = null;
		if (gTransformStack.Count > 0)
		{
			transform = gTransformStack.Peek().mMatrix;
		}
		primitiveBatch.Begin((PrimitiveType)0, mTransX, mTransY, transform, null, NormalSamplerState);
		polyFillBegun = true;
	}

	public void EndPolyFill()
	{
		primitiveBatch.End();
		BeginFrame();
		polyFillBegun = false;
	}

	public void PolyFill(TPoint[] theVertexList, int theNumVertices)
	{
		for (int i = 0; i < theNumVertices; i++)
			primitiveBatch.AddVertex(new((float)(theVertexList[i].mX + mTransX), (float)(theVertexList[i].mY + mTransY)), base.mColor);
	}

	private void PrepareRectsForClipping(ref TRect source, ref TRect destination)
	{
		destination.mX += (int)(mScaleOrigX * (float)destination.mWidth * ((1f - mScaleX) / 2f));
		destination.mY += (int)(mScaleOrigY * (float)destination.mHeight * ((1f - mScaleY) / 2f));
		destination.mWidth = (int)((float)destination.mWidth * mScaleX);
		destination.mHeight = (int)((float)destination.mHeight * mScaleY);

		var val = new Vector2((float)destination.mWidth / (float)((source.mWidth != 0) ? source.mWidth : destination.mWidth), (float)destination.mHeight / (float)((source.mHeight != 0) ? source.mHeight : destination.mHeight));

		if (val.X == 0f)
			val.X = 1f;

		if (val.Y == 0f)
			val.Y = 1f;

		int num = Math.Max(0, (int)((float)(mClipRect.mX - destination.mX) / val.X));
		int num2 = Math.Max(0, (int)((float)(mClipRect.mY - destination.mY) / val.Y));
		source.mX += num;
		source.mY += num2;
		source.mWidth += -num + Math.Min(0, (int)((float)(mClipRect.mX + mClipRect.mWidth - (destination.mX + destination.mWidth)) / val.X));
		source.mHeight += -num2 + Math.Min(0, (int)((float)(mClipRect.mY + mClipRect.mHeight - (destination.mY + destination.mHeight)) / val.Y));
		destination = mClipRect.Intersection(destination);
	}

	private static void GetTransform(out Matrix? transform)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (gTransformStack.Count > 0)
		{
			transform = gTransformStack.Peek().mMatrix;
		}
		else
		{
			transform = null;
		}
	}

	public void BeginPrimitiveBatch(Image texture)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (!primitiveBatch.HasBegun)
		{
			EndFrame();
			GetTransform(out var transform);
			primitiveBatch.Begin((PrimitiveType)0, mTransX, mTransY, transform, texture, NormalSamplerState);
		}
		else
		{
			GetTransform(out var transform2);
			if (transform2.HasValue)
			{
				primitiveBatch.Transform = transform2.Value;
			}
			else
			{
				primitiveBatch.Transform = Matrix.Identity;
			}
			primitiveBatch.Texture = texture;
			primitiveBatch.OffsetX = mTransX;
			primitiveBatch.OffsetY = mTransY;
		}
		SetDrawMode(mDrawMode);
	}

	public void EndDrawImageTransformed()
	{
		EndDrawImageTransformed(startSpritebatch: true);
	}

	public void EndDrawImageTransformed(bool startSpritebatch)
	{
		if (primitiveBatch.HasBegun)
		{
			primitiveBatch.End();
			if (startSpritebatch)
			{
				BeginFrame();
			}
		}
		else if (!spritebatchBegan && startSpritebatch)
		{
			BeginFrame();
		}
	}

	public void DrawImageWithBasicEffect(Image theImage, VertexPositionTexture[] verts, short[] indices, Matrix world, Matrix view, Matrix projection)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (primitiveBatch.HasBegun)
		{
			primitiveBatch.End();
		}
		quadEffect.World = world;
		quadEffect.View = view;
		quadEffect.Projection = projection;
		quadEffect.TextureEnabled = true;
		quadEffect.Texture = theImage.Texture;
		foreach (EffectPass pass in ((Effect)quadEffect).CurrentTechnique.Passes)
		{
			pass.Apply();
			GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>((PrimitiveType)0, verts, 0, 4, indices, 0, 2);
		}
	}

	public void DrawImageTransformed(Image theImage, ref Matrix theTransform, bool center, Color theColor, TRect theSrcRect, bool clip)
	{
		BeginPrimitiveBatch(theImage);
		TRect destination = new TRect(-mTransX, -mTransY, theImage.GetCelWidth(), theImage.GetCelHeight());
		var center2 = center ? new Vector2(theSrcRect.mWidth * 0.5f, theSrcRect.mHeight * 0.5f) : Vector2.Zero;
		if (add)
			theColor.A = 0;

		primitiveBatch.Draw(theImage, destination, theSrcRect, ref theTransform, center2, theColor, extraOffset: false, sourceOffsetsUsed: true);
	}

	public void DrawImageRotatedScaled(Image theImage, TRect dest, TRect src, Color col, float rotation, Vector2 scale, Vector2 origin)
	{
		BeginPrimitiveBatch(theImage);
		primitiveBatch.DrawRotatedScaled(theImage, dest, src, origin, rotation, scale, col, extraOffset: false, sourceOffsetsUsed: false, PrimitiveBatchEffects.None);
	}

	public void DrawImage(Image theImage, float theX, float theY)
	{
		DrawImage(theImage, (int)theX, (int)theY);
	}

	public void DrawImage(Image theImage, int theX, int theY)
	{
		BeginPrimitiveBatch(theImage);
		TRect source = new TRect(theImage.mS, theImage.mT, theImage.mWidth, theImage.mHeight);
		TRect destination = new TRect(theX + mTransX, theY + mTransY, source.mWidth, source.mHeight);
		if (source.mWidth > 0 && source.mHeight > 0)
		{
			PrepareRectsForClipping(ref source, ref destination);
			primitiveBatch.Draw(theImage, destination, source, mColorizeImages ? base.mColor : Color.White, extraOffset: true, sourceOffsetsUsed: false);
		}
	}

	public void DrawImage(Image theImage, int theX, int theY, TRect theSrcRect)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		BeginPrimitiveBatch(theImage);
		TRect source = new TRect(theImage.mS + theSrcRect.mX, theImage.mT + theSrcRect.mY, theSrcRect.mWidth, theSrcRect.mHeight);
		TRect destination = new TRect(theX + mTransX, theY + mTransY, source.mWidth, source.mHeight);
		if (theSrcRect.mWidth > 0 && theSrcRect.mHeight > 0)
		{
			PrepareRectsForClipping(ref source, ref destination);
			primitiveBatch.Draw(theImage, destination, source, mColorizeImages ? base.mColor : Color.White, extraOffset: true, sourceOffsetsUsed: false);
		}
	}

	public void DrawImage(Image theImage, TRect theDestRect, TRect theSrcRect)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		BeginPrimitiveBatch(theImage);
		theDestRect.mX += mTransX;
		theDestRect.mY += mTransY;
		theSrcRect.mX += theImage.mS;
		theSrcRect.mY += theImage.mT;
		PrepareRectsForClipping(ref theSrcRect, ref theDestRect);
		primitiveBatch.Draw(theImage, theDestRect, theSrcRect, mColorizeImages ? base.mColor : Color.White, extraOffset: true, sourceOffsetsUsed: false);
	}

	public void DrawImage(Texture2D theImage, int theX, int theY, int theStretchedWidth, int theStretchedHeight)
	{
		temp.Reset(theImage);
		DrawImage(temp, theX, theY, theStretchedWidth, theStretchedHeight);
	}

	public void DrawImage(Image theImage, int theX, int theY, int theStretchedWidth, int theStretchedHeight)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		BeginPrimitiveBatch(theImage);
		TRect destination = new TRect(theX + mTransX, theY + mTransY, theStretchedWidth, theStretchedHeight);
		TRect source = new TRect(theImage.mS, theImage.mT, theImage.mWidth, theImage.mHeight);
		PrepareRectsForClipping(ref source, ref destination);
		primitiveBatch.Draw(theImage, destination, source, mColorizeImages ? base.mColor : Color.White, extraOffset: true, sourceOffsetsUsed: false);
	}

	public void DrawImageMirrorVertical(Image theImage, int theX, int theY, int theStretchedWidth, int theStretchedHeight)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		BeginPrimitiveBatch(theImage);
		TRect destination = new TRect(theX + mTransX, theY + mTransY, theStretchedWidth, theStretchedHeight);
		TRect source = new TRect(theImage.mS, theImage.mT, theImage.mWidth, theImage.mHeight);
		PrepareRectsForClipping(ref source, ref destination);
		primitiveBatch.Draw(theImage, destination, source, mColorizeImages ? base.mColor : Color.White, extraOffset: true, sourceOffsetsUsed: false, PrimitiveBatchEffects.MirrorVertically);
	}

	public void DrawImageF(Image theImage, float theX, float theY)
	{
		DrawImage(theImage, (int)theX, (int)theY);
	}

	public void DrawImageF(Image theImage, float theX, float theY, TRect theSrcRect)
	{
		DrawImage(theImage, (int)theX, (int)theY, theSrcRect);
	}

	public void DrawImageMirror(Image theImage, int theX, int theY)
	{
		DrawImageMirror(theImage, theX, theY, mirror: true);
	}

	public void DrawImageMirror(Image theImage, int theX, int theY, bool mirror)
	{
		DrawImageMirror(theImage, theX, theY, new TRect(theImage.mS, theImage.mT, theImage.mWidth, theImage.mHeight), mirror);
	}

	public void DrawImageMirror(Image theImage, int theX, int theY, TRect theSrcRect)
	{
		DrawImageMirror(theImage, theX, theY, theSrcRect, mirror: true);
	}

	public void DrawImageMirror(Image theImage, TRect theDestRect, TRect theSrcRect)
	{
		DrawImageMirror(theImage, theDestRect, theSrcRect, mirror: true);
	}

	public void DrawImageMirror(Image theImage, TRect theDestRect, TRect theSrcRect, bool mirror)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		BeginPrimitiveBatch(theImage);
		PrepareRectsForClipping(ref theSrcRect, ref theDestRect);
		primitiveBatch.Draw(theImage, theDestRect, theSrcRect, mColorizeImages ? base.mColor : Color.White, extraOffset: false, sourceOffsetsUsed: true, PrimitiveBatchEffects.MirrorHorizontally);
	}

	public void DrawImageMirror(Image theImage, int theX, int theY, TRect theSrcRect, bool mirror)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		BeginPrimitiveBatch(theImage);
		TRect destination = new TRect(theX + mTransX, theY + mTransY, theSrcRect.mWidth, theSrcRect.mHeight);
		PrepareRectsForClipping(ref theSrcRect, ref destination);
		primitiveBatch.Draw(theImage, destination, theSrcRect, mColorizeImages ? base.mColor : Color.White, extraOffset: true, sourceOffsetsUsed: true, mirror ? PrimitiveBatchEffects.MirrorHorizontally : PrimitiveBatchEffects.None);
	}

	public void DrawImageRotated(Image theImage, int theX, int theY, double theRot)
	{
		DrawImageRotated(theImage, theX, theY, theRot, new TRect(0, 0, theImage.GetWidth(), theImage.GetHeight()));
	}

	public void DrawImageRotated(Image theImage, int theX, int theY, double theRot, TRect theSrcRect)
	{
		theSrcRect.mX += theImage.mS;
		theSrcRect.mY += theImage.mT;
		DrawImageRotatedF(theImage, theX, theY, theRot, theSrcRect);
	}

	public void DrawImageRotated(Image theImage, int theX, int theY, double theRot, int theRotCenterX, int theRotCenterY)
	{
		DrawImageRotated(theImage, theX, theY, theRot, theRotCenterX, theRotCenterY, new TRect(0, 0, theImage.GetWidth(), theImage.GetHeight()));
	}

	public void DrawImageRotated(Image theImage, int theX, int theY, double theRot, int theRotCenterX, int theRotCenterY, TRect theSrcRect)
	{
		DrawImageRotatedF(theImage, theX, theY, theRot, theRotCenterX, theRotCenterY, theSrcRect);
	}

	public void DrawImageRotatedF(Image theImage, float theX, float theY, double theRot)
	{
		DrawImageRotatedF(theImage, theX, theY, theRot, new TRect(theImage.mS, theImage.mT, theImage.GetWidth(), theImage.GetHeight()));
	}

	public void DrawImageRotatedF(Image theImage, float theX, float theY, double theRot, TRect theSrcRect)
	{
		int num = theSrcRect.mWidth / 2;
		int num2 = theSrcRect.mHeight / 2;
		DrawImageRotatedF(theImage, theX, theY, theRot, num, num2, theSrcRect);
	}

	public void DrawImageRotatedF(Image theImage, float theX, float theY, double theRot, float theRotCenterX, float theRotCenterY)
	{
		DrawImageRotatedF(theImage, theX, theY, theRot, theRotCenterX, theRotCenterY, new TRect(theImage.mS, theImage.mT, theImage.GetWidth(), theImage.GetHeight()));
	}

	public void DrawImageRotatedF(Image theImage, float theX, float theY, double theRot, float theRotCenterX, float theRotCenterY, TRect? theSrcRect)
	{
		DrawImageRotatedScaled(theImage, theX, theY, theRot, theRotCenterX, theRotCenterY, theSrcRect, theImage.GetCelWidth(), theImage.GetCelHeight());
	}

	public void DrawImageRotatedScaled(Image theImage, float theX, float theY, double theRot, float theRotCenterX, float theRotCenterY, TRect? theSrcRect, int stretchedHeight, int stretchedWidth)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		BeginPrimitiveBatch(theImage);
		if (!theSrcRect.HasValue)
		{
			TRect source = new TRect(theImage.mS, theImage.mT, theImage.mWidth, theImage.mHeight);
			TRect destination = new TRect((int)theX, (int)theY, stretchedHeight, stretchedWidth);
			primitiveBatch.DrawRotatedScaled(theImage, destination, source, new Vector2(theRotCenterX, theRotCenterY), (float)theRot, new Vector2((float)theImage.mWidth / (float)stretchedWidth, (float)theImage.mHeight / (float)stretchedHeight), mColorizeImages ? base.mColor : Color.White, extraOffset: false, sourceOffsetsUsed: true, PrimitiveBatchEffects.None);
		}
		else
		{
			TRect destination2 = new TRect((int)theX, (int)theY, stretchedHeight, stretchedWidth);
			primitiveBatch.DrawRotatedScaled(theImage, destination2, theSrcRect.Value, new Vector2(theRotCenterX, theRotCenterY), (float)theRot, new Vector2((float)theSrcRect.Value.mWidth / (float)stretchedWidth, (float)theSrcRect.Value.mHeight / (float)stretchedHeight), mColorizeImages ? base.mColor : Color.White, extraOffset: false, sourceOffsetsUsed: true, PrimitiveBatchEffects.None);
		}
	}

	public void DrawTriangle(TriVertex p1, TriVertex p2, TriVertex p3, Color theColor, DrawMode theDrawMode)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		EndDrawImageTransformed();
		bool flag = spritebatchBegan;
		if (spritebatchBegan)
		{
			EndFrame();
		}
		SetupDrawMode(theDrawMode);
		Matrix? transform = null;
		if (gTransformStack.Count > 0)
		{
			transform = gTransformStack.Peek().mMatrix;
		}
		primitiveBatch.Begin((PrimitiveType)0, mTransX, mTransY, transform, null, NormalSamplerState);
		primitiveBatch.AddVertex(new Vector2(p1.x, p1.y), theColor);
		primitiveBatch.AddVertex(new Vector2(p2.x, p2.y), theColor);
		primitiveBatch.AddVertex(new Vector2(p3.x, p3.y), theColor);
		primitiveBatch.End();
		if (flag)
		{
			BeginFrame();
		}
	}

	public void DrawTriangle(TriVertex p1, Color c1, TriVertex p2, Color c2, TriVertex p3, Color c3, DrawMode theDrawMode)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		EndDrawImageTransformed();
		bool flag = spritebatchBegan;
		if (spritebatchBegan)
		{
			EndFrame();
		}
		SetupDrawMode(theDrawMode);
		Matrix? transform = null;
		if (gTransformStack.Count > 0)
		{
			transform = gTransformStack.Peek().mMatrix;
		}
		primitiveBatch.Begin((PrimitiveType)0, mTransX, mTransY, transform, null, NormalSamplerState);
		primitiveBatch.AddVertex(new Vector2(p1.x, p1.y), c1);
		primitiveBatch.AddVertex(new Vector2(p2.x, p2.y), c2);
		primitiveBatch.AddVertex(new Vector2(p3.x, p3.y), c3);
		primitiveBatch.primitiveCount = 1;
		primitiveBatch.End();
		if (flag)
		{
			BeginFrame();
		}
	}

	public void DrawImageCel(Image theImageStrip, int theX, int theY, int theCel)
	{
		DrawImageCel(theImageStrip, theX, theY, theCel % theImageStrip.mNumCols, theCel / theImageStrip.mNumCols);
	}

	public void DrawImageCel(Image theImageStrip, TRect theDestRect, int theCel)
	{
		DrawImageCel(theImageStrip, theDestRect, theCel % theImageStrip.mNumCols, theCel / theImageStrip.mNumCols);
	}

	public void DrawImageCel(Image theImageStrip, int theX, int theY, int theCelCol, int theCelRow)
	{
		if (theCelRow >= 0 && theCelCol >= 0 && theCelRow < theImageStrip.mNumRows && theCelCol < theImageStrip.mNumCols)
		{
			int num = theImageStrip.mWidth / theImageStrip.mNumCols;
			int num2 = theImageStrip.mHeight / theImageStrip.mNumRows;
			TRect theSrcRect = new TRect(num * theCelCol, num2 * theCelRow, num, num2);
			DrawImage(theImageStrip, theX, theY, theSrcRect);
		}
	}

	public void DrawImageCel(Image theImageStrip, TRect theDestRect, int theCelCol, int theCelRow)
	{
		if (theCelRow >= 0 && theCelCol >= 0 && theCelRow < theImageStrip.mNumRows && theCelCol < theImageStrip.mNumCols)
		{
			int num = theImageStrip.mWidth / theImageStrip.mNumCols;
			int num2 = theImageStrip.mHeight / theImageStrip.mNumRows;
			TRect theSrcRect = new TRect(num * theCelCol, num2 * theCelRow, num, num2);
			if (num != theDestRect.mWidth || num2 != theDestRect.mHeight)
			{
				DrawImage(theSrcRect: new TRect(num * theCelCol, num2 * theCelRow, num, num2), theImage: theImageStrip, theDestRect: theDestRect);
				return;
			}
			DrawImage(theImageStrip, theDestRect.mX, theDestRect.mY, theSrcRect);
		}
	}

	public void DrawImageAnim(Image theImageAnim, int theX, int theY, int theTime)
	{
		DrawImageCel(theImageAnim, theX, theY, theImageAnim.GetAnimCel(theTime));
	}

	public void ClearClipRect()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		TRect tRect = ((mDestImage == null) ? new TRect(0, 0, GlobalStaticVars.gSexyAppBase.mWidth, GlobalStaticVars.gSexyAppBase.mHeight) : new TRect(0, 0, ((Texture2D)mDestImage).Bounds.Width, ((Texture2D)mDestImage).Bounds.Height));
		mClipRect = tRect;
	}

	public void SetClipRect(int theX, int theY, int theWidth, int theHeight)
	{
		ClearClipRect();
		mClipRect = mClipRect.Intersection(new TRect(theX + mTransX, theY + mTransY, theWidth, theHeight));
	}

	public void SetClipRect(TRect theRect)
	{
		SetClipRect(ref theRect);
	}

	public void SetClipRect(ref TRect theRect)
	{
		SetClipRect(theRect.mX, theRect.mY, theRect.mWidth, theRect.mHeight);
	}

	public TRect GetClipRect()
	{
		return mClipRect;
	}

	public void ClipRect(int theX, int theY, int theWidth, int theHeight)
	{
		TRect tRect = new TRect(theX + mTransX, theY + mTransY, theWidth, theHeight);
		if (!(tRect == mClipRect))
		{
			mClipRect = mClipRect.Intersection(tRect);
		}
	}

	public void ClipRect(TRect theRect)
	{
		ClipRect(theRect.mX, theRect.mY, theRect.mWidth, theRect.mHeight);
	}

	public int StringWidth(string theString)
	{
		return mFont.StringWidth(theString);
	}

	public void DrawImageBox(TRect theDest, Image theComponentImage)
	{
		DrawImageBox(new TRect(0, 0, theComponentImage.mWidth, theComponentImage.mHeight), theDest, theComponentImage);
	}

	public void DrawImageBox(TRect theSrc, TRect theDest, Image theComponentImage)
	{
		if (theSrc.mWidth <= 0 || theSrc.mHeight <= 0)
		{
			return;
		}
		int num = theSrc.mWidth / 3;
		int num2 = theSrc.mHeight / 3;
		int mX = theSrc.mX;
		int mY = theSrc.mY;
		int num3 = theSrc.mWidth - num * 2;
		int num4 = theSrc.mHeight - num2 * 2;
		DrawImage(theComponentImage, theDest.mX, theDest.mY, new TRect(mX, mY, num, num2));
		DrawImage(theComponentImage, theDest.mX + theDest.mWidth - num, theDest.mY, new TRect(mX + num + num3, mY, num, num2));
		DrawImage(theComponentImage, theDest.mX, theDest.mY + theDest.mHeight - num2, new TRect(mX, mY + num2 + num4, num, num2));
		DrawImage(theComponentImage, theDest.mX + theDest.mWidth - num, theDest.mY + theDest.mHeight - num2, new TRect(mX + num + num3, mY + num2 + num4, num, num2));
		Graphics @new = GetNew(this);
		@new.ClipRect(theDest.mX + num, theDest.mY, theDest.mWidth - num * 2, theDest.mHeight);
		for (int i = 0; i < (theDest.mWidth - num * 2 + num3 - 1) / num3; i++)
		{
			@new.DrawImage(theComponentImage, theDest.mX + num + i * num3, theDest.mY, new TRect(mX + num, mY, num3, num2));
			@new.DrawImage(theComponentImage, theDest.mX + num + i * num3, theDest.mY + theDest.mHeight - num2, new TRect(mX + num, mY + num2 + num4, num3, num2));
		}
		@new.PrepareForReuse();
		Graphics new2 = GetNew(this);
		new2.ClipRect(theDest.mX, theDest.mY + num2, theDest.mWidth, theDest.mHeight - num2 * 2);
		for (int j = 0; j < (theDest.mHeight - num2 * 2 + num4 - 1) / num4; j++)
		{
			new2.DrawImage(theComponentImage, theDest.mX, theDest.mY + num2 + j * num4, new TRect(mX, mY + num2, num, num4));
			new2.DrawImage(theComponentImage, theDest.mX + theDest.mWidth - num, theDest.mY + num2 + j * num4, new TRect(mX + num + num3, mY + num2, num, num4));
		}
		new2.PrepareForReuse();
		Graphics new3 = GetNew(this);
		new3.ClipRect(theDest.mX + num, theDest.mY + num2, theDest.mWidth - num * 2, theDest.mHeight - num2 * 2);
		for (int i = 0; i < (theDest.mWidth - num * 2 + num3 - 1) / num3; i++)
		{
			for (int j = 0; j < (theDest.mHeight - num2 * 2 + num4 - 1) / num4; j++)
			{
				new3.DrawImage(theComponentImage, theDest.mX + num + i * num3, theDest.mY + num2 + j * num4, new TRect(mX + num, mY + num2, num3, num4));
			}
		}
		new3.PrepareForReuse();
	}

	public int WriteString(string theString, int theX, int theY, int theWidth, int theJustification, bool drawString, int theOffset, int theLength)
	{
		return WriteString(theString, theX, theY, theWidth, theJustification, drawString, theOffset, theLength, -1);
	}

	public int WriteString(string theString, int theX, int theY, int theWidth, int theJustification, bool drawString, int theOffset, int theLength, int theOldColor)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		mFont.DrawString(this, theX, theY, theString, new SexyColor(theOldColor.ToString()));
		return theX;
	}

	public int WriteWordWrappedLayer(TRect theRect, string theLine, int theLineSpacing, int theJustification, int layer)
	{
		return WriteWordWrappedLayer(theRect, theLine, theLineSpacing, theJustification, 0, -1, 0, 0, layer, centerVertically: false);
	}

	public int WriteWordWrappedLayer(TRect theRect, string theLine, int theLineSpacing, int theJustification, int layer, bool centerVertically)
	{
		return WriteWordWrappedLayer(theRect, theLine, theLineSpacing, theJustification, 0, -1, 0, 0, layer, centerVertically);
	}

	public int WriteWordWrappedLayer(TRect theRect, string theLine, int theLineSpacing, int theJustification, int theMaxWidth, int theMaxChars, int theLastWidth, int theLineCount, int layer, bool centerVertically)
	{
		Font.CachedStringInfo wordWrappedSubStrings = mFont.GetWordWrappedSubStrings(theLine, theRect);
		theRect.mX += mTransX;
		theRect.mY += mTransY;
		var val = new Vector2((float)theRect.mX, (float)theRect.mY);
		mFont.GetHeight();
		if (centerVertically)
		{
			float num = 0f;
			for (var i = 0; i < wordWrappedSubStrings.Strings.Length; i++)
				num += wordWrappedSubStrings.StringDimensions[i].Y;

			val.Y += (float)(theRect.mHeight / 2) - num / 2f;
		}
		for (int j = 0; j < wordWrappedSubStrings.Strings.Length; j++)
		{
			val.X = theRect.mX;
			if (theJustification == 0)
			{
				val.X += ((float)theRect.mWidth - wordWrappedSubStrings.StringDimensions[j].X) / 2f;
			}
			mFont.DrawStringLayer(this, (int)(val.X + 0.5f), (int)(val.Y + 0.5f), wordWrappedSubStrings.Strings[j], base.mColor, layer);
			val.Y += wordWrappedSubStrings.StringDimensions[j].Y;
		}
		return (int)val.Y;
	}

	public int WriteWordWrapped(TRect theRect, string theLine, int theLineSpacing, int theJustification)
	{
		return WriteWordWrapped(theRect, theLine, theLineSpacing, theJustification, centerVertically: false);
	}

	public int WriteWordWrapped(TRect theRect, string theLine, int theLineSpacing, int theJustification, bool centerVertically)
	{
		return WriteWordWrapped(theRect, theLine, theLineSpacing, theJustification, 0, -1, 0, 0, centerVertically);
	}

	public int WriteWordWrapped(TRect theRect, string theLine, int theLineSpacing, int theJustification, int theMaxWidth, int theMaxChars, int theLastWidth, int theLineCount)
	{
		return WriteWordWrapped(theRect, theLine, theLineSpacing, theJustification, theMaxWidth, theMaxChars, theLastWidth, theLineCount, centerVertically: false);
	}

	public int WriteWordWrapped(TRect theRect, string theLine, int theLineSpacing, int theJustification, int theMaxWidth, int theMaxChars, int theLastWidth, int theLineCount, bool centerVertically)
	{
		return WriteWordWrapped(theRect, theLine, theLineSpacing, theJustification, theMaxWidth, theMaxChars, theLastWidth, theLineCount, centerVertically, doDraw: true);
	}

	public int WriteWordWrapped(TRect theRect, string theLine, int theLineSpacing, int theJustification, int theMaxWidth, int theMaxChars, int theLastWidth, int theLineCount, bool centerVertically, bool doDraw)
	{
		Font.CachedStringInfo wordWrappedSubStrings = mFont.GetWordWrappedSubStrings(theLine, theRect);
		theRect.mX += mTransX;
		theRect.mY += mTransY;
		var val = new Vector2((float)theRect.mX, (float)theRect.mY);
		mFont.GetHeight();
		if (centerVertically)
		{
			float num = 0f;
			for (var i = 0; i < wordWrappedSubStrings.Strings.Length; i++)
				num += wordWrappedSubStrings.StringDimensions[i].Y;

			val.Y += (float)(theRect.mHeight / 2) - num / 2f;
		}
		for (int j = 0; j < wordWrappedSubStrings.Strings.Length; j++)
		{
			val.X = theRect.mX;
			if (theJustification == 0)
			{
				val.X += ((float)theRect.mWidth - wordWrappedSubStrings.StringDimensions[j].X) / 2f;
			}
			if (doDraw)
			{
				mFont.DrawString(this, (int)(val.X + 0.5f), (int)(val.Y + 0.5f), wordWrappedSubStrings.Strings[j], base.mColor);
			}
			val.Y += wordWrappedSubStrings.StringDimensions[j].Y;
		}
		return (int)val.Y;
	}

	public void DrawStringColor(string theLine, int theX, int theY, int theOldColor)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		mFont.DrawString(this, theX, theY, theLine, new SexyColor(theOldColor.ToString()));
	}

	public int GetWordWrappedHeight(int theWidth, string theLine, int theLineSpacing, ref int theMaxWidth, int theMaxChars)
	{
		Graphics @new = GetNew();
		@new.SetFont(mFont);
		@new.SetClipRect(0, 0, 0, 0);
		int result = @new.WriteWordWrapped(new TRect(0, 0, theWidth, 0), theLine, theLineSpacing, -1, theMaxWidth, theMaxChars, 0, 0, centerVertically: false, doDraw: false);
		@new.PrepareForReuse();
		return result;
	}

	public bool Is3D()
	{
		return true;
	}

	internal void PopState()
	{
		if (mStateStack.Count > 0)
		{
			_ = mDrawMode;
			_ = mStateStack.Peek().mDrawMode;
			CopyStateFrom(mStateStack.Peek());
			Graphics graphics = mStateStack.Peek();
			bool flag = graphics.mDrawMode != mDrawMode;
			graphics.PrepareForReuse();
			mStateStack.Pop();
			if (flag && spritebatchBegan)
			{
				EndFrame();
				BeginFrame();
			}
		}
	}

	internal void PushState()
	{
		Graphics @new = GetNew(this);
		mStateStack.Push(@new);
		if (mDrawMode != @new.mDrawMode && spritebatchBegan)
		{
			EndFrame();
			BeginFrame();
		}
	}

	internal void Translate(int x, int y)
	{
		mTransX += x;
		mTransY += y;
	}

	public void Reset()
	{
		mTransX = 0;
		mTransY = 0;
		mScaleX = 1f;
		mScaleY = 1f;
		mScaleOrigX = 0f;
		mScaleOrigY = 0f;
		mFastStretch = false;
		mWriteColoredString = false;
		mLinearBlend = false;
		mClipRect = new TRect(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
		ClearClipRect();
		mColor = Color.White;
		mDrawMode = currentlyActiveDrawMode;
		mColorizeImages = false;
	}

	internal void PrepareDrawing()
	{
		BeginFrame();
	}

	internal void FinishedDrawing()
	{
		EndFrame();
	}

	public void DrawTriangleTex(TriVertex p1, TriVertex p2, TriVertex p3, Color theColor, DrawMode theDrawMode, Image theTexture)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		DrawTriangleTex(p1, p2, p3, theColor, theDrawMode, theTexture, blend: true);
	}

	public void DrawTriangleTex(TriVertex p1, TriVertex p2, TriVertex p3, Color theColor, DrawMode theDrawMode, Image theTexture, bool blend)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		tempTriangles[0, 0] = p1;
		tempTriangles[0, 1] = p2;
		tempTriangles[0, 2] = p3;
		DrawTrianglesTex(tempTriangles, 1, theColor, theDrawMode, theTexture, mTransX, mTransY, blend);
	}

	public void DrawTriangleTex(Image theTexture, TriVertex v1, TriVertex v2, TriVertex v3)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		DrawTriangleTex(v1, v2, v3, mColorizeImages ? base.mColor : Color.White, mDrawMode, theTexture);
	}

	public void DrawTrianglesTex(Image theTexture, TriVertex[,] theVertices, int theNumTriangles)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		DrawTrianglesTex(theVertices, theNumTriangles, mColorizeImages ? base.mColor : Color.White, mDrawMode, theTexture, mTransX, mTransY, mLinearBlend);
	}

	public void DrawTrianglesTex(Image theTexture, TriVertex[,] theVertices, int theNumTriangles, Color? theColor, DrawMode theDrawMode)
	{
		DrawTrianglesTex(theVertices, theNumTriangles, theColor, theDrawMode, theTexture, mTransX, mTransY, mLinearBlend);
	}

	public void DrawTrianglesTex(SamplerState st, Image theTexture, TriVertex[,] theVertices, int theNumTriangles, Color? theColor, DrawMode theDrawMode)
	{
		DrawTrianglesTex(st, theVertices, theNumTriangles, theColor, theDrawMode, theTexture, mTransX, mTransY, mLinearBlend);
	}

	public void DrawTrianglesTex(TriVertex[,] theVertices, int theNumTriangles, Color? theColor, DrawMode theDrawMode, Image theTexture, float tx, float ty, bool blend)
	{
		DrawTrianglesTex(null, theVertices, theNumTriangles, theColor, theDrawMode, theTexture, tx, ty, blend);
	}

	public void DrawTrianglesTex(TriVertex[,] theVertices, int theNumTriangles, Color theColor, DrawMode theDrawMode, Image theTexture)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		DrawTrianglesTex(theVertices, theNumTriangles, theColor, theDrawMode, theTexture, 0f, 0f, blend: true);
	}

	public void DrawTrianglesTex(SamplerState st, TriVertex[,] theVertices, int theNumTriangles, Color? theColor, DrawMode theDrawMode, Image theTexture, float tx, float ty, bool blend)
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		bool flag = spritebatchBegan;
		if (spritebatchBegan)
		{
			EndFrame();
		}
		EndDrawImageTransformed(startSpritebatch: false);
		for (int i = 0; i < theNumTriangles; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int width = theTexture.Texture.Width;
				int height = theTexture.Texture.Height;
				theVertices[i, j].u = (theVertices[i, j].u * (float)theTexture.GetWidth() + (float)theTexture.mS) / (float)width;
				theVertices[i, j].v = (theVertices[i, j].v * (float)theTexture.GetHeight() + (float)theTexture.mT) / (float)height;
			}
		}
		SetupDrawMode(theDrawMode);
		Matrix? transform = null;
		if (gTransformStack.Count > 0)
		{
			transform = gTransformStack.Peek().mMatrix;
		}
		if (st != null)
		{
			GraphicsDevice.SamplerStates[0] = st;
		}
		primitiveBatch.Begin((PrimitiveType)0, mTransX, mTransY, transform, theTexture, NormalSamplerState);
		primitiveBatch.AddTriVertices(theVertices, theNumTriangles, theColor);
		primitiveBatch.End();
		if (flag)
		{
			BeginFrame();
		}
	}

	public void BeginDrawTrianglesTexBatch(SamplerState st, DrawMode theDrawMode, Image theTexture)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (spritebatchBegan)
		{
			EndFrame();
		}
		SetupDrawMode(theDrawMode);
		Matrix? transform = null;
		if (gTransformStack.Count > 0)
		{
			transform = gTransformStack.Peek().mMatrix;
		}
		if (st != null)
		{
			GraphicsDevice.SamplerStates[0] = st;
		}
		primitiveBatch.Begin((PrimitiveType)0, mTransX, mTransY, transform, theTexture, NormalSamplerState);
		triangleBatchTexture = theTexture;
	}

	public void DrawTrianglesTexBatch(TriVertex[,] theVertices, int theNumTriangles, Color? theColor)
	{
		for (int i = 0; i < theNumTriangles; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int width = triangleBatchTexture.Texture.Width;
				int height = triangleBatchTexture.Texture.Height;
				theVertices[i, j].u = (theVertices[i, j].u * (float)triangleBatchTexture.GetWidth() + (float)triangleBatchTexture.mS) / (float)width;
				theVertices[i, j].v = (theVertices[i, j].v * (float)triangleBatchTexture.GetHeight() + (float)triangleBatchTexture.mT) / (float)height;
			}
		}
		primitiveBatch.AddTriVertices(theVertices, theNumTriangles, theColor);
	}

	public void EndDrawTrianglesTexBatch()
	{
		primitiveBatch.End();
		BeginFrame();
	}

	public void pushTransform(ref SexyTransform2D theTransform, bool concatenate)
	{
		if (gTransformStack.empty() || !concatenate)
		{
			gTransformStack.push_back(theTransform);
		}
		else
		{
			SexyTransform2D sexyTransform2D = gTransformStack.back();
			gTransformStack.push_back(theTransform * sexyTransform2D);
		}
		if (spritebatchBegan)
		{
			EndFrame();
			BeginFrame();
		}
	}

	public void popTransform()
	{
		if (!gTransformStack.empty())
		{
			gTransformStack.pop_back();
			if (spritebatchBegan)
			{
				EndFrame();
				BeginFrame();
			}
		}
	}

	public static void PushTransform(ref SexyTransform2D theTransform, bool concatenate)
	{
		GlobalStaticVars.g.pushTransform(ref theTransform, concatenate);
	}

	public static void PushTransform(ref SexyTransform2D theTransform)
	{
		PushTransform(ref theTransform, concatenate: true);
	}

	public static void ClearTransformStack()
	{
		GlobalStaticVars.g.clearTransformStack();
	}

	public void clearTransformStack()
	{
		gTransformStack.Clear();
	}

	public static void PopTransform()
	{
		GlobalStaticVars.g.popTransform();
	}

	public bool MatchesHardWareClipRect(TRect clip)
	{
		return hardwareClippedRectangle == clip.Intersection(new TRect(0, 0, base.mScreenWidth, base.mScreenHeight));
	}

	public void HardwareClip()
	{
		HardwareClip((SpriteSortMode)0);
	}

	public void HardwareClip(SpriteSortMode spriteSortMode)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		EndFrame();
		TRect tRect = mClipRect;
		tRect = tRect.Intersection(new TRect(0, 0, base.mScreenWidth, base.mScreenHeight));
		GraphicsDevice.ScissorRectangle = tRect;
		hardwareClippingEnabled = true;
		hardwareClippedRectangle = mClipRect;
		BeginFrame(hardwareClipState, spriteSortMode);
	}

	public void EndHardwareClip()
	{
		hardwareClippingEnabled = false;
		hardwareClippedRectangle = TRect.Empty;
		EndFrame();
		BeginFrame();
	}

	public void HardwareClipRect(TRect theClip)
	{
		HardwareClipRect(theClip, (SpriteSortMode)0);
	}

	public void HardwareClipRect(TRect theClip, SpriteSortMode sortMode)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		EndFrame();
		theClip.mX += mTransX;
		theClip.mY += mTransY;
		Rectangle val = theClip.Intersection(new TRect(0, 0, base.mScreenWidth, base.mScreenHeight));
		hardwareClippingEnabled = true;
		hardwareClippedRectangle = (TRect)val;
		GraphicsDevice.ScissorRectangle = val;
		BeginFrame(hardwareClipState, sortMode);
	}

	public void ClearHardwareClipRect()
	{
		spriteBatch.End();
		BeginFrame();
	}

	static Graphics()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		SamplerState val = new SamplerState();
		val.AddressU = (TextureAddressMode)0;
		val.AddressV = (TextureAddressMode)0;
		WrapSamplerState = val;
		SamplerState val2 = new SamplerState();
		val2.AddressU = (TextureAddressMode)1;
		val2.AddressV = (TextureAddressMode)1;
		val2.AddressW = (TextureAddressMode)1;
		val2.Filter = (TextureFilter)0;
		NormalSamplerState = val2;
		unusedObjects = new Stack<Graphics>(20);
		gTransformStack = new Stack<SexyTransform2D>();
		mStateStack = new Stack<Graphics>();
		temp = new Image();
		RasterizerState val3 = new RasterizerState();
		val3.ScissorTestEnable = true;
		val3.CullMode = (CullMode)0;
		hardwareClipState = val3;
	}
}
