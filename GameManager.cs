// Управляет общей логикой игры, уровнями, едой и состоянием игрока
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Синглтон для доступа из других классов
    public static GameManager Instance { get; private set; }

    // Параметры игрока
    private int food = 100; // Количество еды
    private int steps = 0; // Количество шагов на уровне
    private int currentDay = 0; // Текущий уровень (день)
    private int playerSuccessScore = 0; // Очки успешности игрока
    private bool isGameOver = false;

    [SerializeField] private MazeGenerator mazeGenerator; // Ссылка на генератор лабиринта
    [SerializeField] private PlayerController playerController; // Ссылка на игрока
    [SerializeField] private EnemyController enemyController; // Ссылка на контроллер врагов
    [SerializeField] private FoodController foodController; // Ссылка на контроллер еды
    [SerializeField] private LevelTransition levelTransition; // Ссылка на анимацию перехода
    [SerializeField] private UIManager uiManager; // Ссылка на UI-менеджер

    // Звуковые эффекты
    [SerializeField] private AudioClip foodCollectSound; // Звук сбора еды
    [SerializeField] private AudioClip gameOverSound; // Звук поражения
    private AudioSource audioSource; // Компонент для воспроизведения звуков

    private void Awake()
    {
        // Инициализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Инициализация AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        StartNewDay();
    }

    // Начать новый день (уровень)
    public void StartNewDay()
    {
        // Сбрасываем шаги
        steps = 0;

        // Генерируем лабиринт с учетом успешности игрока
        int mazeDifficulty = CalculateMazeDifficulty();
        mazeGenerator.GenerateMaze(mazeDifficulty);

        // Размещаем игрока, врагов и еду
        playerController.ResetPosition();
        enemyController.SpawnEnemies(currentDay);
        foodController.SpawnFood(currentDay);

        // Увеличиваем день
        currentDay++;
        Debug.Log($"День {currentDay} начался!");

        // Обновляем UI
        uiManager.UpdateDay(currentDay);
        uiManager.UpdateFood(food);
        uiManager.UpdateSteps(steps);
        uiManager.UpdateHint(playerController.GetPosition(), mazeGenerator.GetExitPosition());
    }

    // Рассчитать сложность лабиринта на основе успешности
    private int CalculateMazeDifficulty()
    {
        // Чем выше successScore, тем больше размер лабиринта
        if (playerSuccessScore > 10) return 15; // Сложный
        else if (playerSuccessScore > 5) return 10; // Средний
        else return 7; // Легкий
    }

    // Обработка хода игрока
    public void ProcessTurn()
    {
        if (isGameOver) return;

        // Увеличиваем шаги
        steps++;
	uiManager.UpdateSteps(steps);

        // Уменьшаем еду
        food -= 3;
	uiManager.UpdateFood(food);
        

        // Проверяем сбор еды
        foodController.CheckFoodCollection(playerController.GetPosition());

        // Проверяем условия проигрыша
        if (food <= 0)
        {
            GameOver("Еда закончилась!");
        }
        else
        {
            // Двигаем врагов
            enemyController.MoveEnemies();
        }

        // Обновляем UI
        uiManager.UpdateFood(food);
        uiManager.UpdateSteps(steps);
        uiManager.UpdateHint(playerController.GetPosition(), mazeGenerator.GetExitPosition());
    }

    // Проверка столкновения с врагом
    public void CheckEnemyCollision(Vector2Int playerPos)
    {
        if (enemyController.IsEnemyAt(playerPos))
        {
            	food -= 30;
        	Debug.Log($"Еда потеряна! Теперь еды: {food}");
        	uiManager.UpdateFood(food);
        }
    }

    // Завершение уровня
    public void CompleteLevel()
    {
        if (isGameOver) return;

        // Проверяем, стоит ли игрок на выходе
        if (mazeGenerator.IsExit(playerController.GetPosition()))
        {
            // Рассчитываем очки успешности на основе количества шагов
            int successPoints = CalculateSuccessPoints(steps);
            AddSuccessScore(successPoints);
            Debug.Log($"Уровень завершен! Сделано шагов: {steps}, Очки успешности: {successPoints}");

            // Показываем анимацию перехода
            levelTransition.ShowTransition(currentDay - 1, StartNewDay);
        }
        else
        {
            Debug.Log("Нужно дойти до выхода, чтобы завершить уровень!");
        }
    }

    // Рассчитать очки успешности на основе количества шагов
    private int CalculateSuccessPoints(int steps)
    {
        // Меньше шагов — больше очков
        if (steps <= 10) return 5; // Очень эффективно
        else if (steps <= 20) return 3; // Нормально
        else return 1; // Много шагов
    }

    // Завершение игры
    private void GameOver(string reason)
    {
        isGameOver = true;
        Debug.Log($"Игра окончена: {reason}");
        audioSource.PlayOneShot(gameOverSound);
        // Перезапуск сцены для новой игры
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Увеличение очков успешности
    public void AddSuccessScore(int score)
    {
        playerSuccessScore += score;
        Debug.Log($"Очки успешности: {playerSuccessScore}");
    }

    // Получение текущего количества еды
    public int GetFood() => food;

    // Добавление еды при сборе
    public void AddFood(int amount)
    {
        food += amount;
        Debug.Log($"Еда собрана! Теперь еды: {food}");
        audioSource.PlayOneShot(foodCollectSound);
        uiManager.UpdateFood(food);
    }
}
