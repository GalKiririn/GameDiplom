// AnimationBreath.cs
// Этот скрипт добавляет анимацию "дыхания" (масштабирование) к спрайтам.

using UnityEngine;

public class AnimationBreath : MonoBehaviour
{
    public float speed = 1f;         // Скорость анимации
    public float amplitude = 0.05f;  // Амплитуда изменения масштаба
    private Vector3 originalScale;   // Исходный масштаб объекта

    // Инициализация при запуске
    void Start()
    {
        originalScale = transform.localScale; // Сохраняем исходный масштаб
    }

    // Обновление каждый кадр
    void Update()
    {
        // Вычисляем изменение масштаба с помощью синусоиды
        float scale = amplitude * Mathf.Sin(Time.time * speed);
        // Применяем новый масштаб только по оси Y
        transform.localScale = originalScale + new Vector3(0, scale, 0);
    }
}
