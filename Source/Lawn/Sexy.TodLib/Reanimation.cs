using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Sexy.TodLib;

internal class Reanimation
{
	public const string Attacher = "attacher__";

	public static bool mInterpolate = true;

	public ReanimationType mReanimationType;

	public float mAnimTime;

	public float mAnimRate;

	public ReanimatorDefinition mDefinition;

	public ReanimLoopType mLoopType;

	public bool mDead;

	public short mFrameStart;

	public short mFrameCount;

	public short mFrameBasePose;

	public SexyTransform2D mOverlayMatrix;

	public SexyColor mColorOverride;

	public ReanimatorTrackInstance[] mTrackInstances = new ReanimatorTrackInstance[100];

	public int mLoopCount;

	public ReanimationHolder mReanimationHolder;

	public bool mIsAttachment;

	public int mRenderOrder;

	public SexyColor mExtraAdditiveColor;

	public bool mEnableExtraAdditiveDraw;

	public SexyColor mExtraOverlayColor;

	public bool mEnableExtraOverlayDraw;

	public float mLastFrameTime;

	public FilterEffectType mFilterEffect;

	public bool mClip;

	public bool mActive;

	private bool mGetFrameTime = true;

	private ReanimatorFrameTime mFrameTime;

	private SexyTransform2D aOverlayMatrix;

	public static string ReanimTrackId_fullscreen = ReanimatorXnaHelpers.ReanimatorTrackNameToId("fullscreen");

	public static string ReanimTrackId__ground = ReanimatorXnaHelpers.ReanimatorTrackNameToId("_ground");

	public static string ReanimTrackId_anim_walk = ReanimatorXnaHelpers.ReanimatorTrackNameToId("anim_walk");

	public static string ReanimTrackId_anim_crawl = ReanimatorXnaHelpers.ReanimatorTrackNameToId("anim_crawl");

	public static string ReanimTrackIdEmpty = ReanimatorXnaHelpers.ReanimatorTrackNameToId("");

	private static Matrix tempMatrix;

	private static Dictionary<string, string> lowercaseCache = new Dictionary<string, string>();

	private static Stack<Reanimation> unusedObjects = new Stack<Reanimation>(1000);

	private static bool didClipIgnore = false;

	private Matrix aBasePoseMatrix = default(Matrix);

	private Matrix tempOverlayMatrix = default(Matrix);

	private SexyTransform2D basePose;

	private static readonly SexyTransform2D identity = new SexyTransform2D(loadIdentity: true);

	public override string ToString()
	{
		return mReanimationType.ToString();
	}

	public static string ToLower(string s)
	{
		if (!lowercaseCache.TryGetValue(s, out var value))
		{
			value = s.ToLower();
			lowercaseCache.Add(s, value);
		}
		return value;
	}

	public static void PreallocateMemory()
	{
		for (int i = 0; i < 1000; i++)
		{
			new Reanimation().PrepareForReuse();
		}
	}

	public static Reanimation GetNewReanimation()
	{
		if (unusedObjects.Count > 0)
		{
			return unusedObjects.Pop();
		}
		return new Reanimation();
	}

	public void PrepareForReuse()
	{
		Reset();
		unusedObjects.Push(this);
	}

