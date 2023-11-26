using System;
using Microsoft.Xna.Framework;

namespace Sexy;

public struct TRectDouble
{
	private Rectangle mRect;

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

	public TRectDouble(int theX, int theY, int theWidth, int theHeight)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		mRect = new Rectangle(theX, theY, theWidth, theHeight);
	}

	public TRectDouble(TRectDouble theTRect)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		mRect = theTRect.mRect;
	}

	public bool Intersects(TRectDouble theTRect)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((Rectangle)(ref mRect)).Intersects(theTRect.mRect);
	}

	public TRectDouble Intersection(TRectDouble theTRect)
	{
		int num = Math.Max(mRect.X, theTRect.mRect.X);
		int num2 = Math.Min(mRect.X + mRect.Width, theTRect.mRect.X + theTRect.mRect.Width);
		int num3 = Math.Max(mRect.Y, theTRect.mRect.Y);
		int num4 = Math.Min(mRect.Y + mRect.Height, theTRect.mRect.Y + theTRect.mRect.Height);
		if (num2 - num < 0 || num4 - num3 < 0)
		{
			return new TRectDouble(0, 0, 0, 0);
		}
		return new TRectDouble(num, num3, num2 - num, num4 - num3);
	}

	public TRectDouble Union(TRectDouble theTRect)
	{
		int num = Math.Min(mRect.X, theTRect.mRect.X);
		int num2 = Math.Max(mRect.X + mRect.Width, theTRect.mRect.X + theTRect.mRect.Width);
		int num3 = Math.Min(mRect.Y, theTRect.mRect.Y);
		int num4 = Math.Max(mRect.Y + mRect.Height, theTRect.mRect.Y + theTRect.mRect.Height);
		return new TRectDouble(num, num3, num2 - num, num4 - num3);
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

	public TRectDouble Inflate(int theX, int theY)
	{
		((Rectangle)(ref mRect)).Inflate(theX, theY);
		return this;
	}

	public static bool operator ==(TRectDouble a, TRectDouble b)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (object.ReferenceEquals(a, b) || object.ReferenceEquals(a.mRect, b.mRect))
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return ((object)(Rectangle)(ref a.mRect)).Equals((object)b);
	}

	public override bool Equals(object obj)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!(obj is TRectDouble tRectDouble))
		{
			return false;
		}
		return mRect == tRectDouble.mRect;
	}

	public override int GetHashCode()
	{
		return ((object)(Rectangle)(ref mRect)).GetHashCode();
	}

	public override string ToString()
	{
		return ((object)(Rectangle)(ref mRect)).ToString();
	}

	public static bool operator !=(TRectDouble a, TRectDouble b)
	{
		return !(a == b);
	}
}
