// Управляет движением игрока, взаимодействием с едой, выходом, Гостем, крестом и мыслями
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Vector2Int position; // Текущая позиция игрока
    private bool canMove = true; // Может ли игрок двигаться
    [SerializeField] private SpriteRenderer spriteRenderer; // Спрайт игрока
    [SerializeField] private Sprite playerSprite; // Обычный спрайт игрока
    [SerializeField] private Sprite playerWithCrossSprite; // Спрайт игрока с крестом
    [SerializeField] private TextMeshPro thoughtText; // Текст для мыслей игрока
    [SerializeField] private AudioClip footstepSound; // Звук шагов
    private AudioSource audioSource; // Источник звука
    private Coroutine thoughtCoroutine; // Корутина для отображения мыслей
    private float thoughtDuration = 2f; // Длительность отображения мыслей
    private float moveCooldown = 0.3f; // Задержка между ходами
    private Vector2Int lastMoveDirection = Vector2Int.zero; // Последнее направление движения
    private bool hasShownFirstCrossSoundThought = false; // Флаг для первой мысли о звуке креста

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = footstepSound;
        thoughtText.gameObject.SetActive(false);
    }

    private void Start()
    {
        position = new Vector2Int(1, 1); // Начальная позиция
        transform.position = new Vector3(position.x, position.y, 0);
        GameManager.Instance.GetComponent<MazeGenerator>().SetPlayerPosition(position);
    }

    private void Update()
    {
        if (!canMove) return;

        Vector2Int moveDirection = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            moveDirection = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveDirection = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveDirection = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveDirection = Vector2Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.Instance.CompleteLevel();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            // Подбор креста
            if (GameManager.Instance.GetCrossController().IsPlayerOnCross(position))
            {
                GameManager.Instance.PickUpCross();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            // Взаимодействие с Гостем
            Vector2Int guestPos = GameManager.Instance.GetGuestController().GetPosition();
            if (IsAdjacentToGuest(position, guestPos))
            {
                GameManager.Instance.GetGuestController().Interact();
            }
        }

        if (moveDirection != Vector2Int.zero)
        {
            Vector2Int newPosition = position + moveDirection;
            if (GameManager.Instance.GetComponent<MazeGenerator>().IsWalkable(newPosition))
            {
                StartCoroutine(Move(newPosition, moveDirection));
            }
        }
    }

    private bool IsAdjacentToGuest(Vector2Int playerPos, Vector2Int guestPos)
    {
        return Mathf.Abs(playerPos.x - guestPos.x) + Mathf.Abs(playerPos.y - guestPos.y) == 1;
    }

    private IEnumerator Move(Vector2Int newPosition, Vector2Int direction)
    {
        canMove = false;
        position = newPosition;
        lastMoveDirection = direction;

        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(position.x, position.y, 0);
        float moveDuration = moveCooldown;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            yield return null;
        }

        transform.position = targetPos;
        GameManager.Instance.GetComponent<MazeGenerator>().SetPlayerPosition(position);
        GameManager.Instance.CheckEnemyCollision(position);
        GameManager.Instance.ProcessTurn();

        if (audioSource != null && footstepSound != null)
        {
            audioSource.PlayOneShot(footstepSound, 0.3f * SettingsManager.Instance.GetTalkVolume());
        }

        yield return new WaitForSeconds(moveCooldown / 2);
        canMove = true;
    }

    public Vector2Int GetPosition() => position;

    public void ResetPosition()
    {
        position = new Vector2Int(1, 1);
        transform.position = new Vector3(position.x, position.y, 0);
        GameManager.Instance.GetComponent<MazeGenerator>().SetPlayerPosition(position);
        canMove = true;
    }

    public void ShowThought(string thought)
    {
        if (thoughtCoroutine != null)
        {
            StopCoroutine(thoughtCoroutine);
        }
        thoughtCoroutine = StartCoroutine(DisplayThought(thought));
    }

    private IEnumerator DisplayThought(string thought)
    {
        thoughtText.text = thought;
        thoughtText.gameObject.SetActive(true);

        Vector3 startPos = thoughtText.transform.localPosition;
        Vector3 targetPos = startPos + new Vector3(0, 0.5f, 0);
        float elapsedTime = 0f;
        float moveDuration = 0.5f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            thoughtText.transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            yield return null;
        }

        yield return new WaitForSeconds(thoughtDuration);

        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            thoughtText.transform.localPosition = Vector3.Lerp(targetPos, startPos, elapsedTime / moveDuration);
            yield return null;
        }

        thoughtText.gameObject.SetActive(false);
        thoughtText.transform.localPosition = startPos;
    }

    public void SetCrossSprite()
    {
        if (spriteRenderer != null && playerWithCrossSprite != null)
        {
            spriteRenderer.sprite = playerWithCrossSprite;
        }
    }

    public void RemoveCrossSprite()
    {
        if (spriteRenderer != null && playerSprite != null)
        {
            spriteRenderer.sprite = playerSprite;
        }
    }

    public void SetHasShownFirstCrossSoundThought(bool value)
    {
        hasShownFirstCrossSoundThought = value;
    }

    public bool HasShownFirstCrossSoundThought() => hasShownFirstCrossSoundThought;
}
