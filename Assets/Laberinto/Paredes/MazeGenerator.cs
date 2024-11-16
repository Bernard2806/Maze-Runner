using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;    // Prefab de la esfera (jugador)
    public GameObject goalPrefab;      // Prefab de la meta
    private int[,] maze;
    private Vector3 exitPosition;      // Para guardar la posición de la salida
    
    // Dimensiones de las paredes
    private const float WALL_LENGTH = 5f;    // x
    private const float WALL_HEIGHT = 1f;    // y
    private const float WALL_WIDTH = 0.5f;   // z

    // Dimensiones del suelo
    private const float FLOOR_LENGTH = 1f;   // x
    private const float FLOOR_HEIGHT = 0.5f; // y
    private const float FLOOR_WIDTH = 1f;    // z

    void Start()
    {
        maze = new int[width, height];
        GenerateMaze();
        CreateFloor();
        DrawMaze();
        PlacePlayerAndGoal();
    }

    void PlacePlayerAndGoal()
    {
        // Calcular offsets para el centro del laberinto
        float offsetX = -(width * WALL_LENGTH) / 2f;
        float offsetZ = -(height * WALL_WIDTH) / 2f;

        // Colocar el jugador en el centro
        Vector3 playerPosition = new Vector3(
            offsetX + (width/2 * WALL_LENGTH),
            1f, // Altura sobre el suelo
            offsetZ + (height/2 * WALL_WIDTH)
        );
        Instantiate(playerPrefab, playerPosition, Quaternion.identity);

        // Colocar la meta en la salida
        if (exitPosition != Vector3.zero)
        {
            // Ajustar la posición Y de la meta según su prefab
            exitPosition.y = 0.5f; // Ajusta esta altura según el tamaño de tu meta
            Instantiate(goalPrefab, exitPosition, Quaternion.identity);
        }
    }

    void GenerateMaze()
    {
        // Inicializar todo como paredes
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 1; // 1 representa pared
            }
        }

        // Empezar desde el centro del laberinto
        int startX = width / 2;
        int startY = height / 2;
        maze[startX, startY] = 0; // 0 representa camino

        // Usar DFS para generar el laberinto
        DFS(startX, startY);
    }

    void CreateExit()
    {
        int startX = width / 2;
        int startY = height / 2;
        
        List<(int x, int y)> possibleExits = new List<(int x, int y)>();
        float offsetX = -(width * WALL_LENGTH) / 2f;
        float offsetZ = -(height * WALL_WIDTH) / 2f;
        
        // Buscar posiciones adyacentes a caminos en los bordes
        // Borde superior
        for (int x = 1; x < width - 1; x++)
        {
            if (maze[x, height - 2] == 0)
            {
                possibleExits.Add((x, height - 1));
            }
        }
        // Borde inferior
        for (int x = 1; x < width - 1; x++)
        {
            if (maze[x, 1] == 0)
            {
                possibleExits.Add((x, 0));
            }
        }
        // Borde izquierdo
        for (int y = 1; y < height - 1; y++)
        {
            if (maze[1, y] == 0)
            {
                possibleExits.Add((0, y));
            }
        }
        // Borde derecho
        for (int y = 1; y < height - 1; y++)
        {
            if (maze[width - 2, y] == 0)
            {
                possibleExits.Add((width - 1, y));
            }
        }

        // Encontrar la salida más alejada del centro
        if (possibleExits.Count > 0)
        {
            int maxDistance = 0;
            (int x, int y) selectedExit = possibleExits[0];

            foreach (var exit in possibleExits)
            {
                int distance = Mathf.Abs(exit.x - startX) + Mathf.Abs(exit.y - startY);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    selectedExit = exit;
                }
            }

            // Crear la salida
            maze[selectedExit.x, selectedExit.y] = 0;
            
            // Guardar la posición de la salida para colocar la meta
            exitPosition = new Vector3(
                offsetX + (selectedExit.x * WALL_LENGTH),
                0,
                offsetZ + (selectedExit.y * WALL_WIDTH)
            );
        }
    }

    void DFS(int x, int y)
    {
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };
        Shuffle(dx, dy);

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i] * 2;
            int ny = y + dy[i] * 2;

            if (nx > 1 && ny > 1 && nx < width - 2 && ny < height - 2 && maze[nx, ny] == 1)
            {
                maze[nx - dx[i], ny - dy[i]] = 0;
                maze[nx, ny] = 0;
                DFS(nx, ny);
            }
        }
    }

    void Shuffle(int[] dx, int[] dy)
    {
        for (int i = 0; i < dx.Length; i++)
        {
            int rnd = Random.Range(0, dx.Length);
            int tempX = dx[rnd];
            int tempY = dy[rnd];
            dx[rnd] = dx[i];
            dy[rnd] = dy[i];
            dx[i] = tempX;
            dy[i] = tempY;
        }
    }

    void CreateFloor()
    {
        // Calcular el tamaño total del laberinto
        float totalWidth = width * WALL_LENGTH;
        float totalDepth = height * WALL_WIDTH;

        // Calcular cuántas piezas de suelo necesitamos en cada dirección
        int floorPiecesX = Mathf.CeilToInt(totalWidth / FLOOR_LENGTH);
        int floorPiecesZ = Mathf.CeilToInt(totalDepth / FLOOR_WIDTH);

        // Calcular el offset para centrar el suelo
        float offsetX = -(totalWidth / 2f);
        float offsetZ = -(totalDepth / 2f);

        // Crear la cuadrícula de suelo
        for (int x = 0; x <= floorPiecesX; x++)
        {
            for (int z = 0; z <= floorPiecesZ; z++)
            {
                Vector3 position = new Vector3(
                    offsetX + (x * FLOOR_LENGTH),
                    -FLOOR_HEIGHT,
                    offsetZ + (z * FLOOR_WIDTH)
                );

                GameObject floorPiece = Instantiate(floorPrefab, position, Quaternion.identity);
                
                float scaleX = FLOOR_LENGTH;
                float scaleZ = FLOOR_WIDTH;

                if (x == floorPiecesX)
                {
                    float remainingX = totalWidth % FLOOR_LENGTH;
                    if (remainingX > 0) scaleX = remainingX;
                }
                if (z == floorPiecesZ)
                {
                    float remainingZ = totalDepth % FLOOR_WIDTH;
                    if (remainingZ > 0) scaleZ = remainingZ;
                }

                floorPiece.transform.localScale = new Vector3(scaleX, FLOOR_HEIGHT, scaleZ);
            }
        }
    }

    void DrawMaze()
    {
        float offsetX = -(width * WALL_LENGTH) / 2f;
        float offsetZ = -(height * WALL_WIDTH) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 1)
                {
                    Vector3 position = new Vector3(
                        offsetX + (x * WALL_LENGTH),
                        0,
                        offsetZ + (y * WALL_WIDTH)
                    );
                    
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
                    wall.transform.localScale = new Vector3(WALL_LENGTH, WALL_HEIGHT, WALL_WIDTH);
                }
            }
        }

        CreateExteriorWalls(offsetX, offsetZ);
        CreateExit(); // Aseguramos que se cree la salida
    }

    void CreateExteriorWalls(float offsetX, float offsetZ)
    {
        // Pared superior e inferior
        for (int x = -1; x <= width; x++)
        {
            // Pared superior
            Vector3 topPosition = new Vector3(
                offsetX + (x * WALL_LENGTH),
                0,
                offsetZ + (height * WALL_WIDTH)
            );
            GameObject topWall = Instantiate(wallPrefab, topPosition, Quaternion.identity);
            topWall.transform.localScale = new Vector3(WALL_LENGTH, WALL_HEIGHT, WALL_WIDTH);

            // Pared inferior
            Vector3 bottomPosition = new Vector3(
                offsetX + (x * WALL_LENGTH),
                0,
                offsetZ - WALL_WIDTH
            );
            GameObject bottomWall = Instantiate(wallPrefab, bottomPosition, Quaternion.identity);
            bottomWall.transform.localScale = new Vector3(WALL_LENGTH, WALL_HEIGHT, WALL_WIDTH);
        }

        // Pared izquierda y derecha
        for (int y = -1; y <= height; y++)
        {
            // Pared izquierda
            Vector3 leftPosition = new Vector3(
                offsetX - WALL_LENGTH,
                0,
                offsetZ + (y * WALL_WIDTH)
            );
            GameObject leftWall = Instantiate(wallPrefab, leftPosition, Quaternion.identity);
            leftWall.transform.localScale = new Vector3(WALL_LENGTH, WALL_HEIGHT, WALL_WIDTH);

            // Pared derecha
            Vector3 rightPosition = new Vector3(
                offsetX + (width * WALL_LENGTH),
                0,
                offsetZ + (y * WALL_WIDTH)
            );
            GameObject rightWall = Instantiate(wallPrefab, rightPosition, Quaternion.identity);
            rightWall.transform.localScale = new Vector3(WALL_LENGTH, WALL_HEIGHT, WALL_WIDTH);
        }
    }
}