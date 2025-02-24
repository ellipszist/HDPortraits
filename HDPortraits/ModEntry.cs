﻿using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using HDPortraits.Models;
using System.Runtime.CompilerServices;
using AeroCore.Utils;
using StardewValley;

namespace HDPortraits
{
	public class ModEntry : Mod
	{
		internal ITranslationHelper i18n => Helper.Translation;
		internal static IMonitor monitor;
		internal static IModHelper helper;
		internal static Harmony harmony;
		internal static string ModID;
		private static readonly IHDPortraitsAPI api = new API();
		private static readonly HashSet<string> failedPaths = new();
		private static Dictionary<string, string> festivals;
		internal static AeroCore.API.API aeroAPI;

		internal static Dictionary<string, MetadataModel> portraitSizes = new();
		internal static Dictionary<string, MetadataModel> backupPortraits = new();

		public override void Entry(IModHelper helper)
		{
			Monitor.Log("Starting up...", LogLevel.Debug);

			monitor = Monitor;
			ModEntry.helper = Helper;
			harmony = new(ModManifest.UniqueID);
			ModID = ModManifest.UniqueID;
			festivals = helper.ModContent.Load<Dictionary<string, string>>("assets/festivals.json");
			helper.Events.Content.AssetRequested += LoadDefaultAsset;
			helper.Events.Content.AssetsInvalidated += TryReloadAsset;
			helper.Events.GameLoop.GameLaunched += GameLaunched;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void GameLaunched(object _, GameLaunchedEventArgs ev)
		{
			aeroAPI = AeroCore.ModEntry.GetStaticApi();
			harmony.PatchAll();
			aeroAPI.InitAll();
			ReloadBaseData();
		}
		public override object GetApi() => api;
		private void LoadDefaultAsset(object _, AssetRequestedEventArgs ev)
		{
			if (ev.Name.IsEquivalentTo("Mods/HDPortraits"))
				ev.LoadFromModFile<Dictionary<string, MetadataModel>>("assets/default.json", AssetLoadPriority.Low);
		}
		private void TryReloadAsset(object _, AssetsInvalidatedEventArgs ev)
		{
			foreach(var name in ev.Names)
				if (name.IsEquivalentTo("Mods/HDPortraits"))
					ReloadBaseData();

				else if(name.IsDirectlyUnderPath("Mods/HDPortraits"))
					ReloadItem(name);
		}
		private static void ReloadItem(IAssetName name)
		{
			monitor.Log($"Reloading portrait metadata for '{name}'...", LogLevel.Debug);
			var localPath = name.WithoutPath("Mods/HDPortraits");
			failedPaths.Remove(localPath);
			portraitSizes.Remove(localPath);
		}
		private static void ReloadBaseData()
		{
			monitor.Log("Reloading portrait data...", LogLevel.Debug);
			backupPortraits = helper.GameContent.Load<Dictionary<string, MetadataModel>>("Mods/HDPortraits");
			foreach ((string id, MetadataModel meta) in backupPortraits)
			{
				meta.originalPath = "Portraits/" + id;
				failedPaths.Remove(id);
			}
		}
		internal static string GetFestivalContext()
		{
			if (Game1.weatherIcon != Game1.weather_festival || Game1.whereIsTodaysFest != Game1.currentLocation.Name)
				return null;

			var key = Game1.currentSeason + Game1.dayOfMonth.ToString();
			if (!festivals.TryGetValue(key, out var context))
			{
				var data = helper.GameContent.Load<Dictionary<string, string>>("Data/Festivals/" + key);
				if (!data.TryGetValue("id", out context))
					return null;
			}
			return context;
		}
		public static bool TryGetMetadata(string name, string suffix, out MetadataModel meta, bool forceSuffix = false)
		{
			var was_null_suffix = suffix is null;
			suffix ??= GetFestivalContext();
			string path = $"{name}_{suffix}";
			if (suffix is not null)
			{
				if (portraitSizes.TryGetValue(path, out meta) && meta is not null)
					return true; //cached

				if (!failedPaths.Contains(path) && 
					(Misc.TryLoadAsset(monitor, helper, "Mods/HDPortraits/" + path, out meta, LogLevel.Trace) ||
					backupPortraits.TryGetValue(path, out meta)) &&
					meta is not null)
				{
					meta.originalPath = "Portraits/" + path;
					portraitSizes[path] = meta;
					return true; //suffix
				}
				if (forceSuffix && !was_null_suffix)
					return false;
			}

			//no suffix or suffix not found
			if (portraitSizes.TryGetValue(name, out meta) && meta is not null)
				return true; //cached

			if (failedPaths.Contains(path))
			{
				meta = null;
				return false;
			}

			if ((Misc.TryLoadAsset(monitor, helper, "Mods/HDPortraits/" + name, out meta, LogLevel.Trace) || 
				backupPortraits.TryGetValue(name, out meta)) && 
				meta is not null)
			{
				meta.originalPath = "Portraits/" + name;
				portraitSizes[name] = meta;
				return true; //base
			}

			monitor.Log($"No Data for {path}");

			failedPaths.Add(path);
			meta = null;
			return false; //not found
		}
	}
}
