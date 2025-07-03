using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas;

[StaticConstructorOnStartup]
public class RPCompPowerPlantWind : CompPowerPlant
{
    private const float MaxUsableWindIntensity = 1.5f;

    private const float PowerReductionPercentPerObstacle = 0.2f;

    private const string TranslateWindPathIsBlockedBy = "WindTurbine_WindPathIsBlockedBy";

    private const string TranslateWindPathIsBlockedByRoof = "WindTurbine_WindPathIsBlockedByRoof";

    private static readonly float SpinRateFactor = 0.035f;

    private static readonly float HorizontalBladeOffset = -0.02f;

    private static readonly float VerticalBladeOffset = 0.7f;

    private static readonly float BladeWidth = 3.7f;

    private static Vector2 BarSize;

    private static readonly Material WindTurbineBarFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

    private static readonly Material WindTurbineBarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

    private static readonly Material WindTurbineBladesMat =
        MaterialPool.MatFrom("Things/Building/Power/RPGrapheneWindTurbine/RPGrapheneWindTurbineBlades");

    private readonly int updateWeatherEveryXTicks = 250;

    private readonly List<Thing> windPathBlockedByThings = [];

    private readonly List<IntVec3> windPathBlockedCells = [];

    private readonly List<IntVec3> windPathCells = [];

    private float cachedPowerOutput;

    private float spinPosition;

    private int ticksSinceWeatherUpdate;

    protected override float DesiredPowerOutput => cachedPowerOutput;

    private float PowerPercent => PowerOutput / ((0f - Props.PowerConsumption) * 1.5f);

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        BarSize = new Vector2(parent.def.size.z - 0.95f, 0.1f);
        RecalculateBlockages();
        spinPosition = Rand.Range(0f, 15f);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksSinceWeatherUpdate, "updateCounter");
        Scribe_Values.Look(ref cachedPowerOutput, "cachedPowerOutput");
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!PowerOn)
        {
            cachedPowerOutput = 0f;
            return;
        }

        ticksSinceWeatherUpdate++;
        if (ticksSinceWeatherUpdate >= updateWeatherEveryXTicks)
        {
            var num = Mathf.Min(parent.Map.windManager.WindSpeed, MaxUsableWindIntensity);
            ticksSinceWeatherUpdate = 0;
            cachedPowerOutput = 0f - (Props.PowerConsumption * num);
            RecalculateBlockages();
            if (windPathBlockedCells.Count > 0)
            {
                var num2 = 0f;
                for (var i = 0; i < windPathBlockedCells.Count; i++)
                {
                    num2 += cachedPowerOutput * 0.2f;
                }

                cachedPowerOutput -= num2;
                if (cachedPowerOutput < 0f)
                {
                    cachedPowerOutput = 0f;
                }
            }
        }

        if (cachedPowerOutput > 0.01f)
        {
            spinPosition += PowerPercent * SpinRateFactor;
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        var r = new GenDraw.FillableBarRequest
        {
            center = parent.DrawPos + (Vector3.up * 0.1f),
            size = BarSize,
            fillPercent = PowerPercent,
            filledMat = WindTurbineBarFilledMat,
            unfilledMat = WindTurbineBarUnfilledMat,
            margin = 0.15f
        };
        var rotation = parent.Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        r.rotation = rotation;
        GenDraw.DrawFillableBar(r);
        var pos = parent.TrueCenter();
        pos += parent.Rotation.FacingCell.ToVector3() * VerticalBladeOffset;
        pos += parent.Rotation.RighthandCell.ToVector3() * HorizontalBladeOffset;
        pos.y += 0.0454545468f;
        var num = BladeWidth * Mathf.Sin(spinPosition);
        if (num < 0f)
        {
            num *= -1f;
        }

        var vector = new Vector2(num, 1f);
        var s = new Vector3(vector.x, 1f, vector.y);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
        Graphics.DrawMesh(spinPosition % 3.14159274f * 2f < 3.14159274f ? MeshPool.plane10 : MeshPool.plane10Flip,
            matrix, WindTurbineBladesMat, 0);
        pos.y -= 0.09090909f;
        matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
        Graphics.DrawMesh(spinPosition % 3.14159274f * 2f < 3.14159274f ? MeshPool.plane10Flip : MeshPool.plane10,
            matrix, WindTurbineBladesMat, 0);
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.CompInspectStringExtra());
        if (windPathBlockedCells.Count <= 0)
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine();
        Thing thing = null;
        if (windPathBlockedByThings != null)
        {
            thing = windPathBlockedByThings[0];
        }

        if (thing != null)
        {
            stringBuilder.Append(TranslateWindPathIsBlockedBy.Translate() + " " + thing.Label);
        }
        else
        {
            stringBuilder.Append(TranslateWindPathIsBlockedByRoof.Translate());
        }

        return stringBuilder.ToString();
    }

    private void RecalculateBlockages()
    {
        if (windPathCells.Count == 0)
        {
            var collection =
                RPWindTurbine_Utility.CalculateWindCells(parent.Position, parent.Rotation, parent.def.size);
            windPathCells.AddRange(collection);
        }

        windPathBlockedCells.Clear();
        windPathBlockedByThings.Clear();
        foreach (var intVec in windPathCells)
        {
            if (!intVec.InBounds(parent.Map))
            {
                continue;
            }

            if (parent.Map.roofGrid.Roofed(intVec))
            {
                windPathBlockedByThings.Add(null);
                windPathBlockedCells.Add(intVec);
            }
            else
            {
                var list = parent.Map.thingGrid.ThingsListAt(intVec);
                foreach (var thing in list)
                {
                    if (!thing.def.blockWind)
                    {
                        continue;
                    }

                    windPathBlockedByThings.Add(thing);
                    windPathBlockedCells.Add(intVec);
                    break;
                }
            }
        }
    }
}