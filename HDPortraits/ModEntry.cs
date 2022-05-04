using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using HDPortraits.Models;

namespace HDPortraits
{
    public class ModEntry : Mod, IAssetLoader
    {
        internal ITranslationHelper i18n => Helper.Translation;
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Harmony harmony;
        internal static string ModID;
        private static IHDPortraitsAPI api = new API();
        private static readonly HashSet<string> failedPaths = new();

        internal static Dictionary<string, MetadataModel> portraitSizes = new();
        internal static Dictionary<string, MetadataModel> backupPortraits = new();

        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Starting up...", LogLevel.Debug);

            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;
            helper.Events.GameLoop.DayStarted += (object sender, DayStartedEventArgs ev) => ReloadData();
            harmony.PatchAll();
        }
        public override object GetApi()
        {
            return api;
        }
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Mods/HDPortraits");
        }
        public T Load<T>(IAssetInfo asset)
        {
            return helper.Content.Load<T>("assets/default.json");
        }
        public static void ReloadData()
        {
            monitor.Log("Reloading portrait data...", LogLevel.Debug);
            failedPaths.Clear();
            backupPortraits = helper.Content.Load<Dictionary<string, MetadataModel>>("Mods/HDPortraits", ContentSource.GameContent);
            foreach ((string id, MetadataModel meta) in backupPortraits)
            {
                meta.defaultPath = "Portraits/" + id;
                meta.Reload();
            }
            portraitSizes.Clear();
        }
        public static bool TryGetMetadata(string name, string suffix, out MetadataModel meta)
        {
            string path = $"{name}_{suffix}";
            if (suffix is not null)
            {
                if (portraitSizes.TryGetValue(path, out meta) && meta is not null)
                    return true; //cached

                if (!failedPaths.Contains(path) && 
                    (Utils.TryLoadAsset("Mods/HDPortraits/" + path, out meta) ||
                    backupPortraits.TryGetValue(path, out meta)) &&
                    meta is not null)
                {
                    meta.defaultPath = "Portraits/" + path;
                    portraitSizes[path] = meta;
                    return true; //suffix
                }
            }

            //no suffix or suffix not found
            if (portraitSizes.TryGetValue(name, out meta) && meta is not null)
                return true; //cached

            if (failedPaths.Contains(path))
            {
                meta = null;
                return false;
            }

            if ((Utils.TryLoadAsset("Mods/HDPortraits/" + name, out meta) || 
                backupPortraits.TryGetValue(name, out meta)) && 
                meta is not null)
            {
                meta.defaultPath = "Portraits/" + name;
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
