using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Text.RegularExpressions;
using StardewValley.BellsAndWhistles;
using Microsoft.Xna.Framework;

namespace MPSpeechBubbles
{
	public class MPSpeechBubbles : Mod
	{

		public int chatCount;
		static ModConfig config;
		public IList<ChatMessage> cMsgs;
		Dictionary<Farmer, List<SpeechBubble>> chatDict = new Dictionary<Farmer, List<SpeechBubble>>();
		bool debug = Kal_Extensions.debug;

		class SpeechBubble
		{
			DateTime born;
			public string msg;
			public int shake;

			public SpeechBubble(DateTime dateTime, string message)
			{
				this.born = dateTime;
				this.msg = message;
				this.shake = checkIsShake();
			}

			/// <summary>
			/// Determines if the message should use the 'shake' variant.
			/// !! x2 = shake
			/// >40% is CAPS = shake
			/// </summary>
			/// <returns></returns>
			private int checkIsShake()
			{
				if (msg.Count(x => x == '!') > 2)
				{
					return 0;
				}

				int countUpper = Regex.Matches(msg, @"\p{Lu}").Count;
				if (countUpper >= (msg.Length * 0.4))
					return 0;

				return 1;
			}

			public bool IsExpired()
			{
				return ((DateTime.Now).Subtract(born) >= TimeSpan.FromSeconds(MPSpeechBubbles.config.MsgTime));
			}
		}


		/// <summary>
		/// main
		/// </summary>
		/// <param name="helper"></param>
		public override void Entry(IModHelper helper)
		{
			config = helper.ReadConfig<ModConfig>(); //For ModConifg settings
			TimeEvents.AfterDayStarted += DayStart;
			GameEvents.UpdateTick += this.CheckChat;
		}

		// --Private Methods

		/// <summary>
		/// Runs on each load/new day
		/// TODO: fetch chat addresses once per load, not once per load/day
		/// </summary>
		/// <param name="o"></param>
		/// <param name="e"></param>
		private void DayStart(object o, EventArgs e)
		{
			this.cMsgs = this.Helper.Reflection.GetField<List<ChatMessage>>(Game1.chatBox, "messages").GetValue();
			this.chatCount = this.cMsgs.Count;
			GraphicsEvents.OnPreRenderHudEvent += this.DrawBubble;
		}

		/// <summary>
		/// Per-frame check of cbox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckChat(object sender, EventArgs e)
		{
			if (!StardewModdingAPI.Context.IsWorldReady)
				return;

			if (this.cMsgs.Count != chatCount)
			{
				if (debug)
					Monitor.LogT($"Spawning bubble");
				SpawnBubble();
				this.chatCount = this.cMsgs.Count;
			}
		}

		/// <summary>
		/// Parses through sent messages, adds them to msg dictionary, and sends them to be drawn
		/// </summary>
		private void SpawnBubble()
		{
			String speaker = "";

			//For each new message
			for (int i = 0; i < this.cMsgs.Count - chatCount; i++)
			{
				//Parse the message
				String message = ChatMessage.makeMessagePlaintext(this.cMsgs[this.cMsgs.Count - 1 - i].message);

				//IsJoinMessage
				if (message[0].Equals('>'))
					return;

				//Bug: ^string causes the text to appear below the speech bubble
				/*
				 * Handle bug present in 1.3.10
				 * See https://community.playstarbound.com/threads/stardew-valley-multiplayer-beta-known-issues-fixes.142850/page-206#post-3278207
				 */
				string[] words = message.Split(' ');
				foreach (string word in words)
					if (word.Length > 30)
					{
						this.Monitor.Log($"Word in message too long, ignoring. See: https://community.playstarbound.com/threads/stardew-valley-multiplayer-beta-known-issues-fixes.142850/page-206#post-3278207");
						return;
					}

				if (debug)
				{
					Monitor.LogT($"All sent messages:");
					for (int x = 0; x < this.cMsgs.Count; x++)
						Monitor.LogT($"{ChatMessage.makeMessagePlaintext(this.cMsgs[x].message)}");
					Monitor.LogT($"Trying to parse message, {message}");
				}

				//Parse msg
				speaker = message.Substring(0, message.IndexOf(':'));
				message = message.Substring(message.IndexOf(':') + 1);

				//Find farmer
				foreach (Farmer me in Game1.getAllFarmers())
				{

					if (me.Name.Equals(speaker))
					{
						SpeechBubble talk = new SpeechBubble(DateTime.Now, message);
						List<SpeechBubble> newMsg = new List<SpeechBubble>();
						if (chatDict.ContainsKey(me))
						{
							newMsg = chatDict[me];
							newMsg.Add(talk);
						}
						else
						{
							newMsg.Add(talk);
							chatDict.Add(me, newMsg);
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// Requests a draw for each farmer, for each active message
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void DrawBubble(object sender, EventArgs e)
		{
			foreach (KeyValuePair<Farmer, List<SpeechBubble>> speaker in chatDict)
			{
				int i = speaker.Value.Count;
				List<SpeechBubble> toRemove = new List<SpeechBubble>();

				foreach (SpeechBubble message in speaker.Value)
				{
					if (message.IsExpired())
					{
						toRemove.Add(message);
						continue;
					}
					(speaker.Key).DrawSpeechBubble(message.msg, (--i * config.OldMsgUpShift), message.shake);
				}

				//Remove old messages
				for (int j = 0; j < toRemove.Count; j++)
					speaker.Value.Remove(toRemove[j]);
			}
		}
	}
}
