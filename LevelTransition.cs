// Управляет анимацией перехода на новый уровень с черным экраном и надписью
using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup transitionCanvasGroup; // UI-элемент для управления прозрачностью
    [SerializeField] private TextMeshProUGUI transitionText; // Текст для отображения "День X пройден"
    [SerializeField] private AudioClip levelCompleteSound; // Звук перехода на новый уровень
    private AudioSource audioSource; // Компонент для воспроизведения звука
    private bool isTransitioning = false; // Флаг, идет ли анимация

    private void Awake()
    {
        // Инициализация AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Показать анимацию перехода
    public void ShowTransition(int completedDay, Action onComplete)
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionCoroutine(completedDay, onComplete));
    }

    // Корутина для анимации
    private IEnumerator TransitionCoroutine(int completedDay, Action onComplete)
    {
        isTransitioning = true;

        // Устанавливаем текст
        transitionText.text = $"День {completedDay} пройден";

        // Воспроизводим звук
        audioSource.PlayOneShot(levelCompleteSound);

        // Показываем экран (fade-in)
        float elapsedTime = 0f;
        float fadeDuration = 1f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            transitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        transitionCanvasGroup.alpha = 1f;

        // Ждем 2 секунды
        yield return new WaitForSeconds(2f);

        // Скрываем экран (fade-out)
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            transitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        transitionCanvasGroup.alpha = 0f;

        // Вызываем callback для продолжения игры
        onComplete?.Invoke();
        isTransitioning = false;
    }
}