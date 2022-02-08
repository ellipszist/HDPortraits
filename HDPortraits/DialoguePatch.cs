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
                item.Animation?.Reset();
            PortraitDrawPatch.lastLoaded.Value.Clear();
        }
    }
}
