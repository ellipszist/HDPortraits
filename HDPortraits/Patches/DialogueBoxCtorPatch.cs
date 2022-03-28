using HarmonyLib;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HDPortraits.Patches
{
    [HarmonyPatch]
    class DialogueBoxCtorPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var ctor in AccessTools.GetDeclaredConstructors(typeof(DialogueBox)))
                yield return ctor;
        }
        public static void Postfix(DialogueBox __instance)
        {
            DialoguePatch.Init(__instance);
        }
    }
}
