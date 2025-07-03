using RimWorld;
using Verse;
using Verse.Sound;

namespace RimPlas;

internal static class Globals
{
    internal static readonly string StunHed = "HED_RPSecStun";

    internal static readonly float StunSev = 1f;

    internal static void HediffEffect(Pawn pawn, HediffDef stunHediffDef, float SeverityToApply)
    {
        if (pawn.RaceProps.IsMechanoid || SeverityToApply == 0f || stunHediffDef == null ||
            ImmuneTo(pawn, stunHediffDef))
        {
            return;
        }

        var health = pawn.health;
        Hediff hediff;
        if (health == null)
        {
            hediff = null;
        }
        else
        {
            var hediffSet = health.hediffSet;
            hediff = hediffSet?.GetFirstHediffOfDef(stunHediffDef);
        }

        var hashediff = hediff;
        if (hashediff != null)
        {
            hashediff.Severity += SeverityToApply;
            return;
        }

        var addhediff = HediffMaker.MakeHediff(stunHediffDef, pawn);
        addhediff.Severity = SeverityToApply;
        pawn.health.AddHediff(addhediff);
    }

    private static bool ImmuneTo(Pawn pawn, HediffDef def)
    {
        var hediffs = pawn.health.hediffSet.hediffs;
        foreach (var hediff in hediffs)
        {
            var curStage = hediff.CurStage;
            if (curStage?.makeImmuneTo == null)
            {
                continue;
            }

            foreach (var hediffDef in curStage.makeImmuneTo)
            {
                if (hediffDef == def)
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal static void RemoveMental(Pawn pawn)
    {
        if (!pawn.InMentalState)
        {
            return;
        }

        var mentalState = pawn.MentalState;

        mentalState?.RecoverFromState();
    }

    public static void DoSecSpecialEffects(Thing t, float radius)
    {
        FleckMaker.Static(t.Position, t.Map, FleckDefOf.ExplosionFlash, radius * 6f);
        for (var f = 1; f <= 4; f++)
        {
            FleckMaker.ThrowSmoke(t.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius * 0.7f), t.Map,
                radius * 0.6f);
        }

        var StunSound = DefDatabase<SoundDef>.GetNamed("Explosion_EMP", false);
        SoundInfo StunInfo = new TargetInfo(t.Position, t.Map);
        StunSound?.PlayOneShot(StunInfo);
    }
}