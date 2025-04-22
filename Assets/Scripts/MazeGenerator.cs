using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private MazeCell mazeCellPrefab;

    [SerializeField]
    private int mazeWidth;

    [SerializeField]
    private int mazeDepth;

    [SerializeField]
    public float cellSize = 1f;

    private MazeCell[,] mazeGrid;

    void Start()
    {
        mazeGrid = new MazeCell[mazeWidth, mazeDepth];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeDepth; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);
                mazeGrid[x, z] = Instantiate(mazeCellPrefab, position, Quaternion.identity);
            }
        }

        GenerateMaze(null, mazeGrid[0, 0]);
    }

    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        TryCreateRoom(currentCell);

        MazeCell nextCell;

        do
        {
            nextCell = GetNextUnvisitedCell(currentCell);

            if (nextCell != null)
            {
                GenerateMaze(currentCell, nextCell);
            }

        } while (nextCell != null);
    }

    private void TryCreateRoom(MazeCell currentCell)
    {
        if (Random.value > 0.1f) return; // 10% šance na vytvoření místnosti

        int x = Mathf.RoundToInt(currentCell.transform.position.x / cellSize);
        int z = Mathf.RoundToInt(currentCell.transform.position.z / cellSize);

        // Seznam buněk v místnosti
        List<MazeCell> roomCells = new List<MazeCell>();

        for (int dx = 0; dx <= 1; dx++)
        {
            for (int dz = 0; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;
                if (nx < mazeWidth && nz < mazeDepth)
                {
                    MazeCell cell = mazeGrid[nx, nz];
                    cell.Visit();
                    roomCells.Add(cell);

                    // Zboření zdí pro vytvoření místnosti
                    if (dx == 1)
                    {
                        cell.ClearLeftWall();
                        mazeGrid[nx - 1, nz].ClearRightWall();
                    }
                    if (dz == 1)
                    {
                        cell.ClearBackWall();
                        mazeGrid[nx, nz - 1].ClearFrontWall();
                    }
                }
            }
        }

        // Generování jednoho lockeru v místnosti s 20% pravděpodobností
        if (Random.value < 0.2f && roomCells.Count > 0)
        {
            // Vybereme náhodnou buňku z místnosti
            MazeCell selectedCell = roomCells[Random.Range(0, roomCells.Count)];
            selectedCell.CreateLocker();
        }
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCell(currentCell);
        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell> GetUnvisitedCell(MazeCell currentCell)
    {
        int x = Mathf.RoundToInt(currentCell.transform.position.x / cellSize);
        int z = Mathf.RoundToInt(currentCell.transform.position.z / cellSize);

        if (x + 1 < mazeWidth)
        {
            var cellToRight = mazeGrid[x + 1, z];
            if (!cellToRight.IsVisited)
                yield return cellToRight;
        }

        if (x - 1 >= 0)
        {
            var cellToLeft = mazeGrid[x - 1, z];
            if (!cellToLeft.IsVisited)
                yield return cellToLeft;
        }

        if (z + 1 < mazeDepth)
        {
            var cellToFront = mazeGrid[x, z + 1];
            if (!cellToFront.IsVisited)
                yield return cellToFront;
        }

        if (z - 1 >= 0)
        {
            var cellToBack = mazeGrid[x, z - 1];
            if (!cellToBack.IsVisited)
                yield return cellToBack;
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
            return;

        int prevX = Mathf.RoundToInt(previousCell.transform.position.x / cellSize);
        int prevZ = Mathf.RoundToInt(previousCell.transform.position.z / cellSize);
        int currX = Mathf.RoundToInt(currentCell.transform.position.x / cellSize);
        int currZ = Mathf.RoundToInt(currentCell.transform.position.z / cellSize);

        if (prevX < currX)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }
        if (prevX > currX)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }
        if (prevZ < currZ)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }
        if (prevZ > currZ)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }
}