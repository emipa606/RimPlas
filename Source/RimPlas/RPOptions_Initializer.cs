using System;
using System.Collections.Generic;
using Verse;

namespace RimPlas;

[StaticConstructorOnStartup]
internal static class RPOptions_Initializer
{
    static RPOptions_Initializer()
    {
        LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
    }

    private static void Setup()
    {
        var allDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
        if (allDefs.Count <= 0)
        {
            return;
        }

        var RPList = RPResearchList();
        foreach (var ResDef in allDefs)
        {
            if (!RPList.Contains(ResDef.defName))
            {
                continue;
            }

            var Resbase = ResDef.baseCost;
            Resbase = checked((int)Math.Round(Resbase * Controller.Settings.ResPct / 100f));
            ResDef.baseCost = Resbase;
        }
    }

    private static List<string> RPResearchList()
    {
        var list = new List<string>();
        list.AddDistinct("RimPlas");
        list.AddDistinct("RimPlas_Bulk");
        list.AddDistinct("RimPlas_SemiSynth");
        list.AddDistinct("RimPlas_InjMld");
        list.AddDistinct("RimPlas_AdvInjMld");
        list.AddDistinct("RimPlas_Synth");
        list.AddDistinct("RimPlas_Synth_Bulk");
        list.AddDistinct("RimPlas_Synth_Plasteel");
        list.AddDistinct("RimPlas_Synth_Plasteel_Bulk");
        list.AddDistinct("RimPlas_CarboSynth");
        list.AddDistinct("RimPlas_Graphene");
        list.AddDistinct("RimPlas_BulkGraphene");
        list.AddDistinct("RimPlas_CarbonComposites");
        list.AddDistinct("RimPlas_CarbonCompositesBulk");
        list.AddDistinct("RimPlas_Components");
        list.AddDistinct("RimPlas_AdvComponents");
        list.AddDistinct("RimPlas_MultiAnalyzer");
        list.AddDistinct("RimThermoPlas");
        list.AddDistinct("RPSecurityDoors");
        list.AddDistinct("RimPlas_AdvIntelMats");
        list.AddDistinct("Rimoprene");
        list.AddDistinct("Rimica");
        list.AddDistinct("FortRimica");
        list.AddDistinct("RPGrapheneElectroliser");
        list.AddDistinct("RPThingMaker");
        return list;
    }
}