using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MPSpeechBubbles
{

	//TODO: Remove existing bubble
	//TODO: Timer per player?


	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{
		private int chatCount;
		private IList<ChatMessage> chatMessages;
		Dictionary<Farmer, List<SpeechBubble>> chatQueue = new Dictionary<Farmer, List<SpeechBubble>>();

		class SpeechBubble
		{
			DateTime timeBorn;
			public string text;

			public SpeechBubble(DateTime dateTime, string message)
			{
				this.timeBorn = dateTime;
				this.text = message;
			}

			public string getText()
			{
				return this.text;
			}

			public bool IsExpired()
			{
				return ((DateTime.Now).Subtract(timeBorn) >= TimeSpan.FromSeconds(5));
			}
		}

		// --Public Methods
		/**
		 * Entry is required
		**/
		public override void Entry(IModHelper helper)
		{
			//On day start/gameload, set a reference to chatBox
			TimeEvents.AfterDayStarted += delegate (object o, EventArgs e)
			{
				this.chatMessages = this.Helper.Reflection.GetField<List<ChatMessage>>(Game1.chatBox, "messages").GetValue();
				this.chatCount = this.chatMessages.Count;
				GraphicsEvents.OnPreRenderHudEvent += this.DrawBubble;
			};
			GameEvents.UpdateTick += CheckChat;
		}


		// --Private Methods

		private void CheckChat(object sender, EventArgs e)
		{
			if (!Context.IsWorldReady)
				return;

			if (this.chatMessages.Count != chatCount)
			{
				this.Monitor.Log($"Spawning bubble");
				SpawnBubble();
			}
		}

		public void SpawnBubble()
		{
			try
			{
				String speaker = "";

				//For each new message
				for (int i = 0; i < this.chatMessages.Count - chatCount; i++)
				{
					this.Monitor.Log($"Number of new messages: {i}");

					//Parse the message
					String message = ChatMessage.makeMessagePlaintext(this.chatMessages[this.chatMessages.Count - 1 - i].message);

					//IsJoinMessage
					if(message[0].Equals('>'))
						goto Exit;

					//Bug: ^string causes the text to appear below the speech bubble
					/**
					 * Handle bug present in 1.3.10
					 * See https://community.playstarbound.com/threads/stardew-valley-multiplayer-beta-known-issues-fixes.142850/page-206#post-3278207
					 */
					string[] words = message.Split(' ');
					foreach (string word in words)
						if (word.Length > 30)
						{
							this.Monitor.Log($"Word in message too long, ignoring. See: https://community.playstarbound.com/threads/stardew-valley-multiplayer-beta-known-issues-fixes.142850/page-206#post-3278207");
							goto Exit;
						}

					this.Monitor.Log($"All sent messages:");

					//Print contents of msg log
					//				for(int x = 0; x < this.chatMessages.Count; x++)
					//					this.Monitor.Log($"{ChatMessage.makeMessagePlaintext(this.chatMessages[x].message)}");

					this.Monitor.Log($"Trying to parse message, speaker: {message}");
					speaker = message.Substring(0, message.IndexOf(':'));
					this.Monitor.Log($"Trying to parse message, message: {message}");
					message = message.Substring(message.IndexOf(':') + 1);

					//Find farmer
					foreach (Farmer me in Game1.getAllFarmers())
					{

						if (me.Name.Equals(speaker))
						{
							SpeechBubble talk = new SpeechBubble(DateTime.Now, message);
							List<SpeechBubble> newMsg = new List<SpeechBubble>();
							if (chatQueue.ContainsKey(me))
							{
								newMsg = chatQueue[me];
								newMsg.Add(talk);
							}
							else
							{
								newMsg.Add(talk);
								chatQueue.Add(me, newMsg);
							}

							break;
						}
					}
				}
				Exit:
					this.chatCount = this.chatMessages.Count;
			}
			catch (Exception e)
			{
				this.Monitor.Log($"Oopsie woops, uwuw we made a fuky wuky! The code ape at our barn is working vEwY HaWd to fix this! Pwease gib him dis ewwah messeg (and a bonana):  {e}");
				this.chatCount = this.chatMessages.Count;
			}
		}

		//you can just increase the y component of the vector to move the speech bubble up
		//Remember each tile is scaled up by 4? So 192/64 = 3, which is approximately a tile above the player's head

		public void DrawBubble(object sender, EventArgs e)
		{
			foreach (KeyValuePair<Farmer, List<SpeechBubble>> speaker in chatQueue)
			{
				int i = 0;
				List<SpeechBubble> toRemove = new List<SpeechBubble>();
				foreach (SpeechBubble message in speaker.Value)
				{
					if (message.IsExpired())
					{
						toRemove.Add(message);
						continue;
					}
					(speaker.Key).DrawSpeechBubble(message.text, ++i, 1);
				}

				//Remove old messages
				for (int j = 0; j < toRemove.Count; j++)
					speaker.Value.Remove(toRemove[j]);
			}
		}
	}
}