// Управляет поведением Гостя на 5-м уровне
using UnityEngine;

public class GuestController : MonoBehaviour
{
    private Vector2Int position; // Позиция Гостя
    private bool isActive = false; // Активен ли Гость
    [SerializeField] private SpriteRenderer spriteRenderer; // Спрайт Гостя

    private void Awake()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.gameObject.SetActive(false);
        }
    }

    public void SpawnGuest(Vector2Int spawnPos)
    {
        position = spawnPos;
        transform.position = new Vector3(position.x, position.y, 0);
        isActive = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.gameObject.SetActive(true);
        }
    }

    public void Interact()
    {
        if (isActive)
        {
            GameManager.Instance.SetHasMetGuest(true);
            GameManager.Instance.GetComponent<NarrativeManager>().ShowGuestDialogue();
            Deactivate();
        }
    }

    private void Deactivate()
    {
        isActive = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.gameObject.SetActive(false);
        }
    }

    public Vector2Int GetPosition() => position;
}
