using HarmonyLib;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;
using TaleWorlds.Library;

namespace PlayerGovernor
{
    public class PlayerGovernorSubModule : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter starterObject)
        {
            new Harmony("HatDogPlayerGovernor").PatchAll();
            InformationManager.DisplayMessage(new InformationMessage("Successfully Loaded Player Governor", Colors.Green));
        }
    }

    [HarmonyPatch(typeof(EnterSettlementAction), "ApplyInternal")]
    class PlayerGovernorEnterAspect
    {
        [HarmonyBefore]
        public static void Prefix(Hero hero, Settlement settlement)
        {
            if (settlement == null 
                || settlement.Town == null
                || settlement.Town.Governor != null
                || settlement.IsVillage 
                || !Hero.MainHero.Equals(hero)
                || !Clan.PlayerClan.Equals(settlement.OwnerClan))
            {
                return;
            }
            MBTextManager.SetTextVariable("SETTTLEMENT_NAME", settlement.Name.ToString());
            var name = Game.Current.PlayerTroop.Name ?? new TextObject("Player");
            MBInformationManager.AddQuickInformation(new TextObject("{=GxhVR5XBTU}" + name + " Governing: {SETTTLEMENT_NAME}"));
            PlayerGovernorSetttlementAspect.governorSettlement = settlement.Town;
            return;
        }
    }

    [HarmonyPatch(typeof(LeaveSettlementAction), "ApplyForParty")]
    class PlayerGovernorLeaveAspect
    {

        [HarmonyBefore]
        public static void Prefix(MobileParty mobileParty)
        {
            if (mobileParty == null
                || mobileParty.CurrentSettlement == null
                || mobileParty.CurrentSettlement.Town == null
                || !Hero.MainHero.Equals(mobileParty.LeaderHero)
                || !mobileParty.CurrentSettlement.Town.Equals(PlayerGovernorSetttlementAspect.governorSettlement))
            {
                return;
            }
            MBTextManager.SetTextVariable("SETTTLEMENT_NAME", mobileParty.CurrentSettlement.Name.ToString());
            var name = Game.Current.PlayerTroop.Name ?? new TextObject("Player");
            MBInformationManager.AddQuickInformation(new TextObject("{=yJTIwVExFTU}" + name + " No Longer Governing: {SETTTLEMENT_NAME}"));
            PlayerGovernorSetttlementAspect.governorSettlement = null;
        }
    }

    [HarmonyPatch(typeof(Town), "Governor", MethodType.Getter)]
    class PlayerGovernorSetttlementAspect
    {
        public static Town governorSettlement;

        [HarmonyBefore]
        public static bool Prefix(ref Hero __result, Town __instance)
        {
            if (governorSettlement == null)
            {
                return true;
            }
            if (Traverse.Create(__instance).Field("_governor").GetValue() != null)
            {
                return true;
            }
            if (!string.Equals(governorSettlement.ToString(), __instance.ToString()))
            {
                return true;
            }
            __result = Hero.MainHero;
            return false;
        }
    }

    [HarmonyPatch(typeof(Hero), "GovernorOf", MethodType.Getter)]
    class PlayerGovernorHeroAspect
    {
        [HarmonyBefore]
        public static bool Prefix(ref Town __result, Hero __instance)
        {
            if (Hero.MainHero.Equals(__instance))
            {
                __result = PlayerGovernorSetttlementAspect.governorSettlement;
                return false;
            }
            return true;
        }
    }


}
