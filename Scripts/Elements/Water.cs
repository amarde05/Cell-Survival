using UnityEngine;

public class Water : Liquid
{
    private static Color[] colors =
    {
        new Color(0.463f, 0.463f, 1.0f, 1.0f),
        new Color(0.275f, 0.275f, 0.741f, 1.0f),
        new Color(0.431f, 0.431f, 0.831f, 1.0f)
    };

    public Water(Vector2Int position) : base(colors, position)
    {

    }
}
