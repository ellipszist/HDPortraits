using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace HDPortraits
{
    [HarmonyPatch]
    class PortraitDrawPatch
    {
        private static ILHelper patcher = SetupPatch();
        internal static readonly PerScreen<HashSet<MetadataModel>> lastLoaded = new(() => new());
        internal static readonly PerScreen<string> contextSuffix = new();
        internal static readonly PerScreen<MetadataModel> currentMeta = new();
        internal static readonly PerScreen<bool> overridden = new(() => false);

        public static void Warped(object sender, WarpedEventArgs ev)
        {
            var context = ev.NewLocation.getMapProperty("UniquePortrait");
            contextSuffix.Value = (context != "") ? context : null;
        }

        [HarmonyPatch(typeof(DialogueBox), "drawPortrait")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in patcher.Run(instructions))
                yield return code;
        }
        public static ILHelper SetupPatch()
        {
            return new ILHelper("Dialogue Patch")
                .SkipTo(new CodeInstruction[]
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, typeof(DialogueBox).FieldNamed("characterDialogue")),
                    new(OpCodes.Ldfld, typeof(Dialogue).FieldNamed("speaker")),
                    new(OpCodes.Callvirt,typeof(NPC).MethodNamed("get_Portrait"))
                })
                .Add(new CodeInstruction(OpCodes.Call, typeof(PortraitDrawPatch).MethodNamed("SwapTexture")))
                .SkipTo(new CodeInstruction[]
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, typeof(DialogueBox).FieldNamed("characterDialogue")),
                    new(OpCodes.Callvirt, typeof(Dialogue).MethodNamed("getPortraitIndex"))
                })
                .Remove(new CodeInstruction[]
                {
                    new(OpCodes.Ldc_I4_S, 64),
                    new(OpCodes.Ldc_I4_S, 64),
                    new(OpCodes.Call, typeof(Game1).MethodNamed("getSourceRectForStandardTileSheet"))
                })
                .Add(new CodeInstruction(OpCodes.Call,typeof(PortraitDrawPatch).MethodNamed("GetData")))
                .SkipTo(new CodeInstruction[]
                {
                    new(OpCodes.Call, typeof(Color).MethodNamed("get_White")),
                    new(OpCodes.Ldc_R4, 0f),
                    new(OpCodes.Call, typeof(Vector2).MethodNamed("get_Zero"))
                })
                .Remove()
                .Add(new CodeInstruction[]{
                    new(OpCodes.Call,typeof(PortraitDrawPatch).MethodNamed("GetScale"))
                })
                .Finish();
        }
        public static Texture2D SwapTexture(Texture2D texture)
        {
            if (overridden.Value)
                return texture;

            return currentMeta.Value?.overrideTexture?.Value ?? texture;
        }
        public static Rectangle GetData(Texture2D texture, int index)
        {
            int asize = !overridden.Value ? currentMeta.Value?.Size ?? 64 : 64;
            Rectangle ret = (currentMeta.Value?.Animation != null) ?
                currentMeta.Value.Animation.GetSourceRegion(texture, asize, index, Game1.currentGameTime.ElapsedGameTime.Milliseconds) :
                Game1.getSourceRectForStandardTileSheet(texture, index, asize, asize);
            return ret;
        }
        public static string GetSuffix(NPC npc)
        {
            return (bool)DialoguePatch.islandwear.GetValue(npc) ? "beach" : contextSuffix.Value;
        }
        public static float GetScale()
        {
            return !overridden.Value ? 256f / (currentMeta.Value?.Size ?? 64) : 4f;
        }
    }
}
