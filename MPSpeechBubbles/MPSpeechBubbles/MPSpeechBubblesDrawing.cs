using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPSpeechBubbles
{
	static class MPSpeechBubblesDrawing
	{

		/// <summary>
		/// Draws a speech bubble above the given player. Color does not work.
		/// style 0 == shake
		/// style 1 == no shake
		/// </summary>
		public static void DrawSpeechBubble(this Character who, string text, int msgNum, int style)
		{
			//possible bug from changes: -msgNum. use to be (-msgNum * 64), is now -(i * 64)
			Vector2 local = Game1.GlobalToLocal(new Vector2(who.getStandingX(), who.getStandingY() - 192 + (-msgNum) + who.yJumpOffset));

			//Shake
			if (style == 0)
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));

			//Draw bubble
			// 1F = scale, probably?
			SpriteText.drawStringWithScrollCenteredAt(Game1.spriteBatch, text, (int)local.X, (int)local.Y, "", 1F, (int)(Color.Red).PackedValue, 1, (float)(who.getTileY() * 64 / 10000.0 + 1.0 / 1000.0 + who.getTileX() / 10000.0));
		}

	}
}
