using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Text.RegularExpressions;

namespace MPSpeechBubbles
{
	/*
	 * --TODO list--
	 * 
	 * Non-critical{
	 * -Param so it doesn't run in SP
	 * -Rewrite all code to not be terrible
	 * -Or at least remove bad code smells
	 * -Use array instead of list for chatDict, add ModConfig param to set length
	 * -Refine coloring strategy
	 * -Emojis in chat
	 * -Better solution to caret
	 * -Flicker from disappearing old messages (might be fixed via array as above)
	 * -Messages playing above farmers with the same name
	 * 
	 * Planned Features
	 * Additional text-icon emotes
	 *		Usable multiplayer emoticons available in the vanilla chatbox
	 *		A farmer emote-menu
	 *		Some messages play different farmer emotes (ex, "zzz" plays speechbubble your get when you wake up/are up late, or a ":(" makes your farmer pout)
	 *		Optional value to disable chat bubbles in singleplayer
	 */
	public class MPSpeechBubbles : Mod
	{

		bool debug = false;

		static ModConfig config;
		public int chatCount;
		public IList<ChatMessage> cMsgs;
		Dictionary<Farmer, List<SpeechBubble>> chatDict = new Dictionary<Farmer, List<SpeechBubble>>();


		/// <summary>
		/// Data-type for sent messages
		/// </summary>
		public class SpeechBubble
		{
			DateTime born;
			public string msg;
			public int shake;
			public int color;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="dateTime"></param>
			/// <param name="message"></param>
			public SpeechBubble(DateTime dateTime, string message, string inColor = "black")
			{
				this.born = dateTime;
				this.msg = message;
				this.shake = checkIsShake();
				this.color = MPSpeechBubbles.getIndexFromColor(inColor);
			}

			/// <summary>
			/// Determines if the message should use the 'shake' variant.
			/// !! x2 = shake
			/// >40% is CAPS = shake
			/// </summary>
			/// <returns></returns>
			private int checkIsShake()
			{
				if (msg.Count(x => x == '!') > 1)
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
				return ((DateTime.Now).Subtract(born) >= TimeSpan.FromSeconds(MPSpeechBubbles.config.OldMessageDisaplyLife));
			}

			internal void setColor(string inColor)
			{
				this.color = MPSpeechBubbles.getIndexFromColor(inColor);
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

		/// <summary>
		/// Uses SDV's awful color selection process for selecting text colors.
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static int getIndexFromColor(string color)
		{
			//White is ignored for readability
			switch (color)
			{
				case "black":
					return -2;
				case "brown":
					return -1;
				case "blue":
				case "aqua":
					return 1;
				case "red":
				case "pink":
				case "salmon":
				case "peach":
					return 2;
				case "purple":
				case "plum":
					return 3;
				case "orange":
				case "yellow":
					return 5;
				case "yellowgreen":
				case "jungle":
				case "green":
					return 6;
				case "jade":
					return 7;
				case "gray":
				case "cream":
					return 8;
				default:
					return -2;
			}
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
				//Monitor.Log($"Spawning bubble");
				if (!config.BubblesInSP)
				{
					if (Context.IsMultiplayer)
						return;
					else
						SpawnBubble();
				}
				else
					SpawnBubble();
				this.chatCount = this.cMsgs.Count;
			}
		}

		/// <summary>
		/// Parses through sent messages, adds them to msg dictionary, and sends them to be drawn
		/// </summary>
		private void SpawnBubble()
		{

			//For each new message
			for (int i = 0; i < this.cMsgs.Count - chatCount; i++)
			{
				//Parse the message
				String msg = ChatMessage.makeMessagePlaintext(this.cMsgs[this.cMsgs.Count - 1 - i].message);

				//Isn't Player Message
				if (msg[0].Equals('>') || !msg.Contains(":"))
					return;


				//Monitor.Log($"All sent messages:");
				//for (int x = 0; x < this.cMsgs.Count; x++)
				//Monitor.Log($"{ChatMessage.makeMessagePlaintext(this.cMsgs[x].message)}");
				//Monitor.Log($"Trying to parse message, {msg}");

				//Parse msg
				String farmer = msg.Substring(0, msg.IndexOf(':'));
				msg = msg.Substring(msg.IndexOf(':') + 2).TrimEnd(' ');

				//Bug: ^string causes the text to appear below the speech bubble. Temp bruteforce
				msg = msg.Replace('^', 'v');

				//MSG color check debug
				//Monitor.Log($"color: {this.cMsgs[this.cMsgs.Count - 1 - i].color}");


				//Find farmer
				foreach (Farmer me in Game1.getAllFarmers())
				{

					if (me.Name.Equals(farmer))
					{
						SpeechBubble talk = new SpeechBubble(DateTime.Now, msg);
						List<SpeechBubble> newMsg = new List<SpeechBubble>();


						//TODO: rewrite this to not be absolutely terrible
						if (msg.Contains("   ["))
						{
							if (msg.EndsWith("]"))
							{
								if (config.UseSpeechBubbleColors)
									talk.setColor(msg.Substring(msg.IndexOf("   [") + 4).TrimEnd(']'));

								msg = msg.Substring(0, msg.IndexOf("   ["));
								talk.msg = msg;
							}
							else
								Monitor.Log("Message could not be displayed. Message contains a [color] indicator, and is too long. Waiting on Ape for bugfix");
						}


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
			foreach (KeyValuePair<Farmer, List<SpeechBubble>> farmer in chatDict)
			{
				int i = farmer.Value.Count;
				List<SpeechBubble> toRemove = new List<SpeechBubble>();

				foreach (SpeechBubble msg in farmer.Value)
				{
					if (msg.IsExpired())
					{
						toRemove.Add(msg);
						continue;
					}

					(farmer.Key).DrawSpeechBubble(msg, (--i * config.OldMessageVerticalShift), config.BubbleOpacity);
				}

				//Remove old messages
				for (int j = 0; j < toRemove.Count; j++)
					farmer.Value.Remove(toRemove[j]);
			}
		}
	}
}
