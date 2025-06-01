// Управляет появлением красного креста на 9-м уровне, его мерцанием, гудением и свечением
using UnityEngine;
using System.Collections;

public class CrossSymbolController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer crossSprite; // Спрайт креста
    [SerializeField] private SpriteRenderer glowSprite; // Спрайт свечения
    [SerializeField] private AudioSource audioSource; // Источник звука
    [SerializeField] private AudioClip humSound; // Звук гудения
    private Vector2Int position; // Позиция креста в лабиринте
    private bool isActive = false; // Активен ли крест
    private float humDistance = 2f; // Дистанция для гудения (2 клетки)
    private float targetVolume = 0f; // Целевая громкость
    private Coroutine volumeFadeCoroutine; // Корутина для изменения громкости
    private bool hasShownFirstSoundThought = false; // Флаг для первой мысли о звуке

    private void Awake()
    {
        if (crossSprite != null)
        {
            crossSprite.gameObject.SetActive(false);
        }
        if (glowSprite != null)
        {
            glowSprite.gameObject.SetActive(false);
        }
        if (audioSource != null)
        {
            audioSource.clip = humSound;
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.Stop();
        }
    }

    // Инициализация креста
    public void SpawnCross(Vector2Int spawnPos)
    {
        position = spawnPos;
        transform.position = new Vector3(position.x, position.y, 0);
        isActive = true;
        if (crossSprite != null)
        {
            crossSprite.gameObject.SetActive(true);
        }
        if (glowSprite != null)
        {
            glowSprite.gameObject.SetActive(true);
        }
        StartCoroutine(FlickerAnimation()); // Запускаем мерцание
        StartCoroutine(GlowAnimation()); // Запускаем свечение
    }

    // Проверка, стоит ли игрок на кресте
    public bool IsPlayerOnCross(Vector2Int playerPos)
    {
        return isActive && playerPos == position;
    }

    // Деактивировать крест после реакции
    public void Deactivate()
    {
        isActive = false;
        if (crossSprite != null)
        {
            crossSprite.gameObject.SetActive(false);
        }
        if (glowSprite != null)
        {
            glowSprite.gameObject.SetActive(false);
        }
        if (audioSource != null)
        {
            targetVolume = 0f;
            audioSource.Stop();
        }
    }

    // Позиция креста
    public Vector2Int GetPosition() => position;

    // Анимация мерцания
    private IEnumerator FlickerAnimation()
    {
        while (isActive)
        {
            float flicker = 0.8f + 0.2f * Mathf.Sin(Time.time * Mathf.PI); // ±0.2, ~0.5 Гц
            Color spriteColor = crossSprite.color;
            spriteColor.a = flicker;
            crossSprite.color = spriteColor;
            yield return null;
        }
    }

    // Анимация свечения
    private IEnumerator GlowAnimation()
    {
        while (isActive)
        {
            float glow = 0.4f + 0.1f * Mathf.Sin(Time.time * 0.6f * Mathf.PI); // ±0.1, ~0.3 Гц
            Color glowColor = glowSprite.color;
            glowColor.a = glow;
            glowSprite.color = glowColor;
            yield return null;
        }
    }

    // Проверка расстояния до игрока и управление звуком
    private void Update()
    {
        if (!isActive || audioSource == null) return;

        Vector2Int playerPos = GameManager.Instance.GetComponent<PlayerController>().GetPosition();
        float distance = Vector2Int.Distance(playerPos, position);
        if (distance <= humDistance)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            // Громкость зависит от расстояния (максимум 0.2 на расстоянии 0, минимум 0 на humDistance)
            float newVolume = Mathf.Lerp(0f, 0.2f, 1f - (distance / humDistance)) * SettingsManager.Instance.GetWhisperVolume();
            if (newVolume != targetVolume)
            {
                targetVolume = newVolume;
                if (volumeFadeCoroutine != null)
                {
                    StopCoroutine(volumeFadeCoroutine);
                }
                volumeFadeCoroutine = StartCoroutine(FadeVolume(targetVolume));
            }
            // Первая мысль о звуке креста
            if (!hasShownFirstSoundThought)
            {
                GameManager.Instance.GetComponent<NarrativeManager>().ShowCrossSoundThought();
                hasShownFirstSoundThought = true;
                GameManager.Instance.GetComponent<PlayerController>().SetHasShownFirstCrossSoundThought(true);
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                targetVolume = 0f;
                if (volumeFadeCoroutine != null)
                {
                    StopCoroutine(volumeFadeCoroutine);
                }
                volumeFadeCoroutine = StartCoroutine(FadeVolume(targetVolume));
            }
        }
    }

    // Плавное изменение громкости
    private IEnumerator FadeVolume(float target)
    {
        float fadeDuration = 0.5f; // Быстрее, чем музыка, для отзывчивости
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, target, elapsedTime / fadeDuration);
            yield return null;
        }
        audioSource.volume = target;
        if (target == 0f)
        {
            audioSource.Stop();
        }
    }

    // Обновление громкости при изменении настроек
    public void UpdateVolume()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            float distance = Vector2Int.Distance(GameManager.Instance.GetComponent<PlayerController>().GetPosition(), position);
            if (distance <= humDistance)
            {
                targetVolume = Mathf.Lerp(0f, 0.2f, 1f - (distance / humDistance)) * SettingsManager.Instance.GetWhisperVolume();
                if (volumeFadeCoroutine != null)
                {
                    StopCoroutine(volumeFadeCoroutine);
                }
                volumeFadeCoroutine = StartCoroutine(FadeVolume(targetVolume));
            }
        }
    }
}
