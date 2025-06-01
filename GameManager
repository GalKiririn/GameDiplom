// Управляет общей логикой игры, уровнями, едой, состоянием игрока, Гостем, крестом, музыкой и сохранениями
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int food = 100; // Количество еды
    private int steps = 0; // Количество шагов на уровне
    private int currentDay = 1; // Текущий уровень (день)
    private int playerSuccessScore = 0; // Очки успешности игрока
    private bool isGameOver = false;
    private bool didNotTakeFoodOnLevel2 = true; // Флаг: не брал еду на 2-м уровне
    private bool hasMetGuest = false; // Флаг: встретил Гостя
    private bool hasSeenCross = false; // Флаг: видел крест
    private bool hasCross = false; // Флаг: подобрал крест
    private const int MAX_DAYS = 10; // Максимум 10 дней

    [SerializeField] private MazeGenerator mazeGenerator; // Ссылка на генератор лабиринта
    [SerializeField] private PlayerController playerController; // Ссылка на игрока
    [SerializeField] private EnemyController enemyController; // Ссылка на контроллер врагов
    [SerializeField] private FoodController foodController; // Ссылка на контроллер еды
    [SerializeField] private LevelTransition levelTransition; // Ссылка на анимацию перехода
    [SerializeField] private UIManager uiManager; // Ссылка на UI-менеджер
    [SerializeField] private NarrativeManager narrativeManager; // Ссылка на менеджер повествования
    [SerializeField] private GuestController guestController; // Ссылка на Гостя
    [SerializeField] private CrossSymbolController crossController; // Ссылка на крест
    [SerializeField] private AudioClip foodCollectSound; // Звук сбора еды
    [SerializeField] private AudioClip gameOverSound; // Звук поражения
    [SerializeField] private AudioClip crossHumSound; // Звук гудения креста
    [SerializeField] private AudioClip introMusic; // Музыка вступления
    [SerializeField] private AudioClip levelMusic; // Музыка обычных уровней
    [SerializeField] private AudioClip guestLevelMusic; // Музыка уровня с Гостем
    [SerializeField] private AudioClip finalMusic; // Музыка финала
    private AudioSource audioSource; // Компонент для звуков
    private AudioSource musicSource; // Компонент для музыки
    private Coroutine musicFadeCoroutine; // Корутина для перехода музыки

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = SettingsManager.Instance.GetMusicVolume();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("CurrentDay"))
        {
            uiManager.ShowMenu(LoadGame, StartNewGame);
        }
        else
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        food = 100;
        steps = 0;
        currentDay = 1;
        playerSuccessScore = 0;
        didNotTakeFoodOnLevel2 = true;
        hasMetGuest = false;
        hasSeenCross = false;
        hasCross = false;
        PlayerPrefs.DeleteAll();
        narrativeManager.ShowIntro(() => {
            PlayMusic(introMusic);
            StartNewDay();
        });
    }

    public void LoadGame()
    {
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        food = PlayerPrefs.GetInt("Food", 100);
        playerSuccessScore = PlayerPrefs.GetInt("SuccessScore", 0);
        didNotTakeFoodOnLevel2 = PlayerPrefs.GetInt("DidNotTakeFoodOnLevel2", 1) == 1;
        hasMetGuest = PlayerPrefs.GetInt("HasMetGuest", 0) == 1;
        hasSeenCross = PlayerPrefs.GetInt("HasSeenCross", 0) == 1;
        hasCross = PlayerPrefs.GetInt("HasCross", 0) == 1;
        steps = 0;
        if (hasCross)
        {
            playerController.SetCrossSprite();
        }
        StartNewDay();
    }

    public void StartNewDay()
    {
        steps = 0;
        int mazeDifficulty = CalculateMazeDifficulty();
        mazeGenerator.GenerateMaze(mazeDifficulty);
        playerController.ResetPosition();
        if (!(currentDay == 5 && didNotTakeFoodOnLevel2))
        {
            enemyController.SpawnEnemies(currentDay); // Пропускаем врагов на 5-м уровне с Гостем
        }
        foodController.SpawnFood(currentDay);
        if (currentDay == 5 && didNotTakeFoodOnLevel2)
        {
            Vector2Int guestPos = mazeGenerator.GetRandomWalkablePosition();
            guestController.SpawnGuest(guestPos);
            PlayMusic(guestLevelMusic); // Музыка для уровня с Гостем
        }
        else
        {
            PlayMusic(levelMusic); // Музыка для обычных уровней
        }
        if (currentDay == 9 && hasMetGuest && !hasSeenCross)
        {
            Vector2Int crossPos = mazeGenerator.GetRandomWalkablePosition();
            crossController.SpawnCross(crossPos);
        }
        currentDay++;
        Debug.Log($"День {currentDay} начался!");
        uiManager.UpdateDay(currentDay);
        uiManager.UpdateFood(food);
        uiManager.UpdateSteps(steps);
        uiManager.UpdateHint(playerController.GetPosition(), mazeGenerator.GetExitPosition());
    }

    private int CalculateMazeDifficulty()
    {
        if (playerSuccessScore > 10) return 15; // Сложный
        else if (playerSuccessScore > 5) return 10; // Средний
        else return 7; // Легкий
    }

    public void ProcessTurn()
    {
        if (isGameOver) return;
        steps++;
        Debug.Log($"Шагов сделано: {steps}");
        food -= 10;
        Debug.Log($"Еды осталось: {food}");
        foodController.CheckFoodCollection(playerController.GetPosition());
        if (food <= 0)
        {
            GameOver("Еда закончилась!");
        }
        else
        {
            enemyController.MoveEnemies();
        }
        uiManager.UpdateFood(food);
        uiManager.UpdateSteps(steps);
        uiManager.UpdateHint(playerController.GetPosition(), mazeGenerator.GetExitPosition());
        if (steps % 5 == 0)
        {
            narrativeManager.ShowRandomThought();
        }
    }

    public void CheckEnemyCollision(Vector2Int playerPos)
    {
        if (enemyController.IsEnemyAt(playerPos))
        {
            if (hasCross)
            {
                UseCross();
            }
            else
            {
                GameOver("Враг поймал вас!");
            }
        }
    }

    public void CompleteLevel()
    {
        if (isGameOver) return;
        if (mazeGenerator.IsExit(playerController.GetPosition()))
        {
            int successPoints = CalculateSuccessPoints(steps);
            AddSuccessScore(successPoints);
            Debug.Log($"Уровень завершен! Сделано шагов: {steps}, Очки успешности: {successPoints}");
            SaveGame();
            if (currentDay >= MAX_DAYS)
            {
                PlayMusic(finalMusic, false); // Финальная музыка, не зацикленная
                narrativeManager.ShowFinal(() => {
                    if (hasMetGuest)
                    {
                        narrativeManager.ShowGuestEnding();
                    }
                    else
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                });
            }
            else
            {
                levelTransition.ShowTransition(currentDay - 1, StartNewDay);
            }
        }
        else
        {
            Debug.Log("Нужно дойти до выхода, чтобы завершить уровень!");
        }
    }

    private int CalculateSuccessPoints(int steps)
    {
        if (steps <= 10) return 10;
        else if (steps <= 20) return 5;
        else return 2;
    }

    private void GameOver(string reason)
    {
        isGameOver = true;
        Debug.Log($"Игра окончена: {reason}");
        audioSource.PlayOneShot(gameOverSound);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddSuccessScore(int score)
    {
        playerSuccessScore += score;
        Debug.Log($"Очки успешности: {playerSuccessScore}");
    }

    public int GetFood() => food;

    public void AddFood(int amount)
    {
        food += amount;
        Debug.Log($"Еда собрана! Теперь еды: {food}");
        audioSource.PlayOneShot(foodCollectSound, SettingsManager.Instance.GetTalkVolume());
        uiManager.UpdateFood(food);
        if (currentDay == 2) didNotTakeFoodOnLevel2 = false; // Сбрасываем флаг на 2-м уровне
        narrativeManager.ShowHelpfulPhrase("Food");
    }

    private void SaveGame()
    {
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.SetInt("Food", food);
        PlayerPrefs.SetInt("SuccessScore", playerSuccessScore);
        PlayerPrefs.SetInt("DidNotTakeFoodOnLevel2", didNotTakeFoodOnLevel2 ? 1 : 0);
        PlayerPrefs.SetInt("HasMetGuest", hasMetGuest ? 1 : 0);
        PlayerPrefs.SetInt("HasSeenCross", hasSeenCross ? 1 : 0);
        PlayerPrefs.SetInt("HasCross", hasCross ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Игра сохранена!");
    }

    public void SetHasMetGuest(bool value)
    {
        hasMetGuest = value;
    }

    public bool HasMetGuest() => hasMetGuest;

    public void SetHasSeenCross(bool value)
    {
        hasSeenCross = value;
        SaveGame();
    }

    public bool HasSeenCross() => hasSeenCross;

    public void PickUpCross()
    {
        hasCross = true;
        hasSeenCross = true;
        SaveGame();
        playerController.SetCrossSprite();
        crossController.Deactivate();
        narrativeManager.ShowCrossThought();
    }

    public void UseCross()
    {
        hasCross = false;
        SaveGame();
        playerController.RemoveCrossSprite();
        narrativeManager.ShowCrossUsedThought();
    }

    public bool HasCross() => hasCross;

    public GuestController GetGuestController() => guestController;

    public CrossSymbolController GetCrossController() => crossController;

    public AudioClip GetCrossHumSound() => crossHumSound;

    public int GetCurrentDay() => currentDay;

    private void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return; // Не перезапускаем тот же трек
            }
            if (musicFadeCoroutine != null)
            {
                StopCoroutine(musicFadeCoroutine);
            }
            musicFadeCoroutine = StartCoroutine(FadeMusic(clip, loop));
        }
    }

    private IEnumerator FadeMusic(AudioClip clip, bool loop)
    {
        float fadeDuration = 1f;
        float startVolume = musicSource.volume;

        // Fade-out
        if (musicSource.isPlaying)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        // Смена трека
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        // Fade-in
        float targetVolume = SettingsManager.Instance.GetMusicVolume();
        float elapsedTimeFadeIn = 0f;
        while (elapsedTimeFadeIn < fadeDuration)
        {
            elapsedTimeFadeIn += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTimeFadeIn / fadeDuration);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    public void UpdateAllVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = SettingsManager.Instance.GetMusicVolume();
        }
        if (audioSource != null)
        {
            audioSource.volume = SettingsManager.Instance.GetTalkVolume();
        }
        if (crossController != null)
        {
            crossController.UpdateVolume();
        }
        if (uiManager != null)
        {
            uiManager.UpdateDialogueVolume();
        }
    }
}
