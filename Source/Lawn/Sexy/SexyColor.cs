using Microsoft.Xna.Framework;

namespace Sexy;

public struct SexyColor
{
	public Color Color;

	private static SexyColor mWhite = new SexyColor(Color.White);

	private static SexyColor mBlack = new SexyColor(Color.Black);

	public int mRed
	{
		get
		{
			return ((Color)(ref Color)).R;
		}
		set
		{
			((Color)(ref Color)).R = (byte)value;
		}
	}

	public int mGreen
	{
		get
		{
			return ((Color)(ref Color)).G;
		}
		set
		{
			((Color)(ref Color)).G = (byte)value;
		}
	}

	public int mBlue
	{
		get
		{
			return ((Color)(ref Color)).B;
		}
		set
		{
			((Color)(ref Color)).B = (byte)value;
		}
	}

	public int mAlpha
	{
		get
		{
			return ((Color)(ref Color)).A;
		}
		set
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			float num = (float)value / 255f;
			Color *= num;
			((Color)(ref Color)).A = (byte)value;
		}
	}

	public static SexyColor Black => mBlack;

	public static SexyColor White => mWhite;

	public int this[int theIdx] => theIdx switch
	{
		0 => ((Color)(ref Color)).R, 
		1 => ((Color)(ref Color)).G, 
		2 => ((Color)(ref Color)).B, 
		3 => ((Color)(ref Color)).A, 
		_ => 0, 
	};

	public static SexyColor Premultiply(SexyColor col)
	{
		col.mRed = (int)((float)(col.mRed * col.mAlpha) / 255f);
		col.mGreen = (int)((float)(col.mGreen * col.mAlpha) / 255f);
		col.mBlue = (int)((float)(col.mBlue * col.mAlpha) / 255f);
		return col;
	}

	public void PremultiplyAlpha()
	{
		mRed = (int)((float)(mRed * mAlpha) / 255f);
		mGreen = (int)((float)(mGreen * mAlpha) / 255f);
		mBlue = (int)((float)(mBlue * mAlpha) / 255f);
	}

	public SexyColor(int theRed, int theGreen, int theBlue)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Color = new Color(theRed, theGreen, theBlue);
	}

	public SexyColor(int theRed, int theGreen, int theBlue, int theAlpha)
		: this(theRed, theGreen, theBlue, theAlpha, premultiply: true)
	{
	}

	public SexyColor(int theRed, int theGreen, int theBlue, int theAlpha, bool premultiply)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Color = new Color(theRed, theGreen, theBlue, theAlpha);
		if (premultiply)
		{
			Color = Color.Multiply(Color, (float)theAlpha / 255f);
			((Color)(ref Color)).A = (byte)theAlpha;
		}
	}

	public SexyColor(string theElements)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Color = new Color((int)theElements[0], (int)theElements[1], (int)theElements[2], 255);
	}

	public SexyColor(Color theColor)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Color = theColor;
	}

	public static bool operator ==(SexyColor a, SexyColor b)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return a.Color == b.Color;
	}

	public static bool operator !=(SexyColor a, SexyColor b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (obj is SexyColor)
		{
			return Color == ((SexyColor)obj).Color;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((object)(Color)(ref Color)).GetHashCode();
	}

	public override string ToString()
	{
		return ((object)(Color)(ref Color)).ToString();
	}

	public static implicit operator SexyColor(Color color)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		SexyColor result = default(SexyColor);
		result.Color = color;
		return result;
	}

	public static implicit operator Color(SexyColor aColor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return aColor.Color;
	}

	public static SexyColor FromColor(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return new SexyColor(c);
	}

	internal void CopyFrom(Color c)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Color = c;
	}
}
