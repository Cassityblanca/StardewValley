namespace MPSpeechBubbles
{
	class ModConfig
	{
		/// <summary>
		/// How much older messages are shifted up. 
		/// 86: small gap between speech bubbles
		/// DEFAULT: 64
		/// </summary>
		public int OldMessageVerticalShift { get; set; } = 60;

		/// <summary>
		/// Seconds it takes for old messages to disappear. 
		/// DEFAULT: 6
		/// </summary>
		public int OldMessageDisaplyLife { get; set; } = 6;

		/// <summary>
		/// Allows text in speech bubbles to be colored. This may cause some readability issues with the current system.
		/// DEFAULT: true
		/// </summary>
		public bool UseSpeechBubbleColors { get; set; } = true;
		/// <summary>
		/// Opacity of speech bubbles. 0.0 is invisible, 1.0 is solid. Do not go above 1.0! 
		/// DEFAULT: 0.95f
		/// </summary>
		public float BubbleOpacity { get; set; } = 0.95f;

		/// <summary>
		/// Shows chat bubbles in singleplayer?
		/// DEFAUT: true
		/// </summary>
		public bool BubblesInSP { get; set; } = true;
	}
}