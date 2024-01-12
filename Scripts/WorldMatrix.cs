using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMatrix : MonoBehaviour
{
    [Header("World Parameters")]
    [SerializeField] private float stepTime = 0.1f;
    private float stepCounter;

    [SerializeField] private Vector2 matrixOrigin = new Vector2(-5, -10);
    [SerializeField] private Vector2Int matrixSize = new Vector2Int(250, 250);

    [Header("Texture Parameters")]
    [SerializeField] private float pixelsPerUnit;

    [Tooltip("The size of the texture in relation to the window size (i.e a value of 1.25 would meant the texture size is 1.25x the window size)")]
    [SerializeField] private float worldTextureRelativeSize = 1.25f;

    [SerializeField] private int worldTextureSortingLayerID = 0;
    [SerializeField] private string worldTextureSortingLayerName = "Default";

    [SerializeField] private Color clearColor;

    [Header("Viewport Parameters")]
    [SerializeField] private int viewportEdgeMinDistance = 10;

    [Space]

    [SerializeField] private float sandSpawnTime = 0.25f;
    private float sandSpawnCounter;


    private Texture2D worldTexture;
    private SpriteRenderer spriteRenderer;

    private float aspectRatio;

    private RectInt viewport;

    private Element[,] matrix;

    private List<CellAction> actions;

    private void Start()
    {
        InitializeTexture();

        matrix = new Element[matrixSize.x, matrixSize.y];

        actions = new List<CellAction>();

        for (int y = 0; y < 3; y++)
        {
            for (int x = -10; x <= 10; x++)
            {
                AddElement(new Stone(worldPosToMatrixPos(transform.position) + new Vector2Int(x, y)));
            }
        }

        for (int y = 3; y < 12; y++)
        {
            for (int x = -10; x <= -7; x++)
            {
                AddElement(new Stone(worldPosToMatrixPos(transform.position) + new Vector2Int(x, y)));
                AddElement(new Stone(worldPosToMatrixPos(transform.position) + new Vector2Int(-x, y)));
            }
        }

        for (int y = 3; y < 12; y++)
        {
            for (int x = -6; x <= 6; x++)
            {
                AddElement(new Water(worldPosToMatrixPos(transform.position) + new Vector2Int(x, y)));
            }
        }

        //AddElement(new Sand(worldPosToMatrixPos(transform.position) + new Vector2Int(0, 20)));

        //AddElement(new Water(worldPosToMatrixPos(transform.position) + new Vector2Int(0, 10)));
        //AddElement(new Water(worldPosToMatrixPos(transform.position) + new Vector2Int(0, 12)));
    }

    private void InitializeTexture()
    {
        aspectRatio = (float)Screen.width / Screen.height;

        // Calculate the resolution of the texture from the pixels per unit
        int texHeight = (int)(2 * (Camera.main.orthographicSize * worldTextureRelativeSize) * pixelsPerUnit);
        int texWidth = (int)(texHeight * aspectRatio);

        worldTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, -1, false);
        worldTexture.filterMode = FilterMode.Point;

        if (spriteRenderer == null)
        {
            GameObject srObject = new GameObject("Global Sprite Renderer");
            srObject.transform.parent = null;

            spriteRenderer = srObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerID = worldTextureSortingLayerID;
            spriteRenderer.sortingLayerName = worldTextureSortingLayerName;
        }

        spriteRenderer.sprite = Sprite.Create(worldTexture, new Rect(0, 0, worldTexture.width, worldTexture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);

        UpdateViewport();
    }

    private void Update()
    {
        // Check if camera is within the viewport, if it is not, move the viewport so that the camera is in the center
        if (!CheckCameraInViewport())
        {
            spriteRenderer.transform.position = (Vector2)Camera.main.transform.position;

            UpdateViewport();
        }

        stepCounter += Time.deltaTime;

        if (stepCounter >= stepTime)
        {
            stepCounter = 0;

            StepAll();
            PerformActions();
        }

        Render();

        sandSpawnCounter += Time.deltaTime;

        if (sandSpawnCounter >= sandSpawnTime)
        {
            sandSpawnCounter = 0;

            AddElement(new Sand(worldPosToMatrixPos(transform.position) + new Vector2Int(0, 50)));
        }
    }

    // Updates the viewport to center on the camera
    private void UpdateViewport()
    {
        Vector2Int matPos = worldPosToMatrixPos(Camera.main.transform.position);

        Vector2Int texSize = new Vector2Int(worldTexture.width, worldTexture.height);

        Vector2Int bottomCorner = new Vector2Int(matPos.x - (texSize.x / 2), matPos.y - (texSize.y / 2));
        Vector2Int topCorner = bottomCorner + texSize;

        if (bottomCorner.x < 0)
            bottomCorner.x = 0;
        else if (topCorner.x >= matrixSize.x) {
            topCorner.x = matrixSize.x - 1;
            bottomCorner.x = topCorner.x - texSize.x;
        }

        if (bottomCorner.y < 0)
            bottomCorner.y = 0;
        else if (topCorner.y >= matrixSize.y) {
            topCorner.y = matrixSize.y - 1;
            bottomCorner.y = topCorner.y - texSize.y;
        }

        viewport = new RectInt(bottomCorner, texSize);

        spriteRenderer.transform.position = matrixPosToWorldPos(bottomCorner) + (matrixSizeToWorldSize(texSize) / 2.0f);
    }

    // Returns true if camera is within a specified threshold of the viewports edge
    private bool CheckCameraInViewport()
    {
        Vector2 camSize = new Vector2(2 * Camera.main.orthographicSize * aspectRatio, 2 * Camera.main.orthographicSize);

        Vector2 camPos = Camera.main.transform.position;
        Vector2 camCorner = new Vector2(camPos.x - (camSize.x / 2), camPos.y - (camSize.y / 2));

        RectInt camRect = new RectInt(worldPosToMatrixPos(camCorner), worldSizeToMatrixSize(camSize));

        //Debug.Log("Right: " + (viewport.xMax - camRect.xMax) +
        //          "\nLeft: " + (camRect.xMin - viewport.xMin) +
        //          "\nUp: " + (viewport.yMax - camRect.yMax) +
        //          "\nDown: " + (camRect.yMin - viewport.yMin));

        //Debug.Log("Camera: " + camRect + "\nViewport" + viewport);

        return (viewport.xMax - camRect.xMax > viewportEdgeMinDistance) &&
               (camRect.xMin - viewport.xMin > viewportEdgeMinDistance) &&
               (viewport.yMax - camRect.yMax > viewportEdgeMinDistance) &&
               (camRect.yMin - viewport.yMin > viewportEdgeMinDistance);
    }


    private void Render()
    {
        ClearTexture();

        for (int y = viewport.yMax - 1; y >= viewport.yMin; y--)
        {
            for (int x = viewport.xMin; x < viewport.xMax; x++)
            {
                Element element = matrix[x,y];

                if (element != null)
                {
                    worldTexture.SetPixel(x - viewport.xMin, y - viewport.yMin, element.color);
                }
            }
        }

        worldTexture.Apply();
    }

    private void ClearTexture()
    {
        for (int y = 0; y < worldTexture.height; y++)
        {
            for (int x = 0; x < worldTexture.width; x++)
            {
                worldTexture.SetPixel(x, y, clearColor);
            }
        }
    }

    
    private void StepAll()
    {
        for (int y = matrixSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < matrixSize.x; x++)
            {
                if (matrix[x,y] != null)
                    matrix[x,y].Step(this);
            }
        }
    }

    private void PerformActions()
    {
        foreach (CellAction action in actions)
        {
            if (action.actionType == CellAction.ActionType.Move)
                MoveElement(action.from, action.to);
            else if (action.actionType == CellAction.ActionType.Swap)
                SwapElements(action.from, action.to);
        }

        actions.Clear();
    }

    public void AddElement(Element element)
    {
        if (IsValidPosition(element.position))
        {
            matrix[element.position.x, element.position.y] = element;
        }
    }

    public void RemoveElement(Vector2Int position)
    {
        if (IsValidPosition(position))
            matrix[position.x, position.y] = null;
    }

    public bool MoveElement(Vector2Int from, Vector2Int to)
    {
        if (IsValidPosition(from) && IsValidPosition(to))
        {
            matrix[to.x, to.y] = matrix[from.x, from.y];
            matrix[from.x, from.y] = null;

            if (matrix[to.x, to.y] != null)
                matrix[to.x, to.y].position = to;

            return true;
        }

        return false;
    }

    public bool SwapElements(Vector2Int from, Vector2Int to)
    {
        if (IsValidPosition(from) && IsValidPosition(to))
        {
            Element temp = matrix[to.x, to.y];
            matrix[to.x, to.y] = matrix[from.x, from.y];
            matrix[from.x, from.y] = temp;

            if (matrix[to.x, to.y] != null)
                matrix[to.x, to.y].position = to;

            if (matrix[from.x, from.y] != null)
                matrix[from.x, from.y].position = from;

            return true;
        }

        return false;
    }

    public bool AddAction(CellAction action)
    {
        if (IsValidPosition(action.from) && IsValidPosition(action.to))
        {
            actions.Add(action);

            return true;
        }

        return false;
    }

    public Element GetElementAtPosition(Vector2Int pos)
    {
        if (!IsValidPosition(pos))
            return null;

        return matrix[pos.x, pos.y];
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < matrixSize.x && pos.y >= 0 && pos.y < matrixSize.y;
    }

    public Vector2Int worldSizeToMatrixSize(Vector2 worldSize)
    {
        return new Vector2Int((int)(worldSize.x * pixelsPerUnit), (int)(worldSize.y * pixelsPerUnit));
    }

    public Vector2 matrixSizeToWorldSize(Vector2Int matrixSize)
    {
        return new Vector2(matrixSize.x / pixelsPerUnit, matrixSize.y / pixelsPerUnit);
    }

    public Vector2Int worldPosToMatrixPos(Vector2 worldPos)
    {
        return new Vector2Int((int)((worldPos.x - matrixOrigin.x) * pixelsPerUnit), (int)((worldPos.y - matrixOrigin.y) * pixelsPerUnit));
    }

    public Vector2 matrixPosToWorldPos(Vector2Int matrixPos)
    {
        return new Vector2((matrixPos.x / pixelsPerUnit) + matrixOrigin.x, (matrixPos.y / pixelsPerUnit) + matrixOrigin.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(matrixPosToWorldPos(new Vector2Int(0, 0)), 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(matrixPosToWorldPos(new Vector2Int(0, 0)), matrixPosToWorldPos(new Vector2Int(250, 0)));
        Gizmos.DrawLine(matrixPosToWorldPos(new Vector2Int(0, 0)), matrixPosToWorldPos(new Vector2Int(0, 250)));
        Gizmos.DrawLine(matrixPosToWorldPos(new Vector2Int(250, 0)), matrixPosToWorldPos(new Vector2Int(250, 250)));
        Gizmos.DrawLine(matrixPosToWorldPos(new Vector2Int(0, 250)), matrixPosToWorldPos(new Vector2Int(250, 250)));
    }

    public struct CellAction
    {
        public Vector2Int from;
        public Vector2Int to;

        public enum ActionType
        {
            Move,
            Swap
        }

        public ActionType actionType;

        public CellAction(Vector2Int from, Vector2Int to, ActionType actionType)
        {
            this.from = from;
            this.to = to;

            this.actionType = actionType;
        }
    }
}