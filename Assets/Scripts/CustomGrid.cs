using UnityEngine;
using UnityEngine.Rendering;

public class CustomGrid : MonoBehaviour
{

    private int width;
    private int height;
    private int cellSize;
    private Vector3 originPosition;
    private int[,] gridArray; //(x,z)

    public CustomGrid(int width, int height, int cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.black, 1000f);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.black, 1000f);

            }
        }
    }


    public int GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }

    public int[] GetCoords(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        int z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);

        return new int[]{x, z};
    }
}
