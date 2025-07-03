using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas;

public class RPGraphene_Vent : Building_TempControl
{
    [NoTranslate]
    private readonly string commandTexture = "Things/Building/Temperature/RPGraphene_Vent/UI/RPGraphene_Vent";

    private CompFlickable flickableComp;

    private bool InRangeATM = true;

    private bool UseFixed = true;

    public override Graphic Graphic => flickableComp.CurrentGraphic;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        flickableComp = GetComp<CompFlickable>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref UseFixed, "UseFixed", true);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (Faction == Faction.OfPlayer)
        {
            yield return new Command_Toggle
            {
                icon = ContentFinder<Texture2D>.Get(commandTexture),
                defaultLabel = "RimPlas.Fixed".Translate(),
                defaultDesc =
                    "RimPlas.FixedDesc".Translate(Controller.Settings.GVentMin, Controller.Settings.GVentMax),
                isActive = () => UseFixed,
                toggleAction = delegate { ToggleUseFixed(UseFixed); }
            };
        }
    }

    private void ToggleUseFixed(bool flag)
    {
        UseFixed = !flag;
    }

    public override void TickRare()
    {
        var tempRange = default(FloatRange);
        GetSafeTemps(UseFixed, out var tempMin, out var tempMax);
        tempRange.min = tempMin;
        tempRange.max = tempMax;
        if (UseFixed && RPGVentCheckTemps(this, tempRange) || !UseFixed)
        {
            if (!InRangeATM && VentIsWorking(this))
            {
                InRangeATM = !InRangeATM;
                if (!flickableComp.SwitchIsOn)
                {
                    flickableComp.DoFlick();
                }
            }

            if (!FlickUtility.WantsToBeOn(this))
            {
                return;
            }

            if (VentIsWorking(this))
            {
                GenTemperature.EqualizeTemperaturesThroughBuilding(this, 20f, true);
                return;
            }

            GenTemperature.EqualizeTemperaturesThroughBuilding(this, 14f, true);
        }
        else
        {
            InRangeATM = false;
            if (VentIsWorking(this) && flickableComp.SwitchIsOn)
            {
                flickableComp.DoFlick();
            }
        }
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (FlickUtility.WantsToBeOn(this))
        {
            return stringBuilder.ToString();
        }

        if (stringBuilder.Length > 0)
        {
            stringBuilder.AppendLine();
        }

        stringBuilder.Append("VentClosed".Translate());

        return stringBuilder.ToString();
    }

    private bool VentIsWorking(Building b)
    {
        return !b.IsBrokenDown();
    }

    private void GetSafeTemps(bool Fixed, out float min, out float max)
    {
        min = -9999f;
        max = 9999f;
        if (!Fixed)
        {
            return;
        }

        min = Controller.Settings.GVentMin;
        max = Controller.Settings.GVentMax;
    }

    private bool RPGVentCheckTemps(Building b, FloatRange tempRange)
    {
        for (var i = 0; i < 2; i++)
        {
            var intVec = i != 0 ? b.Position - b.Rotation.FacingCell : b.Position + b.Rotation.FacingCell;
            if (!intVec.InBounds(b.Map))
            {
                continue;
            }

            var roomGroup = intVec.GetRoomOrAdjacent(b.Map);
            if (roomGroup == null)
            {
                continue;
            }

            var tempChk = roomGroup.Temperature;
            if (!tempRange.Includes(tempChk))
            {
                return false;
            }
        }

        return true;
    }
}