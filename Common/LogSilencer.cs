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
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Log_Silencer
{
	// Specify this as a plugin that gets loaded by BepInEx
	[BepInPlugin(GUID, ModName, Version)]
	// Tell BepInEx that we need KKAPI to run, and that we need the latest version of it.
	// Check documentation of KoikatuAPI.VersionConst for more info.
	[BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
	public partial class LogSilencer_Core : BaseUnityPlugin
	{
		public const string ModName = "Log Silencer";
		public const string GUID = "prolo.logsilencer";//never change this
		public const string Version = "1.0.0";

		internal static LogSilencer_Core Instance;
		internal static new ManualLogSource Logger;


		public static LogSilencerConfig cfg;
		public static Dictionary<string, LogSilencerConfig> modCfgs = new Dictionary<string, LogSilencerConfig>();
		public class LogSilencerConfig
		{
			public ConfigEntry<bool> enable;

			//public ConfigEntry<string> btnSaveToMod;
			public ConfigEntry<bool> disableDebugLogs;
			public ConfigEntry<bool> disableWarningLogs;
			public ConfigEntry<bool> disableInfoLogs;
			public ConfigEntry<bool> disableMessageLogs;
			public ConfigEntry<bool> disableErrorLogs;

			public ConfigEntry<string> btnSaveToMod;
			public ConfigEntry<string> btnResetAllMods;

			public ConfigEntry<bool> resetOnLaunch;//not implemented
			public ConfigEntry<bool> showModInAdvanced;
			public ConfigEntry<bool> disableUnityDebugLogs;//not implemented
		}

		static int selectedMod = 0;
		static bool selectingMod = false;
		static Vector2 scrollview = Vector2.zero;
		static void BtnSaveDefaultToMod(ConfigEntryBase entry)
		{
			var modList = modCfgs?.Keys?.ToArray() ?? new string[] { };
			//	modList.Remove(ModName.Trim());
			Array.Sort(modList, (a, b) => a.CompareTo(b));


			try
			{
				GUILayout.BeginVertical();
				var onPress = GUILayout.Button(new GUIContent { text = entry.Definition.Key, tooltip = entry.Description.Description }, GUILayout.ExpandWidth(true));
				GUILayout.Space(3);

				bool btn;
				int maxWidth = 300;
				if(modList.Length > 0)
					if((btn = GUILayout.Button(new GUIContent { text = $"selected mod: {modList[selectedMod]}" },
						GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(maxWidth))) || selectingMod)
					{
						selectingMod = !(btn && selectingMod);//if dropdown btn was pressed

						scrollview = GUILayout.BeginScrollView(scrollview, false, false,
							GUILayout.ExpandWidth(true),
							GUILayout.ExpandHeight(true), GUILayout.Height(200), GUILayout.MaxWidth(maxWidth));


						var select = GUILayout.SelectionGrid(selectedMod, modList, 1, GUILayout.ExpandWidth(true));
						if(select != selectedMod) selectingMod = false;
						selectedMod = select;

						GUILayout.EndScrollView();
					}

				GUILayout.Space(5);
				GUILayout.EndVertical();

				if(onPress)
				{
					//Logger.Log(BepInEx.Logging.LogLevel.Message, $"Set values for: {modList[selectedMod]}");

					var cfg1 = modCfgs[modList[selectedMod]];

					cfg1.enable.Value = cfg.enable.Value;
					cfg1.disableDebugLogs.Value = cfg.disableDebugLogs.Value;
					cfg1.disableWarningLogs.Value = cfg.disableWarningLogs.Value;
					cfg1.disableInfoLogs.Value = cfg.disableInfoLogs.Value;
					cfg1.disableMessageLogs.Value = cfg.disableMessageLogs.Value;
					cfg1.disableErrorLogs.Value = cfg.disableErrorLogs.Value;
				}
			}
			catch { }
		}

		static void BtnResetAllMods(ConfigEntryBase entry)
		{

			GUILayout.BeginVertical();
			if(GUILayout.Button(new GUIContent
			{
				text = entry.Definition.Key,
				tooltip = entry.Description.Description
			}, GUILayout.ExpandWidth(true)))
				foreach(var key in modCfgs?.Keys)
				{
					//Logger.Log(BepInEx.Logging.LogLevel.Message, $"Reset default values for: {key}");

					var cfg1 = modCfgs[key.Trim()];
					cfg1.enable.ConfigDefaulter();
					cfg1.disableDebugLogs.ConfigDefaulter();
					cfg1.disableWarningLogs.ConfigDefaulter();
					cfg1.disableInfoLogs.ConfigDefaulter();
					cfg1.disableMessageLogs.ConfigDefaulter();
					cfg1.disableErrorLogs.ConfigDefaulter();
				}
			GUILayout.EndVertical();
		}


		void Awake()
		{
			Instance = this;
			Logger = base.Logger;
			Config.Clear();


			int index = 0;
			Config.SaveOnConfigSet = false;

			cfg = new LogSilencerConfig
			{

				enable = Config.Bind("_Default Settings_", "Enable", true, new ConfigDescription("Enables the mod", null, new ConfigurationManagerAttributes { Order = --index })),
				disableDebugLogs = Config.Bind("_Default Settings_", "Disable Debug Logs", true, new ConfigDescription("Disables debug logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index })),
				disableWarningLogs = Config.Bind("_Default Settings_", "Disable Warning Logs", true, new ConfigDescription("Disables warning logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index })),
				disableInfoLogs = Config.Bind("_Default Settings_", "Disable Info Logs", false, new ConfigDescription("Disables info logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index })),
				disableMessageLogs = Config.Bind("_Default Settings_", "Disable Message Logs", false, new ConfigDescription("Disables message logs from popping up on screen/log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index })),
				disableErrorLogs = Config.Bind("_Default Settings_", "Disable Error Logs", false, new ConfigDescription("Disables error logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index })),

				btnSaveToMod = Config.Bind("_Per Mod Settings_", "Save Default To Mod", "", new ConfigDescription("Save the currently set default to a specific Mod", null, new ConfigurationManagerAttributes { Order = --index, CustomDrawer = BtnSaveDefaultToMod, HideDefaultButton = true })),
				btnResetAllMods = Config.Bind("_Per Mod Settings_", "Clear All Mod Configs", "", new ConfigDescription("Clear all set Mod configs", null, new ConfigurationManagerAttributes { Order = --index, CustomDrawer = BtnResetAllMods, HideDefaultButton = true })),
				showModInAdvanced = Config.Bind("_Per Mod Settings_", "Show Per Mod Configs", false, new ConfigDescription("Show the entire list of config settings with advanced settings (MUST CLOSE AND OPEN \"PLUGIN SETTINGS\" FOR IT TO TAKE AFFECT)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true, })),

				resetOnLaunch = Config.Bind("_Default Settings_", "Reset Defaults On Launch", true, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = false, })),
				disableUnityDebugLogs = Config.Bind("_Default Settings_", "Disable Unity Debug Logs", true, new ConfigDescription("Disables Unity's Debug.Log() logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = false })),
			};

			void resetBrowsable()
			{
				foreach(var key in modCfgs?.Keys)
				{
					try
					{
						var cfg1 = modCfgs[key.Trim()];

						(cfg1.enable.Description.Tags[0] as ConfigurationManagerAttributes).Browsable = cfg.showModInAdvanced.Value;
						(cfg1.disableDebugLogs.Description.Tags[0] as ConfigurationManagerAttributes).Browsable = cfg.showModInAdvanced.Value;
						(cfg1.disableErrorLogs.Description.Tags[0] as ConfigurationManagerAttributes).Browsable = cfg.showModInAdvanced.Value;
						(cfg1.disableInfoLogs.Description.Tags[0] as ConfigurationManagerAttributes).Browsable = cfg.showModInAdvanced.Value;
						(cfg1.disableMessageLogs.Description.Tags[0] as ConfigurationManagerAttributes).Browsable = cfg.showModInAdvanced.Value;
						(cfg1.disableWarningLogs.Description.Tags[0] as ConfigurationManagerAttributes).Browsable = cfg.showModInAdvanced.Value;
					}
					catch(Exception e) { Logger.LogError(e); }
				}
			};

			cfg.showModInAdvanced.SettingChanged += (s, o) => resetBrowsable();


			IEnumerator TheMeat(string modname)
			{
				yield return null;
				var plugins = FindObjectsOfType<BaseUnityPlugin>();
				modCfgs.Clear();

				foreach(var plug in plugins)
				{
					string name = plug.Info.Metadata.Name.Trim();
					if(name == Logger.SourceName.Trim()) continue;

					modCfgs[name] = new LogSilencerConfig()
					{
						enable = Config.Bind($"Mod: {name}", $"Enable", false, new ConfigDescription("Enables the mod", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
						disableDebugLogs = Config.Bind($"Mod: {name}", $"Disable Debug Logs", false, new ConfigDescription("Disables debug logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
						disableWarningLogs = Config.Bind($"Mod: {name}", $"Disable Warning Logs", false, new ConfigDescription("Disables warning logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
						disableInfoLogs = Config.Bind($"Mod: {name}", $"Disable Info Logs", false, new ConfigDescription("Disables info logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
						disableMessageLogs = Config.Bind($"Mod: {name}", $"Disable Message Logs", false, new ConfigDescription("Disables message logs from popping up on screen/log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
						disableErrorLogs = Config.Bind($"Mod: {name}", $"Disable Error Logs", false, new ConfigDescription("Disables error logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
					};
				}

				string name2 = ModName.Trim();
				modCfgs[name2] = new LogSilencerConfig()//the name must stay like this
				{
					enable = Config.Bind($"Mod: ({name2})", $"Enable", true, new ConfigDescription("Enables the mod", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
					disableDebugLogs = Config.Bind($"Mod: ({name2})", $"Disable Debug Logs", false, new ConfigDescription("Disables debug logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
					disableWarningLogs = Config.Bind($"Mod: ({name2})", $"Disable Warning Logs", false, new ConfigDescription("Disables warning logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
					disableInfoLogs = Config.Bind($"Mod: ({name2})", $"Disable Info Logs", false, new ConfigDescription("Disables info logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
					disableMessageLogs = Config.Bind($"Mod: ({name2})", $"Disable Message Logs", false, new ConfigDescription("Disables message logs from popping up on screen/log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
					disableErrorLogs = Config.Bind($"Mod: ({name2})", $"Disable Error Logs", false, new ConfigDescription("Disables error logs from being written to the log file (enabling this can help improve performance)", null, new ConfigurationManagerAttributes { Order = --index, IsAdvanced = true, Browsable = true })),
				};

				resetBrowsable();
				Config.Save();
				Config.SaveOnConfigSet = true;

				Hooks.Init();
			}

			StartCoroutine(TheMeat(ModName));

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
			return val;
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
		/// Defaults the ConfigEntry to the default value set.
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

		public static T[] Remove<T>(this T[] array, T val) => array.RemoveIndex(Array.FindIndex(array, (a) => a.Equals(val)));
		public static T[] RemoveIndex<T>(this T[] array, int index)
		{
			if(index < 0) return array;
			int length;
			Array.Copy(array, index + 1, array, index, length = array.Length - (index + 1));
			Array.Resize(ref array, array.Length - length);

			return array;
		}

	}

}
