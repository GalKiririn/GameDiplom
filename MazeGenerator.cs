// Генерирует лабиринт с использованием алгоритма Recursive Backtracking и выход на следующий уровень
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    // Размер лабиринта
    private int width, height;
    // Массив клеток: 0 - стена, 1 - проход
    private int[,] maze;
    // Префабы для стен, проходов и выхода
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject exitPrefab; // Префаб выхода
    private Vector2Int exitPosition; // Позиция выхода

    // Генерация лабиринта
    public void GenerateMaze(int size)
    {
        width = size;
        height = size;
        maze = new int[width, height];

        // Инициализируем стены
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0; // Все клетки - стены
            }
        }

        // Начинаем генерацию с клетки (1,1)
        GenerateMazeRecursive(1, 1);

        // Устанавливаем выход
        SetExit();

        // Отрисовываем лабиринт
        RenderMaze();
    }

    // Рекурсивный алгоритм генерации
    private void GenerateMazeRecursive(int x, int y)
    {
        maze[x, y] = 1; // Делаем клетку проходом

        // Возможные направления
        int[] directions = { 0, 1, 2, 3 };
        Shuffle(directions);

        foreach (int dir in directions)
        {
            int nx = x, ny = y;
            if (dir == 0) nx += 2; // Вправо
            else if (dir == 1) nx -= 2; // Влево
            else if (dir == 2) ny += 2; // Вверх
            else if (dir == 3) ny -= 2; // Вниз

            // Проверяем границы и непосещенные клетки
            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && maze[nx, ny] == 0)
            {
                // Делаем проход между клетками
                maze[x + (nx - x) / 2, y + (ny - y) / 2] = 1;
                GenerateMazeRecursive(nx, ny);
            }
        }
    }

    // Установка выхода в случайной проходимой клетке
    private void SetExit()
    {
        // Выбираем случайную клетку, которая является проходом
        do
        {
            exitPosition = new Vector2Int(Random.Range(2, width - 1), Random.Range(2, height - 1));
        } while (maze[exitPosition.x, exitPosition.y] != 1 || exitPosition == new Vector2Int(1, 1)); // Не в начальной позиции игрока
    }

    // Перемешивание массива направлений
    private void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    // Отрисовка лабиринта
    private void RenderMaze()
    {
        // Очищаем старый лабиринт
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Создаем стены, пол и выход
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                if (x == exitPosition.x && y == exitPosition.y)
                {
                    Instantiate(exitPrefab, pos, Quaternion.identity, transform);
                }
                else if (maze[x, y] == 0)
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }

    // Проверка, является ли клетка проходом
    public bool IsWalkable(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return false;
        return maze[pos.x, pos.y] == 1;
    }

    // Проверка, является ли клетка выходом
    public bool IsExit(Vector2Int pos)
    {
        return pos == exitPosition;
    }

    // Получение позиции выхода
    public Vector2Int GetExitPosition()
    {
        return exitPosition;
    }

    // Получение размера лабиринта
    public Vector2Int GetMazeSize() => new Vector2Int(width, height);
}