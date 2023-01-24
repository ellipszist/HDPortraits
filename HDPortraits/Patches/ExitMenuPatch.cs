using HarmonyLib;
using StardewValley;

namespace HDPortraits.Patches
{
	[HarmonyPatch]
	internal class ExitMenuPatch
	{
		[HarmonyPatch(typeof(Game1), "exitActiveMenu")]
		[HarmonyPrefix]
		internal static void PrefixExitMenu()
		{
			DialoguePatch.Finish();
		}

		[HarmonyPatch(typeof(Event), "cleanup")]
		[HarmonyPostfix]
		internal static void PostfixEventCleanup()
		{
			PortraitDrawPatch.NpcEventSuffixes.Value.Clear();
			DialoguePatch.Finish();
		}
	}
}
