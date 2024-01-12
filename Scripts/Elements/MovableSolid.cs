using UnityEngine;

public abstract class MovableSolid : Solid
{
    protected MovableSolid(Color[] colors, Vector2Int position, float hardness) : base(colors, position, hardness)
    {

    }

    public override void Step(WorldMatrix matrix)
    {
        Vector2Int down = new Vector2Int(m_position.x, m_position.y - 1);

        Element target = matrix.GetElementAtPosition(down);

        if (target == null)
        {
            MoveTo(matrix, down);
        }
        else if (target.GetType().IsSubclassOf(typeof(Liquid)))
        {
            SwapWith(matrix, down);
        }
        else if (target.GetType().IsSubclassOf(typeof(Solid)))
        {
            Vector2Int leftDiag = new Vector2Int(m_position.x - 1, m_position.y - 1);
            Vector2Int rightDiag = new Vector2Int(m_position.x + 1, m_position.y - 1);

            Element diag1 = matrix.GetElementAtPosition(leftDiag);
            Element diag2 = matrix.GetElementAtPosition(rightDiag);

            if (diag1 == null)
            {
                MoveTo(matrix, leftDiag);
            }
            else if (diag2 == null)
            {
                MoveTo(matrix, rightDiag);
            }
        }
    }
}
