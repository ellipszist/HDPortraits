using AeroCore;
using AeroCore.Utils;
using StardewValley;
using StardewValley.Menus;

namespace HDPortraits.Integration
{
	[ModInit(WhenHasMod = "FlashShifter.SVECode")]
	internal class SVE
	{
		internal static void Init()
		{
			var target = Reflection.TypeNamed("StardewValleyExpanded.ModEntry").MethodNamed("setUpShopOwner_Postfix");
			if (target is null)
				return;

			ModEntry.monitor.Log("Patching SVE for traveling merchant...");
			ModEntry.harmony.Patch(target, new(typeof(SVE).MethodNamed(nameof(Postfix))));
		}
		private static void Postfix(string __0, ref ShopMenu __1)
		{
			if (__1.portraitPerson is null)
				return;

			if ("Traveler".Equals(__0) || "TravelerNightMarket".Equals(__0))
				__1.portraitPerson.syncedPortraitPath.Value = "Portraits\\Suki";
			else if (Game1.dayOfMonth == 8 && Game1.currentSeason == "winter" && Game1.player.currentLocation.NameOrUniqueName == "Temp")
				__1.portraitPerson.syncedPortraitPath.Value = "Portraits\\Suki_IceFestival";
		}
	}
}
