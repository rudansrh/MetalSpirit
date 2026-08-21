using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IBreakable
{
    [SerializeField] MinigameManager minigame;
    public int maxHp = 1;
    public int currentHp = 1;
    public bool minigameStarted = false;
    [Header("Shake Effect")]
    [SerializeField] private float shakeDuration = 0.2f;  
    [SerializeField] private float shakeMagnitude = 0.1f; 
    private Coroutine shakeCoroutine;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void objectDamaged()
    {
        if (minigameStarted || !PlayerController.Instance.hasGloves) return;

        currentHp--;
        if (currentHp <= 0) 
        {
            minigame.MinigameReady();
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.BoxOpen); 
            gameObject.SetActive(false);
        }
        else
        {
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
