using System;
using Microsoft.Xna.Framework;

namespace Sexy;

public struct SexyTransform2D
{
	private bool isInitialised;

	public Matrix mMatrix;

	public SexyTransform2D(bool loadIdentity)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (loadIdentity)
		{
			mMatrix = Matrix.Identity;
		}
		else
		{
			mMatrix = default(Matrix);
		}
		isInitialised = true;
	}

	public void LoadIdentity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		mMatrix = Matrix.Identity;
		isInitialised = true;
	}

	public SexyTransform2D(Matrix theMatrix)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		mMatrix = theMatrix;
		isInitialised = true;
	}

	private void Initiliase()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		mMatrix = Matrix.Identity;
		isInitialised = true;
	}

	public void Translate(float tx, float ty)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!isInitialised)
		{
			Initiliase();
		}
		Matrix val = Matrix.CreateTranslation(tx, ty, 0f);
		Matrix.Multiply(ref mMatrix, ref val, out mMatrix);
	}

	public void RotateRad(float rot)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!isInitialised)
		{
			Initiliase();
		}
		Matrix val = Matrix.CreateRotationZ(rot);
		Matrix.Multiply(ref mMatrix, ref val, out mMatrix);
	}

	public void RotateDeg(float rot)
	{
		if (!isInitialised)
		{
			Initiliase();
		}
		RotateRad((float)Math.PI * rot / 180f);
	}

	public void Scale(float sx, float sy)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!isInitialised)
		{
			Initiliase();
		}
		Matrix val = Matrix.CreateScale(sx, sy, 1f);
		Matrix.Multiply(ref mMatrix, ref val, out mMatrix);
	}

	public static bool operator ==(SexyTransform2D a, SexyTransform2D b)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return a.mMatrix == b.mMatrix;
	}

	public static bool operator !=(SexyTransform2D a, SexyTransform2D b)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return !(a.mMatrix == b.mMatrix);
	}

	public override int GetHashCode()
	{
		return mMatrix.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return obj is SexyTransform2D sexyTransform2D && sexyTransform2D == this;
	}

	public static Vector2 operator *(SexyTransform2D transform, Vector2 v)
	{
		if (!transform.isInitialised)
			transform.Initiliase();

		Vector2.Transform(ref v, ref transform.mMatrix, out var result);
		return result;
	}

	public static Vector2 operator *(Vector2 v, SexyTransform2D transform)
	{
		return transform * v;
	}

	public static SexyTransform2D operator *(SexyTransform2D a, SexyTransform2D b)
	{
		if (!a.isInitialised)
			a.Initiliase();

		if (!b.isInitialised)
			b.Initiliase();

		SexyTransform2D result = new SexyTransform2D(true);
		Matrix.Multiply(ref a.mMatrix, ref b.mMatrix, out result.mMatrix);
		return result;
	}
}
