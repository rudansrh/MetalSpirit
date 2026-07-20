using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public static cameraFollow Instance { get; private set; }

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

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