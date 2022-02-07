using HarmonyLib;
using StardewValley.Menus;

namespace HDPortraits
{
    [HarmonyPatch(typeof(DialogueBox),"closeDialogue")]
    class DialogueClosePatch
    {
        public static void Postfix()
        {
            PortraitDrawPatch.justOpened.Value = true;
        }
    }
}
