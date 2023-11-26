using Microsoft.Xna.Framework;
using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class LawnStoneButton : DialogButton
{
	private float mFontScale = 1f;

	public override string mLabel
	{
		get
		{
			return base.mLabel;
		}
		set
		{
			base.mLabel = value;
			CalculateTextScale();
		}
	}

	public LawnStoneButton(Image theComponentImage, int theId, ButtonListener theListener)
		: base(theComponentImage, theId, theListener)
	{
		mFont = Resources.FONT_DWARVENTODCRAFT15;
	}

	public override void Draw(Graphics g)
	{
		if (!mBtnNoDraw)
		{
			bool flag = mIsDown && mIsOver && !mDisabled;
			flag ^= mInverted;
			GameButton.DrawStoneButton(g, 0, 0, mWidth, mHeight, flag, mIsOver, mLabel, mFont, mFontScale);
		}
	}

	public override void Resize(int theX, int theY, int theWidth, int theHeight)
	{
		base.Resize(theX, theY, theWidth, theHeight);
		CalculateTextScale();
	}

	public override void Resize(TRect theRect)
	{
		base.Resize(theRect);
		CalculateTextScale();
	}

	private void CalculateTextScale()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		mFontScale = 1f;
		if (mFont != null)
		{
			Vector2 val = mFont.MeasureString(mLabel);
			int num = (int)((float)mWidth - Constants.S * 30f);
			if (val.X > (float)num)
			{
				mFontScale = (float)num / val.X;
			}
		}
	}

	public void SetLabel(string theLabel)
	{
		mLabel = TodStringFile.TodStringTranslate(theLabel);
	}
}
