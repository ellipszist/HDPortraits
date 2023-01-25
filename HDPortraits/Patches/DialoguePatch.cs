﻿using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

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
			var name = GetNameToUse(__instance.characterDialogue, out var has_suffix);

			if (name is not null)
			{
				if (name.Length is 0)
					ModEntry.monitor.Log("Could not retrieve portrait name for nameless NPC!", StardewModdingAPI.LogLevel.Warn);

				var npc = __instance.characterDialogue.speaker;
				if (ModEntry.TryGetMetadata(name, npc is not null && !has_suffix ? PortraitDrawPatch.GetSuffix(npc) : null, out var meta))
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

		public static string GetNameToUse(Dialogue dialogue, out bool includes_suffix)
		{
			includes_suffix = false;

			if (dialogue is null)
				return null;

			if (dialogue.overridePortrait is not null)
				return PortraitDrawPatch.overrideName.Value ?? "";

			return GetTextureNameSync(dialogue.speaker, out includes_suffix);
		}
		
		public static string GetTextureNameSync(NPC npc, out bool includes_suffix)
		{
			includes_suffix = false;
			if (npc is not null)
			{
				if (npc.Name is not null)
					return npc.getTextureName();

				var synced = npc.syncedPortraitPath.Value;
				if (synced is not null && synced.Length > 0)
				{
					includes_suffix = true;
					return synced.Replace("Portraits\\", "");
				}

				return "";
			}
			return null;
		}
	}
}
