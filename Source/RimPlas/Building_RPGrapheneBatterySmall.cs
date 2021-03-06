﻿using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimPlas
{
    // Token: 0x02000005 RID: 5
    [StaticConstructorOnStartup]
    public class Building_RPGrapheneBatterySmall : Building
    {
        // Token: 0x04000009 RID: 9
        private const float MinEnergyToExplode = 500f;

        // Token: 0x0400000A RID: 10
        private const float EnergyToLoseWhenExplode = 400f;

        // Token: 0x0400000B RID: 11
        private const float ExplodeChancePerDamage = 0.05f;

        // Token: 0x04000008 RID: 8
        private static readonly Vector2 BarSize = new Vector2(0.65f, 0.2f);

        // Token: 0x0400000C RID: 12
        private static readonly Material BatteryBarFilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));

        // Token: 0x0400000D RID: 13
        private static readonly Material BatteryBarUnfilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        // Token: 0x04000006 RID: 6
        private int ticksToExplode;

        // Token: 0x04000007 RID: 7
        private Sustainer wickSustainer;

        // Token: 0x0600000E RID: 14 RVA: 0x0000254B File Offset: 0x0000074B
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToExplode, "ticksToExplode");
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002568 File Offset: 0x00000768
        public override void Draw()
        {
            base.Draw();
            var comp = GetComp<CompPowerBattery>();
            var r = default(GenDraw.FillableBarRequest);
            r.center = DrawPos + (Vector3.up * 0.1f);
            r.size = BarSize;
            r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            r.filledMat = BatteryBarFilledMat;
            r.unfilledMat = BatteryBarUnfilledMat;
            r.margin = 0.15f;
            var rotation = Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
            if (ticksToExplode > 0 && Spawned)
            {
                Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BurningWick);
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002634 File Offset: 0x00000834
        public override void Tick()
        {
            base.Tick();
            if (ticksToExplode <= 0)
            {
                return;
            }

            if (wickSustainer == null)
            {
                StartWickSustainer();
            }
            else
            {
                wickSustainer.Maintain();
            }

            ticksToExplode--;
            if (ticksToExplode != 0)
            {
                return;
            }

            var randomCell = this.OccupiedRect().RandomCell;
            var radius = Rand.Range(0.5f, 1f) * 3f;
            GenExplosion.DoExplosion(randomCell, Map, radius, DamageDefOf.Flame, null);
            GetComp<CompPowerBattery>().DrawPower(400f);
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000026F4 File Offset: 0x000008F4
        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (Destroyed || ticksToExplode != 0 || dinfo.Def != DamageDefOf.Flame || !(Rand.Value < 0.05f) ||
                !(GetComp<CompPowerBattery>().StoredEnergy > 500f))
            {
                return;
            }

            ticksToExplode = Rand.Range(70, 150);
            StartWickSustainer();
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002760 File Offset: 0x00000960
        private void StartWickSustainer()
        {
            var info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            wickSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
        }
    }
}