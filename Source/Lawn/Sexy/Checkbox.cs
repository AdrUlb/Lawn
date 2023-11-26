using Microsoft.Xna.Framework;

namespace Sexy;

internal class Checkbox : Widget
{
	protected CheckboxListener mListener;

	public int mId;

	public bool mChecked;

	public Image mUncheckedImage;

	public Image mCheckedImage;

	public TRect mCheckedRect = default(TRect);

	public TRect mUncheckedRect = default(TRect);

	public Color mOutlineColor = default(Color);

	public Color mBkgColor = default(Color);

	public Color mCheckColor = default(Color);

	public virtual void SetChecked(bool isChecked)
	{
		SetChecked(isChecked, tellListener: true);
	}

	public virtual void SetChecked(bool @checked, bool tellListener)
	{
		mChecked = @checked;
		if (tellListener && mListener != null)
		{
			mListener.CheckboxChecked(mId, mChecked);
		}
		MarkDirty();
	}

	public virtual bool IsChecked()
	{
		return mChecked;
	}

	public override void MouseDown(int x, int y, int theClickCount)
	{
		base.MouseDown(x, y, theClickCount);
	}

	public override void MouseDown(int x, int y, int theBtnNum, int theClickCount)
	{
		base.MouseDown(x, y, theBtnNum, theClickCount);
		mChecked = !mChecked;
		if (mListener != null)
		{
			mListener.CheckboxChecked(mId, mChecked);
		}
		MarkDirty();
	}

	public override void Draw(Graphics g)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		base.Draw(g);
		if (mCheckedRect.mWidth == 0 && mCheckedImage != null && mUncheckedImage != null)
		{
			if (mChecked)
			{
				g.DrawImage(mCheckedImage, 0, 0);
			}
			else
			{
				g.DrawImage(mUncheckedImage, 0, 0);
			}
		}
		else if (mCheckedRect.mWidth != 0 && mUncheckedImage != null)
		{
			if (mChecked)
			{
				g.DrawImage(mUncheckedImage, 0, 0, new TRect(mCheckedRect));
			}
			else
			{
				g.DrawImage(mUncheckedImage, 0, 0, new TRect(mUncheckedRect));
			}
		}
		else if (mUncheckedImage == null && mCheckedImage == null)
		{
			g.SetColor(new SexyColor(mOutlineColor));
			g.FillRect(0, 0, mWidth, mHeight);
			g.SetColor(new SexyColor(mBkgColor));
			g.FillRect(1, 1, mWidth - 2, mHeight - 2);
			if (mChecked)
			{
				g.SetColor(new SexyColor(mCheckColor));
				g.DrawLine(1, 1, mWidth - 2, mHeight - 2);
				g.DrawLine(mWidth - 1, 1, 1, mHeight - 2);
			}
		}
	}

	public Checkbox(Image theUncheckedImage, Image theCheckedImage, int theId, CheckboxListener theCheckboxListener)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		mUncheckedImage = theUncheckedImage;
		mCheckedImage = theCheckedImage;
		mId = theId;
		mListener = theCheckboxListener;
		mChecked = false;
		mOutlineColor = new SexyColor(Color.White);
		mBkgColor = new SexyColor(new Color(80, 80, 80));
		mCheckColor = new SexyColor(new Color(255, 255, 0));
		mDoFinger = true;
	}
}
