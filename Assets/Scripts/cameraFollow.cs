using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    private static cameraFollow instance;

    public static cameraFollow Instance => instance == null ? null : instance;

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
        if (target == null) return;

        transform.position = target.position + offset;
    }

    /// 카메라가 따라갈 대상을 변경
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
