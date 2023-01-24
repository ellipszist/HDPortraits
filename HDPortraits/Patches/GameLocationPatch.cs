using HarmonyLib;
using StardewValley;

namespace HDPortraits.Patches
{
	[HarmonyPatch(typeof(GameLocation))]
	internal class GameLocationPatch
	{
		[HarmonyPatch("answerDialogueAction")]
		[HarmonyPrefix]
		public static void answerDialogueActionPrefix(string questionAndAnswer)
		{
			if(questionAndAnswer.StartsWith("telephone_"))
				PortraitDrawPatch.overrideName.Value = "AnsweringMachine";
			else
				PortraitDrawPatch.overrideName.Value = null;
		}
	}
}
