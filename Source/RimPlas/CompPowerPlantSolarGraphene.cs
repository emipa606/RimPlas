using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas;

[StaticConstructorOnStartup]
public class CompPowerPlantSolarGraphene : CompPowerPlant
{
    private const float FullSunPower = 2500f;

    private const float NightPower = 0f;

    private static readonly Vector2 BarSize = new Vector2(2.3f, 0.14f);

    private static readonly Material PowerPlantSolarBarFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

    private static readonly Material PowerPlantSolarBarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

    protected override float DesiredPowerOutput =>
        Mathf.Lerp(NightPower, FullSunPower, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;

    private float RoofedPowerOutputFactor
    {
        get
        {
            var num = 0;
            var num2 = 0;
            foreach (var item in parent.OccupiedRect())
            {
                num++;
                if (parent.Map.roofGrid.Roofed(item))
                {
                    num2++;
                }
            }

            return (num - num2) / (float)num;
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        var r = default(GenDraw.FillableBarRequest);
        r.center = parent.DrawPos + (Vector3.up * 0.1f);
        r.size = BarSize;
        r.fillPercent = PowerOutput / FullSunPower;
        r.filledMat = PowerPlantSolarBarFilledMat;
        r.unfilledMat = PowerPlantSolarBarUnfilledMat;
        r.margin = 0.15f;
        var rotation = parent.Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        r.rotation = rotation;
        GenDraw.DrawFillableBar(r);
    }
}