using HarmonyLib;
using StardewValley;
using System;

namespace HDPortraits.Patches
{
    [HarmonyPatch(typeof(GameLocation))]
    internal class GameLocationPatch
    {
        [HarmonyPatch("answerDialogueAction")]
        [HarmonyPrefix]
        public static void answerDialogueActionPrefix(string questionAndAnswer, GameLocation __instance)
        {
            if(questionAndAnswer.StartsWith("telephone_"))
                PortraitDrawPatch.overrideName.Value = "AnsweringMachine";
            else
                PortraitDrawPatch.overrideName.Value = null;
        }
    }
}
