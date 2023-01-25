using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

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
			//	Harmony.CreateAndPatchAll(typeof(Silance2Patch), GUID);
			}


			[HarmonyPatch(typeof(ManualLogSource), nameof(ManualLogSource.Log),
				new Type[] { typeof(BepInEx.Logging.LogLevel), typeof(object), }),]
			static class SilancePatch
			{


				static bool Prefix(BepInEx.Logging.LogLevel __0)
				{

					if((__0 & BepInEx.Logging.LogLevel.Debug) > 0)
						if(cfg.disableDebugLogs.Value)
							return !cfg.disableDebugLogs.Value;
					
					if((__0 & BepInEx.Logging.LogLevel.Warning) > 0)
						if(cfg.disableWarningLogs.Value)
							return !cfg.disableWarningLogs.Value;
					
					if((__0 & BepInEx.Logging.LogLevel.Info) > 0)
						if(cfg.disableInfoLogs.Value)
							return !cfg.disableInfoLogs.Value;
					
					if((__0 & BepInEx.Logging.LogLevel.Message) > 0)
						if(cfg.disableMessageLogs.Value)
							return !cfg.disableMessageLogs.Value;
				
					if((__0 & BepInEx.Logging.LogLevel.Error) > 0)
						if(cfg.disableErrorLogs.Value)
							return !cfg.disableErrorLogs.Value;

					return true;
				}
			}


		}

	}
}
