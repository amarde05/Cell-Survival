using UnityEngine;

public abstract class Solid : Element
{
    [Space]

    [SerializeField] protected float m_hardness;

    protected Solid(Color[] colors, Vector2Int position, float hardness) : base(colors, position)
    {
        m_hardness = hardness;
    }
}
