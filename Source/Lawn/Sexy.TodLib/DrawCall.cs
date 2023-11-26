using Microsoft.Xna.Framework;

namespace Sexy.TodLib;

internal struct DrawCall
{
	public TRect mClipRect;

	public SexyColor mColor;

	public TRect mSrcRect;

	public Vector2 mPosition;

	public double mRotation;

	public Vector2 mScale;

	public void SetTransform(ReanimatorTransform transform)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		mPosition = new Vector2(transform.mTransX, transform.mTransY);
		mRotation = 0.0;
		mScale = new Vector2(transform.mScaleX, transform.mScaleY);
	}
}
