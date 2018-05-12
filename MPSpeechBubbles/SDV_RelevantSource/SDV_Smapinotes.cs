

If you want to keep track of chat messages, you can use SMAPI's reflection helper to access Game1.chatBox.messages. You can then monitor the length every update tick to see if new messages were received and get the new message as a string with ChatMessage.makeMessagePlaintext(<your List<ChatMessage>>[<length of list> - 1].message)(edited)

this.Helper.Reflection.GetField<List<ChatMessage>>(Game1.chatBox, "messages") inside your Mod subclass





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



private int DrawTimer = -1;

	public void AddBubble()
	{
		if (this.DrawTimer < 0)
			GameEvents.OneSecondTick += this.Draw;
		this.DrawTimer = 5 * 60;
	}

	private void Draw(object sender, EventArgs e)
	{
		if (this.DrawTimer-- < 0)
		{
			GameEvents.OneSecondTick -= this.Draw;
			return;
		}

		// draw bubble
	}
	
	
//getting there

//You're welcome to create a delegate wrapper with a built-in timer that unregisters itself if you really want to.
		public delegate void EventDelegate(object o, EventArgs ev);
		public delegate void test();

		//Example
		private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
		{

			if (Context.IsWorldReady) // save is loaded
			{
				if (e.Button == SButton.Enter)
				{
					Debug.WriteLine("");
					GraphicsEvents.OnPreRenderHudEvent += this.RenderTest;

					int time = 5;

					System.EventHandler removeTimer = delegate(object o, EventArgs ev) { };

					System.EventHandler timer = delegate (object o, EventArgs ev)
					{
						if (--time < 1)
						{
							this.Monitor.Log($"Time: {time}.");
							GraphicsEvents.OnPreRenderHudEvent -= this.RenderTest;
							GraphicsEvents.OnPreRenderHudEvent -= removeTimer;
						};
					};

					removeTimer = timer;
					GameEvents.OneSecondTick += timer;

				};
			}

			//need to somehow use: OnPreRenderHudEvent
			//this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.");
		}

		
From Cat:
If you want to keep track of chat messages, you can use SMAPI's reflection helper to access Game1.chatBox.messages. You can then monitor the length every update tick to see if new messages were received and get the new message as a string with ChatMessage.makeMessagePlaintext(<your List<ChatMessage>>[<length of list> - 1].message)
Bouhm - Today at 9:51 PM
The general process for finding such a thing would be to look at the decompiled code, and in this case you want to get stuff from the chat box. 
1. Search (Ctrl+T) for chatbox on dotpeek, then you get the ChatBox class. 
2. Look through the fields for the one you want, in this case private List<ChatMessage> messages = new List<ChatMessage>();. 
3. Put in the type after GetField List<ChatMessage> and then in the params, the object Game1.chatBox and the name of the fields "messages"(edited)
RyuuInu - Today at 9:52 PM
Seems like the swimsuit mod doesn’t work
Pathoschild - Today at 9:52 PM
The reflection API is a way to access fields, properties, and methods that you normally couldn't access. Here's a breakdown of what that line does:
this.Helper.Reflection  // reflection API
   .GetField            // you want to access a field (not a property or method)
   <List<ChatMessage>>  // the field you want to access has this type: List<ChestMessage>
   (Game1.chatBox,      // it's a field on the Game1.chatBox instance
   "messages"           // the name of the field
).GetValue()            // get the value contained by the field


Chatbox is not a menu, it is an IClickableMenu
It is specifically:
public class ChatBox : IClickableMenu

//Try
private void Example(object sender, EventArgs e)
{    
    if (Game1.chatBox == null || !Game1.chatBox.isActive())
        return;
//Place the rest of your code here for your chat box.
}
``
private void Example(object sender, EventArgs e)
{    
    if (Game1.chatBox != null && Game1.chatBox.isActive())
    {
        //Place the rest of your code here for your chat box.
    }
}


Cat:There's Game1.isChatting, which I think is a wrapper around Game1.chatBox.isActive