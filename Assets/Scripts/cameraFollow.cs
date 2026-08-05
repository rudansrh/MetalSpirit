using System.Collections;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    private static cameraFollow instance;

    public static cameraFollow Instance => instance == null ? null : instance;

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 normalOffset = new Vector3(0, 0, -10);
    private Vector3 currentOffset;

    public bool followTarget = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            currentOffset = normalOffset;
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

        transform.position = target.position + currentOffset;
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
}
