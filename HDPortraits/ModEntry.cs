using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;

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
            helper.Events.Player.Warped += PortraitDrawPatch.Warped;
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
            backupPortraits = helper.Content.Load<Dictionary<string, MetadataModel>>("Mods/HDPortraits", ContentSource.GameContent);
            portraitSizes.Clear();
        }
        public static bool TryGetMetadata(string name, string suffix, out MetadataModel meta)
        {
            if (((suffix != null && portraitSizes.TryGetValue($"{name}_{suffix}", out meta)) || 
                portraitSizes.TryGetValue(name, out meta)) && meta != null)
            {
                return true; //cached
            }

            string path = $"{name}_{suffix}";

            if (suffix != null && (Utils.TryLoadAsset("Mods/HDPortraits/" + path, out meta) || backupPortraits.TryGetValue(path, out meta)) && meta != null)
            {
                portraitSizes[path] = meta;
                return true; //suffix
            }

            if ((Utils.TryLoadAsset("Mods/HDPortraits/" + name, out meta) || backupPortraits.TryGetValue(name, out meta)) && meta != null)
            {
                portraitSizes[name] = meta;
                return true; //base
            }

            meta = null;
            return false; //not found
        }
    }
}
