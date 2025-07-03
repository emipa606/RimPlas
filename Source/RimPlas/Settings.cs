using UnityEngine;
using Verse;

namespace RimPlas;

public class Settings : ModSettings
{
    public bool AllowMentalCtrlHB = true;

    public bool AllowPainCtrlHB = true;

    public bool AllowRecCtrlHB = true;

    public float GVentMax = 30f;

    public float GVentMin = 10f;

    public float ResPct = 100f;

    public void DoWindowContents(Rect canvas)
    {
        var listingStandard = new Listing_Standard { ColumnWidth = canvas.width };
        listingStandard.Begin(canvas);
        listingStandard.Gap();
        checked
        {
            listingStandard.Label("RimPlas.ResPct".Translate() + "  " + (int)ResPct);
            ResPct = (int)listingStandard.Slider((int)ResPct, 10f, 200f);
            listingStandard.Gap();
            Text.Font = GameFont.Tiny;
            listingStandard.Label("          " + "RimPlas.ResWarn".Translate());
            listingStandard.Gap();
            listingStandard.Label("          " + "RimPlas.ResTip".Translate());
            Text.Font = GameFont.Small;
            listingStandard.Gap();
            listingStandard.Label("RimPlas.GVentMin".Translate() + "  " + (int)GVentMin);
            GVentMin = (int)listingStandard.Slider(GVentMin, -20f, 20f);
            listingStandard.Gap();
            listingStandard.Label("RimPlas.GVentMax".Translate() + "  " + (int)GVentMax);
            GVentMax = (int)listingStandard.Slider(GVentMax, 25f, 50f);
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("RimPlas.AllowPainCtrlHB".Translate(), ref AllowPainCtrlHB);
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("RimPlas.AllowRecCtrlHB".Translate(), ref AllowRecCtrlHB);
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("RimPlas.AllowMentalCtrlHB".Translate(), ref AllowMentalCtrlHB);
            if (Controller.CurrentVersion != null)
            {
                listingStandard.Gap();
                GUI.contentColor = Color.gray;
                listingStandard.Label("RimPlas.CurrentModVersion".Translate(Controller.CurrentVersion));
                GUI.contentColor = Color.white;
            }

            listingStandard.End();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ResPct, "ResPct", 100f);
        Scribe_Values.Look(ref GVentMin, "GVentMin", 10f);
        Scribe_Values.Look(ref GVentMax, "GVentMax", 30f);
        Scribe_Values.Look(ref AllowPainCtrlHB, "AllowPainCtrlHB", true);
        Scribe_Values.Look(ref AllowRecCtrlHB, "AllowRecCtrlHB", true);
        Scribe_Values.Look(ref AllowMentalCtrlHB, "AllowMentalCtrlHB", true);
    }
}