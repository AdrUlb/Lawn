using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Sexy.TodLib;

internal class TrailReader : ContentTypeReader<TrailDefinition>
{
	protected override TrailDefinition Read(ContentReader input, TrailDefinition value)
	{
		TrailDefinition trailDefinition = new TrailDefinition();
		trailDefinition.mImageName = ((BinaryReader)(object)input).ReadString();
		trailDefinition.mMaxPoints = ((BinaryReader)(object)input).ReadInt32();
		trailDefinition.mMinPointDistance = (float)((BinaryReader)(object)input).ReadDouble();
		trailDefinition.mTrailFlags = ((BinaryReader)(object)input).ReadInt32();
		ReadFloatParameterTrack(ref input, out trailDefinition.mTrailDuration);
		ReadFloatParameterTrack(ref input, out trailDefinition.mWidthOverLength);
		ReadFloatParameterTrack(ref input, out trailDefinition.mWidthOverTime);
		ReadFloatParameterTrack(ref input, out trailDefinition.mAlphaOverLength);
		ReadFloatParameterTrack(ref input, out trailDefinition.mAlphaOverTime);
		return trailDefinition;
	}

	private void ReadFloatParameterTrack(ref ContentReader input, out FloatParameterTrack value)
	{
		value = new FloatParameterTrack();
		value.mCountNodes = ((BinaryReader)(object)input).ReadInt32();
		value.mNodes = new FloatParameterTrackNode[value.mCountNodes];
		for (int i = 0; i < value.mCountNodes; i++)
		{
			FloatParameterTrackNode floatParameterTrackNode = new FloatParameterTrackNode();
			floatParameterTrackNode.mCurveType = (TodCurves)((BinaryReader)(object)input).ReadInt32();
			floatParameterTrackNode.mDistribution = (TodCurves)((BinaryReader)(object)input).ReadInt32();
			floatParameterTrackNode.mHighValue = (float)((BinaryReader)(object)input).ReadDouble();
			floatParameterTrackNode.mLowValue = (float)((BinaryReader)(object)input).ReadDouble();
			floatParameterTrackNode.mTime = (float)((BinaryReader)(object)input).ReadDouble();
			value.mNodes[i] = floatParameterTrackNode;
		}
	}
}
