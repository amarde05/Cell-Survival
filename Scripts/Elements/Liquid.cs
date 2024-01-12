using UnityEngine;

public abstract class Liquid : Element
{
    protected int preferredLeft;

    protected Vector2Int prevPos;

    protected Liquid(Color[] colors, Vector2Int position) : base(colors, position)
    {
        preferredLeft = Random.Range(0, 2);
    }

    public override void Step(WorldMatrix matrix)
    {
        Vector2Int down = new Vector2Int(m_position.x, m_position.y - 1);

        Element target = matrix.GetElementAtPosition(down);

        if (target == null)
        {
            MoveTo(matrix, down);
        }
        else if (target.GetType().IsSubclassOf(typeof(Solid)) || target.GetType().IsSubclassOf(typeof(Liquid)) || !matrix.IsValidPosition(down))
        {
            Vector2Int left = new Vector2Int(m_position.x - 1, m_position.y);
            Vector2Int right = new Vector2Int(m_position.x + 1, m_position.y);

            Element horiz1 = matrix.GetElementAtPosition(left);
            Element horiz2 = matrix.GetElementAtPosition(right);

            if (preferredLeft == 0)
            {
                if (horiz1 == null && prevPos != left)
                {
                    MoveTo(matrix, left);
                }
                else if (horiz2 == null && prevPos != right)
                {
                    MoveTo(matrix, right);
                }
            }
            else
            {
                if (horiz2 == null && prevPos != right)
                {
                    MoveTo(matrix, right);
                }
                else if (horiz1 == null && prevPos != left)
                {
                    MoveTo(matrix, left);
                }
            }

            prevPos = position;
        }
    }
}
