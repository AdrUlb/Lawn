using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sexy;

public struct TriVertex
{
	public VertexPositionColorTexture mVert;

	public Color color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return mVert.Color;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			mVert.Color = value;
		}
	}

	public float x
	{
		get
		{
			return mVert.Position.X;
		}
		set
		{
			mVert.Position.X = value;
		}
	}

	public float y
	{
		get
		{
			return mVert.Position.Y;
		}
		set
		{
			mVert.Position.Y = value;
		}
	}

	public float u
	{
		get
		{
			return mVert.TextureCoordinate.X;
		}
		set
		{
			mVert.TextureCoordinate.X = value;
		}
	}

	public float v
	{
		get
		{
			return mVert.TextureCoordinate.Y;
		}
		set
		{
			mVert.TextureCoordinate.Y = value;
		}
	}

	public TriVertex(float theX, float theY)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		mVert.Position = new Vector3(theX, theY, 0f);
		mVert.Color = Color.White;
		mVert.TextureCoordinate = Vector2.Zero;
	}

	public TriVertex(float theX, float theY, float theU, float theV)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		mVert.Position = new Vector3(theX, theY, 0f);
		mVert.TextureCoordinate = new Vector2(theU, theV);
		mVert.Color = Color.White;
	}

	public TriVertex(float theX, float theY, float theU, float theV, Color theColor)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		mVert.Position = new Vector3(theX, theY, 0f);
		mVert.TextureCoordinate = new Vector2(theU, theV);
		mVert.Color = theColor;
	}
}
