using UnityEngine;

public abstract class Element
{
    [SerializeField] protected Color m_color;

    [SerializeField] protected Vector2Int m_position;

    public Color color { get => m_color; }
    public Vector2Int position { get => m_position; set => m_position = value; }

    protected Element(Color[] colors, Vector2Int position)
    {
        m_color = PickRandomColor(colors);
        m_position = position;
    }

    protected static Color PickRandomColor(Color[] colors)
    {
        return colors[Random.Range(0, colors.Length)];
    }

    protected void MoveTo(WorldMatrix matrix, Vector2Int newPos)
    {
        matrix.AddAction(new WorldMatrix.CellAction(m_position, newPos, WorldMatrix.CellAction.ActionType.Move));
    }

    protected void SwapWith(WorldMatrix matrix, Vector2Int newPos)
    {
        matrix.AddAction(new WorldMatrix.CellAction(m_position, newPos, WorldMatrix.CellAction.ActionType.Swap));
    }

    public abstract void Step(WorldMatrix matrix);
}
