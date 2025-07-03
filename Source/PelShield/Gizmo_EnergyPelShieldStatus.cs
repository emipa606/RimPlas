using RimWorld;
using UnityEngine;
using Verse;

namespace PelShield;

[StaticConstructorOnStartup]
public class Gizmo_EnergyPelShieldStatus : Gizmo
{
    private static readonly Texture2D FullShieldBarTex =
        SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

    private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

    public PelShieldApparel shield;

    public Gizmo_EnergyPelShieldStatus()
    {
        base.Order = -100f;
    }

    public override float GetWidth(float maxWidth)
    {
        return 140f;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
        {
            Rect rect2;
            var rect4 = rect2 = overRect.AtZero().ContractedBy(6f);
            rect2.height = overRect.height / 2f;
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect2, shield.LabelCap);
            var rect3 = rect4;
            rect3.yMin = overRect.height / 2f;
            var fillPercent = shield.energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
            Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect3,
                $"{shield.energy * 100f:F0} / {shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f:F0}");
            Text.Anchor = TextAnchor.UpperLeft;
        });
        return new GizmoResult(GizmoState.Clear);
    }
}