	protected void Reset()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < mTrackInstances.Length; i++)
		{
			if (mTrackInstances[i] != null)
			{
				mTrackInstances[i].PrepareForReuse();
			}
			mTrackInstances[i] = null;
		}
		mClip = false;
		mAnimTime = 0f;
		mAnimRate = 12f;
		mLastFrameTime = -1f;
		mDefinition = null;
		mLoopType = ReanimLoopType.REANIM_PLAY_ONCE;
		mDead = false;
		mFrameStart = 0;
		mFrameCount = 0;
		mFrameBasePose = -1;
		mOverlayMatrix.LoadIdentity();
		mColorOverride = new SexyColor(Color.White);
		mExtraAdditiveColor = new SexyColor(Color.White);
		mEnableExtraAdditiveDraw = false;
		mExtraOverlayColor = new SexyColor(Color.White);
		mEnableExtraOverlayDraw = false;
		mLoopCount = 0;
		mIsAttachment = false;
		mRenderOrder = 0;
		mReanimationHolder = null;
		mFilterEffect = FilterEffectType.FILTER_EFFECT_NONE;
		mReanimationType = ReanimationType.REANIM_NONE;
		mActive = false;
		mGetFrameTime = true;
	}

	private Reanimation()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Reset();
	}

	public void ReanimationInitialize(float theX, float theY, int theDefinition)
	{
		mDefinition = ReanimatorXnaHelpers.gReanimatorDefArray[theDefinition];
		mDead = false;
		SetPosition(theX, theY);
		mAnimRate = mDefinition.mFPS;
		mLastFrameTime = -1f;
		if (mDefinition.mTrackCount != 0)
		{
			mFrameCount = mDefinition.mTracks[0].mTransformCount;
			for (int i = 0; i < mDefinition.mTrackCount; i++)
			{
				ReanimatorTrackInstance newReanimatorTrackInstance = ReanimatorTrackInstance.GetNewReanimatorTrackInstance();
				mTrackInstances[i] = newReanimatorTrackInstance;
			}
		}
		else
		{
			mFrameCount = 0;
		}
	}

	public void ReanimationInitializeType(float theX, float theY, ReanimationType theReanimType)
	{
		ReanimatorXnaHelpers.ReanimatorEnsureDefinitionLoaded(theReanimType, theIsPreloading: false);
		mReanimationType = theReanimType;
		ReanimationInitialize(theX, theY, (int)theReanimType);
	}

	public void ReanimationDie()
	{
		if (!mDead)
		{
			mActive = false;
			mDead = true;
			for (int i = 0; i < mDefinition.mTrackCount; i++)
			{
				ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
				GlobalMembersAttachment.AttachmentDie(ref reanimatorTrackInstance.mAttachmentID);
			}
		}
	}

	public void Update()
	{
		mGetFrameTime = true;
		if (mFrameCount == 0 || mDead)
		{
			return;
		}
		mLastFrameTime = mAnimTime;
		mAnimTime += ReanimatorXnaHelpers.SECONDS_PER_UPDATE * mAnimRate / (float)mFrameCount;
		if (mAnimRate > 0f)
		{
			if (mLoopType == ReanimLoopType.REANIM_LOOP || mLoopType == ReanimLoopType.REANIM_LOOP_FULL_LAST_FRAME)
			{
				while (mAnimTime >= 1f)
				{
					mLoopCount++;
					mAnimTime -= 1f;
				}
			}
			else if (mLoopType == ReanimLoopType.REANIM_PLAY_ONCE || mLoopType == ReanimLoopType.REANIM_PLAY_ONCE_FULL_LAST_FRAME)
			{
				if (mAnimTime >= 1f)
				{
					mAnimTime = 1f;
					mLoopCount = 1;
					mDead = true;
				}
			}
			else if ((mLoopType == ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD || mLoopType == ReanimLoopType.REANIM_PLAY_ONCE_FULL_LAST_FRAME_AND_HOLD) && mAnimTime >= 1f)
			{
				mLoopCount = 1;
				mAnimTime = 1f;
			}
		}
		else if (mLoopType == ReanimLoopType.REANIM_LOOP || mLoopType == ReanimLoopType.REANIM_LOOP_FULL_LAST_FRAME)
		{
			while (mAnimTime < 0f)
			{
				mLoopCount++;
				mAnimTime += 1f;
			}
		}
		else if (mLoopType == ReanimLoopType.REANIM_PLAY_ONCE || mLoopType == ReanimLoopType.REANIM_PLAY_ONCE_FULL_LAST_FRAME)
		{
			if (mAnimTime < 0f)
			{
				mAnimTime = 0f;
				mLoopCount = 1;
				mDead = true;
			}
		}
		else if ((mLoopType == ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD || mLoopType == ReanimLoopType.REANIM_PLAY_ONCE_FULL_LAST_FRAME_AND_HOLD) && mAnimTime < 0f)
		{
			mLoopCount = 1;
			mAnimTime = 0f;
		}
		int mTrackCount = mDefinition.mTrackCount;
		for (int i = 0; i < mTrackCount; i++)
		{
			ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
			if (reanimatorTrackInstance.mRenderGroup != ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN)
			{
				if (reanimatorTrackInstance.mBlendCounter > 0)
				{
					reanimatorTrackInstance.mBlendCounter--;
				}
				if (reanimatorTrackInstance.mShakeOverride != 0f)
				{
					reanimatorTrackInstance.mShakeX = TodCommon.RandRangeFloat(0f - reanimatorTrackInstance.mShakeOverride, reanimatorTrackInstance.mShakeOverride);
					reanimatorTrackInstance.mShakeY = TodCommon.RandRangeFloat(0f - reanimatorTrackInstance.mShakeOverride, reanimatorTrackInstance.mShakeOverride);
				}
				ReanimatorTrack reanimatorTrack = mDefinition.mTracks[i];
				if (reanimatorTrack.IsAttacher)
				{
					UpdateAttacherTrack(i);
				}
				if (reanimatorTrackInstance.mAttachmentID != null)
				{
					GetAttachmentOverlayMatrix(i, out aOverlayMatrix);
					GlobalMembersAttachment.AttachmentUpdateAndSetMatrix(ref reanimatorTrackInstance.mAttachmentID, ref aOverlayMatrix);
				}
			}
		}
	}

	public void Draw(Graphics g)
	{
		mGetFrameTime = true;
		DrawRenderGroup(g, ReanimatorXnaHelpers.RENDER_GROUP_NORMAL);
	}

	public void DrawRenderGroup(Graphics g, int theRenderGroup)
	{
		if (mDead)
		{
			return;
		}
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
			if (reanimatorTrackInstance.mRenderGroup != theRenderGroup)
			{
				continue;
			}
			bool flag = DrawTrack(g, i, theRenderGroup);
			if (reanimatorTrackInstance.mAttachmentID == null)
			{
				continue;
			}
			Attachment mAttachmentID = reanimatorTrackInstance.mAttachmentID;
			for (int j = 0; j < mAttachmentID.mNumEffects; j++)
			{
				AttachEffect attachEffect = mAttachmentID.mEffectArray[j];
				if (attachEffect.mEffectType == EffectType.EFFECT_REANIM)
				{
					Reanimation reanimation = (Reanimation)attachEffect.mEffectID;
					reanimation.mColorOverride = mColorOverride;
					reanimation.mExtraAdditiveColor = mExtraAdditiveColor;
					reanimation.mExtraOverlayColor = mExtraOverlayColor;
				}
			}
			GlobalMembersAttachment.AttachmentDraw(reanimatorTrackInstance.mAttachmentID, g, !flag, doScale: false);
		}
	}

	private bool DrawTrack(Graphics g, int theTrackIndex, int theRenderGroup)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Unknown result type (might be due to invalid IL or missing references)
		GetCurrentTransform(theTrackIndex, out var aTransformCurrent, nullIfInvalidFrame: true);
		if (aTransformCurrent == null)
		{
			return false;
		}
		if (aTransformCurrent.mFrame < 0f)
		{
			aTransformCurrent.PrepareForReuse();
			return false;
		}
		int num = (int)(aTransformCurrent.mFrame + 0.5f);
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
		SexyColor mTrackColor = reanimatorTrackInstance.mTrackColor;
		if (!reanimatorTrackInstance.mIgnoreColorOverride)
		{
			mTrackColor.Color.R = (byte)(mColorOverride.mRed * mTrackColor.mRed / 255f);
			mTrackColor.Color.G = (byte)(mColorOverride.mGreen * mTrackColor.mGreen / 255f);
			mTrackColor.Color.B = (byte)(mColorOverride.mBlue * mTrackColor.mBlue / 255f);
			mTrackColor.Color.A = (byte)(mColorOverride.mAlpha * mTrackColor.mAlpha / 255f);
		}
		if (g.mColorizeImages)
		{
			mTrackColor.Color.R = (byte)(g.mColor.R * mTrackColor.mRed / 255f);
			mTrackColor.Color.G = (byte)(g.mColor.G * mTrackColor.mGreen / 255f);
			mTrackColor.Color.B = (byte)(g.mColor.B * mTrackColor.mBlue / 255f);
			mTrackColor.Color.A = (byte)(g.mColor.A * mTrackColor.mAlpha / 255f);
		}
		int num2 = TodCommon.ClampInt((int)(aTransformCurrent.mAlpha * (float)mTrackColor.mAlpha + 0.5f), 0, 255);
		if (num2 <= 0)
		{
			aTransformCurrent.PrepareForReuse();
			return false;
		}
		mTrackColor.mAlpha = num2;
		SexyColor theColor = ((!mEnableExtraAdditiveDraw) ? default(SexyColor) : new SexyColor(mExtraAdditiveColor.mRed, mExtraAdditiveColor.mGreen, mExtraAdditiveColor.mBlue, TodCommon.ColorComponentMultiply(mExtraAdditiveColor.mAlpha, num2)));
		Image image = aTransformCurrent.mImage;
		ReanimAtlasImage reanimAtlasImage = null;
		if (mDefinition.mReanimAtlas != null && image != null)
		{
			reanimAtlasImage = mDefinition.mReanimAtlas.GetEncodedReanimAtlas(image);
			if (reanimAtlasImage != null)
			{
				image = reanimAtlasImage.mOriginalImage;
			}
			if (reanimatorTrackInstance.mImageOverride != null)
			{
				reanimAtlasImage = null;
			}
		}
		bool flag = false;
		float num3 = 0f;
		float num4 = 0f;
		if (image != null)
		{
			float num5 = image.GetCelWidth();
			float num6 = image.GetCelHeight();
			num3 = num5 * 0.5f;
			num4 = num6 * 0.5f;
		}
		else if (aTransformCurrent.mFont != null && !string.IsNullOrEmpty(aTransformCurrent.mText))
		{
			float num7 = aTransformCurrent.mFont.StringWidth(aTransformCurrent.mText);
			num3 = (0f - num7) * 0.5f;
			num4 = aTransformCurrent.mFont.mAscent;
		}
		else
		{
			if (!(mDefinition.mTracks[theTrackIndex].mName == "fullscreen"))
			{
				aTransformCurrent.PrepareForReuse();
				return false;
			}
			flag = true;
		}
		TRect theClipRect = g.mClipRect;
		didClipIgnore = false;
		if (reanimatorTrackInstance.mIgnoreClipRect)
		{
			theClipRect = new TRect(0, 0, 800, 600);
			didClipIgnore = true;
		}
		float num8 = aTransformCurrent.mSkewXCos * aTransformCurrent.mScaleX;
		float num9 = (0f - aTransformCurrent.mSkewXSin) * aTransformCurrent.mScaleX;
		float num10 = aTransformCurrent.mSkewYSin * aTransformCurrent.mScaleY;
		float num11 = aTransformCurrent.mSkewYCos * aTransformCurrent.mScaleY;
		float num12 = num8 * num3 + num10 * num4 + aTransformCurrent.mTransX;
		float num13 = num9 * num3 + num11 * num4 + aTransformCurrent.mTransY;
		Matrix val = default(Matrix);
		val.M11 = num8 * mOverlayMatrix.mMatrix.M11 + num9 * mOverlayMatrix.mMatrix.M21;
		val.M12 = num8 * mOverlayMatrix.mMatrix.M12 + num9 * mOverlayMatrix.mMatrix.M22;
		val.M13 = 0f;
		val.M14 = 0f;
		val.M21 = num10 * mOverlayMatrix.mMatrix.M11 + num11 * mOverlayMatrix.mMatrix.M21;
		val.M22 = num10 * mOverlayMatrix.mMatrix.M12 + num11 * mOverlayMatrix.mMatrix.M22;
		val.M23 = 0f;
		val.M24 = 0f;
		val.M31 = 0f;
		val.M32 = 0f;
		val.M33 = 1f;
		val.M34 = 0f;
		val.M41 = num12 * mOverlayMatrix.mMatrix.M11 + num13 * mOverlayMatrix.mMatrix.M21 + mOverlayMatrix.mMatrix.M41 + (float)g.mTransX + reanimatorTrackInstance.mShakeX - 0.5f;
		val.M42 = num12 * mOverlayMatrix.mMatrix.M12 + num13 * mOverlayMatrix.mMatrix.M22 + mOverlayMatrix.mMatrix.M42 + (float)g.mTransY + reanimatorTrackInstance.mShakeY - 0.5f;
		val.M43 = 0f;
		val.M44 = 1f;
		tempMatrix = val;
		if (theTrackIndex == 9)
		{
			int num14 = 0;
			num14++;
		}
		if (reanimAtlasImage == null)
		{
			if (image != null)
			{
				if (reanimatorTrackInstance.mImageOverride != null)
				{
					image = reanimatorTrackInstance.mImageOverride;
				}
				while (num >= image.mNumCols)
				{
					num -= image.mNumCols;
				}
				int num15 = 0;
				int celWidth = image.GetCelWidth();
				int celHeight = image.GetCelHeight();
				TRect theSrcRect = new TRect(celWidth * num, celHeight * num15, celWidth, celHeight);
				ReanimBltMatrix(g, image, ref tempMatrix, ref theClipRect, mTrackColor, Graphics.DrawMode.DRAWMODE_NORMAL, theSrcRect);
				if (mEnableExtraAdditiveDraw)
				{
					ReanimBltMatrix(g, image, ref tempMatrix, ref theClipRect, theColor, Graphics.DrawMode.DRAWMODE_ADDITIVE, theSrcRect);
				}
				TodCommon.OffsetForGraphicsTranslation = true;
			}
			else if (aTransformCurrent.mFont != null && !string.IsNullOrEmpty(aTransformCurrent.mText))
			{
				TodCommon.TodDrawStringMatrix(g, aTransformCurrent.mFont, tempMatrix, aTransformCurrent.mText, mTrackColor);
				if (mEnableExtraAdditiveDraw)
				{
					Graphics.DrawMode mDrawMode = g.mDrawMode;
					g.SetDrawMode(Graphics.DrawMode.DRAWMODE_ADDITIVE);
					TodCommon.TodDrawStringMatrix(g, aTransformCurrent.mFont, tempMatrix, aTransformCurrent.mText, theColor);
					g.SetDrawMode(mDrawMode);
				}
			}
			else if (flag)
			{
				Color color5 = g.GetColor();
				g.SetColor(mTrackColor);
				g.FillRect(-g.mTransX, -g.mTransY, Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT);
				g.SetColor(color5);
			}
		}
		aTransformCurrent.PrepareForReuse();
		return true;
	}

	public void GetCurrentTransform(int theTrackIndex, out ReanimatorTransform aTransformCurrent, bool nullIfInvalidFrame)
	{
		GetFrameTime(out var theFrameTime);
		GetTransformAtTime(theTrackIndex, out aTransformCurrent, theFrameTime, nullIfInvalidFrame);
		if (aTransformCurrent == null)
		{
			return;
		}
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
		int num = (int)(aTransformCurrent.mFrame + 0.5f);
		if (num >= 0 && reanimatorTrackInstance.mBlendCounter > 0)
		{
			float theBlendFactor = (float)(int)reanimatorTrackInstance.mBlendCounter / (float)(int)reanimatorTrackInstance.mBlendTime;
			ReanimatorXnaHelpers.BlendTransform(out var theResult, ref aTransformCurrent, ref reanimatorTrackInstance.mBlendTransform, theBlendFactor);
			if (aTransformCurrent != null)
			{
				aTransformCurrent.PrepareForReuse();
			}
			aTransformCurrent = theResult;
		}
	}

	public void GetTransformAtTime(int theTrackIndex, out ReanimatorTransform aTransform, ReanimatorFrameTime theFrameTime, bool nullIfInvalidFrame)
	{
		ReanimatorTrack reanimatorTrack = mDefinition.mTracks[theTrackIndex];
		ReanimatorTransform reanimatorTransform = reanimatorTrack.mTransforms[theFrameTime.mAnimFrameBeforeInt];
		ReanimatorTransform reanimatorTransform2 = reanimatorTrack.mTransforms[theFrameTime.mAnimFrameAfterInt];
		if (nullIfInvalidFrame && (reanimatorTransform.mFrame == -1f || (reanimatorTransform.mFrame != -1f && reanimatorTransform2.mFrame == -1f && theFrameTime.mFraction > 0f && mTrackInstances[theTrackIndex].mTruncateDisappearingFrames)))
		{
			aTransform = null;
			return;
		}
		float mFraction = theFrameTime.mFraction;
		aTransform = ReanimatorTransform.GetNewReanimatorTransform();
		if (mInterpolate)
		{
			aTransform.mTransX = reanimatorTransform.mTransX + mFraction * (reanimatorTransform2.mTransX - reanimatorTransform.mTransX);
			aTransform.mTransY = reanimatorTransform.mTransY + mFraction * (reanimatorTransform2.mTransY - reanimatorTransform.mTransY);
			aTransform.mSkewX = reanimatorTransform.mSkewX + mFraction * (reanimatorTransform2.mSkewX - reanimatorTransform.mSkewX);
			aTransform.mSkewY = reanimatorTransform.mSkewY + mFraction * (reanimatorTransform2.mSkewY - reanimatorTransform.mSkewY);
			aTransform.mScaleX = reanimatorTransform.mScaleX + mFraction * (reanimatorTransform2.mScaleX - reanimatorTransform.mScaleX);
			aTransform.mScaleY = reanimatorTransform.mScaleY + mFraction * (reanimatorTransform2.mScaleY - reanimatorTransform.mScaleY);
			aTransform.mAlpha = reanimatorTransform.mAlpha + mFraction * (reanimatorTransform2.mAlpha - reanimatorTransform.mAlpha);
			aTransform.mSkewXCos = reanimatorTransform.mSkewXCos + mFraction * (reanimatorTransform2.mSkewXCos - reanimatorTransform.mSkewXCos);
			aTransform.mSkewXSin = reanimatorTransform.mSkewXSin + mFraction * (reanimatorTransform2.mSkewXSin - reanimatorTransform.mSkewXSin);
			aTransform.mSkewYCos = reanimatorTransform.mSkewYCos + mFraction * (reanimatorTransform2.mSkewYCos - reanimatorTransform.mSkewYCos);
			aTransform.mSkewYSin = reanimatorTransform.mSkewYSin + mFraction * (reanimatorTransform2.mSkewYSin - reanimatorTransform.mSkewYSin);
		}
		else
		{
			aTransform.mTransX = reanimatorTransform.mTransX;
			aTransform.mTransY = reanimatorTransform.mTransY;
			aTransform.mSkewX = reanimatorTransform.mSkewX;
			aTransform.mSkewY = reanimatorTransform.mSkewY;
			aTransform.mScaleX = reanimatorTransform.mScaleX;
			aTransform.mScaleY = reanimatorTransform.mScaleY;
			aTransform.mAlpha = reanimatorTransform.mAlpha;
			aTransform.mSkewXCos = reanimatorTransform.mSkewXCos;
			aTransform.mSkewXSin = reanimatorTransform.mSkewXSin;
			aTransform.mSkewYCos = reanimatorTransform.mSkewYCos;
			aTransform.mSkewYSin = reanimatorTransform.mSkewYSin;
		}
		aTransform.mImage = reanimatorTransform.mImage;
		aTransform.mFont = reanimatorTransform.mFont;
		aTransform.mText = reanimatorTransform.mText;
		if (reanimatorTransform.mFrame != -1f && reanimatorTransform2.mFrame == -1f && theFrameTime.mFraction > 0f && mTrackInstances[theTrackIndex].mTruncateDisappearingFrames)
		{
			aTransform.mFrame = -1f;
		}
		else
		{
			aTransform.mFrame = reanimatorTransform.mFrame;
		}
	}

	public void GetFrameTime(out ReanimatorFrameTime theFrameTime)
	{
		if (!mGetFrameTime)
		{
			theFrameTime = mFrameTime;
			return;
		}
		mGetFrameTime = false;
		theFrameTime = default(ReanimatorFrameTime);
		int num = ((mLoopType != ReanimLoopType.REANIM_PLAY_ONCE_FULL_LAST_FRAME && mLoopType != ReanimLoopType.REANIM_LOOP_FULL_LAST_FRAME && mLoopType != ReanimLoopType.REANIM_PLAY_ONCE_FULL_LAST_FRAME_AND_HOLD) ? (mFrameCount - 1) : mFrameCount);
		float num2 = (float)mFrameStart + (float)num * mAnimTime;
		float num3 = (int)num2;
		theFrameTime.mFraction = num2 - num3;
		theFrameTime.mAnimFrameBeforeInt = (short)(num3 + 0.5f);
		if (theFrameTime.mAnimFrameBeforeInt >= mFrameStart + mFrameCount - 1)
		{
			theFrameTime.mAnimFrameBeforeInt = (short)(mFrameStart + mFrameCount - 1);
			theFrameTime.mAnimFrameAfterInt = theFrameTime.mAnimFrameBeforeInt;
		}
		else
		{
			theFrameTime.mAnimFrameAfterInt = (short)(theFrameTime.mAnimFrameBeforeInt + 1);
		}
		mFrameTime = theFrameTime;
	}

	public int FindTrackIndex(string theTrackName)
	{
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			string mName = mDefinition.mTracks[i].mName;
			string text = ToLower(theTrackName);
			if (mName == text)
			{
				return i;
			}
		}
		return 0;
	}

	public void AttachToAnotherReanimation(ref Reanimation theAttachReanim, string theTrackName)
	{
		if (theAttachReanim.mDefinition.mTrackCount != 0)
		{
			if (theAttachReanim.mFrameBasePose == -1)
			{
				theAttachReanim.mFrameBasePose = theAttachReanim.mFrameStart;
			}
			int num = theAttachReanim.FindTrackIndex(theTrackName);
			ReanimatorTrackInstance reanimatorTrackInstance = theAttachReanim.mTrackInstances[num];
			GlobalMembersAttachment.AttachReanim(ref reanimatorTrackInstance.mAttachmentID, this, 0f, 0f);
		}
	}

	public void GetAttachmentOverlayMatrix(int theTrackIndex, out SexyTransform2D theOverlayMatrix)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		GetCurrentTransform(theTrackIndex, out var aTransformCurrent, nullIfInvalidFrame: false);
		float num = aTransformCurrent.mSkewXCos * aTransformCurrent.mScaleX;
		float num2 = (0f - aTransformCurrent.mSkewXSin) * aTransformCurrent.mScaleX;
		float num3 = aTransformCurrent.mSkewYSin * aTransformCurrent.mScaleY;
		float num4 = aTransformCurrent.mSkewYCos * aTransformCurrent.mScaleY;
		float mTransX = aTransformCurrent.mTransX;
		float mTransY = aTransformCurrent.mTransY;
		aTransformCurrent.PrepareForReuse();
		GetTrackBasePoseMatrix(theTrackIndex, out basePose);
		Matrix.Invert(ref basePose.mMatrix, ref aBasePoseMatrix);
		theOverlayMatrix = identity;
		tempOverlayMatrix = new Matrix
		{
			M11 = aBasePoseMatrix.M11 * num + aBasePoseMatrix.M12 * num3,
			M12 = aBasePoseMatrix.M11 * num2 + aBasePoseMatrix.M12 * num4,
			M13 = 0f,
			M14 = 0f,
			M21 = aBasePoseMatrix.M21 * num + aBasePoseMatrix.M22 * num3,
			M22 = aBasePoseMatrix.M21 * num2 + aBasePoseMatrix.M22 * num4,
			M23 = 0f,
			M24 = 0f,
			M31 = 0f,
			M32 = 0f,
			M33 = 1f,
			M34 = 0f,
			M41 = aBasePoseMatrix.M41 * num + aBasePoseMatrix.M42 * num3 + mTransX,
			M42 = aBasePoseMatrix.M41 * num2 + aBasePoseMatrix.M42 * num4 + mTransY,
			M43 = 0f,
			M44 = 1f
		};
		theOverlayMatrix.mMatrix = new Matrix
		{
			M11 = tempOverlayMatrix.M11 * mOverlayMatrix.mMatrix.M11 + tempOverlayMatrix.M12 * mOverlayMatrix.mMatrix.M21,
			M12 = tempOverlayMatrix.M11 * mOverlayMatrix.mMatrix.M12 + tempOverlayMatrix.M12 * mOverlayMatrix.mMatrix.M22,
			M13 = 0f,
			M14 = 0f,
			M21 = tempOverlayMatrix.M21 * mOverlayMatrix.mMatrix.M11 + tempOverlayMatrix.M22 * mOverlayMatrix.mMatrix.M21,
			M22 = tempOverlayMatrix.M21 * mOverlayMatrix.mMatrix.M12 + tempOverlayMatrix.M22 * mOverlayMatrix.mMatrix.M22,
			M23 = 0f,
			M24 = 0f,
			M31 = 0f,
			M32 = 0f,
			M33 = 1f,
			M34 = 0f,
			M41 = tempOverlayMatrix.M41 * mOverlayMatrix.mMatrix.M11 + tempOverlayMatrix.M42 * mOverlayMatrix.mMatrix.M21 + mOverlayMatrix.mMatrix.M41,
			M42 = tempOverlayMatrix.M41 * mOverlayMatrix.mMatrix.M12 + tempOverlayMatrix.M42 * mOverlayMatrix.mMatrix.M22 + mOverlayMatrix.mMatrix.M42,
			M43 = 0f,
			M44 = 1f
		};
	}

	public void SetFramesForLayer(string theTrackName)
	{
		if (mAnimRate >= 0f)
		{
			mAnimTime = 0f;
		}
		else
		{
			mAnimTime = 0.9999999f;
		}
		mLastFrameTime = -1f;
		GetFramesForLayer(theTrackName, out mFrameStart, out mFrameCount);
	}

	public static void MatrixFromTransform(ReanimatorTransform theTransform, out Matrix theMatrix)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		theMatrix = new Matrix
		{
			M11 = (float)Math.Cos(theTransform.mSkewX * (0f - TodCommon.DEG_TO_RAD)) * theTransform.mScaleX,
			M12 = (float)(0.0 - Math.Sin(theTransform.mSkewX * (0f - TodCommon.DEG_TO_RAD))) * theTransform.mScaleX,
			M13 = 0f,
			M14 = 0f,
			M21 = (float)Math.Sin(theTransform.mSkewY * (0f - TodCommon.DEG_TO_RAD)) * theTransform.mScaleY,
			M22 = (float)Math.Cos(theTransform.mSkewY * (0f - TodCommon.DEG_TO_RAD)) * theTransform.mScaleY,
			M23 = 0f,
			M24 = 0f,
			M31 = 0f,
			M32 = 0f,
			M33 = 1f,
			M34 = 0f,
			M41 = theTransform.mTransX,
			M42 = theTransform.mTransY,
			M43 = 0f,
			M44 = 1f
		};
	}

	public bool TrackExists(string theTrackName)
	{
		string text = ToLower(theTrackName);
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			string text2 = ToLower(mDefinition.mTracks[i].mName);
			if (text == text2)
			{
				return true;
			}
		}
		return false;
	}

	public void StartBlend(byte theBlendTime)
	{
		mGetFrameTime = true;
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			GetCurrentTransform(i, out var aTransformCurrent, nullIfInvalidFrame: true);
			if (aTransformCurrent == null)
			{
				continue;
			}
			int num = TodCommon.FloatRoundToInt(aTransformCurrent.mFrame);
			if (num < 0)
			{
				aTransformCurrent.PrepareForReuse();
				continue;
			}
			ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
			if (reanimatorTrackInstance.mBlendTransform != null)
			{
				reanimatorTrackInstance.mBlendTransform.PrepareForReuse();
			}
			reanimatorTrackInstance.mBlendTransform = aTransformCurrent;
			reanimatorTrackInstance.mBlendCounter = (byte)((float)(int)theBlendTime / 3f);
			reanimatorTrackInstance.mBlendTime = (byte)((float)(int)theBlendTime / 3f);
			reanimatorTrackInstance.mBlendTransform.mFont = null;
			reanimatorTrackInstance.mBlendTransform.mText = string.Empty;
			reanimatorTrackInstance.mBlendTransform.mImage = null;
		}
	}

	public void SetShakeOverride(string theTrackName, float theShakeAmount)
	{
		int num = FindTrackIndex(theTrackName);
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[num];
		reanimatorTrackInstance.mShakeOverride = theShakeAmount;
	}

	public void SetPosition(float theX, float theY)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		mOverlayMatrix.mMatrix.Translation = new Vector3(theX, theY, 0f);
	}

	public void OverrideScale(float theScaleX, float theScaleY)
	{
		mOverlayMatrix.mMatrix.M11 = theScaleX;
		mOverlayMatrix.mMatrix.M22 = theScaleY;
	}

	public int GetTrackIndex(string theTrackName)
	{
		return FindTrackIndex(theTrackName);
	}

	public float GetTrackVelocity(string theTrackName)
	{
		return GetTrackVelocity(GetTrackIndex(theTrackName));
	}

	public float GetTrackVelocity(int aTrackIndex)
	{
		GetFrameTime(out var theFrameTime);
		ReanimatorTrack reanimatorTrack = mDefinition.mTracks[aTrackIndex];
		ReanimatorTransform reanimatorTransform = reanimatorTrack.mTransforms[theFrameTime.mAnimFrameBeforeInt];
		ReanimatorTransform reanimatorTransform2 = reanimatorTrack.mTransforms[theFrameTime.mAnimFrameAfterInt];
		return (reanimatorTransform2.mTransX - reanimatorTransform.mTransX) * ReanimatorXnaHelpers.SECONDS_PER_UPDATE * mAnimRate;
	}

	public void SetImageOverride(string theTrackName, Image theImage)
	{
		int num = FindTrackIndex(theTrackName);
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[num];
		reanimatorTrackInstance.mImageOverride = theImage;
	}

	public Image GetImageOverride(string theTrackName)
	{
		int num = FindTrackIndex(theTrackName);
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[num];
		return reanimatorTrackInstance.mImageOverride;
	}

	public void ShowOnlyTrack(string theTrackName)
	{
		string text = theTrackName.ToLower();
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			ReanimatorTrack reanimatorTrack = mDefinition.mTracks[i];
			ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
			string text2 = reanimatorTrack.mName.ToLower();
			if (text == text2)
			{
				reanimatorTrackInstance.mRenderGroup = ReanimatorXnaHelpers.RENDER_GROUP_NORMAL;
			}
			else
			{
				reanimatorTrackInstance.mRenderGroup = ReanimatorXnaHelpers.RENDER_GROUP_HIDDEN;
			}
		}
	}

	public void GetTrackMatrix(int theTrackIndex, ref SexyTransform2D theMatrix)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
		mGetFrameTime = true;
		GetCurrentTransform(theTrackIndex, out var aTransformCurrent, nullIfInvalidFrame: false);
		int num = TodCommon.FloatRoundToInt(aTransformCurrent.mFrame);
		Image image = aTransformCurrent.mImage;
		if (mDefinition.mReanimAtlas != null && image != null)
		{
			ReanimAtlasImage encodedReanimAtlas = mDefinition.mReanimAtlas.GetEncodedReanimAtlas(image);
			if (encodedReanimAtlas != null)
			{
				image = encodedReanimAtlas.mOriginalImage;
			}
		}
		theMatrix.LoadIdentity();
		tempMatrix = Matrix.Identity;
		if (image != null && num >= 0)
		{
			int celWidth = image.GetCelWidth();
			int celHeight = image.GetCelHeight();
			Matrix.CreateTranslation((float)celWidth * 0.5f, (float)celHeight * 0.5f, 0f, ref tempMatrix);
		}
		else if (aTransformCurrent.mFont != null && !string.IsNullOrEmpty(aTransformCurrent.mText))
		{
			Matrix.CreateTranslation(0f, (float)aTransformCurrent.mFont.mAscent, 0f, ref tempMatrix);
		}
		SexyTransform2D sexyTransform2D = default(SexyTransform2D);
		MatrixFromTransform(aTransformCurrent, out sexyTransform2D.mMatrix);
		TodCommon.SexyMatrix3Multiply(ref tempMatrix, sexyTransform2D.mMatrix, tempMatrix);
		TodCommon.SexyMatrix3Multiply(ref tempMatrix, mOverlayMatrix.mMatrix, tempMatrix);
		TodCommon.SexyMatrix3Translation(ref tempMatrix, reanimatorTrackInstance.mShakeX - 0.5f, reanimatorTrackInstance.mShakeY - 0.5f);
		theMatrix.mMatrix = tempMatrix;
		aTransformCurrent.PrepareForReuse();
	}

	public void GetTrackTranslationMatrix(int theTrackIndex, ref SexyTransform2D theMatrix)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
		mGetFrameTime = true;
		GetCurrentTransform(theTrackIndex, out var aTransformCurrent, nullIfInvalidFrame: false);
		int num = TodCommon.FloatRoundToInt(aTransformCurrent.mFrame);
		Image image = aTransformCurrent.mImage;
		if (mDefinition.mReanimAtlas != null && image != null)
		{
			ReanimAtlasImage encodedReanimAtlas = mDefinition.mReanimAtlas.GetEncodedReanimAtlas(image);
			if (encodedReanimAtlas != null)
			{
				image = encodedReanimAtlas.mOriginalImage;
			}
		}
		theMatrix.LoadIdentity();
		tempMatrix = Matrix.Identity;
		if (image != null && num >= 0)
		{
			int celWidth = image.GetCelWidth();
			int celHeight = image.GetCelHeight();
			Matrix.CreateTranslation((float)celWidth * 0.5f, (float)celHeight * 0.5f, 0f, ref tempMatrix);
		}
		else if (aTransformCurrent.mFont != null && !string.IsNullOrEmpty(aTransformCurrent.mText))
		{
			Matrix.CreateTranslation(0f, (float)aTransformCurrent.mFont.mAscent, 0f, ref tempMatrix);
		}
		SexyTransform2D sexyTransform2D = default(SexyTransform2D);
		MatrixFromTransform(aTransformCurrent, out sexyTransform2D.mMatrix);
		tempMatrix.M41 = sexyTransform2D.mMatrix.M41 + mOverlayMatrix.mMatrix.M41 + reanimatorTrackInstance.mShakeX - 0.5f;
		tempMatrix.M42 = sexyTransform2D.mMatrix.M42 + mOverlayMatrix.mMatrix.M42 + reanimatorTrackInstance.mShakeY - 0.5f;
		theMatrix.mMatrix = tempMatrix;
		aTransformCurrent.PrepareForReuse();
	}

	public void AssignRenderGroupToTrack(string theTrackName, int theRenderGroup)
	{
		string text = ToLower(theTrackName);
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			ReanimatorTrack reanimatorTrack = mDefinition.mTracks[i];
			string text2 = ToLower(reanimatorTrack.mName);
			if (!(text != text2))
			{
				mTrackInstances[i].mRenderGroup = theRenderGroup;
				break;
			}
		}
	}

	public void AssignRenderGroupToPrefix(string theTrackName, int theRenderGroup)
	{
		int length = theTrackName.Length;
		string s = ToLower(theTrackName);
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			ReanimatorTrack reanimatorTrack = mDefinition.mTracks[i];
			if (reanimatorTrack.mName.Length >= length)
			{
				string contains = ToLower(reanimatorTrack.mName);
				if (s.StartsWithCharLimit(contains, length))
				{
					mTrackInstances[i].mRenderGroup = theRenderGroup;
				}
			}
		}
	}

	public void PropogateColorToAttachments()
	{
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
			GlobalMembersAttachment.AttachmentPropogateColor(reanimatorTrackInstance.mAttachmentID, mColorOverride, mEnableExtraAdditiveDraw, mExtraAdditiveColor, mEnableExtraOverlayDraw, mExtraOverlayColor);
		}
	}

	public bool ShouldTriggerTimedEvent(float theEventTime)
	{
		if (mFrameCount == 0)
		{
			return false;
		}
		if (mLastFrameTime < 0f)
		{
			return false;
		}
		if (mAnimRate <= 0f)
		{
			return false;
		}
		if (mAnimTime >= mLastFrameTime)
		{
			if (theEventTime >= mLastFrameTime && theEventTime < mAnimTime)
			{
				return true;
			}
		}
		else if (theEventTime >= mLastFrameTime || theEventTime < mAnimTime)
		{
			return true;
		}
		return false;
	}

	public void TodTriangleGroupDraw(Graphics g, ref TodTriangleGroup theTriangleGroup)
	{
	}

	public Image GetCurrentTrackImage(string theTrackName)
	{
		int theTrackIndex = FindTrackIndex(theTrackName);
		GetCurrentTransform(theTrackIndex, out var aTransformCurrent, nullIfInvalidFrame: false);
		Image image = aTransformCurrent.mImage;
		if (mDefinition.mReanimAtlas != null && image != null)
		{
			ReanimAtlasImage encodedReanimAtlas = mDefinition.mReanimAtlas.GetEncodedReanimAtlas(image);
			if (encodedReanimAtlas != null)
			{
				image = encodedReanimAtlas.mOriginalImage;
			}
		}
		aTransformCurrent.PrepareForReuse();
		return image;
	}

	public AttachEffect AttachParticleToTrack(string theTrackName, ref TodParticleSystem theParticleSystem, float thePosX, float thePosY)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int num = FindTrackIndex(theTrackName);
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[num];
		GetTrackBasePoseMatrix(num, out var theBasePoseMatrix);
		Vector2 val = theBasePoseMatrix * new Vector2(thePosX, thePosY);
		return GlobalMembersAttachment.AttachParticle(ref reanimatorTrackInstance.mAttachmentID, theParticleSystem, val.X, val.Y);
	}

	public void GetTrackBasePoseMatrix(int theTrackIndex, out SexyTransform2D theBasePoseMatrix)
	{
		theBasePoseMatrix = default(SexyTransform2D);
		if (mFrameBasePose == ReanimatorXnaHelpers.NO_BASE_POSE)
		{
			theBasePoseMatrix.LoadIdentity();
			return;
		}
		short num = mFrameBasePose;
		if (num == -1)
		{
			num = mFrameStart;
		}
		ReanimatorFrameTime theFrameTime = default(ReanimatorFrameTime);
		theFrameTime.mFraction = 0f;
		theFrameTime.mAnimFrameBeforeInt = num;
		theFrameTime.mAnimFrameAfterInt = (short)(num + 1);
		GetTransformAtTime(theTrackIndex, out var aTransform, theFrameTime, nullIfInvalidFrame: false);
		MatrixFromTransform(aTransform, out theBasePoseMatrix.mMatrix);
		aTransform.PrepareForReuse();
	}

	public bool IsTrackShowing(string theTrackName)
	{
		GetFrameTime(out var theFrameTime);
		int num = FindTrackIndex(theTrackName);
		ReanimatorTrack reanimatorTrack = mDefinition.mTracks[num];
		ReanimatorTransform reanimatorTransform = reanimatorTrack.mTransforms[theFrameTime.mAnimFrameAfterInt];
		if (reanimatorTransform.mFrame >= 0f)
		{
			return true;
		}
		return false;
	}

	public void SetTruncateDisappearingFrames(string theTrackName, bool theTruncateDisappearingFrames)
	{
		if (string.IsNullOrEmpty(theTrackName))
		{
			for (int i = 0; i < mDefinition.mTrackCount; i++)
			{
				ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
				reanimatorTrackInstance.mTruncateDisappearingFrames = theTruncateDisappearingFrames;
			}
		}
		else
		{
			int num = FindTrackIndex(theTrackName);
			ReanimatorTrackInstance reanimatorTrackInstance2 = mTrackInstances[num];
			reanimatorTrackInstance2.mTruncateDisappearingFrames = theTruncateDisappearingFrames;
		}
	}

	public void PlayReanim(string theTrackName, ReanimLoopType theLoopType, byte theBlendTime, float theAnimRate)
	{
		if (theBlendTime > 0)
		{
			StartBlend(theBlendTime);
		}
		if (theAnimRate != 0f)
		{
			mAnimRate = theAnimRate;
		}
		mLoopType = theLoopType;
		mLoopCount = 0;
		SetFramesForLayer(theTrackName);
	}

	public void ReanimationDelete()
	{
		if (mTrackInstances != null)
		{
			for (int i = 0; i < mTrackInstances.Length; i++)
			{
				mTrackInstances[i] = null;
			}
			mTrackInstances = null;
		}
	}

	public ReanimatorTrackInstance GetTrackInstanceByName(string theTrackName)
	{
		int num = FindTrackIndex(theTrackName);
		return mTrackInstances[num];
	}

	public void GetFramesForLayer(string theTrackName, out short theFrameStart, out short theFrameCount)
	{
		if (mDefinition.mTrackCount == 0)
		{
			theFrameStart = 0;
			theFrameCount = 0;
			return;
		}
		int num = FindTrackIndex(theTrackName);
		ReanimatorTrack reanimatorTrack = mDefinition.mTracks[num];
		theFrameStart = 0;
		theFrameCount = 1;
		short num2;
		for (num2 = 0; num2 < reanimatorTrack.mTransformCount; num2++)
		{
			ReanimatorTransform reanimatorTransform = reanimatorTrack.mTransforms[num2];
			if (reanimatorTransform.mFrame >= 0f)
			{
				theFrameStart = num2;
				break;
			}
		}
		for (int num3 = reanimatorTrack.mTransformCount - 1; num3 >= num2; num3--)
		{
			ReanimatorTransform reanimatorTransform2 = reanimatorTrack.mTransforms[num3];
			if (reanimatorTransform2.mFrame >= 0f)
			{
				theFrameCount = (short)(num3 - theFrameStart + 1);
				break;
			}
		}
	}

	public void UpdateAttacherTrack(int theTrackIndex)
	{
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
		GetCurrentTransform(theTrackIndex, out var aTransformCurrent, nullIfInvalidFrame: false);
		ParseAttacherTrack(aTransformCurrent, out var theAttacherInfo);
		ReanimationType reanimationType = ReanimationType.REANIM_NONE;
		if (theAttacherInfo.mReanimName.Length != 0)
		{
			string text = string.Format("reanim\\%s.reanim", theAttacherInfo.mReanimName);
			string text2 = text.ToLower();
			for (int i = 0; i < ReanimatorXnaHelpers.gReanimationParamArraySize; i++)
			{
				ReanimationParams reanimationParams = ReanimatorXnaHelpers.gReanimationParamArray[i];
				string text3 = reanimationParams.mReanimFileName.ToLower();
				if (text2 == text3)
				{
					reanimationType = reanimationParams.mReanimationType;
					break;
				}
			}
		}
		if (reanimationType == ReanimationType.REANIM_NONE)
		{
			GlobalMembersAttachment.AttachmentDie(ref reanimatorTrackInstance.mAttachmentID);
			return;
		}
		Reanimation theAttachReanim = GlobalMembersAttachment.FindReanimAttachment(reanimatorTrackInstance.mAttachmentID);
		if (theAttachReanim == null || theAttachReanim.mReanimationType != reanimationType)
		{
			GlobalMembersAttachment.AttachmentDie(ref reanimatorTrackInstance.mAttachmentID);
			theAttachReanim = EffectSystem.gEffectSystem.mReanimationHolder.AllocReanimation(0f, 0f, 0, reanimationType);
			theAttachReanim.mLoopType = theAttacherInfo.mLoopType;
			theAttachReanim.mAnimRate = theAttacherInfo.mAnimRate;
			GlobalMembersAttachment.AttachReanim(ref reanimatorTrackInstance.mAttachmentID, theAttachReanim, 0f, 0f);
			mFrameBasePose = ReanimatorXnaHelpers.NO_BASE_POSE;
		}
		if (theAttacherInfo.mTrackName.Length != 0)
		{
			theAttachReanim.GetFramesForLayer(theAttacherInfo.mTrackName, out var theFrameStart, out var theFrameCount);
			if (theAttachReanim.mFrameStart != theFrameStart || theAttachReanim.mFrameCount != theFrameCount)
			{
				theAttachReanim.StartBlend(20);
				theAttachReanim.SetFramesForLayer(theAttacherInfo.mTrackName);
			}
			if (theAttacherInfo.mAnimRate == 12f && theAttacherInfo.mTrackName == "anim_walk" && theAttachReanim.TrackExists("_ground"))
			{
				AttacherSynchWalkSpeed(theTrackIndex, ref theAttachReanim, theAttacherInfo);
			}
			else
			{
				theAttachReanim.mAnimRate = theAttacherInfo.mAnimRate;
			}
			theAttachReanim.mLoopType = theAttacherInfo.mLoopType;
		}
		SexyColor theColor = TodCommon.ColorsMultiply(mColorOverride, reanimatorTrackInstance.mTrackColor);
		theColor.mAlpha = TodCommon.ClampInt(TodCommon.FloatRoundToInt(aTransformCurrent.mAlpha * (float)theColor.mAlpha), 0, 255);
		GlobalMembersAttachment.AttachmentPropogateColor(reanimatorTrackInstance.mAttachmentID, theColor, mEnableExtraAdditiveDraw, mExtraAdditiveColor, mEnableExtraOverlayDraw, mExtraOverlayColor);
	}

	public static void ParseAttacherTrack(ReanimatorTransform theTransform, out AttacherInfo theAttacherInfo)
	{
		theAttacherInfo = new AttacherInfo();
		theAttacherInfo.mReanimName = "";
		theAttacherInfo.mTrackName = "";
		theAttacherInfo.mAnimRate = 12f;
		theAttacherInfo.mLoopType = ReanimLoopType.REANIM_LOOP;
		if (theTransform.mFrame == -1f)
		{
			return;
		}
		int num = theTransform.mText.IndexOf("__");
		if (num == -1)
		{
			return;
		}
		int num2 = theTransform.mText.IndexOf("[", num + 2);
		int num3 = theTransform.mText.IndexOf("__", num + 2);
		if (num2 != -1 && num3 != -1 && num2 < num3)
		{
			return;
		}
		if (num3 != -1)
		{
			theAttacherInfo.mReanimName = theTransform.mText.Substring(num + 2, num3 - num - 2);
			if (num2 != -1)
			{
				theAttacherInfo.mTrackName = theTransform.mText.Substring(num3 + 2, num2 - num3 - 2);
			}
			else
			{
				theAttacherInfo.mTrackName = theTransform.mText.Substring(num3 + 2);
			}
		}
		else if (num2 != -1)
		{
			theAttacherInfo.mReanimName = theTransform.mText.Substring(num + 2, num2 - num - 2);
		}
		else
		{
			theAttacherInfo.mReanimName = theTransform.mText.Substring(num + 2);
		}
		while (num2 != -1)
		{
			int num4 = theTransform.mText.IndexOf("]", num2 + 1);
			if (num4 == -1)
			{
				break;
			}
			string text = theTransform.mText.Substring(num2 + 1, num4 - num2 - 1);
			if (float.TryParse(text, out var result))
			{
				theAttacherInfo.mAnimRate = result;
			}
			else if (text == "hold")
			{
				theAttacherInfo.mLoopType = ReanimLoopType.REANIM_PLAY_ONCE_AND_HOLD;
			}
			else if (text == "once")
			{
				theAttacherInfo.mLoopType = ReanimLoopType.REANIM_PLAY_ONCE;
			}
			num2 = theTransform.mText.IndexOf("[", num4 + 1);
		}
	}

	public void AttacherSynchWalkSpeed(int theTrackIndex, ref Reanimation theAttachReanim, AttacherInfo theAttacherInfo)
	{
		ReanimatorTrack reanimatorTrack = mDefinition.mTracks[theTrackIndex];
		GetFrameTime(out var theFrameTime);
		int num = theFrameTime.mAnimFrameBeforeInt;
		while (num > mFrameStart && !(reanimatorTrack.mTransforms[num - 1].mText != reanimatorTrack.mTransforms[num].mText))
		{
			num--;
		}
		int i;
		for (i = theFrameTime.mAnimFrameBeforeInt; i < mFrameStart + mFrameCount - 1 && !(reanimatorTrack.mTransforms[i + 1].mText != reanimatorTrack.mTransforms[i].mText); i++)
		{
		}
		int num2 = i - num;
		ReanimatorTransform reanimatorTransform = reanimatorTrack.mTransforms[num];
		ReanimatorTransform reanimatorTransform2 = reanimatorTrack.mTransforms[num + num2 - 1];
		if (TodCommon.FloatApproxEqual(mAnimRate, 0f))
		{
			theAttachReanim.mAnimRate = 0f;
			return;
		}
		float num3 = 0f - (reanimatorTransform2.mTransX - reanimatorTransform.mTransX);
		float num4 = (float)num2 / mAnimRate;
		if (TodCommon.FloatApproxEqual(num4, 0f))
		{
			theAttachReanim.mAnimRate = 0f;
			return;
		}
		int num5 = theAttachReanim.FindTrackIndex("_ground");
		ReanimatorTrack reanimatorTrack2 = theAttachReanim.mDefinition.mTracks[num5];
		ReanimatorTransform reanimatorTransform3 = reanimatorTrack2.mTransforms[theAttachReanim.mFrameStart];
		ReanimatorTransform reanimatorTransform4 = reanimatorTrack2.mTransforms[theAttachReanim.mFrameStart + theAttachReanim.mFrameCount - 1];
		float num6 = reanimatorTransform4.mTransX - reanimatorTransform3.mTransX;
		if (num6 < ReanimatorXnaHelpers.EPSILON || num3 < ReanimatorXnaHelpers.EPSILON)
		{
			theAttachReanim.mAnimRate = 0f;
			return;
		}
		float num7 = num3 / num6;
		theAttachReanim.GetCurrentTransform(num5, out var aTransformCurrent, nullIfInvalidFrame: false);
		float num8 = aTransformCurrent.mTransX - reanimatorTransform3.mTransX;
		float num9 = num6 * theAttachReanim.mAnimTime;
		ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[theTrackIndex];
		AttachEffect attachEffect = GlobalMembersAttachment.FindFirstAttachment(reanimatorTrackInstance.mAttachmentID);
		if (attachEffect != null)
		{
			attachEffect.mOffset.mMatrix.M13 = num9 - num8;
		}
		theAttachReanim.mAnimRate = num7 * (float)theAttachReanim.mFrameCount / num4;
	}

	public bool IsAnimPlaying(string theTrackName)
	{
		GetFramesForLayer(theTrackName, out var theFrameStart, out var theFrameCount);
		if (mFrameStart == theFrameStart && mFrameCount == theFrameCount)
		{
			return true;
		}
		return false;
	}

	public void SetBasePoseFromAnim(string theTrackName)
	{
		GetFramesForLayer(theTrackName, out var theFrameStart, out var _);
		mFrameBasePose = theFrameStart;
	}

	public void ReanimBltMatrix(Graphics g, Image theImage, ref Matrix theTransform, ref TRect theClipRect, SexyColor theColor, Graphics.DrawMode theDrawMode, TRect theSrcRect)
	{
		ReanimationParams reanimationParams = ReanimatorXnaHelpers.gReanimationParamArray[(int)mReanimationType];
		if (!GlobalStaticVars.gSexyAppBase.Is3DAccelerated() && TodCommon.TestBit((uint)reanimationParams.mReanimParamFlags, 1) && TodCommon.FloatApproxEqual(theTransform.M12, 0f) && TodCommon.FloatApproxEqual(theTransform.M21, 0f) && theTransform.M11 > 0f && theTransform.M22 > 0f && theColor == SexyColor.White)
		{
			float m = theTransform.M11;
			float m2 = theTransform.M22;
			int theX = TodCommon.FloatRoundToInt(theTransform.M41 - m * (float)theSrcRect.mWidth * 0.5f);
			int theY = TodCommon.FloatRoundToInt(theTransform.M42 - m2 * (float)theSrcRect.mHeight * 0.5f);
			Graphics.DrawMode drawMode = g.GetDrawMode();
			g.SetDrawMode(theDrawMode);
			TRect theRect = g.mClipRect;
			g.SetClipRect(ref theClipRect);
			if (TodCommon.FloatApproxEqual(m, 1f) && TodCommon.FloatApproxEqual(m2, 1f))
			{
				g.DrawImage(theImage, theX, theY, theSrcRect);
			}
			else
			{
				int theWidth = TodCommon.FloatRoundToInt(m * (float)theSrcRect.mWidth);
				int theHeight = TodCommon.FloatRoundToInt(m2 * (float)theSrcRect.mHeight);
				TRect theDestRect = new TRect(theX, theY, theWidth, theHeight);
				g.DrawImage(theImage, theDestRect, theSrcRect);
			}
			g.SetDrawMode(drawMode);
			g.SetClipRect(ref theRect);
		}
		else
		{
			TodCommon.TodBltMatrix(g, theImage, ref theTransform, theClipRect, theColor, theDrawMode, theSrcRect, mClip);
		}
	}

	public Reanimation FindSubReanim(ReanimationType theReanimType)
	{
		if (mReanimationType == theReanimType)
		{
			return this;
		}
		for (int i = 0; i < mDefinition.mTrackCount; i++)
		{
			ReanimatorTrackInstance reanimatorTrackInstance = mTrackInstances[i];
			Reanimation reanimation = GlobalMembersAttachment.FindReanimAttachment(reanimatorTrackInstance.mAttachmentID);
			if (reanimation != null)
			{
				Reanimation reanimation2 = reanimation.FindSubReanim(theReanimType);
				if (reanimation2 != null)
				{
					return reanimation2;
				}
			}
		}
		return null;
	}
}
