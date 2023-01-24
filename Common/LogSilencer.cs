using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Utilities;
using KKAPI.Maker.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine;


namespace Log_Silencer
{
	public partial class LogSilencer_Core : BaseUnityPlugin
	{
		public const string ModName = "Log Silencer";
		public const string GUID = "prolo.logsilencer";//never change this
		public const string Version = "1.0.0";

		internal static LogSilencer_Core Instance;
		internal static new ManualLogSource Logger;


		public static LogSilencerConfig cfg;
		public struct LogSilencerConfig
		{
			public ConfigEntry<bool> disableDebugLogs;
			public ConfigEntry<bool> disableWarningLogs;
			public ConfigEntry<bool> disableInfoLogs;
			public ConfigEntry<bool> disableMessageLogs;
			public ConfigEntry<bool> disableErrorLogs;
			public ConfigEntry<bool> resetOnLaunch;
		}

		void Awake()
		{
			Instance = this;
			Logger = base.Logger;


			int index = 0;
			cfg = new LogSilencerConfig
			{
				disableDebugLogs = Config.Bind("_Main_", "Disable Debug Logs", true, new ConfigDescription("Disables debug logs from being written to the log file",null, new ConfigurationManagerAttributes { Order = --index })),
				disableWarningLogs = Config.Bind("_Main_", "Disable Warning Logs", true, new ConfigDescription("Disables warning logs from being written to the log file",null, new ConfigurationManagerAttributes { Order = --index })),
				disableInfoLogs = Config.Bind("_Main_", "Disable info Logs", false, new ConfigDescription("Disables info logs from being written to the log file",null, new ConfigurationManagerAttributes { Order = --index })),
				disableMessageLogs = Config.Bind("_Main_", "Disable Message Logs", false, new ConfigDescription("Disables message logs from being written to the log file",null, new ConfigurationManagerAttributes { Order = --index })),
				disableErrorLogs = Config.Bind("_Main_", "Disable Error Logs", false, new ConfigDescription("Disables error logs from being written to the log file",null, new ConfigurationManagerAttributes {Order=--index })),
				resetOnLaunch = Config.Bind("_Main_", "Reset On Launch", true, new ConfigDescription("",null, new ConfigurationManagerAttributes { IsAdvanced = true,Browsable=false })),
			};

			Hooks.Init();

		}
	}

	internal static class LogUtil
	{
		/// <summary>
		/// Adds a value to the end of a list and returns it
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static T AddNReturn<T>(this ICollection<T> list, T val)
		{
			list.Add(val);
			return list.Last();
		}

		/// <summary>
		/// makes sure a path fallows the format "this/is/a/path" and not "this//is\\a/path" or similar
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public static string MakeDirPath(string dir)
		{

			dir = (dir ?? "").Trim().Replace('\\', '/').Replace("//", "/");

			if((dir.LastIndexOf('.') < dir.LastIndexOf('/'))
				&& dir.Last() != '/')
				dir += '/';

			return dir;
		}

		/// <summary>
		/// Returns a list of the regestered handeler specified. returns empty list otherwise 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetFuncCtrlOfType<T>()
		{
			foreach(var hnd in CharacterApi.RegisteredHandlers)
				if(hnd.ControllerType == typeof(T))
					return hnd.Instances.Cast<T>();

			return new T[] { };
		}

		/// <summary>
		/// Defaults the ConfigEntry on game launch
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		public static ConfigEntry<T> ConfigDefaulter<T>(this ConfigEntry<T> v1, T v2)
		{

			if(v1 == null || !LogSilencer_Core.cfg.resetOnLaunch.Value) return v1;

			v1.Value = v2;
			v1.SettingChanged += (m, n) => { if(v2 != null) v2 = v1.Value; };
			return v1;
		}

		/// <summary>
		/// Defaults the ConfigEntry on game launch
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		public static ConfigEntry<T> ConfigDefaulter<T>(this ConfigEntry<T> v1) =>
			v1.ConfigDefaulter((T)v1.DefaultValue);


	
		/// <summary>
		/// Crates Image Texture based on path
		/// </summary>
		/// <param name="path">directory path to image (i.e. C:/path/to/image.png)</param>
		/// <returns>An Texture2D created from path if passed, else a black texture</returns>
		public static Texture2D CreateTexture(this string path) =>
			File.Exists(path) ?
			File.ReadAllBytes(path)?
			.LoadTexture(TextureFormat.RGBA32) ??
			Texture2D.blackTexture : Texture2D.blackTexture;

		public static BaseGuiEntry OnGUIExists(this BaseGuiEntry gui, UnityAction<BaseGuiEntry> act)
		{
			IEnumerator func(BaseGuiEntry gui1, UnityAction<BaseGuiEntry> act1)
			{
				yield return new WaitUntil(() => gui1.Exists);//the thing neeeds to exist first
				act1(gui);
			}
			LogSilencer_Core.Instance.StartCoroutine(func(gui, act));

			return gui;
		}

	}

}
