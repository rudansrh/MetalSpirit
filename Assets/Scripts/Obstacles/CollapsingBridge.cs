using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CollapsingBridge : MonoBehaviour
{
    [Header("Collapse Settings")]
    [SerializeField] private float fallDelay = 0.5f;        // 밟고 나서 떨어지기까지의 대기 시간
    [SerializeField] private float shakeMagnitude = 0.05f;  // 떨어지기 전 흔들리는 강도
    [SerializeField] private float destroyDelay = 3.0f;     // 추락한 뒤 씬에서 삭제되기까지의 시간

    private Rigidbody2D rb;
    private bool isCollapsing = false;
    private Vector3 originalPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        originalPosition = transform.localPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCollapsing && collision.gameObject.TryGetComponent<PlayerController>(out _))
        {
            if (collision.transform.position.y > transform.position.y)
            {
                StartCoroutine(CollapseRoutine());
            }
        }
    }

    private IEnumerator CollapseRoutine()
    {
        isCollapsing = true;
        float elapsed = 0f;

        while (elapsed < fallDelay)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;

        rb.bodyType = RigidbodyType2D.Dynamic;

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}