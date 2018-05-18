v1.0.0
-Initial Release

v1.0.1
-Fixed UpdateKey
-Debug logging no longer enabled by default (woops!)
-Removed extra spaces before/after messages
-Compatabile with '/color ' commands - no longer displays ex. '[red]' in speechbubble
-Replaces carets with v, until better solution is found
-Beginning of SpriteText.cs rewrite, for improved control in the future
-Added additional optional parameters in ModConfig.cs
	(bool) UseColors - use new Colored Text feature. Some colors may be difficult to read
		Default is true, but may be false in future releases. Please give feedback!
	(float) MsgOpacity - opacity of speech bubbles
	(bool) toggleSP - toggle speech bubbles in singleplayer