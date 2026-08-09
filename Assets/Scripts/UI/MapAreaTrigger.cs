using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MapAreaTrigger : MonoBehaviour
{
    [SerializeField] private Sprite mapSprite;

    public Sprite MapSprite => mapSprite;

    private void Reset()
    {
        Collider2D areaCollider = GetComponent<Collider2D>();
        if (areaCollider != null)
        {
            areaCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController>(out _))
        {
            return;
        }

        MapUIManager.Instance?.SetCurrentArea(this);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController>(out _))
        {
            return;
        }

        MapUIManager.Instance?.SetCurrentArea(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController>(out _))
        {
            return;
        }

        MapUIManager.Instance?.ClearCurrentArea(this);
    }
}
