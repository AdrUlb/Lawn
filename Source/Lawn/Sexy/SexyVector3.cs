using System;
using Microsoft.Xna.Framework;

namespace Sexy;

internal struct SexyVector3
{
	public Vector3 mVector;

	public float x
	{
		get
		{
			return mVector.X;
		}
		set
		{
			mVector.X = value;
		}
	}

	public float y
	{
		get
		{
			return mVector.Y;
		}
		set
		{
			mVector.Y = value;
		}
	}

	public float z
	{
		get
		{
			return mVector.Z;
		}
		set
		{
			mVector.Z = value;
		}
	}

	public SexyVector3(float theX, float theY, float theZ)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		mVector = new Vector3(theX, theY, theZ);
	}

	public SexyVector3(Vector3 theVector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		mVector = theVector;
	}

	public float Dot(SexyVector3 rhs)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Dot(mVector, rhs.mVector);
	}

	public SexyVector3 Cross(SexyVector3 v)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 theVector = default(Vector3);
		Vector3.Cross(ref mVector, ref v.mVector, ref theVector);
		return new SexyVector3(theVector);
	}

	public static SexyVector3 operator +(SexyVector3 lhs, SexyVector3 rhs)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new SexyVector3(lhs.mVector + rhs.mVector);
	}

	public static SexyVector3 operator -(SexyVector3 lhs, SexyVector3 rhs)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return new SexyVector3(lhs.mVector - rhs.mVector);
	}

	public static SexyVector3 operator *(float t, SexyVector3 rhs)
	{
		return new SexyVector3(t * rhs.x, t * rhs.y, t * rhs.z);
	}

	public static SexyVector3 operator /(float t, SexyVector3 rhs)
	{
		return new SexyVector3(rhs.x / t, rhs.y / t, rhs.z / t);
	}

	public float Norm()
	{
		return x * x + y * y + z * z;
	}

	public float Magnitude()
	{
		return (float)Math.Sqrt(Norm());
	}

	public SexyVector3 Normalize()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((Vector3)(ref mVector)).Normalize();
		return new SexyVector3(mVector);
	}

	public override string ToString()
	{
		return ((object)(Vector3)(ref mVector)).ToString();
	}
}
