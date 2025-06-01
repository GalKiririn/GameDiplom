// Генерирует лабиринт с использованием алгоритма Recursive Backtracking
using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    private int width = 7; // Ширина лабиринта (по умолчанию 7)
    private int height = 7; // Высота лабиринта (по умолчанию 7)
    private bool[,] maze; // Массив клеток (true - стена, false - проход)
    private Vector2Int playerPosition; // Позиция игрока
    private Vector2Int exitPosition; // Позиция выхода
    [SerializeField] private GameObject wallPrefab; // Префаб стены
    [SerializeField] private GameObject floorPrefab; // Префаб пола
    [SerializeField] private GameObject exitPrefab; // Префаб выхода

    public void GenerateMaze(int difficulty)
    {
        width = difficulty;
        height = difficulty;
        maze = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = true; // Изначально все клетки - стены
            }
        }

        // Начинаем с клетки (1,1)
        GeneratePath(1, 1);

        // Устанавливаем выход в случайной проходимой клетке
        exitPosition = GetRandomWalkablePosition();
        while (exitPosition == new Vector2Int(1, 1)) // Избегаем начальной позиции
        {
            exitPosition = GetRandomWalkablePosition();
        }

        // Создаем визуальное представление лабиринта
        RenderMaze();
    }

    private void GeneratePath(int x, int y)
    {
        maze[x, y] = false; // Отмечаем клетку как проход

        // Список направлений в случайном порядке
        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up * 2, Vector2Int.right * 2, Vector2Int.down * 2, Vector2Int.left * 2
        };
        Shuffle(directions);

        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            // Проверяем, находится ли новая клетка в пределах лабиринта и является ли стеной
            if (newX >= 0 && newX < width && newY >= 0 && newY < height && maze[newX, newY])
            {
                // Прокладываем проход между текущей клеткой и новой
                maze[x + dir.x / 2, y + dir.y / 2] = false;
                GeneratePath(newX, newY);
            }
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void RenderMaze()
    {
        // Очищаем старый лабиринт
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Создаем клетки
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefab = maze[x, y] ? wallPrefab : floorPrefab;
                if (!maze[x, y] && new Vector2Int(x, y) == exitPosition)
                {
                    prefab = exitPrefab;
                }
                GameObject tile = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";
            }
        }
    }

    public bool IsWalkable(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height && !maze[pos.x, pos.y];
    }

    public Vector2Int GetRandomWalkablePosition()
    {
        List<Vector2Int> walkablePositions = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!maze[x, y])
                {
                    walkablePositions.Add(new Vector2Int(x, y));
                }
            }
        }
        return walkablePositions[Random.Range(0, walkablePositions.Count)];
    }

    public void SetPlayerPosition(Vector2Int pos)
    {
        playerPosition = pos;
    }

    public Vector2Int GetExitPosition() => exitPosition;

    public bool IsExit(Vector2Int pos) => pos == exitPosition;
}
