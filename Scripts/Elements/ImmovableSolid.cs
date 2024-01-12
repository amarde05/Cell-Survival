using UnityEngine;

public abstract class ImmovableSolid : Solid
{
    protected ImmovableSolid(Color[] colors, Vector2Int position, float hardness) : base(colors, position, hardness)
    {

    }

    public override void Step(WorldMatrix matrix)
    {

    }
}
