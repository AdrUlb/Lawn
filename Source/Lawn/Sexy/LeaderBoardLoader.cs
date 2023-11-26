using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.GamerServices;

namespace Sexy;

internal class LeaderBoardLoader
{
	public enum LoaderState
	{
		Idle,
		Loading,
		Loaded
	}

	public enum ErrorStates
	{
		None,
		GameUpdateRequired,
		Error
	}

	public delegate void LoadingCompletedhandler(LeaderBoardLoader loader);

	private const int REQUEST_RESEND_DELAY = 30;

	private const int PAGE_SIZE = 5;

	public int CACHE_DURATION = int.MaxValue;

	public Dictionary<int, LeaderboardEntry> LeaderboardEntries = new Dictionary<int, LeaderboardEntry>(100);

	public int LeaderboardEntryCount;

	private DateTime requestSendTime = DateTime.MinValue;

	private DateTime resultsReceived = DateTime.MinValue;

	private bool pagingUp;

	private bool pagingDown;

	private LeaderboardReader reader;

	public ErrorStates ErrorState { get; private set; }

	public int SignedInGamerIndex { get; private set; }

	public int GameMode { get; private set; }

	public LoaderState LeaderboardConnectionState
	{
		get
		{
			if ((DateTime.UtcNow - resultsReceived).TotalSeconds < (double)CACHE_DURATION)
			{
				return LoaderState.Loaded;
			}
			if (!((DateTime.UtcNow - requestSendTime).TotalSeconds >= 30.0))
			{
				return LoaderState.Loading;
			}
			return LoaderState.Idle;
		}
	}

	public event LoadingCompletedhandler LoadingCompleted;

	public void ResetCache()
	{
		resultsReceived = DateTime.MinValue;
	}

	public void SendRequest()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (((ReadOnlyCollection<SignedInGamer>)(object)Gamer.SignedInGamers).Count == 0)
		{
			return;
		}
		SignedInGamer gamer = Main.GetGamer();
		if (LeaderboardConnectionState == LoaderState.Idle)
		{
			try
			{
				LeaderboardIdentity val = LeaderboardIdentity.Create((LeaderboardKey)0, GameMode);
				LeaderboardReader.BeginRead(val, (Gamer)(object)gamer, 5, (AsyncCallback)RequestReceived, (object)gamer);
			}
			catch (GameUpdateRequiredException)
			{
				GlobalStaticVars.gSexyAppBase.ShowUpdateRequiredMessage();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error in GetResultsCallBack. {0}", ex.Message);
			}
			requestSendTime = DateTime.UtcNow;
		}
	}

	public void LoadEntry(int index)
	{
		if (index < reader.PageStart)
		{
			if (!pagingUp && reader.CanPageUp)
			{
				pagingUp = true;
				reader.BeginPageUp((AsyncCallback)PageUpRequestReceived, (object)Main.GetGamer());
			}
		}
		else if (!pagingDown && reader.CanPageDown)
		{
			pagingDown = true;
			reader.BeginPageDown((AsyncCallback)PageDownRequestReceived, (object)Main.GetGamer());
		}
	}

	private void PageUpRequestReceived(IAsyncResult result)
	{
		pagingUp = false;
		ProcessData(result, clearList: false);
	}

	private void PageDownRequestReceived(IAsyncResult result)
	{
		pagingDown = false;
		ProcessData(result, clearList: false);
	}

	private void RequestReceived(IAsyncResult result)
	{
		ProcessData(result, clearList: true);
	}

	private void ProcessData(IAsyncResult result, bool clearList)
	{
		requestSendTime = DateTime.MinValue;
		lock (LeaderBoardComm.LeaderboardLock)
		{
			try
			{
				reader = LeaderboardReader.EndRead(result);
				LeaderboardEntryCount = reader.TotalLeaderboardSize;
				if (clearList)
				{
					LeaderboardEntries.Clear();
				}
				UpdateEntriesFromReader();
				resultsReceived = DateTime.UtcNow;
			}
			catch (GameUpdateRequiredException)
			{
				ErrorState = ErrorStates.GameUpdateRequired;
			}
			catch (Exception ex)
			{
				_ = ex.Message;
				Console.WriteLine("Error in RequestReceived in LeaderBoardLoader. {0}", ex.Message);
				if (LeaderboardEntries.Count == 0)
				{
					ErrorState = ErrorStates.Error;
				}
				else
				{
					ErrorState = ErrorStates.None;
				}
			}
		}
		if (this.LoadingCompleted != null)
		{
			this.LoadingCompleted(this);
		}
	}

	private void UpdateEntriesFromReader()
	{
		Gamer gamer = (Gamer)(object)Main.GetGamer();
		for (int i = 0; i < reader.Entries.Count; i++)
		{
			int key = reader.PageStart + i;
			LeaderboardEntry val = reader.Entries[i];
			if (LeaderboardEntries.ContainsKey(key))
			{
				LeaderboardEntries[key] = val;
			}
			else
			{
				LeaderboardEntries.Add(key, val);
			}
			if (gamer != null && val.Gamer.Gamertag == gamer.Gamertag)
			{
				SignedInGamerIndex = i;
			}
		}
	}

	public LeaderBoardLoader(LeaderboardGameMode gameMode)
	{
		ErrorState = ErrorStates.None;
		GameMode = LeaderBoardHelper.GetLeaderboardNumber(gameMode);
	}
}
