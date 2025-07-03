using System.Linq;
using RimWorld;
using Verse;

namespace RimPlas;

public class CompSecurityReact : ThingComp
{
    private CompProperties_SecurityReact Props => (CompProperties_SecurityReact)props;

    public override void CompTick()
    {
        if (Find.TickManager.TicksGame % 60 != 0 || IsDisabled(parent))
        {
            return;
        }

        var target = MentalDetect(parent, Props.radius);
        if (target == null)
        {
            return;
        }

        var thingWithComps = parent;
        var radius = Props.radius;
        var compPropertiesSecurityReact = Props;
        DoStunner(thingWithComps, radius, target, compPropertiesSecurityReact?.SecurityHediff,
            Props.SecuritySeverity);
    }

    private static void DoStunner(ThingWithComps securityThing, float radius, Thing target, string SecHed,
        float SecSev)
    {
        HediffDef secHediffDef;
        if (SecHed != null)
        {
            secHediffDef = DefDatabase<HediffDef>.GetNamed(SecHed, false);
            if (secHediffDef == null)
            {
                secHediffDef = DefDatabase<HediffDef>.GetNamed(Globals.StunHed, false);
            }
        }
        else
        {
            secHediffDef = DefDatabase<HediffDef>.GetNamed(Globals.StunHed, false);
        }

        var SeverityToApply = Globals.StunSev;
        if (SecSev > 0f)
        {
            SeverityToApply = SecSev;
        }

        Globals.RemoveMental(target as Pawn);
        Globals.HediffEffect(target as Pawn, secHediffDef, SeverityToApply);
        Globals.DoSecSpecialEffects(securityThing, radius);
    }

    private static Thing MentalDetect(ThingWithComps securityThing, float radius)
    {
        var DetectThings = GenRadial
            .RadialDistinctThingsAround(securityThing.Position, securityThing.Map, radius, true).ToList();
        if (DetectThings.Count <= 0)
        {
            return null;
        }

        foreach (var mentalDetect in DetectThings)
        {
            if (mentalDetect is not Pawn pawn || pawn.IsBurning() || pawn.Dead ||
                pawn.Downed || !pawn.Awake())
            {
                continue;
            }

            if (pawn.IsPrisoner &&
                PrisonBreakUtility.IsPrisonBreaking(pawn))
            {
                return pawn;
            }

            if (pawn.IsColonist && pawn.InMentalState)
            {
                return pawn;
            }
        }

        return null;
    }

    private static bool IsDisabled(ThingWithComps checkThing)
    {
        if (checkThing.IsBrokenDown())
        {
            return true;
        }

        var compFlickable = checkThing.TryGetComp<CompFlickable>();
        if (compFlickable is { SwitchIsOn: false })
        {
            return true;
        }

        var compPowerTrader = checkThing.TryGetComp<CompPowerTrader>();
        if (compPowerTrader is { PowerOn: false })
        {
            return true;
        }

        var compRefuelable = checkThing.TryGetComp<CompRefuelable>();
        return compRefuelable is { HasFuel: false };
    }
}