using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private static bool overridden = false;
        private static MetadataModel meta = null;
        internal static readonly PerScreen<HashSet<MetadataModel>> lastLoaded = new(() => new());

        [HarmonyPatch(typeof(DialogueBox), "drawPortrait")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in patcher.Run(instructions))
                yield return code;
        }
        public static ILHelper SetupPatch()
        {
            return new ILHelper("Portrait Region Patch")
                .Add(new CodeInstruction[] {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Call,typeof(PortraitDrawPatch).MethodNamed("Init"))
                })
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
                .RemoveAt(new CodeInstruction[]
                {
                    new(OpCodes.Ldc_I4_S, 64),
                    new(OpCodes.Ldc_I4_S, 64),
                    new(OpCodes.Call, typeof(Game1).MethodNamed("getSourceRectForStandardTileSheet"))
                })
                .Add(new CodeInstruction(OpCodes.Call,typeof(PortraitDrawPatch).MethodNamed("GetData")))
                .SkipTo(new CodeInstruction[]
                {
                    new(OpCodes.Call, typeof(Color).MethodNamed("get_White")),
                    new(OpCodes.Ldc_R4, null),
                    new(OpCodes.Call, typeof(Vector2).MethodNamed("get_Zero"))
                })
                .Remove()
                .Add(new CodeInstruction[]{
                    new(OpCodes.Call,typeof(PortraitDrawPatch).MethodNamed("GetScale")),
                    new(OpCodes.Call,typeof(PortraitDrawPatch).MethodNamed("Finish"))
                })
                .Finish();
        }
        public static void Init(DialogueBox box)
        {
            meta = ModEntry.portraitSizes.TryGetValue(box.characterDialogue.speaker?.name, out var data) ? data : null;
            lastLoaded.Value.Add(meta);
            overridden = box.characterDialogue.overridePortrait != null && (meta == null || !meta.AlwaysUse);
        }
        public static void Finish()
        {
            overridden = false;
            meta = null;
        }
        public static Texture2D SwapTexture(Texture2D texture)
        {
            return overridden ? texture : meta?.overrideTexture ?? texture;
        }
        public static Rectangle GetData(Texture2D texture, int index)
        {
            int asize = !overridden ? meta?.Size ?? 64 : 64;
            Rectangle ret = (meta?.Animation != null) ?
                meta.Animation.GetSourceRegion(texture, asize, index, Game1.currentGameTime.ElapsedGameTime.Milliseconds) :
                Game1.getSourceRectForStandardTileSheet(texture, index, asize, asize);
            return ret;
        }
        public static float GetScale()
        {
            return !overridden ? 256f / (meta?.Size ?? 64) : 4f;
        }
    }
}
