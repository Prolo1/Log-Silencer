using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace Log_Silencer
{
	public partial class LogSilencer_Core
	{
		private static class Hooks
		{
			static void Init()
			{
				Harmony.CreateAndPatchAll(typeof(Hooks), GUID);

			}
		}

	}
}
