// Управляет поведением и движением врагов
using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab; // Префаб врага
    [SerializeField] private MazeGenerator mazeGenerator; // Ссылка на лабиринт
    private List<Vector2Int> enemyPositions = new List<Vector2Int>(); // Позиции врагов

    // Создание врагов на уровне
    public void SpawnEnemies(int day)
    {
        // Очищаем старых врагов
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        enemyPositions.Clear();

        // Количество врагов зависит от дня
        int enemyCount = Mathf.Min(day, 4); // Максимум 4 врагов
        Vector2Int mazeSize = mazeGenerator.GetMazeSize();

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int pos;
            do
            {
                pos = new Vector2Int(Random.Range(2, mazeSize.x - 1), Random.Range(2, mazeSize.y - 1));
            } while (!mazeGenerator.IsWalkable(pos) || pos == new Vector2Int(1, 1) || enemyPositions.Contains(pos));

            enemyPositions.Add(pos);
            Instantiate(enemyPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform);
        }
    }

    // Движение врагов к игроку
    public void MoveEnemies()
    {
        Vector2Int playerPos = GameManager.Instance.GetComponent<PlayerController>().GetPosition();
        List<Vector2Int> newPositions = new List<Vector2Int>();

        foreach (Vector2Int enemyPos in enemyPositions)
        {
            Vector2Int newPos = enemyPos;
            int dx = playerPos.x - enemyPos.x;
            int dy = playerPos.y - enemyPos.y;

            // Двигаемся по большей разнице
            if (Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                newPos.x += dx > 0 ? 1 : -1;
            }
            else
            {
                newPos.y += dy > 0 ? 1 : -1;
            }

            // Проверяем, можно ли двигаться
            if (mazeGenerator.IsWalkable(newPos) && !newPositions.Contains(newPos))
            {
                newPositions.Add(newPos);
            }
            else
            {
                newPositions.Add(enemyPos); // Остаемся на месте
            }
        }

        // Обновляем позиции врагов
        enemyPositions = newPositions;
        int i = 0;
        foreach (Transform child in transform)
        {
            child.position = new Vector3(enemyPositions[i].x, enemyPositions[i].y, 0);
            i++;
        }
    }

    // Проверка, есть ли враг в позиции
    public bool IsEnemyAt(Vector2Int pos)
    {
        return enemyPositions.Contains(pos);
    }
}
