using System;
using Microsoft.Xna.Framework;

namespace Sexy;

public struct TRect : IEquatable<TRect>
{
	private Rectangle mRect;

	public static TRect Empty = new TRect(0, 0, 0, 0);

	public Rectangle Rect
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mRect;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			mRect = value;
		}
	}

	public int mX
	{
		get
		{
			return mRect.X;
		}
		set
		{
			mRect.X = value;
		}
	}

	public int mY
	{
		get
		{
			return mRect.Y;
		}
		set
		{
			mRect.Y = value;
		}
	}

	public int mWidth
	{
		get
		{
			return mRect.Width;
		}
		set
		{
			mRect.Width = value;
		}
	}

	public int mHeight
	{
		get
		{
			return mRect.Height;
		}
		set
		{
			mRect.Height = value;
		}
	}

	public bool IsEmpty => ((Rectangle)(ref mRect)).IsEmpty;

	public TRect(int theX, int theY, int theWidth, int theHeight)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		mRect = new Rectangle
		{
			X = theX,
			Y = theY,
			Width = theWidth,
			Height = theHeight
		};
	}

	public TRect(TRect theTRect)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		mRect = theTRect.mRect;
	}

	public bool Intersects(TRect theTRect)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((Rectangle)(ref mRect)).Intersects(theTRect.mRect);
	}

	public TRect Intersection(TRect theTRect)
	{
		int num = Math.Max(mRect.X, theTRect.mRect.X);
		int num2 = Math.Min(mRect.X + mRect.Width, theTRect.mRect.X + theTRect.mRect.Width);
		int num3 = Math.Max(mRect.Y, theTRect.mRect.Y);
		int num4 = Math.Min(mRect.Y + mRect.Height, theTRect.mRect.Y + theTRect.mRect.Height);
		if (num2 - num < 0 || num4 - num3 < 0)
		{
			return new TRect(0, 0, 0, 0);
		}
		return new TRect(num, num3, num2 - num, num4 - num3);
	}

	public TRect Union(TRect theTRect)
	{
		int num = Math.Min(mRect.X, theTRect.mRect.X);
		int num2 = Math.Max(mRect.X + mRect.Width, theTRect.mRect.X + theTRect.mRect.Width);
		int num3 = Math.Min(mRect.Y, theTRect.mRect.Y);
		int num4 = Math.Max(mRect.Y + mRect.Height, theTRect.mRect.Y + theTRect.mRect.Height);
		return new TRect(num, num3, num2 - num, num4 - num3);
	}

	public bool Contains(int theX, int theY)
	{
		return ((Rectangle)(ref mRect)).Contains(theX, theY);
	}

	public bool Contains(TPoint thePoint)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((Rectangle)(ref mRect)).Contains(thePoint.Point);
	}

	public void Offset(int theX, int theY)
	{
		((Rectangle)(ref mRect)).Offset(theX, theY);
	}

	public void Offset(TPoint thePoint)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((Rectangle)(ref mRect)).Offset(thePoint.Point);
	}

	public TRect Inflate(int theX, int theY)
	{
		((Rectangle)(ref mRect)).Inflate(theX, theY);
		return this;
	}

	public static bool operator ==(TRect a, TRect b)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return ((Rectangle)(ref a.mRect)).Equals(b.mRect);
	}

	public override bool Equals(object obj)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!(obj is TRect tRect))
		{
			return false;
		}
		return mRect == tRect.mRect;
	}

	public override int GetHashCode()
	{
		return ((object)(Rectangle)(ref mRect)).GetHashCode();
	}

	public override string ToString()
	{
		return ((object)(Rectangle)(ref mRect)).ToString();
	}

	public static bool operator !=(TRect a, TRect b)
	{
		return !(a == b);
	}

	public static explicit operator TRect(Rectangle rect)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		TRect result = default(TRect);
		result.Rect = rect;
		return result;
	}

	public static implicit operator Rectangle(TRect aRect)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return aRect.Rect;
	}

	bool IEquatable<TRect>.Equals(TRect other)
	{
		return this == other;
	}
}
