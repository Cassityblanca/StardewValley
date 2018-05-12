using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MPSpeechBubbles
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{
		private int BubbleTime = -1;
		private int bubbleCooldown = 2;
		private string newMessage;
		private List<ChatMessage> messages;

		/*********
        ** Public methods
        *********/

		/**
		 * Entry is required
		**/
		public override void Entry(IModHelper helper)
		{
			InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
			//MenuEvents.MenuClosed += this.PrintMenu;

			//			Extensions.DrawSpeechBubble(Game1.player, text, ...);
		}

		private void PrintMenu(object sender, EventArgsClickableMenuClosed e)
		{
			this.Monitor.Log($"Closed {e}");
		}

		private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
		{

			if (Context.IsWorldReady) // save is loaded
			{
				if (e.Button == SButton.Enter)
				{
					SpawnBubble();
				};
			}

			//need to somehow use: OnPreRenderHudEvent
			//this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.");
		}

		/*
		 * If you want to keep track of chat messages, you can use SMAPI's reflection helper to access Game1.chatBox.messages. 
		 * You can then monitor the length every update tick to see if new messages were received and get the new message as a string with 
		 *		ChatMessage.makeMessagePlaintext(<your List<ChatMessage>>[<length of list> - 1].message)
		 * 
		 */

		public void SpawnBubble()
		{
			if (this.BubbleTime < 0 && !Game1.chatBox.isActive())
			{
				//In the ChatBox class, get a <ListChatMessage> object, with the name messages.
				this.messages = this.Helper.Reflection.GetField<List<ChatMessage>>(Game1.chatBox, "messages").GetValue();


				//So yeah, there's no event. Game1.isChatting tells you if its open, which is a wrapper around Game1.chatBox.isActive() which is a wrapper around Game1.chatBox.chatBox.Selected. 
				//You can simulate your own event by checking that bool every update tick, and then if it changes you know the status changed. (does that make sense?)
				if (this.messages.Count > 0)
				{
					//Parse text
					newMessage = ChatMessage.makeMessagePlaintext(this.messages[this.messages.Count - 1].message);
					newMessage = newMessage.Substring(newMessage.IndexOf(':') + 1);

					//TODO: so /commands don't even get send to the chatbox. Check if there is a new message?
					//Likely a /command, ignore.
					//Answer from Cat: /commands that are invalid are ignored by the game and SMAPI
					if (newMessage[0].CompareTo('/') == 0)
						return;

					//Debug
					this.Monitor.Log($"message: {newMessage}");

					//Draw bubble
					GraphicsEvents.OnPreRenderHudEvent += this.DrawBubble;
				}
				else
					return;

				//Start Timer
				GameEvents.OneSecondTick += this.BubbleTimer;
			}
			this.BubbleTime = bubbleCooldown;
		}

		private void BubbleTimer(object sender, EventArgs e)
		{
			//this.Monitor.Log($"Bubble Time: {this.BubbleTime}.");
			//TODO: Array to avoid massive list?
			//make sure it's this player's message, if not go backwards through the list (but don't go out of bounds!)
			//make sure the chatbox just got closed, presumabily will fix the /command text and other issues
			//constantly build message array?
			//hotkey?
			//multiplayer D:

			if (--this.BubbleTime < 0)
			{
				//Remove Timer
				GameEvents.OneSecondTick -= this.BubbleTimer;
				//Remove Bubble
				GraphicsEvents.OnPreRenderHudEvent -= this.DrawBubble;
			}
		}

		public void DrawBubble(object sender, EventArgs e)
		{
			Game1.player.DrawSpeechBubble(newMessage, Color.Red);
		}
	}
}
