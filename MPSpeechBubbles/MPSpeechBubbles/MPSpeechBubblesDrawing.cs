using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.Reflection;
using System.Linq;
using System;

namespace MPSpeechBubbles
{
	static class MPSpeechBubblesDrawing
	{

		//To look at: sparkling text

		/// <summary>
		/// Draws a speech bubble above the given player. Color does not work.
		/// style 0 == shake
		/// style 1 == no shake
		/// </summary>
		public static void DrawSpeechBubble(this Character who, MPSpeechBubbles.SpeechBubble msg, int msgNum, float opacity)
		{
			//possible bug from changes: -msgNum. use to be (-msgNum * 64), is now -(i * 64)
			Vector2 local = Game1.GlobalToLocal(new Vector2(who.getStandingX(), who.getStandingY() - 192 + (-msgNum) + who.yJumpOffset));

			//Shake
			if (msg.shake == 0)
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));

			//Draw bubble
			//PackedValue
			// 1F = scale, probably?
			SpriteText.drawStringWithScrollCenteredAt(Game1.spriteBatch, msg.msg, (int)local.X, (int)local.Y, "", opacity, msg.color, 1, (float)(who.getTileY() * 64 / 10000.0 + 1.0 / 1000.0 + who.getTileX() / 10000.0));
		}

		/// <summary>
		/// Selects a color from the given string. For future rewrite of SpriteText.cs
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static Color FindColor(string color)
		{
			switch (color)
			{
				//white red blue green jade yellowgreen pink purple yellow orange brown gray cream salmon peach aqua jungle plum
				//case "brown":
				//	if (LocalizedContentManager.CurrentLanguageLatin)
				//		return Color.White;
				//	return new Color(86, 22, 12);
				case "white":
					return Color.White;
				case "red":
					return Color.Red;
				case "blue":
					return Color.SkyBlue;
				case "green":
					return Color.Green;
				case "jade":
					return Color.MintCream;
				case "yellowgreen":
					return Color.YellowGreen;
				case "pink":
					return Color.Pink;
				case "purple":
					return Color.Purple;
				case "yellow":
					return new Color(60, 60, 60);
				case "orange":
					return Color.Orange;
				case "brown":
					return Color.Brown;
				case "gray":
					return Color.Gray;
				case "cream":
					return new Color(255,253,208);
				case "salmon":
					return Color.Salmon;
				case "peach":
					return Color.PeachPuff;
				case "aqua":
					return Color.Aqua;
				case "jungle":
					return new Color(28, 53, 45);
				case "plum":
					return Color.Plum;
				default:
					return Color.Black;
			}
		}


		
	}

}
