using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPSpeechBubbles
{
	public static class Kal_Extensions
	{
		public static bool debug => true;

		//LogLevel level = LogLevel.Debug; optional param. Defaults to LogLevel.Trace
		public static void LogT(this IMonitor monitor, string message, LogLevel level = LogLevel.Trace)
		{
			if (debug)
				monitor.Log(message, level);
		}
	}
}
