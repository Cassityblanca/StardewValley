using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace MPSpeechBubbles
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{
		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
			
			//			Extensions.DrawSpeechBubble(Game1.player, text, ...);
		}



		/*********
        ** Private methods
        *********/
		/// <summary>The method invoked when the player presses a controller, keyboard, or mouse button.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
		{
			//[MPSpeechBubbles] This mod failed in the InputEvents.ButtonPressed event. Technical details:
			//System.InvalidOperationException: Begin must be called successfully before a Draw can be called.
			if (Context.IsWorldReady) // save is loaded
			{
				//need to somehow use: OnPreRenderHudEvent
				GraphicsEvents.OnPreRenderHudEvent += this.RenderTest;
				this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.");
			}
		}

		public void RenderTest(object sender, EventArgs e)
		{
			Game1.player.DrawSpeechBubble("Text", Color.Red);
		}
		
	}
}