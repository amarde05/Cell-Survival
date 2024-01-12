using UnityEngine;

public class Sand : MovableSolid
{
    private static Color[] colors =
    {
        new Color(0.859f, 0.847f, 0.149f, 1.0f),
        new Color(0.639f, 0.631f, 0.18f, 1.0f),
        new Color(0.91f, 0.906f, 0.635f, 1.0f)
    };

    public Sand(Vector2Int position) : base(colors, position, 0.25f)
    {

    }
}
