using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UniRx.Triggers;
using ADV.Commands.Base;

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


				static bool Prefix(ref BepInEx.Logging.LogLevel __0, ref object __1, ManualLogSource __instance)
				{
					if(!cfg.enable.Value) return true;
					var name = __instance.SourceName.Trim();
					modCfgs.TryGetValue(name, out var val);
					bool useDefault = !(val?.enable?.Value ?? false);



					//if a log level is set
					bool active = __0 != BepInEx.Logging.LogLevel.None;

					if(useDefault ? cfg.disableDebugLogs.Value :
						val.disableDebugLogs.Value)
						__0 &= ~BepInEx.Logging.LogLevel.Debug;

					if(useDefault ? cfg.disableWarningLogs.Value :
						val.disableWarningLogs.Value)
						__0 &= ~BepInEx.Logging.LogLevel.Warning;

					if(useDefault ? cfg.disableInfoLogs.Value :
						val.disableInfoLogs.Value)
						__0 &= ~BepInEx.Logging.LogLevel.Info;

					if(useDefault ? cfg.disableMessageLogs.Value :
						val.disableMessageLogs.Value)
						__0 &= ~BepInEx.Logging.LogLevel.Message;

					if(useDefault ? cfg.disableErrorLogs.Value :
						val.disableErrorLogs.Value)
						__0 &= ~BepInEx.Logging.LogLevel.Error;

					if(active && __0 != 0) return true;

					if(!active && name == ModName.Trim()) return true;//only this mod can print null logs

					return false;
				}
			}


		}

	}
}
