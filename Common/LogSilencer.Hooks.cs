using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;


namespace Log_Silencer
{
	public partial class LogSilencer_Core
	{
		private static class Hooks
		{
			public static void Init()
			{
				Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
				Harmony.CreateAndPatchAll(typeof(SilancePatch), GUID);
				Logger.LogDebug("");
			}


			[HarmonyPatch(typeof(ManualLogSource), nameof(ManualLogSource.Log),
				new Type[] { typeof(BepInEx.Logging.LogLevel), typeof(object), }),]
			static class SilancePatch
			{


				static bool Prefix(BepInEx.Logging.LogLevel __0)
				{
					switch(__0)
					{

					case BepInEx.Logging.LogLevel.Debug:
						return !cfg.disableDebugLogs.Value;

					case BepInEx.Logging.LogLevel.Message:
						return !cfg.disableMessageLogs.Value;

					case BepInEx.Logging.LogLevel.Warning:
						return !cfg.disableWarningLogs.Value;

					case BepInEx.Logging.LogLevel.Error:
						return !cfg.disableErrorLogs.Value;

					case BepInEx.Logging.LogLevel.Info:
						return !cfg.disableInfoLogs.Value;
					}
					return true;


				}
			}

		}

	}
}
