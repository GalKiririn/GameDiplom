// Управляет движением игрока и его взаимодействием с игрой
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2Int position = new Vector2Int(1, 1); // Начальная позиция
    [SerializeField] private MazeGenerator mazeGenerator; // Ссылка на лабиринт
    [SerializeField] private AudioClip stepSound; // Звук шага
    private AudioSource audioSource; // Компонент для воспроизведения звука

    private void Awake()
    {
        // Инициализация AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        // Обрабатываем ввод только в пошаговом режиме
        if (GameManager.Instance.GetFood() > 0)
        {
            Vector2Int newPos = position;
            if (Input.GetKeyDown(KeyCode.W)) newPos.y += 1;
            else if (Input.GetKeyDown(KeyCode.S)) newPos.y -= 1;
            else if (Input.GetKeyDown(KeyCode.A)) newPos.x -= 1;
            else if (Input.GetKeyDown(KeyCode.D)) newPos.x += 1;
            else if (Input.GetKeyDown(KeyCode.E)) // Завершение уровня
            {
                GameManager.Instance.CompleteLevel();
                return;
            }

            // Проверяем, можно ли двигаться
            if (newPos != position && mazeGenerator.IsWalkable(newPos))
            {
                position = newPos;
                transform.position = new Vector3(position.x, position.y, 0);
                audioSource.PlayOneShot(stepSound); // Воспроизводим звук шага
                GameManager.Instance.ProcessTurn();
                GameManager.Instance.CheckEnemyCollision(position);
                GameManager.Instance.AddSuccessScore(1); // Успех за ход
            }
        }
    }

    // Сброс позиции игрока
    public void ResetPosition()
    {
        position = new Vector2Int(1, 1);
        transform.position = new Vector3(position.x, position.y, 0);
    }

    // Получение текущей позиции
    public Vector2Int GetPosition() => position;
}