using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimPlas;

public class RPThingMakerUtility
{
    public const string RCP_RPoly = "RimPoly";

    public const string RCP_EWool = "RimPlas_ElectroSteel_Wool";

    public const string RCP_GWool = "RimPlas_Gold_Wool";

    public const string RCP_PWool = "RimPlas_Plasteel_Wool";

    [NoTranslate]
    private static readonly string ThingIconPath = "Things/Building/Misc/RPThingMaker/UI/RPThingMaker_ThingIcon";

    public static Texture2D GetRPThingIcon(ThingDef t)
    {
        if (t == null)
        {
            return ContentFinder<Texture2D>.Get(ThingIconPath);
        }

        if (t.uiIcon != null)
        {
            return t.uiIcon;
        }

        var graphicData = t.graphicData;
        if (graphicData?.texPath == null)
        {
            return ContentFinder<Texture2D>.Get(ThingIconPath);
        }

        var texturePath = t.graphicData.texPath;
        if (t.graphicData.graphicClass.Name == "Graphic_StackCount")
        {
            texturePath = $"{texturePath}/{t.defName}_a";
        }

        return ContentFinder<Texture2D>.Get(texturePath);
    }

    public static bool RCPProdValues(ThingDef t, out int ticks, out int minProd, out int maxProd,
        out string research)
    {
        ticks = 0;
        minProd = 0;
        maxProd = 0;
        research = "";
        if (t.defName == "ComponentIndustrial")
        {
            ticks = 1000;
            minProd = 1;
            maxProd = 1;
            research = "RimPlas_Components";
            return true;
        }

        if (t.defName != "ComponentSpacer")
        {
            return false;
        }

        ticks = 2000;
        minProd = 1;
        maxProd = 1;
        research = "RimPlas_AdvComponents";
        return true;
    }

    public static List<RPRCPListItem> GetRCPList(ThingDef thingdef)
    {
        var list = new List<RPRCPListItem>();
        list.Clear();
        var item = default(RPRCPListItem);
        switch (thingdef.defName)
        {
            case "ComponentIndustrial":
                item.def = DefDatabase<ThingDef>.GetNamed("RimPoly");
                item.mixgrp = 1;
                item.num = 10;
                item.ratio = 1f;
                list.Add(item);
                item.def = DefDatabase<ThingDef>.GetNamed("RimPlas_ElectroSteel_Wool");
                item.mixgrp = 2;
                item.num = 2;
                item.ratio = 1f;
                list.Add(item);
                break;
            case "ComponentSpacer":
                item.def = DefDatabase<ThingDef>.GetNamed("RimPoly");
                item.mixgrp = 1;
                item.num = 30;
                item.ratio = 1f;
                list.Add(item);
                item.def = DefDatabase<ThingDef>.GetNamed("RimPlas_ElectroSteel_Wool");
                item.mixgrp = 2;
                item.num = 12;
                item.ratio = 1f;
                list.Add(item);
                item.def = DefDatabase<ThingDef>.GetNamed("RimPlas_Gold_Wool");
                item.mixgrp = 3;
                item.num = 3;
                item.ratio = 1f;
                list.Add(item);
                break;
        }

        return list;
    }

    public static List<string> GetMakeList()
    {
        return
        [
            "ComponentIndustrial",
            "ComponentSpacer"
        ];
    }

    public static List<int> GetMaxStock()
    {
        return
        [
            25,
            50,
            75,
            100,
            150,
            200,
            250,
            300,
            400,
            500,
            750,
            1000,
            0
        ];
    }

    public static int StringToInt(string ToConvert)
    {
        if (ToConvert != "No Limit" && int.TryParse(ToConvert, out var Value))
        {
            return Value;
        }

        return 0;
    }

    public struct RPRCPListItem
    {
        internal ThingDef def;

        internal bool mixed;

        internal int mixgrp;

        internal int num;

        internal float ratio;
    }
}