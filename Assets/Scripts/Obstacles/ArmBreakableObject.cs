using System.Collections;
using UnityEngine;

public class ArmBreakableObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int requiredHits = 3; // ºÎ¼ö±â À§ÇØ ÇÊ¿äÇÑ Å¸°Ý È½¼ö
    private int currentHits = 0;

    [Header("Shake Effect")]
    [SerializeField] private float shakeDuration = 0.2f;  // Èçµé¸®´Â ½Ã°£
    [SerializeField] private float shakeMagnitude = 0.1f; // Èçµé¸®´Â °­µµ(°Å¸®)

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
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.BoxOpen); //***
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
