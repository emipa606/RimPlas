using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimPlas;

public class Building_Heal : Building
{
    public CompBuildHeal HealComp => GetComp<CompBuildHeal>();

    private int GetHashOffset(Thing t)
    {
        var text = t.GetHashCode().ToString();
        return text[^1];
    }


    protected override void Tick()
    {
        base.Tick();
        var offset = GetHashOffset(this);
        if (Find.TickManager.TicksGame % (120 + offset) != 0)
        {
            return;
        }

        var HealFactor = GetComp<CompBuildHeal>().Props.HealFactor;
        var PowerNeeded = GetComp<CompBuildHeal>().Props.PowerNeeded;
        var HP = base.HitPoints;
        var MaxHP = MaxHitPoints;
        if (HP >= MaxHP || this.IsBrokenDown() || this.IsBurning())
        {
            return;
        }

        var Healing = true;
        if (PowerNeeded)
        {
            var power = GetComp<CompPowerTrader>();
            if (power is not { PowerOn: true })
            {
                Healing = false;
            }
        }

        if (!Healing)
        {
            return;
        }

        var HealPts = (int)(HealFactor * MaxHP);
        if (MaxHP - HP > HealPts)
        {
            base.HitPoints = HP + HealPts;
        }
        else
        {
            base.HitPoints = MaxHP;
            if (Map != null)
            {
                Map.listerBuildingsRepairable.Notify_BuildingRepaired(this);
                ResetRepairers(this);
            }
        }

        if (Map != null)
        {
            FleckMaker.ThrowMetaIcon(Position, Map, FleckDefOf.HealingCross);
        }
    }

    private static void ResetRepairers(Building wall)
    {
        if (wall.Map == null)
        {
            return;
        }

        var map = wall.Map;
        List<Pawn> list;
        if (map == null)
        {
            list = null;
        }
        else
        {
            var mapPawns = map.mapPawns;
            if (mapPawns == null)
            {
                list = null;
            }
            else
            {
                var freeColonists = mapPawns.FreeColonists;
                list = freeColonists?.ToList();
            }
        }

        var pawnList = list;
        if (pawnList is not { Count: > 0 })
        {
            return;
        }

        var repairer = new List<Pawn>();
        foreach (var element in pawnList)
        {
            var curjob = element?.CurJob;
            if (curjob != null && curjob.def.defName == "Repair" && curjob.targetA.Thing == wall)
            {
                repairer.AddDistinct(element);
            }
        }

        if (repairer.Count <= 0)
        {
            return;
        }

        foreach (var pawn in repairer)
        {
            if (pawn?.CurJob == null)
            {
                continue;
            }

            var jobs = pawn.jobs;
            jobs?.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
}