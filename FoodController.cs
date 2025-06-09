// Управляет генерацией и сбором еды
using UnityEngine;
using System.Collections.Generic;

public class FoodController : MonoBehaviour
{
    [SerializeField] private GameObject foodPrefab; // Префаб еды
    [SerializeField] private MazeGenerator mazeGenerator; // Ссылка на лабиринт
    private List<Vector2Int> foodPositions = new List<Vector2Int>(); // Позиции еды

    // Создание еды на уровне
    public void SpawnFood(int day)
    {
        // Очищаем старую еду
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foodPositions.Clear();

        // Количество еды зависит от дня
        int foodCount = Mathf.Min(day + 2, 10); // Максимум 10 единиц еды
        Vector2Int mazeSize = mazeGenerator.GetMazeSize();

        for (int i = 0; i < foodCount; i++)
        {
            Vector2Int pos;
            do
            {
                pos = new Vector2Int(Random.Range(2, mazeSize.x - 1), Random.Range(2, mazeSize.y - 1));
            } while (!mazeGenerator.IsWalkable(pos) || pos == new Vector2Int(1, 1) || foodPositions.Contains(pos));

            foodPositions.Add(pos);
            Instantiate(foodPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform);
        }
    }

    // Проверка сбора еды
    public void CheckFoodCollection(Vector2Int playerPos)
    {
        if (foodPositions.Contains(playerPos))
        {
            foodPositions.Remove(playerPos);
            foreach (Transform child in transform)
            {
                if (Mathf.RoundToInt(child.position.x) == playerPos.x && Mathf.RoundToInt(child.position.y) == playerPos.y)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
            GameManager.Instance.AddFood(30); // Добавляем 30 еды
        }
    }
}
