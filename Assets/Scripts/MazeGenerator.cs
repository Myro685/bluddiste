using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;

/// <summary>
/// Generuje bludiště, místnosti, rozmísťuje objekty a aktualizuje NavMesh.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    public int width = 20;                    // Šířka bludiště v buňkách
    public int height = 20;                   // Výška bludiště v buňkách
    public int corridorWidth = 2;             // Šířka chodby v buňkách
    public int numberOfRooms = 3;             // Počet místností
    public int minRoomSize = 3;               // Minimální velikost místnosti
    public int maxRoomSize = 5;               // Maximální velikost místnosti
    public GameObject wallPrefab;             // Prefab zdi
    public GameObject collectiblePrefab;      // Prefab sběratelného předmětu
    public GameObject enemyPrefab;            // Prefab nepřítele
    public GameObject tablePrefab;            // Prefab stolu (židle)
    public int numberOfCollectibles = 5;      // Počet sběratelných předmětů
    public int numberOfEnemies = 3;           // Počet nepřátel
    public int numberOfTables = 2;            // Počet stolů (židlí)
    private int[,] maze;                      // Pole reprezentující bludiště (0 = volno, 1 = zeď)
    private List<Vector2Int> freeCells;       // Seznam volných buněk pro umístění objektů
    private List<RectInt> rooms;              // Seznam místností
    private NavMeshSurface navMeshSurface;    // NavMeshSurface pro generování NavMesh

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface není připojený k objektu Maze!");
            return;
        }

        rooms = new List<RectInt>();
        GenerateMaze();         // Vygeneruje základní bludiště
        GenerateRooms();        // Přidá místnosti
        BuildMaze();            // Postaví zdi podle pole
        PlaceCollectibles();    // Rozmístí sběratelné předměty
        PlaceEnemies();         // Rozmístí nepřátele
        PlaceChairs();          // Rozmístí stoly/židle

        UpdateNavMesh();        // Aktualizuje NavMesh pro AI
    }

    /// <summary>
    /// Vygeneruje základní bludiště pomocí backtracking algoritmu.
    /// </summary>
    void GenerateMaze()
    {
        int gridWidth = (width / corridorWidth) + 2;
        int gridHeight = (height / corridorWidth) + 2;

        maze = new int[gridWidth, gridHeight];
        freeCells = new List<Vector2Int>();
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                maze[x, y] = 1; // Všechny buňky jsou zdi

        RecursiveBacktrack(1, 1); // Začátek generování

        // Okrajové zdi
        for (int x = 0; x < gridWidth; x++)
        {
            maze[x, 0] = 1;
            maze[x, gridHeight - 1] = 1;
        }
        for (int y = 0; y < gridHeight; y++)
        {
            maze[0, y] = 1;
            maze[gridWidth - 1, y] = 1;
        }

        UpdateFreeCells(); // Aktualizuje seznam volných buněk
    }

    /// <summary>
    /// Vygeneruje místnosti a propojí je s bludištěm.
    /// </summary>
    void GenerateRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomX = Random.Range(1, maze.GetLength(0) - roomWidth - 1);
            int roomY = Random.Range(1, maze.GetLength(1) - roomHeight - 1);

            rooms.Add(new RectInt(roomX, roomY, roomWidth, roomHeight));

            // Vyčistí prostor pro místnost
            for (int x = roomX; x < roomX + roomWidth; x++)
            {
                for (int y = roomY; y < roomY + roomHeight; y++)
                {
                    maze[x, y] = 0;
                }
            }

            ConnectRoomToMaze(roomX, roomY, roomWidth, roomHeight); // Propojí místnost s bludištěm
        }

        UpdateFreeCells();
    }

    /// <summary>
    /// Propojí místnost s bludištěm vytvořením otvoru ve zdi.
    /// </summary>
    void ConnectRoomToMaze(int roomX, int roomY, int roomWidth, int roomHeight)
    {
        int side = Random.Range(0, 4);
        int connectX, connectY;

        if (side == 0)
        {
            connectX = roomX + Random.Range(0, roomWidth);
            connectY = roomY - 1;
        }
        else if (side == 1)
        {
            connectX = roomX + Random.Range(0, roomWidth);
            connectY = roomY + roomHeight;
        }
        else if (side == 2)
        {
            connectX = roomX - 1;
            connectY = roomY + Random.Range(0, roomHeight);
        }
        else
        {
            connectX = roomX + roomWidth;
            connectY = roomY + Random.Range(0, roomHeight);
        }

        if (connectX >= 0 && connectX < maze.GetLength(0) && connectY >= 0 && connectY < maze.GetLength(1))
        {
            maze[connectX, connectY] = 0;
        }
    }

    /// <summary>
    /// Rekurzivní backtracking algoritmus pro generování bludiště.
    /// </summary>
    void RecursiveBacktrack(int x, int y)
    {
        maze[x, y] = 0;

        int[] directions = { 0, 1, 2, 3 };
        Shuffle(directions);

        for (int i = 0; i < directions.Length; i++)
        {
            int dx = 0, dy = 0;
            if (directions[i] == 0) { dx = 0; dy = 2; }
            else if (directions[i] == 1) { dx = 0; dy = -2; }
            else if (directions[i] == 2) { dx = -2; dy = 0; }
            else if (directions[i] == 3) { dx = 2; dy = 0; }

            int newX = x + dx;
            int newY = y + dy;

            // Pokud je sousední buňka uvnitř a je zeď, pokračuj rekurzí
            if (newX >= 1 && newX < maze.GetLength(0) - 1 && newY >= 1 && newY < maze.GetLength(1) - 1 && maze[newX, newY] == 1)
            {
                maze[x + dx / 2, y + dy / 2] = 0;
                RecursiveBacktrack(newX, newY);
            }
        }
    }

    /// <summary>
    /// Aktualizuje seznam volných buněk (pro umístění objektů).
    /// </summary>
    void UpdateFreeCells()
    {
        freeCells.Clear();

        for (int x = corridorWidth; x < width + corridorWidth; x++)
        {
            for (int y = corridorWidth; y < height + corridorWidth; y++)
            {
                int gridX = (x - corridorWidth) / corridorWidth;
                int gridY = (y - corridorWidth) / corridorWidth;

                if (gridX >= 0 && gridX < maze.GetLength(0) && gridY >= 0 && gridY < maze.GetLength(1) && maze[gridX, gridY] == 0)
                {
                    freeCells.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    /// <summary>
    /// Zamíchá pole (Fisher-Yates shuffle).
    /// </summary>
    void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    /// <summary>
    /// Vytvoří GameObjecty zdí podle pole bludiště.
    /// </summary>
    void BuildMaze()
    {
        for (int x = 0; x < width + 2 * corridorWidth; x++)
        {
            for (int y = 0; y < height + 2 * corridorWidth; y++)
            {
                int gridX = (x - corridorWidth) / corridorWidth;
                int gridY = (y - corridorWidth) / corridorWidth;

                bool isBorder = x < corridorWidth || x >= width + corridorWidth || y < corridorWidth || y >= height + corridorWidth;

                if (isBorder || (gridX >= 0 && gridX < maze.GetLength(0) && gridY >= 0 && gridY < maze.GetLength(1) && maze[gridX, gridY] == 1))
                {
                    Vector3 pos = new Vector3(x, 1, y);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                    wall.tag = "Wall";
                }
            }
        }
    }

    /// <summary>
    /// Rozmístí sběratelné předměty na volná místa.
    /// </summary>
    void PlaceCollectibles()
    {
        if (collectiblePrefab == null || freeCells.Count == 0) return;

        for (int i = 0; i < numberOfCollectibles && freeCells.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, freeCells.Count);
            Vector2Int cell = freeCells[randomIndex];
            freeCells.RemoveAt(randomIndex); 

            Vector3 pos = new Vector3(cell.x, 0.5f, cell.y);
            Instantiate(collectiblePrefab, pos, Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// Rozmístí nepřátele na volná místa a nastaví jejich pozici na NavMesh.
    /// </summary>
    void PlaceEnemies()
    {
        if (enemyPrefab == null || freeCells.Count == 0) return;

        for (int i = 0; i < numberOfEnemies && freeCells.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, freeCells.Count);
            Vector2Int cell = freeCells[randomIndex];
            freeCells.RemoveAt(randomIndex);

            Vector3 pos = new Vector3(cell.x, 0, cell.y);
            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, transform);
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(pos, out hit, 1f, NavMesh.AllAreas))
                {
                    enemy.transform.position = hit.position;
                }
            }
        }
    }

    /// <summary>
    /// Rozmístí stoly/židle na volná místa.
    /// </summary>
    void PlaceChairs()
    {
        if (tablePrefab == null || freeCells.Count == 0) return;

        for (int i = 0; i < numberOfTables && freeCells.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, freeCells.Count);
            Vector2Int cell = freeCells[randomIndex];
            freeCells.RemoveAt(randomIndex); 

            Vector3 pos = new Vector3(cell.x, 0f, cell.y);
            Instantiate(tablePrefab, pos, Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// Aktualizuje NavMesh pro AI navigaci.
    /// </summary>
    void UpdateNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh byl aktualizován!");
        }
    }

    /// <summary>
    /// Vrací kopii seznamu volných buněk (pro AI patrolování apod.).
    /// </summary>
    public List<Vector2Int> GetFreeCells()
    {
        return new List<Vector2Int>(freeCells);
    }
}