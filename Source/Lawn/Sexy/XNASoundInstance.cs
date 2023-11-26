using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Sexy;

internal class XNASoundInstance : SoundInstance
{
	private float mBaseVolume;

	private float mBasePan;

	private float mVolume;

	private float mPan;

	private float mPitch;

	private bool didPlay;

	private bool mIsReleased;

	private uint mId;

	private SoundEffectInstance mSound;

	private static Stack<XNASoundInstance> unusedObjects = new Stack<XNASoundInstance>(50);

	public uint SoundId => mId;

	public static void PreallocateMemory()
	{
		for (int i = 0; i < 50; i++)
		{
			new XNASoundInstance().PrepareForReuse();
		}
	}

	public static XNASoundInstance GetNewXNASoundInstance(uint id, SoundEffectInstance instance)
	{
		if (unusedObjects.Count > 0)
		{
			XNASoundInstance xNASoundInstance = unusedObjects.Pop();
			xNASoundInstance.Reset(id, instance);
			return xNASoundInstance;
		}
		Console.Write("newsound");
		return new XNASoundInstance(id, instance);
	}

	private XNASoundInstance()
	{
		Reset(0u, null);
	}

	private XNASoundInstance(uint id, SoundEffectInstance instance)
	{
		Reset(id, instance);
	}

	public void Reset(uint id, SoundEffectInstance instance)
	{
		mBaseVolume = 1f;
		mVolume = 1f;
		mBasePan = 0f;
		mPan = 0f;
		mPitch = 0f;
		mIsReleased = false;
		mId = id;
		didPlay = false;
		mSound = instance;
	}

	public void PrepareForReuse()
	{
		unusedObjects.Push(this);
	}

	public override void Release()
	{
		if (mSound != null)
		{
			mSound.Dispose();
			mSound = null;
		}
		mIsReleased = true;
	}

	public override void SetBaseVolume(double theBaseVolume)
	{
		mBaseVolume = (float)theBaseVolume;
	}

	public override void SetBasePan(int theBasePan)
	{
		mBasePan = (float)theBasePan / 100f;
	}

	public override void AdjustPitch(double theNumSteps)
	{
		mPitch = (float)theNumSteps;
	}

	public override void SetVolume(double theVolume)
	{
		mVolume = (float)theVolume;
	}

	public override void SetPan(int thePosition)
	{
		mPan = (float)thePosition / 10000f;
	}

	public override bool Play(bool looping)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		didPlay = true;
		if ((int)mSound.State == 2)
		{
			mSound.IsLooped = looping;
		}
		mSound.Volume = mBaseVolume * mVolume;
		mSound.Pan = Math.Min(Math.Max(-1f, mBasePan + mPan), 1f);
		mSound.Pitch = mPitch / 10f;
		mSound.Play();
		return true;
	}

	public override bool Play(bool looping, bool autoRelease)
	{
		return Play(looping);
	}

	public override void Stop()
	{
		if (mSound != null)
		{
			mSound.Stop();
		}
	}

	public override bool IsPlaying()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (mSound != null)
		{
			return (int)mSound.State == 0;
		}
		return false;
	}

	public override bool IsDormant()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (didPlay)
		{
			return (int)mSound.State == 2;
		}
		return false;
	}

	public override double GetVolume()
	{
		return mVolume;
	}

	public override bool IsReleased()
	{
		return mIsReleased;
	}
}
