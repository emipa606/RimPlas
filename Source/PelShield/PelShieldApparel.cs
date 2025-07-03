using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PelShield;

[StaticConstructorOnStartup]
public class PelShieldApparel : Apparel
{
    public const float MinDrawSize = 1.2f;

    public const float MaxDrawSize = 1.55f;

    public const float MaxDamagedJitterDist = 0.05f;

    public const int JitterDurationTicks = 8;

    private static readonly Material BubbleMat =
        MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

    private readonly float ApparelScorePerEnergyMax = 0.25f;

    private readonly float EnergyLossPerDamage = 0.03f;

    private readonly float EnergyOnReset = 0.2f;

    private readonly SoundDef EnergyShield_Broken = SoundDef.Named("EnergyShield_Broken");

    private readonly int KeepDisplayingTicks = 1000;

    private readonly int StartingTicksToReset = 2500;

    public float energy;

    private Vector3 impactAngleVect;

    private int lastAbsorbDamageTick = -9999;

    private int lastKeepDisplayTick = -9999;

    private int ticksToReset = -1;

    private float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

    private float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

    public float Energy => energy;

    private ShieldState ShieldState => ticksToReset > 0 ? ShieldState.Resetting : ShieldState.Active;

    private bool ShouldDisplay
    {
        get
        {
            var wearer = Wearer;
            return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState ||
                                                                        wearer.Drafted ||
                                                                        wearer.Faction
                                                                            .HostileTo(Faction.OfPlayer) &&
                                                                        !wearer.IsPrisoner ||
                                                                        Find.TickManager.TicksGame <
                                                                        lastKeepDisplayTick + KeepDisplayingTicks);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref energy, "energy");
        Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
    }

    public override IEnumerable<Gizmo> GetWornGizmos()
    {
        if (Find.Selector.SingleSelectedThing == Wearer)
        {
            yield return new Gizmo_EnergyPelShieldStatus
            {
                shield = this
            };
        }
    }

    public override float GetSpecialApparelScoreOffset()
    {
        return EnergyMax * ApparelScorePerEnergyMax;
    }

    protected override void Tick()
    {
        base.Tick();
        var wearer = Wearer;
        if (wearer == null)
        {
            energy = 0f;
        }
        else
        {
            switch (ShieldState)
            {
                case ShieldState.Resetting:
                {
                    ticksToReset--;
                    if (ticksToReset <= 0)
                    {
                        reset();
                    }

                    break;
                }
                case ShieldState.Active:
                {
                    energy += EnergyGainPerTick;
                    if (energy > EnergyMax)
                    {
                        energy = EnergyMax;
                    }

                    break;
                }
            }
        }

        if (wearer == null || !wearer.IsHashIntervalTick(2500))
        {
            return;
        }

        const float regenEnergy = 0.1f;
        if (energy > regenEnergy && !wearer.Drafted && isAutoRepair(this))
        {
            DoAutoRepair(this, regenEnergy);
        }
    }

    private static void DoAutoRepair(Apparel apparel, float regenEnergy)
    {
        if (!(((PelShieldApparel)apparel).energy > regenEnergy))
        {
            return;
        }

        var regenAmount = 5;
        if (!apparel.def.useHitPoints)
        {
            return;
        }

        var maxhp = apparel.MaxHitPoints;
        var hp = apparel.HitPoints;
        if (hp >= maxhp)
        {
            return;
        }

        if (hp + regenAmount <= maxhp)
        {
            apparel.HitPoints += regenAmount;
        }

        ((PelShieldApparel)apparel).energy -= regenEnergy;
    }

    private static bool isAutoRepair(Apparel apparel)
    {
        if (apparel == null || !apparel.def.useHitPoints)
        {
            return false;
        }

        var apparel2 = apparel.def.apparel;
        if (apparel2 == null || apparel2.tags.Count <= 0)
        {
            return false;
        }

        var apparel3 = apparel.def.apparel;
        var tags = apparel3?.tags;
        if (tags is { Count: <= 0 })
        {
            return false;
        }

        if (tags == null)
        {
            return false;
        }

        using var enumerator = tags.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current == "PelRegenFromShield")
            {
                return true;
            }
        }

        return false;
    }

    public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
    {
        if (ShieldState != ShieldState.Active)
        {
            return false;
        }

        if (dinfo.Def == DamageDefOf.EMP)
        {
            energy = 0f;
            shieldBroken();
            return false;
        }

        var haywire = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP", false);
        if (haywire != null && dinfo.Def == haywire)
        {
            energy = 0f;
            shieldBroken();
            return false;
        }

        if (!dinfo.Def.isRanged && !dinfo.Def.isExplosive)
        {
            return false;
        }

        energy -= dinfo.Amount * EnergyLossPerDamage;
        if (energy < 0f)
        {
            shieldBroken();
        }
        else
        {
            absorbedDamage(dinfo);
        }

        return true;
    }

    private void keepDisplaying()
    {
        lastKeepDisplayTick = Find.TickManager.TicksGame;
    }

    private void absorbedDamage(DamageInfo dinfo)
    {
        var wearer = Wearer;
        SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
        impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
        var loc = wearer.TrueCenter() + (impactAngleVect.RotatedBy(180f) * 0.5f);
        var num = Mathf.Min(10f, 2f + (dinfo.Amount / 10f));
        FleckMaker.Static(loc, wearer.Map, FleckDefOf.ExplosionFlash, num);
        var num2 = (int)num;
        for (var i = 0; i < num2; i++)
        {
            FleckMaker.ThrowDustPuff(loc, wearer.Map, Rand.Range(0.8f, 1.2f));
        }

        lastAbsorbDamageTick = Find.TickManager.TicksGame;
        keepDisplaying();
    }

    private void shieldBroken()
    {
        var wearer = Wearer;
        EnergyShield_Broken.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
        FleckMaker.Static(wearer.TrueCenter(), wearer.Map, FleckDefOf.ExplosionFlash, 12f);
        for (var i = 0; i < 6; i++)
        {
            FleckMaker.ThrowDustPuff(
                wearer.TrueCenter() + (Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) *
                                       Rand.Range(0.3f, 0.6f)), wearer.Map, Rand.Range(0.8f, 1.2f));
        }

        energy = 0f;
        ticksToReset = StartingTicksToReset;
    }

    private void reset()
    {
        var wearer = Wearer;
        if (wearer.Spawned)
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
            FleckMaker.ThrowLightningGlow(wearer.TrueCenter(), wearer.Map, 3f);
        }

        ticksToReset = -1;
        energy = EnergyOnReset;
    }

    public override void DrawWornExtras()
    {
        if (ShieldState != ShieldState.Active || !ShouldDisplay)
        {
            return;
        }

        var wearer = Wearer;
        var num = Mathf.Lerp(1.2f, 1.55f, energy);
        var vector = wearer.Drawer.DrawPos;
        vector.y = AltitudeLayer.Blueprint.AltitudeFor();
        var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
        if (num2 < 8)
        {
            var num3 = (8 - num2) / 8f * 0.05f;
            vector += impactAngleVect * num3;
            num -= num3;
        }

        float angle = Rand.Range(0, 360);
        var s = new Vector3(num, 1f, num);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
    }

    public override bool AllowVerbCast(Verb v)
    {
        return true;
    }
}