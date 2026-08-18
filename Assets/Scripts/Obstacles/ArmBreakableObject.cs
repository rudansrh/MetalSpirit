using System.Collections;
using UnityEngine;

public class ArmBreakableObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int requiredHits = 3; // 부수기 위해 필요한 타격 횟수
    private int currentHits = 0;

    [Header("Shake Effect")]
    [SerializeField] private float shakeDuration = 0.2f;  // 흔들리는 시간
    [SerializeField] private float shakeMagnitude = 0.1f; // 흔들리는 강도(거리)

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void BreakObject()
    {
        currentHits++;

        if (currentHits >= requiredHits)
        {
            Destroy(gameObject);
        }
        else
        {

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalPosition;
            }
            shakeCoroutine = StartCoroutine(ShakeRoutine());
        }
    }
    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}