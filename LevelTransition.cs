// Управляет анимацией перехода между уровнями
using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI transitionText; // Текст перехода
    [SerializeField] private CanvasGroup canvasGroup; // Группа для управления прозрачностью

    public void ShowTransition(int day, Action onComplete)
    {
        StartCoroutine(TransitionAnimation(day, onComplete));
    }

    private IEnumerator TransitionAnimation(int day, Action onComplete)
    {
        transitionText.text = $"День {day} завершен";
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);

        // Fade-in
        float elapsedTime = 0f;
        float fadeDuration = 1f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        // Fade-out
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}
