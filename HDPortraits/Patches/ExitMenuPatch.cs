using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPortraits.Patches
{
    [HarmonyPatch]
    internal class ExitMenuPatch
    {
        [HarmonyPatch(typeof(Game1), "exitActiveMenu")]
        [HarmonyPrefix]
        internal static void PrefixExitMenu()
        {
            if (Game1.activeClickableMenu is DialogueBox)
                DialoguePatch.Finish();
        }

        [HarmonyPatch(typeof(Event), "exitEvent")]
        [HarmonyPostfix]
        internal static void PostfixEventExit()
        {
            DialoguePatch.Finish();
        }
    }
}
