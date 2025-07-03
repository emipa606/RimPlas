using RimWorld;
using Verse;

namespace RimPlas;

public class CompRPHospBed : ThingComp
{
    private CompProperties_RPHospBed Props => (CompProperties_RPHospBed)props;

    public override void CompTick()
    {
        base.CompTick();
        Thing bed = parent;
        var CPT = bed.TryGetComp<CompPowerTrader>();
        if (CPT is not { PowerOn: true } || bed.IsBrokenDown())
        {
            return;
        }

        var Occupant = OccupiedBy(bed);
        if (Occupant == null)
        {
            return;
        }

        if (Controller.Settings.AllowPainCtrlHB && Props.PainCtrlTicks > 0 &&
            bed.IsHashIntervalTick(Props.PainCtrlTicks))
        {
            var patient = OccupiedByPawnInPain(Occupant);
            if (patient != null)
            {
                var PainCtrl = HediffDef.Named(Props.PainCtrlHediff);
                var PainCtrlSev = Props.PainCtrlSev;
                if (!RPHediffEffecter.HasHediff(patient, PainCtrl))
                {
                    RPHediffEffecter.HediffEffect(PainCtrl, PainCtrlSev, patient, null);
                }
            }
        }

        if (!bed.IsHashIntervalTick(625))
        {
            return;
        }

        if (Controller.Settings.AllowMentalCtrlHB)
        {
            var patient2 = OccupiedByPawnWithMentalState(Occupant);
            patient2?.mindState.mentalStateHandler.Reset();
        }

        if (!Controller.Settings.AllowRecCtrlHB)
        {
            return;
        }

        var patient3 = OccupiedByPawnWithRec(Occupant);
        if (patient3 != null)
        {
            patient3.needs.joy.CurLevel += Props.RecPerHour / 4f;
        }
    }


    private Pawn OccupiedBy(Thing bed)
    {
        Pawn p = null;
        if (bed is { Spawned: true } and Building_Bed buildingBed)
        {
            p = buildingBed.GetCurOccupant(0);
        }

        return p;
    }

    private Pawn OccupiedByPawnWithMentalState(Pawn p)
    {
        if (p != null && !p.mindState.mentalStateHandler.InMentalState)
        {
            p = null;
        }

        return p;
    }

    private Pawn OccupiedByPawnWithRec(Pawn p)
    {
        if (p == null)
        {
            return null;
        }

        var needs = p.needs;

        if (needs?.joy != null)
        {
            if (p.needs.joy.CurLevel >= 0.75f)
            {
                p = null;
            }
        }
        else
        {
            p = null;
        }

        return p;
    }

    private Pawn OccupiedByPawnInPain(Pawn p)
    {
        if (p != null && p.health.hediffSet.PainTotal <= 0.1f)
        {
            p = null;
        }

        return p;
    }
}