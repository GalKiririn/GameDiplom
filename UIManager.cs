// Управляет пользовательским интерфейсом для отображения еды, дня, шагов и подсказки
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI foodText; // Текст для отображения количества еды
    [SerializeField] private UnityEngine.UI.Image foodIcon; // Иконка для еды
    [SerializeField] private TextMeshProUGUI dayText; // Текст для отображения номера дня
    [SerializeField] private UnityEngine.UI.Image dayIcon; // Иконка для дня
    [SerializeField] private TextMeshProUGUI stepsText; // Текст для отображения количества шагов
    [SerializeField] private UnityEngine.UI.Image stepsIcon; // Иконка для шагов
    [SerializeField] private TextMeshProUGUI hintText; // Текст подсказки
    private float pulseTimer = 0f; // Таймер для пульсации еды
    private float hintPulseTimer = 0f; // Таймер для пульсации подсказки
    private bool isPulsingFood = false; // Флаг пульсации еды
    private bool isPulsingHint = false; // Флаг пульсации подсказки

    private void Start()
    {
        // Устанавливаем текст подсказки
        hintText.text = "Найдите выход и нажмите E";
    }

    // Обновление текста и иконки с количеством еды
    public void UpdateFood(int food)
    {
        foodText.text = $" {food}";
        // Запускаем или останавливаем пульсацию
        isPulsingFood = food < 40;
    }

    // Обновление текста и иконки с номером дня
    public void UpdateDay(int day)
    {
        dayText.text = $" {day}";
    }

    // Обновление текста и иконки с количеством шагов
    public void UpdateSteps(int steps)
    {
        stepsText.text = $" {steps}";
    }

    // Обновление подсказки в зависимости от расстояния до выхода
    public void UpdateHint(Vector2Int playerPos, Vector2Int exitPos)
    {
        float distance = Vector2Int.Distance(playerPos, exitPos);
        isPulsingHint = distance <= 3f; // Пульсация, если игрок в радиусе 3 клеток
    }

    private void Update()
    {
        // Пульсация для еды
        if (isPulsingFood)
        {
            pulseTimer += Time.deltaTime;
            float scale = 1f + 0.1f * Mathf.Sin(pulseTimer * 3f); // Период ~2 раза в секунду
            foodText.transform.localScale = Vector3.one * scale;
            foodIcon.transform.localScale = Vector3.one * scale;
        }
        else
        {
            foodText.transform.localScale = Vector3.one;
            foodIcon.transform.localScale = Vector3.one;
        }

        // Пульсация для подсказки
        if (isPulsingHint)
        {
            hintPulseTimer += Time.deltaTime;
            float alpha = 0.5f + 0.5f * Mathf.Sin(hintPulseTimer * 2f); // Период ~1 раз в секунду
            Color color = hintText.color;
            color.a = alpha;
            hintText.color = color;
        }
        else
        {
            Color color = hintText.color;
            color.a = 1f;
            hintText.color = color;
        }
    }
}
