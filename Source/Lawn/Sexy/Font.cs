using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sexy;

internal class Font
{
	internal class CachedStringInfo
	{
		public string[] Strings { get; private set; }

		public Vector2[] StringDimensions { get; private set; }

		public CachedStringInfo(string[] substrings, Font fontUsed)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			Strings = substrings;
			StringDimensions = (Vector2[])(object)new Vector2[Strings.Length];
			for (int i = 0; i < Strings.Length; i++)
			{
				ref Vector2 reference = ref StringDimensions[i];
				reference = fontUsed.MeasureString(Strings[i]);
				ref Vector2 reference2 = ref StringDimensions[i];
				reference2.Y *= fontUsed.scale.Y;
			}
		}

		public override int GetHashCode()
		{
			return Strings.GetHashCode();
		}
	}

	public int mAscent;

	public int mAscentPadding;

	public int characterOffsetMagic;

	private int height;

	public float mScaleX = 1f;

	public float mScaleY = 1f;

	private static int nextId;

	private List<bool> enabledLayers = new List<bool>();

	public int mLineSpacingOffset;

	private List<SpriteFont> mFonts;

	private List<Vector2> mOffsets;

	private Dictionary<char, Vector2> mCharOffsets = new Dictionary<char, Vector2>();

	public string SpaceChar = "k";

	private StringBuilder drawStringBuilder = new StringBuilder();

	public bool StringWidthCachingEnabled = true;

	private static Dictionary<TRect, Dictionary<string, CachedStringInfo>> allCachedStringWidths;

	private static Stack<Dictionary<char, int>> charDictionaries;

	private static Stack<Dictionary<string, int>> stringDictionaries;

	private Dictionary<float, Dictionary<string, int>> cachedStringWidths = new Dictionary<float, Dictionary<string, int>>(10);

	private Dictionary<float, Dictionary<char, int>> cachedStringBuilderWidths = new Dictionary<float, Dictionary<char, int>>(5);

	public int FontId { get; private set; }

	public Vector2 scale => new Vector2(mScaleX * FrameworkConstants.Font_Scale, mScaleY * FrameworkConstants.Font_Scale);

	public int mHeight
	{
		get
		{
			return (int)((float)height * mScaleY * FrameworkConstants.Font_Scale);
		}
		set
		{
			height = value;
		}
	}

	public int LayerCount => mFonts.Count;

	public void AddCharacterOffset(char c, Vector2 offset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		mCharOffsets.Add(c, offset);
	}

	static Font()
	{
		allCachedStringWidths = new Dictionary<TRect, Dictionary<string, CachedStringInfo>>(20);
		charDictionaries = new Stack<Dictionary<char, int>>(20);
		stringDictionaries = new Stack<Dictionary<string, int>>(20);
		for (int i = 0; i < 40; i++)
		{
			charDictionaries.Push(new Dictionary<char, int>(100));
		}
		for (int j = 0; j < 40; j++)
		{
			stringDictionaries.Push(new Dictionary<string, int>(100));
		}
	}

	private static Dictionary<char, int> GetNewCharDictionary()
	{
		if (charDictionaries.Count > 0)
		{
			return charDictionaries.Pop();
		}
		return new Dictionary<char, int>(100);
	}

	private static Dictionary<string, int> GetNewStringDictionary()
	{
		if (stringDictionaries.Count > 0)
		{
			return stringDictionaries.Pop();
		}
		return new Dictionary<string, int>(100);
	}

	public CachedStringInfo GetWordWrappedSubStrings(string theLine, TRect theRect)
	{
		if (!allCachedStringWidths.TryGetValue(theRect, out var value))
		{
			value = new Dictionary<string, CachedStringInfo>(10);
			allCachedStringWidths.Add(theRect, value);
		}
		if (!value.TryGetValue(theLine, out var value2))
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			List<StringBuilder> list = new List<StringBuilder>();
			list.Add(new StringBuilder());
			for (int i = 0; i < theLine.Length; i++)
			{
				if (char.IsWhiteSpace(theLine[i]))
				{
					int num5 = StringWidth(theLine.Substring(num2, i - num2));
					if (num5 < theRect.mWidth)
					{
						list[list.Count - 1].Append(theLine, num, i - num);
						num3 = (num = i);
						num4 = num5;
					}
					else
					{
						list.Add(new StringBuilder());
						list[list.Count - 1].Append(theLine, num, i - num);
						num2 = num;
						num4 = StringWidth(theLine.Substring(num, i - num));
						num3 = (num = i);
					}
				}
			}
			if (num4 + StringWidth(theLine.Substring(num3, theLine.Length - num3)) > theRect.mWidth && list[list.Count - 1].Length > 0)
			{
				list.Add(new StringBuilder());
			}
			list[list.Count - 1].Append(theLine, num3, theLine.Length - num3);
			string[] array = new string[list.Count];
			for (int j = 0; j < list.Count; j++)
			{
				array[j] = list[j].ToString().Trim();
			}
			value2 = new CachedStringInfo(array, this);
			value.Add(theLine, value2);
		}
		return value2;
	}

	public Font()
	{
		FontId = nextId++;
		drawStringBuilder.Append("A");
		mAscent = 0;
		mHeight = 0;
		mAscentPadding = 0;
		mLineSpacingOffset = 0;
		mFonts = new List<SpriteFont>();
		mOffsets = new List<Vector2>();
	}

	public Font(Font theFont)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		FontId = nextId++;
		drawStringBuilder.Append("A");
		mAscent = theFont.mAscent;
		mHeight = theFont.mHeight;
		mAscentPadding = theFont.mAscentPadding;
		mLineSpacingOffset = theFont.mLineSpacingOffset;
		mFonts = new List<SpriteFont>();
		for (int i = 0; i < theFont.mFonts.Count; i++)
		{
			mFonts.Add(theFont.mFonts[i]);
		}
		mOffsets = new List<Vector2>();
		for (int j = 0; j < theFont.mOffsets.Count; j++)
		{
			mOffsets.Add(theFont.mOffsets[j]);
		}
	}

	public static int FontComparer(Font a, Font b)
	{
		if (a == null && b == null)
		{
			return 0;
		}
		if (a == null)
		{
			return 1;
		}
		if (b == null)
		{
			return -1;
		}
		return a.FontId.CompareTo(b.FontId);
	}

	public SpriteFont GetFontLayer(int i)
	{
		return mFonts[i];
	}

	public void AddLayer(SpriteFont font)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		AddLayer(font, Vector2.Zero);
	}

	public void AddLayer(SpriteFont font, Vector2 offset)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		mFonts.Add(font);
		mOffsets.Add(offset);
		enabledLayers.Add(item: true);
	}

	public void EnableLayer(int layer, bool enabled)
	{
		enabledLayers[layer] = enabled;
	}

	public void Dispose()
	{
	}

	public int GetAscent()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return (int)((float)mAscent * scale.Y);
	}

	public int GetAscentPadding()
	{
		return mAscentPadding;
	}

	public int GetDescent()
	{
		int num = GetHeight();
		return num + mAscent;
	}

	public int GetHeight()
	{
		return StringHeight("1");
	}

	public int GetLineSpacingOffset()
	{
		return mLineSpacingOffset;
	}

	public int GetLineSpacing()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < mFonts.Count; i++)
		{
			if (mFonts[i].LineSpacing > num)
			{
				num = mFonts[i].LineSpacing;
			}
		}
		return (int)((float)num * scale.Y);
	}

	public int StringWidth(string theString)
	{
		int value2;
		if (StringWidthCachingEnabled)
		{
			Dictionary<string, int> value = null;
			if (StringWidthCachingEnabled && !cachedStringWidths.TryGetValue(mScaleX, out value))
			{
				value = GetNewStringDictionary();
				cachedStringWidths.Add(mScaleX, value);
			}
			if (!value.TryGetValue(theString, out value2))
			{
				value2 = ComputeStringWidthForCache(theString);
				value.Add(theString, value2);
			}
		}
		else
		{
			value2 = ComputeStringWidthForCache(theString);
		}
		return value2;
	}

	private int ComputeStringWidthForCache(string theString)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		for (int i = 0; i < mFonts.Count; i++)
		{
			float num2 = mOffsets[i].X * mScaleX * FrameworkConstants.Font_Scale;
			for (int j = 0; j < theString.Length; j++)
			{
				if (char.IsWhiteSpace(theString[j]))
				{
					num2 += (float)StringWidth(SpaceChar);
					continue;
				}
				drawStringBuilder[0] = theString[j];
				num2 += (float)(StringWidth(drawStringBuilder) - characterOffsetMagic);
			}
			if (num2 > num)
			{
				num = num2;
			}
		}
		return (int)num;
	}

	public int StringWidth(StringBuilder theString)
	{
		int result = 0;
		if (theString.Length == 0)
		{
			return result;
		}
		return ComputeStringWidthForCache(theString);
	}

	private int ComputeStringWidthForCache(StringBuilder theString)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		int num = int.MaxValue;
		for (int i = 0; i < mFonts.Count; i++)
		{
			int num2 = 0;
			if (theString.Length <= 1)
			{
				if (char.IsWhiteSpace(theString[0]))
				{
					num2 += StringWidth(SpaceChar);
				}
				else if (mCharOffsets.ContainsKey(theString[0]))
				{
					num2 += (int)(mFonts[i].MeasureString(theString).X * mScaleX * FrameworkConstants.Font_Scale);
				}
			}
			else
			{
				for (int j = 0; j < theString.Length; j++)
				{
					drawStringBuilder[0] = theString[j];
					num2 += StringWidth(drawStringBuilder);
				}
			}
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num - characterOffsetMagic * theString.Length;
	}

	public int StringHeight(string theString)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < mFonts.Count; i++)
		{
			for (int j = 0; j < theString.Length; j++)
			{
				if (char.IsWhiteSpace(theString[j]))
				{
					drawStringBuilder[0] = SpaceChar[0];
				}
				else
				{
					drawStringBuilder[0] = theString[j];
				}
				int num2 = 0;
				if (mCharOffsets.ContainsKey(drawStringBuilder[0]))
				{
					num2 = (int)mFonts[i].MeasureString(drawStringBuilder).Y;
				}
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return (int)((float)num * scale.Y);
	}

	public Vector2 MeasureString(string theString)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)StringWidth(theString), (float)StringHeight(theString));
	}

	public int CharWidth(char theChar)
	{
		drawStringBuilder[0] = theChar;
		return StringWidth(drawStringBuilder);
	}

	public int CharWidthKern(char theChar, char thePrevChar)
	{
		return CharWidth(theChar);
	}

	public void DrawStringLayer(Graphics g, int theX, int theY, string theString, Color theColor, int layer)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		if (!enabledLayers[layer])
		{
			return;
		}
		var val = new Vector2(theX, theY);
		var val2 = val + mOffsets[layer] * mScaleX * FrameworkConstants.Font_Scale;
		if (!string.IsNullOrEmpty(theString) && mCharOffsets.ContainsKey(theString[0]))
		{
			val2.X -= mCharOffsets[theString[0]].X * mScaleX * FrameworkConstants.Font_Scale;
		}
		for (int i = 0; i < theString.Length; i++)
		{
			if (char.IsWhiteSpace(theString[i]))
			{
				val2.X += (float)StringWidth(SpaceChar);
				continue;
			}
			drawStringBuilder[0] = theString[i];
			if (mCharOffsets.ContainsKey(theString[i]))
			{
				Vector2 val3 = val2 + mCharOffsets[theString[i]] * mScaleX * FrameworkConstants.Font_Scale;
				Graphics.spriteBatch.DrawString(mFonts[layer], drawStringBuilder, val3, theColor, 0f, Vector2.Zero, scale, (SpriteEffects)0, 1f - (float)layer / (float)mFonts.Count);
				val2.X += (float)(StringWidth(drawStringBuilder) - characterOffsetMagic);
			}
			else
			{
				Vector2 val3 = val2;
			}
		}
	}

	public void DrawString(Graphics g, int theX, int theY, string theString, Color theColor)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(theString))
		{
			g.EndDrawImageTransformed();
			for (int i = 0; i < mFonts.Count; i++)
			{
				DrawStringLayer(g, theX, theY, theString, theColor, i);
			}
		}
	}

	public void DrawString(Graphics g, int theX, int theY, StringBuilder theString, Color theColor)
	{
		if (theString.Length == 0)
			return;

		g.EndDrawImageTransformed();
		var val = new Vector2((float)theX, (float)theY);
		for (int i = 0; i < mFonts.Count; i++)
		{
			if (!enabledLayers[i])
			{
				continue;
			}
			Vector2 val2 = val + mOffsets[i] * mScaleX * FrameworkConstants.Font_Scale;
			for (int j = 0; j < theString.Length; j++)
			{
				if (char.IsWhiteSpace(theString[j]))
				{
					val2.X += (float)StringWidth(SpaceChar);
					continue;
				}
				drawStringBuilder[0] = theString[j];
				if (mCharOffsets.ContainsKey(theString[j]))
				{
					Vector2 val3 = val2 + mCharOffsets[theString[j]] * mScaleX * FrameworkConstants.Font_Scale;
					Graphics.spriteBatch.DrawString(mFonts[i], drawStringBuilder, val3, theColor, 0f, Vector2.Zero, scale, (SpriteEffects)0, 1f - (float)i / (float)mFonts.Count);
					val2.X += (float)(StringWidth(drawStringBuilder) - characterOffsetMagic);
				}
				else
				{
					Vector2 val3 = val2;
				}
			}
		}
	}

	internal Font Duplicate()
	{
		return this;
	}
}
