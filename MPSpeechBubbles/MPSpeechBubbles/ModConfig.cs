namespace MPSpeechBubbles
{
	class ModConfig
	{
		/// <summary>
		/// How much older messages are shifted up. DEFAULT: 64
		/// 86: small gap between speech bubbles
		/// </summary>
		public int OldMsgUpShift { get; set; } = 60;

		/// <summary>
		/// Seconds it takes for old messages to disappear. DEFAULT: 6
		/// </summary>
		public int MsgTime { get; set; } = 6;
	}
}