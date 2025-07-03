using HarmonyLib;
using RimWorld;
using Verse;

namespace PelShield;

[HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.CanWearTogether))]
public class ApparelUtility_CanWearTogether
{
    public static void Postfix(ref bool __result, ThingDef A, ThingDef B, BodyDef body)
    {
        if (__result && A.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax) &&
            B.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax))
        {
            __result = false;
        }
    }
}