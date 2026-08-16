using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerBounds2D : MonoBehaviour
{
    private Collider2D boundsCollider;

    private void Awake()
    {
        boundsCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        PlayerController.Instance.SetMovementBounds(boundsCollider);
    }

    private void Start()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        PlayerController.Instance.SetMovementBounds(boundsCollider);
    }

    private void OnDisable()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        PlayerController.Instance.ClearMovementBounds(boundsCollider);
    }
}
