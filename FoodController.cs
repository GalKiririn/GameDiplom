// Управляет появлением и сбором еды в лабиринте
using UnityEngine;
using System.Collections.Generic;

public class FoodController : MonoBehaviour
{
    private List<Vector2Int> foodPositions = new List<Vector2Int>(); // Позиции еды
    [SerializeField] private GameObject foodPrefab; // Префаб еды
    private List<GameObject> foodObjects = new List<GameObject>(); // Объекты еды

    public void SpawnFood(int day)
    {
        // Очищаем старую еду
        foreach (GameObject food in foodObjects)
        {
            Destroy(food);
        }
        foodObjects.Clear();
        foodPositions.Clear();

        // Количество еды зависит от уровня
        int foodCount = Mathf.Max(1, 3 - day / 3); // Меньше еды на высоких уровнях
        for (int i = 0; i < foodCount; i++)
        {
            Vector2Int pos = GameManager.Instance.GetComponent<MazeGenerator>().GetRandomWalkablePosition();
            while (pos == new Vector2Int(1, 1) || foodPositions.Contains(pos) || pos == GameManager.Instance.GetComponent<MazeGenerator>().GetExitPosition())
            {
                pos = GameManager.Instance.GetComponent<MazeGenerator>().GetRandomWalkablePosition();
            }
            foodPositions.Add(pos);
            GameObject food = Instantiate(foodPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, transform);
            foodObjects.Add(food);
            food.name = $"Food_{i}";
        }
    }

    public void CheckFoodCollection(Vector2Int playerPos)
    {
        for (int i = foodPositions.Count - 1; i >= 0; i--)
        {
            if (playerPos == foodPositions[i])
            {
                GameManager.Instance.AddFood(50); // Добавляем 50 еды
                Destroy(foodObjects[i]);
                foodObjects.RemoveAt(i);
                foodPositions.RemoveAt(i);
            }
        }
    }
}
