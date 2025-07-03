using Verse;

namespace RimPlas;

public class RPGrapheneConduit : Building
{
    private static readonly ThingDef buriedDef = ThingDef.Named("RPGraphenePowerConduit_Buried");

    private bool buried;

    private Graphic buriedGraphic;

    public override Graphic Graphic
    {
        get
        {
            Graphic defaultGraphic;
            if (!buried)
            {
                defaultGraphic = DefaultGraphic;
            }
            else
            {
                buriedGraphic = GraphicDatabase.Get(buriedDef.graphicData.graphicClass,
                    buriedDef.graphicData.texPath, buriedDef.graphicData.shaderType.Shader,
                    buriedDef.graphicData.drawSize, DrawColor, DrawColorTwo);
                defaultGraphic = buriedGraphic;
            }

            return defaultGraphic;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        buried = true;
        base.SpawnSetup(map, respawningAfterLoad);
    }
}