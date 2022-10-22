using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Reflection;

namespace HDPortraits.Patches
{
    [HarmonyPatch]
    class DialoguePatch
    {
        [HarmonyPatch(typeof(Dialogue),"exitCurrentDialogue")]
        [HarmonyPostfix]
        public static void Cleanup()
        {
            PortraitDrawPatch.currentMeta.Value?.Animation?.Reset();
        }

        public static void Init(DialogueBox __instance)
        {
            bool overriden = __instance.characterDialogue?.overridePortrait != null;
            NPC npc = __instance.characterDialogue?.speaker;
            if (npc != null || overriden)
            {
                if (ModEntry.TryGetMetadata(overriden ? PortraitDrawPatch.overrideName.Value ?? "NULL" : npc.getTextureName(), PortraitDrawPatch.GetSuffix(npc), out var meta))
                {
                    PortraitDrawPatch.lastLoaded.Value.Add(meta);
                    PortraitDrawPatch.currentMeta.Value = meta;
                    meta.Animation?.Reset();
                } else
                {
                    PortraitDrawPatch.currentMeta.Value = null;
                }
            }
        }

        public static void Finish()
        {
            Cleanup();
            PortraitDrawPatch.overrideName.Value = null;
            PortraitDrawPatch.currentMeta.Value = null;
        }
    }
}
