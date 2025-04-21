using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;



static class Extensions
{
    public static IEnumerable<(int, T)> Enumerate<T>(
    this IEnumerable<T> input,
    int start = 0
    )
    {
        int i = start;
        foreach (var t in input)
            yield return (i++, t);
    }
}

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] GridSpawner gridSpawner;
    [Range(0, 100)][SerializeField] int R_WallSpawnPercentage;
    [Range(0, 100)][SerializeField] int B_WallSpawnPercentage;
    [SerializeField] int wall_yValue;
    [SerializeField] GameObject wall;
    [SerializeField] GameObject wallHolder;
    private CustomGrid gridXZ;
    private int max_set_value = 1;

    private void Start()
    {
        gridXZ = gridSpawner.grid;
        SpawnMaze();
        wallHolder.transform.GetChild(0).gameObject.SetActive(false);
        wallHolder.transform.GetChild(wallHolder.transform.childCount - 1).gameObject.SetActive(false);
    }

    private void SpawnMaze()
    {
        //Initial row generation
        List<(int[], int)> first_row = new List<(int[], int)> ();

        for (int x = 0; x < gridSpawner.width; x++)
        {
            int[] coordinates = new int[] { x, 0 };
            var cellTup = (coordinates, max_set_value);

            first_row.Add(cellTup);

            max_set_value += 1;
        }
        //WorldText_Row(first_row, Color.white, 0);

        PlaceWall_SameDir(first_row, Vector3.right);

        //Maze generation loop
        List<(int[], int)> row = first_row;
        for (int z = 0; z < gridSpawner.height; z++)
        {
            //Step (5) Finalize Row:
            if (z == gridSpawner.height - 1)
            {
                var last_row = JoinRow_VerticalWalls(row, true);
                PlaceWall_SameDir(IncreaseAxisRow(1, row), Vector3.right);
                //WorldText_Row(last_row, Color.red, 1);
                //FinalRow(row);
                break;
            }

            //Step (3), Vertical Walls & Joining:
            var joined_row = JoinRow_VerticalWalls(row, false);
            //WorldText_Row(joined_row, Color.red, 1);

            //Step (4), Bottom Walls:
            BottomWalls(joined_row, out List<int[]> b_wall_loc);



            //Step (5 + 2), Adding New Row:
            row = IncreaseAxisRow(1, EmptyCells_NewRow(joined_row, b_wall_loc));

        }

    }

    private List<(int[], int)> JoinRow_VerticalWalls(List<(int[], int)> row, bool last)
    {
        var row_copy = new List<(int[], int)> (row);

        PlaceWall_XZ(Vector3.forward, row_copy[0].Item1[0], row_copy[0].Item1[1]);

        for (int i = 0; i < row_copy.Count; i++)
        {
            if (i + 1 < row_copy.Count)
            {
                var curr_cell = row_copy[i];
                var next_cell = row_copy[i + 1];
                bool join = false;

                if (curr_cell.Item2 == next_cell.Item2)
                {
                    join = false;
                }
                else
                {
                    if (!last)
                    {
                        int rand_num = UnityEngine.Random.Range(0, 101);
                        if (inRange_Inclusive(rand_num, 0, R_WallSpawnPercentage))
                        {
                            join = true;
                        }
                        else
                        {
                            join = false;
                        }
                    }
                    else
                    {
                        join = true;
                    }

                }

                //Wall
                if (join == false)
                {
                    var coord = next_cell.Item1;
                    PlaceWall_XZ(Vector3.forward, coord[0], coord[1]);
                }
                else
                {
                    var newCellTup = (next_cell.Item1, curr_cell.Item2);
                    row_copy[i + 1] = newCellTup;
                }
            }

        }
        PlaceWall_XZ(Vector3.forward, row_copy[row_copy.Count - 1].Item1[0] + 1, row_copy[row_copy.Count - 1].Item1[1]);
        return row_copy;
    }

    private void BottomWalls(List<(int[], int)> row, out List<int[]> bottomWallLocations)
    {
        var row_copy = new List<(int[], int)>(row);
        var bwall_cells = new List<(int[], int)>();
        var no_bwall_cells = new List<(int[], int)>();

        foreach (var set_list in RowSortedBySet(row_copy))
        {
            var shuffledSets = set_list.OrderBy(a => rng.Next()).ToList();

            foreach (var (i, cell) in shuffledSets.Enumerate())
            {
                if (i == 0)
                {
                    no_bwall_cells.Add(cell);
                    continue;
                }

                int rand_num = UnityEngine.Random.Range(0, 101);

                if (inRange_Inclusive(rand_num, 0, B_WallSpawnPercentage))
                {
                    bwall_cells.Add(cell);
                }
                else
                {
                    no_bwall_cells.Add(cell);
                }
            }
        }

        List<int[]> bWalls = new List<int[]>();
        foreach (var cell in bwall_cells)
        {
            bWalls.Add(cell.Item1);
        }
        bottomWallLocations = bWalls;


        foreach (var cellTup in IncreaseAxisRow(1, bwall_cells))
        {
            var coord = cellTup.Item1;
            PlaceWall_XZ(Vector3.right, coord[0], coord[1]);
        }
    }
    private List<(int[], int)> EmptyCells_NewRow(List<(int[], int)> row, List<int[]> empty_cells)
    {
        var new_row = new List<(int[], int)>(row);
        foreach (var (i, cell) in row.Enumerate())
        {
            int cellX = cell.Item1[0];
            int cellZ = cell.Item1[1];

            foreach (var coord in empty_cells)
            {
                int eCellX = coord[0];
                int eCellZ = coord[1];

                if (cellX == eCellX && cellZ == eCellZ)
                {
                    var new_cell_tup = (cell.Item1, max_set_value);
                    new_row[i] = new_cell_tup;
                    max_set_value += 1;
                }
            }
        }

        return new_row;
    }


    #region Helper_Functions
    private static System.Random rng = new System.Random();
    private void WorldText_Row(List<(int[], int)> row, Color textColor, int yVal)
    {
        GameObject rowObj = new GameObject();
        foreach (var cellTup in row)
        {
            var coords = cellTup.Item1;
            var set = cellTup.Item2;

            var text = new GameObject();
            var mesh = text.AddComponent<TextMesh>();
            mesh.text = set.ToString();
            mesh.color = textColor;

            Vector3 gridPos = gridXZ.GetWorldPosition(coords[0], coords[1]);
            gridPos = new Vector3(gridPos.x + gridXZ.GetCellSize() / 2, yVal, gridPos.z + gridXZ.GetCellSize() / 2);
            text.transform.position = gridPos;
            text.transform.parent = rowObj.transform;
        }
    }

    private bool inRange_Inclusive(int num, int minRange, int maxRange)
    {
        if (minRange <= num && num <= maxRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void PlaceWall_XZ(Vector3 dir, int x, int z)
    {
        Vector3 position = gridXZ.GetWorldPosition(x, z);
        position.y = wall_yValue;

        GameObject w = GameObject.Instantiate(wall, position, Quaternion.LookRotation(dir));
        w.transform.SetParent(wallHolder.transform);

    }

    private void PlaceWall_SameDir(List<(int[], int)> row, Vector3 dir)
    {
        foreach (var cellTup in row)
        {
            var coords = cellTup.Item1;
            PlaceWall_XZ(dir, coords[0], coords[1]);
        }
    }

    private List<List<(int[], int)>> RowSortedBySet(List<(int[], int)> row)
    {
        return row
            .Select((x) => new { Value = x })
            .GroupBy(x => x.Value.Item2)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }

    private List<(int[], int)> IncreaseAxisRow(int add_amount, List<(int[], int)> row)
    {
        var new_row = new List<(int[], int)>();

        foreach (var celltup in row)
        {
            var coords = celltup.Item1;
            var set = celltup.Item2;

            int x = coords[0];
            int z = coords[1] + add_amount;

            int[] new_coords = { x, z };

            var new_cell_tup = (new_coords, set);

            new_row.Add(new_cell_tup);
        }

        return new_row;
    }

    #endregion
}
