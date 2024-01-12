using UnityEngine;

public class Stone : ImmovableSolid
{
    private static Color[] colors =
    {
        new Color(0.5f, 0.5f, 0.5f, 1.0f),
        new Color(0.4f, 0.4f, 0.4f, 1.0f),
        new Color(0.3f, 0.3f, 0.3f, 1.0f)
    };

    public Stone(Vector2Int position) : base(colors, position, 1.0f)
    {

    }
}
