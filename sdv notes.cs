npc.doEmote(emoteID)

 Yep, you can use "Action": "EditData" to add/edit dialogue entries in a file.
 
 places to look: 
 Game1.client, Game1.server, Game1.multiplayer
 
 
 you'd want to get the latest message from Game1.chatBox.messages
 
 protected string textAboveHead;
 
 
 public void showTextAboveHead(string Text, int spriteTextColor = -1, int style = 2, int duration = 3000, int preTimer = 0)
 
 "Farmer doesn't extend NPC, so you'll have to manually render it."
	just copy the code from here: 
	// NPC.cs
	public override void drawAboveAlwaysFrontLayer(SpriteBatch b) {...}
	
	
	
    public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
    {
      if (this.textAboveHeadTimer <= 0 || this.textAboveHead == null)
        return;
      Vector2 local = Game1.GlobalToLocal(new Vector2((float) this.getStandingX(), (float) (this.getStandingY() - Game1.tileSize * 3 + this.yJumpOffset)));
      if (this.textAboveHeadStyle == 0)
        local += new Vector2((float) Game1.random.Next(-1, 2), (float) Game1.random.Next(-1, 2));
      SpriteText.drawStringWithScrollCenteredAt(b, this.textAboveHead, (int) local.X, (int) local.Y, "", this.textAboveHeadAlpha, this.textAboveHeadColor, 1, (float) ((double) (this.getTileY() * Game1.tileSize) / 10000.0 + 1.0 / 1000.0 + (double) this.getTileX() / 10000.0), false);
    }
	
	
//What CatPers used:
int style = 1;
Vector2 local = Game1.GlobalToLocal(new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY() - 192 + Game1.player.yJumpOffset));

if (style == 0)
    local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));

SpriteText.drawStringWithScrollCenteredAt(Game1.spriteBatch, "Your mother is a hamster,\nand your father smelt of elderberries!", (int) local.X, (int) local.Y, "", 1F, (int) Color.Black.PackedValue, 1, (float) (Game1.player.getTileY() * 64 / 10000.0 + 1.0 / 1000.0 + (double) Game1.player.getTileX() / 10000.0), false);





//GraphicsEvents.OnPostRenderHudEvent += this.PostRenderHud;
mine's in PostRenderHud, but i'd put it somewhere else
GraphicsEvent.OnPostRenderHudEvent is a static event
though i'd put it somewhere else
probably OnPreRenderHudEvent

See:
https://stardewvalleywiki.com/Modding:SMAPI_APIs#Graphics_events
OnPreRenderHudEvent



here's what i have @Kal: 
public static void DrawSpeechBubble(this Character who, string text, Color color, int style = 1) {
	//this character who == an extension method?
    Vector2 local = Game1.GlobalToLocal(new Vector2(who.getStandingX(), who.getStandingY() - 192 + who.yJumpOffset));
	
    if (style == 0)
		//Makes it shake if style = 0
        local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
	
	//Draws the bubble
    SpriteText.drawStringWithScrollCenteredAt(Game1.spriteBatch, text, (int) local.X, (int) local.Y, "", 1F, (int) color.PackedValue, 1, (float) (who.getTileY() * 64 / 10000.0 + 1.0 / 1000.0 + who.getTileX() / 10000.0));
}
//in relation to 'this character who':
you can also do 
Extensions.DrawSpeechBubble(Game1.player, text, ...);

Game1.player.DrawSpeechBubble("Some text", Color.Black);











[MPSpeechBubbles] This mod failed in the InputEvents.ButtonPressed event. Technical details:

System.InvalidOperationException: Begin must be called successfully before a Draw can be called.

   at Microsoft.Xna.Framework.Graphics.SpriteBatch.InternalDraw(Texture2D texture, Vector4& destination, Boolean scaleDestination, Nullable`1& sourceRectangle, Color color, Single rotation, Vector2& origin, SpriteEffects effects, Single depth)
   at Microsoft.Xna.Framework.Graphics.SpriteBatch.Draw(Texture2D texture, Vector2 position, Nullable`1 sourceRectangle, Color color, Single rotation, Vector2 origin, Single scale, SpriteEffects effects, Single layerDepth)
   at StardewValley.BellsAndWhistles.SpriteText.drawString(SpriteBatch b, String s, Int32 x, Int32 y, Int32 characterPosition, Int32 width, Int32 height, Single alpha, Single layerDepth, Boolean junimoText, Int32 drawBGScroll, String placeHolderScrollWidthText, Int32 color) in C:\Users\gitlab-runner\gitlab-runner\builds\5c0f9387\0\chucklefish\stardewvalley\Farmer\Farmer\BellsAndWhistles\SpriteText.cs:line 431
   at StardewValley.BellsAndWhistles.SpriteText.drawStringWithScrollCenteredAt(SpriteBatch b, String s, Int32 x, Int32 y, String placeHolderWidthText, Single alpha, Int32 color, Int32 scrollType, Single layerDepth, Boolean junimoText) in C:\Users\gitlab-runner\gitlab-runner\builds\5c0f9387\0\chucklefish\stardewvalley\Farmer\Farmer\BellsAndWhistles\SpriteText.cs:line 345
   at MPSpeechBubbles.Test.DrawSpeechBubble(Character who, String text, Color color, Int32 style) in C:\Users\Taylor Cassity\source\repos\MPSpeechBubbles\MPSpeechBubbles\Test.cs:line 26
   at MPSpeechBubbles.ModEntry.InputEvents_ButtonPressed(Object sender, EventArgsInput e) in C:\Users\Taylor Cassity\source\repos\MPSpeechBubbles\MPSpeechBubbles\ModEntry.cs:line 38
   at StardewModdingAPI.Framework.Events.ManagedEvent`1.Raise(TEventArgs args) in C:\source\_Stardew\SMAPI\src\SMAPI\Framework\Events\ManagedEvent.cs:line 54





