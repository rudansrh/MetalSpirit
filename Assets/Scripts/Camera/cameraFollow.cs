using System.Collections;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    private static cameraFollow instance;

    public static cameraFollow Instance => instance == null ? null : instance;

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 normalOffset = new Vector3(0, 0, -10);
    private Vector3 currentOffset;
    private Camera cachedCamera;
    private Collider2D boundsSource;
    private Bounds cameraBounds;
    private bool hasBounds;

    public bool followTarget = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            currentOffset = normalOffset;
            cachedCamera = GetComponent<Camera>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void LateUpdate()
    {
        if (target == null || !followTarget) return;

        Vector3 desiredPosition = target.position + currentOffset;
        transform.position = GetClampedPosition(desiredPosition);
    }

    // 카메라가 따라갈 대상을 변경
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // 카메라 오프셋 변경
    public void ChangeOffset(Vector3 offset)
    {
        StartCoroutine(offsetCorutine(offset));
    }

    public void SetBounds(Collider2D boundsCollider)
    {
        if (boundsCollider == null)
        {
            hasBounds = false;
            boundsSource = null;
            return;
        }

        boundsSource = boundsCollider;
        cameraBounds = boundsCollider.bounds;
        hasBounds = true;
    }

    public void ClearBounds(Collider2D boundsCollider)
    {
        if (boundsSource != boundsCollider)
        {
            return;
        }

        hasBounds = false;
        boundsSource = null;
    }

    public void ReturnToNormal()
    {
        StartCoroutine(offsetCorutine(normalOffset));
    }

    IEnumerator offsetCorutine(Vector3 offset)
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime;
            currentOffset = Vector3.Lerp(currentOffset, offset, Time.deltaTime);
            yield return null;
        }
    }

    private Vector3 GetClampedPosition(Vector3 desiredPosition)
    {
        if (!hasBounds || cachedCamera == null || !cachedCamera.orthographic)
        {
            return desiredPosition;
        }

        float halfHeight = cachedCamera.orthographicSize;
        float halfWidth = halfHeight * cachedCamera.aspect;

        float minX = cameraBounds.min.x + halfWidth;
        float maxX = cameraBounds.max.x - halfWidth;
        float minY = cameraBounds.min.y + halfHeight;
        float maxY = cameraBounds.max.y - halfHeight;

        if (minX > maxX)
        {
            desiredPosition.x = cameraBounds.center.x;
        }
        else
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        if (minY > maxY)
        {
            desiredPosition.y = cameraBounds.center.y;
        }
        else
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        return desiredPosition;
    }
}
