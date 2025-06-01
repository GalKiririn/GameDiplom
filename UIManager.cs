// Управляет пользовательским интерфейсом для отображения еды, дня, шагов, подсказки, повествования, диалогов, меню, настроек и выбора концовки
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI foodText; // Текст для отображения количества еды
    [SerializeField] private Image foodIcon; // Иконка для еды
    [SerializeField] private TextMeshProUGUI dayText; // Текст для отображения номера дня
    [SerializeField] private Image dayIcon; // Иконка для дня
    [SerializeField] private TextMeshProUGUI stepsText; // Текст для отображения количества шагов
    [SerializeField] private Image stepsIcon; // Иконка для шагов
    [SerializeField] private TextMeshProUGUI hintText; // Текст подсказки
    [SerializeField] private TextMeshProUGUI narrativeText; // Текст для повествования
    [SerializeField] private Image narrativeBackground; // Фон для повествования
    [SerializeField] private TextMeshProUGUI dialogueText; // Текст для полезных диалогов
    [SerializeField] private Image dialogueBackground; // Фон с рамкой для диалогов
    [SerializeField] private GameObject menuPanel; // Панель главного меню
    [SerializeField] private Button newGameButton; // Кнопка "Новая игра"
    [SerializeField] private Button continueButton; // Кнопка "Продолжить"
    [SerializeField] private Button settingsButton; // Кнопка "Настройки"
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private Slider whisperVolumeSlider; // Ползунок громкости шепота
    [SerializeField] private Slider talkVolumeSlider; // Ползунок громкости разговора
    [SerializeField] private Slider musicVolumeSlider; // Ползунок громкости музыки
    [SerializeField] private Button saveSettingsButton; // Кнопка "Сохранить" в настройках
    [SerializeField] private Button agreeButton; // Кнопка "Согласиться" для концовки
    [SerializeField] private Button refuseButton; // Кнопка "Отказаться" для концовки
    [SerializeField] private AudioClip talkSound; // Звук разговора для диалогов
    private float pulseTimer = 0f; // Таймер для пульсации еды
    private float hintPulseTimer = 0f; // Таймер для пульсации подсказки
    private bool isPulsingFood = false; // Флаг пульсации еды
    private bool isPulsingHint = false; // Флаг пульсации подсказки
    private Action narrativeCallback; // Callback после завершения повествования
    private bool hasShownExitHint = false; // Флаг для подсказки о выходе
    private AudioSource audioSource; // Компонент для воспроизведения звука
    private float lastDialogueTime = 0f; // Время последнего диалога
    private const float dialogueCooldown = 0.5f; // Задержка между диалогами

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        hintText.text = "Найдите выход и нажмите E";
        narrativeText.gameObject.SetActive(false);
        narrativeBackground.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        dialogueBackground.gameObject.SetActive(false);
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (agreeButton != null) agreeButton.gameObject.SetActive(false);
        if (refuseButton != null) refuseButton.gameObject.SetActive(false);

        if (whisperVolumeSlider != null && talkVolumeSlider != null && musicVolumeSlider != null)
        {
            whisperVolumeSlider.value = SettingsManager.Instance.GetWhisperVolume();
            talkVolumeSlider.value = SettingsManager.Instance.GetTalkVolume();
            musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ShowSettings);
        }

        if (saveSettingsButton != null)
        {
            saveSettingsButton.onClick.AddListener(SaveSettings);
        }
    }

    public void UpdateFood(int food)
    {
        foodText.text = $" {food}";
        isPulsingFood = food < 40;
    }

    public void UpdateDay(int day)
    {
        dayText.text = $" {day}";
    }

    public void UpdateSteps(int steps)
    {
        stepsText.text = $" {steps}";
    }

    public void UpdateHint(Vector2Int playerPos, Vector2Int exitPos)
    {
        float distance = Vector2Int.Distance(playerPos, exitPos);
        isPulsingHint = distance <= 3f;
        if (isPulsingHint && !hasShownExitHint)
        {
            GameManager.Instance.GetComponent<NarrativeManager>().ShowHelpfulPhrase("Exit");
            hasShownExitHint = true;
        }
        else if (!isPulsingHint)
        {
            hasShownExitHint = false;
        }
    }

    public void ShowNarrative(string text, Action onComplete)
    {
        narrativeText.text = "";
        narrativeText.gameObject.SetActive(true);
        narrativeBackground.gameObject.SetActive(true);
        agreeButton.gameObject.SetActive(false);
        refuseButton.gameObject.SetActive(false);
        narrativeCallback = onComplete;
        StartCoroutine(DisplayNarrative(text));
    }

    public void ShowNarrativeWithChoice(string text, Action onAgree, Action onRefuse)
    {
        narrativeText.text = "";
        narrativeText.gameObject.SetActive(true);
        narrativeBackground.gameObject.SetActive(true);
        StartCoroutine(DisplayNarrativeWithChoice(text, onAgree, onRefuse));
    }

    public void ShowDialogue(string text)
    {
        if (Time.time - lastDialogueTime >= dialogueCooldown)
        {
            dialogueText.text = "";
            StartCoroutine(DisplayDialogue(text));
            audioSource.volume = SettingsManager.Instance.GetTalkVolume();
            audioSource.PlayOneShot(talkSound);
            lastDialogueTime = Time.time;
        }
    }

    public void ShowMenu(Action onContinue, Action onNewGame)
    {
        menuPanel.SetActive(true);
        newGameButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        newGameButton.onClick.AddListener(() => { menuPanel.SetActive(false); onNewGame(); });
        continueButton.onClick.AddListener(() => { menuPanel.SetActive(false); onContinue(); });
    }

    private void ShowSettings()
    {
        settingsPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    private void SaveSettings()
    {
        if (whisperVolumeSlider != null && talkVolumeSlider != null && musicVolumeSlider != null)
        {
            SettingsManager.Instance.SaveVolumes(whisperVolumeSlider.value, talkVolumeSlider.value, musicVolumeSlider.value);
            GameManager.Instance.UpdateAllVolumes();
        }
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void HideNarrative()
    {
        StartCoroutine(FadeOutNarrative());
    }

    private IEnumerator FadeInNarrative()
    {
        float elapsedTime = 0f;
        float fadeDuration = 1f;
        Color textColor = narrativeText.color;
        Color bgColor = narrativeBackground.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            textColor.a = alpha;
            bgColor.a = alpha;
            narrativeText.color = textColor;
            narrativeBackground.color = bgColor;
            yield return null;
        }
    }

    private IEnumerator FadeOutNarrative()
    {
        float elapsedTime = 0f;
        float fadeDuration = 1f;
        Color textColor = narrativeText.color;
        Color bgColor = narrativeBackground.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textColor.a = alpha;
            bgColor.a = alpha;
            narrativeText.color = textColor;
            narrativeBackground.color = bgColor;
            yield return null;
        }

        narrativeText.gameObject.SetActive(false);
        narrativeBackground.gameObject.SetActive(false);
        agreeButton.gameObject.SetActive(false);
        refuseButton.gameObject.SetActive(false);
        narrativeCallback?.Invoke();
    }

    private IEnumerator DisplayNarrative(string text)
    {
        narrativeText.gameObject.SetActive(true);
        narrativeBackground.gameObject.SetActive(true);
        Color textColor = narrativeText.color;
        Color bgColor = narrativeBackground.color;

        textColor.a = 0f;
        bgColor.a = 0f;
        narrativeText.color = textColor;
        narrativeBackground.color = bgColor;

        float elapsedTime = 0f;
        float fadeDuration = 1f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            textColor.a = alpha;
            bgColor.a = alpha;
            narrativeText.color = textColor;
            narrativeBackground.color = bgColor;
            yield return null;
        }

        narrativeText.text = "";
        float charDelay = 1f / text.Length;
        foreach (char c in text)
        {
            narrativeText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        StartCoroutine(FadeOutNarrative());
    }

    private IEnumerator DisplayNarrativeWithChoice(string text, Action onAgree, Action onRefuse)
    {
        narrativeText.gameObject.SetActive(true);
        narrativeBackground.gameObject.SetActive(true);
        Color textColor = narrativeText.color;
        Color bgColor = narrativeBackground.color;

        textColor.a = 0f;
        bgColor.a = 0f;
        narrativeText.color = textColor;
        narrativeBackground.color = bgColor;

        float elapsedTime = 0f;
        float fadeDuration = 1f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            textColor.a = alpha;
            bgColor.a = alpha;
            narrativeText.color = textColor;
            narrativeBackground.color = bgColor;
            yield return null;
        }

        narrativeText.text = "";
        float charDelay = 1f / text.Length;
        foreach (char c in text)
        {
            narrativeText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        agreeButton.gameObject.SetActive(true);
        refuseButton.gameObject.SetActive(true);
        agreeButton.onClick.RemoveAllListeners();
        refuseButton.onClick.RemoveAllListeners();
        agreeButton.onClick.AddListener(() => {
            agreeButton.gameObject.SetActive(false);
            refuseButton.gameObject.SetActive(false);
            onAgree();
        });
        refuseButton.onClick.AddListener(() => {
            agreeButton.gameObject.SetActive(false);
            refuseButton.gameObject.SetActive(false);
            onRefuse();
        });

        while (agreeButton.gameObject.activeSelf || refuseButton.gameObject.activeSelf)
        {
            yield return null;
        }
    }

    private IEnumerator DisplayDialogue(string text)
    {
        dialogueText.gameObject.SetActive(true);
        dialogueBackground.gameObject.SetActive(true);
        Color textColor = dialogueText.color;
        Color bgColor = dialogueBackground.color;

        textColor.a = 0f;
        bgColor.a = 0f;
        dialogueText.color = textColor;
        dialogueBackground.color = bgColor;

        float elapsedTime = 0f;
        float fadeDuration = 1f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            bgColor.a = Mathf.Lerp(0f, 0.8f, elapsedTime / fadeDuration);
            dialogueText.color = textColor;
            dialogueBackground.color = bgColor;
            yield return null;
        }

        dialogueText.text = "";
        float charDelay = 1f / text.Length;
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        yield return new WaitForSeconds(3f);

        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            bgColor.a = Mathf.Lerp(0.8f, 0f, elapsedTime / fadeDuration);
            dialogueText.color = textColor;
            dialogueBackground.color = bgColor;
            yield return null;
        }

        dialogueText.gameObject.SetActive(false);
        dialogueBackground.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPulsingFood)
        {
            pulseTimer += Time.deltaTime;
            float scale = 1f + 0.1f * Mathf.Sin(pulseTimer * 3f);
            foodText.transform.localScale = Vector3.one * scale;
            foodIcon.transform.localScale = Vector3.one * scale;
        }
        else
        {
            foodText.transform.localScale = Vector3.one;
            foodIcon.transform.localScale = Vector3.one;
        }

        if (isPulsingHint)
        {
            hintPulseTimer += Time.deltaTime;
            float alpha = 0.5f + 0.5f * Mathf.Sin(hintPulseTimer * 2f);
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

        if (narrativeText.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Space) && !agreeButton.gameObject.activeSelf && !refuseButton.gameObject.activeSelf)
        {
            HideNarrative();
        }
    }

    // Обновление громкости диалогов
    public void UpdateDialogueVolume()
    {
        if (audioSource != null)
        {
            audioSource.volume = SettingsManager.Instance.GetTalkVolume();
        }
    }
}
