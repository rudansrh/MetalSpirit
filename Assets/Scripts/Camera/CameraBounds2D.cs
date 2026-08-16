using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraBounds2D : MonoBehaviour
{
    private Collider2D boundsCollider;

    private void Awake()
    {
        boundsCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (cameraFollow.Instance == null)
        {
            return;
        }

        cameraFollow.Instance.SetBounds(boundsCollider);
    }

    private void Start()
    {
        if (cameraFollow.Instance == null)
        {
            return;
        }

        cameraFollow.Instance.SetBounds(boundsCollider);
    }

    private void OnDisable()
    {
        if (cameraFollow.Instance == null)
        {
            return;
        }

        cameraFollow.Instance.ClearBounds(boundsCollider);
    }
}
