using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace HDPortraits
{
    [HarmonyPatch]
    class DialoguePatch
    {
        [HarmonyPatch(typeof(Dialogue), "exitCurrentDialogue")]
        [HarmonyPostfix]
        public static void Cleanup()
        {
            foreach (var item in PortraitDrawPatch.lastLoaded.Value)
                item.Reload();
            PortraitDrawPatch.lastLoaded.Value.Clear();
        }
    }
}
