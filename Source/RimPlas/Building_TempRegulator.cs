using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas;

public class Building_TempRegulator : Building_TempControl
{
    private const float HeatOutputMultiplier = 1.25f;

    private const float EfficiencyFalloffSpan = 100f;

    private const float EfficiencyLossPerDegreeDifference = 0.0076923077f;

    [NoTranslate] private readonly string commandTexture = "Things/Building/Temperature/UI/BoostPower";

    private bool UseBoost;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref UseBoost, "UseBoost");
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
                defaultLabel = "RimPlas.Boost".Translate(),
                defaultDesc = "RimPlas.BoostDesc".Translate(),
                isActive = () => UseBoost,
                toggleAction = delegate { ToggleUseBoost(UseBoost); }
            };
        }
    }

    public void ToggleUseBoost(bool flag)
    {
        UseBoost = !flag;
    }

    public override void TickRare()
    {
        if (!compPowerTrader.PowerOn)
        {
            return;
        }

        var props = compPowerTrader.Props;
        var coolercell = Position + IntVec3.South.RotatedBy(Rotation);
        var exhaustcell = Position + IntVec3.North.RotatedBy(Rotation);
        var ambientTemperature = coolercell.GetTemperature(Map);
        var targtemp = compTempControl.targetTemperature;
        var powered = false;
        var temptolerance = 0.5f;
        var boost = 1f;
        if (UseBoost)
        {
            boost = 2f;
        }

        if (ambientTemperature < targtemp)
        {
            if (!coolercell.Impassable(Map))
            {
                var numheat = ambientTemperature < 20f ? 1f :
                    ambientTemperature <= 120f ? Mathf.InverseLerp(120f, 20f, ambientTemperature) : 0f;
                var energyLimit = compTempControl.Props.energyPerSecond * numheat * 4.16666651f * boost;
                var tempdelta = GenTemperature.ControlTemperatureTempChange(coolercell, Map, energyLimit,
                    compTempControl.targetTemperature);
                if (!Mathf.Approximately(tempdelta, temptolerance))
                {
                    coolercell.GetRoomOrAdjacent(Map).Temperature += tempdelta;
                    powered = true;
                }
            }
        }
        else if (!exhaustcell.Impassable(Map) && !coolercell.Impassable(Map))
        {
            var tempexhaust = exhaustcell.GetTemperature(Map);
            var tempcool = coolercell.GetTemperature(Map);
            var tempdiff = tempexhaust - tempcool;
            if (tempexhaust - 40f > tempdiff)
            {
                tempdiff = tempexhaust - 40f;
            }

            var numcool = 1f - (tempdiff * EfficiencyLossPerDegreeDifference);
            if (numcool < 0f)
            {
                numcool = 0f;
            }

            var energyLimit = -1f * compTempControl.Props.energyPerSecond * numcool * 4.16666651f * boost;
            var tempdelta2 = GenTemperature.ControlTemperatureTempChange(coolercell, Map, energyLimit,
                compTempControl.targetTemperature);
            if (!Mathf.Approximately(tempdelta2, temptolerance))
            {
                coolercell.GetRoomOrAdjacent(Map).Temperature += tempdelta2;
                GenTemperature.PushHeat(exhaustcell, Map, (0f - energyLimit) * HeatOutputMultiplier);
                powered = true;
            }
        }

        if (powered)
        {
            compPowerTrader.PowerOutput = 0f - (props.PowerConsumption * boost);
        }
        else
        {
            compPowerTrader.PowerOutput =
                (0f - props.PowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
        }

        compTempControl.operatingAtHighPower = powered;
    }
}