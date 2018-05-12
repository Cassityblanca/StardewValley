using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPSpeechBubbles
{
	public static class Test
	{

		public static void DrawSpeechBubble(this Character who, string text, Color color, int style = 1)
		{
			//this character who == an extension method?
			Vector2 local = Game1.GlobalToLocal(new Vector2(who.getStandingX(), who.getStandingY() - 192 + who.yJumpOffset));

			if (style == 0)
				//Makes it shake if style = 0
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));

			//Draws the bubble
			SpriteText.drawStringWithScrollCenteredAt(Game1.spriteBatch, text, (int)local.X, (int)local.Y, "", 1F, (int)color.PackedValue, 1, (float)(who.getTileY() * 64 / 10000.0 + 1.0 / 1000.0 + who.getTileX() / 10000.0));
		}
	}
}
