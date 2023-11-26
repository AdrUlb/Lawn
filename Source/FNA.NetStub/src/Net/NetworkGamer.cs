﻿#region License
/* FNA.NetStub - XNA4 Xbox Live Stub DLL
 * Copyright 2019 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;

using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public class NetworkGamer : Gamer
	{
		#region Public Properties

		public bool HasLeftSession
		{
			get;
			private set;
		}

		public bool HasVoice
		{
			get;
			private set;
		}

		public byte Id
		{
			get
			{
				return 0;
			}
		}

		public bool IsGuest
		{
			get;
			private set;
		}

		public bool IsHost
		{
			get
			{
				return true;
			}
		}

		public bool IsLocal
		{
			get
			{
				return (this is LocalNetworkGamer);
			}
		}

		public bool IsMutedByLocalUser
		{
			get;
			private set;
		}

		public bool IsPrivateSlot
		{
			get;
			private set;
		}

		public bool IsReady
		{
			// InvalidOperationException: This operation is only valid when the session state is NetworkSessionState.Lobby.
			// InvalidOperationException: This method cannot be called on remote gamer instances. It is only valid when NetworkGamer.IsLocal is true.
			// ObjectDisposedException: This NetworkGamer is no longer valid. The gamer may have left the session.
			get;
			set;
		}

		public bool IsTalking
		{
			get;
			private set;
		}

		public NetworkMachine Machine
		{
			get;
			set;
		}

		public TimeSpan RoundtripTime
		{
			get;
			private set;
		}

		public NetworkSession Session
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal NetworkGamer(
			NetworkSession session
		) : base(
			"Stub Gamer",
			"Stub Gamer"
		) {
			Session = session;

			// TODO: Everything below
			HasLeftSession = false;
			HasVoice = false;
			IsGuest = false;
			IsMutedByLocalUser = false;
			IsPrivateSlot = false;
			IsReady = false;
			IsTalking = false;
			Machine = new NetworkMachine();
			RoundtripTime = TimeSpan.Zero;
		}

		#endregion
	}
}
