// Управляет поведением врагов в лабиринте
using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    private List<Vector2Int> enemyPositions = new List<Vector2Int>(); // Позиции врагов
    [SerializeField] private GameObject enemyPrefab; // Префаб врага
    private List<GameObject> enemyObjects = new List<GameObject>(); // Объекты врагов

    public void SpawnEnemies(int day)
    {
        // Очищаем старых врагов
        foreach (GameObject enemy in enemyObjects)
        {
            Destroy(enemy);
        }
        enemyObjects.Clear();
        enemyPositions.Clear();

        // Количество врагов зависит от уровня
        int enemyCount = Mathf.Min(day, 5); // Максимум 5 врагов
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int pos = GameManager.Instance.GetComponent<MazeGenerator>().GetRandomWalkablePosition();
            while (pos == new Vector2Int(1, 1) || enemyPositions.Contains(pos)) // Избегаем начальной позиции и занятых клеток
            {
                pos = GameManager.Instance.GetComponent<MazeGenerator>().GetRandomWalkablePosition();
            }
            enemyPositions.Add(pos);
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform);
            enemyObjects.Add(enemy);
            enemy.name = $"Enemy_{i}";
        }
    }

    public void MoveEnemies()
    {
        Vector2Int playerPos = GameManager.Instance.GetComponent<PlayerController>().GetPosition();
        List<Vector2Int> newPositions = new List<Vector2Int>();

        for (int i = 0; i < enemyPositions.Count; i++)
        {
            Vector2Int enemyPos = enemyPositions[i];
            Vector2Int direction = Vector2Int.zero;

            // Простое поведение: двигаться к игроку
            int dx = playerPos.x - enemyPos.x;
            int dy = playerPos.y - enemyPos.y;

            if (Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                direction = new Vector2Int(dx > 0 ? 1 : -1, 0);
            }
            else
            {
                direction = new Vector2Int(0, dy > 0 ? 1 : -1);
            }

            Vector2Int newPos = enemyPos + direction;
            if (GameManager.Instance.GetComponent<MazeGenerator>().IsWalkable(newPos) && !newPositions.Contains(newPos) && !enemyPositions.Contains(newPos))
            {
                newPositions.Add(newPos);
                enemyObjects[i].transform.position = new Vector3(newPos.x, newPos.y, 0);
            }
            else
            {
                newPositions.Add(enemyPos); // Остается на месте
            }
        }

        enemyPositions = newPositions;
    }

    public bool IsEnemyAt(Vector2Int pos)
    {
        return enemyPositions.Contains(pos);
    }
}
