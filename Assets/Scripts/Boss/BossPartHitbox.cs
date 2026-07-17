using UnityEngine;

public class BossPartHitbox : MonoBehaviour
{
    [Header("Part Settings")]
    [SerializeField] BossWeakPointType weakPointType;   // 약점 포인트 유형
    [SerializeField] Collider2D hitboxCollider;         // 타격 범위 콜라이더

    [Header("Visual Feedback")]
    [SerializeField] SpriteRenderer targetRenderer;                     // 시각적 피드백을 위한 Sprite Renderer
    [SerializeField] Color activeColor = new Color(1f, 0.4f, 0.4f, 1f); // 활성화 상태 색상
    [SerializeField] Color inactiveColor = Color.white;                 // 비활성화 상태 색상
    [SerializeField] GameObject activeMarker;                           // 활성화 상태 표시기
    [SerializeField] GameObject inactiveMarker;                         // 비활성화 상태 표시기

    BossWeakPointManager weakPointManager;
    bool isWeakPointActive;

    public BossWeakPointType WeakPointType => weakPointType;
    public bool IsWeakPointActive => isWeakPointActive;

    void Reset()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider2D>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Awake()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider2D>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void Configure(BossWeakPointManager manager)
    {
        weakPointManager = manager;
    }

    public void SetWeakPointActive(bool isActive)
    {
        isWeakPointActive = isActive;

        if (targetRenderer != null)
        {
            targetRenderer.color = isActive ? activeColor : inactiveColor;
        }

        if (activeMarker != null)
        {
            activeMarker.SetActive(isActive);
        }

        if (inactiveMarker != null)
        {
            inactiveMarker.SetActive(!isActive);
        }
    }

    public bool ApplyDamage(float amount, BossController bossController)
    {
        if (!isWeakPointActive || bossController == null || amount <= 0f)
        {
            return false;
        }

        bossController.ApplyDamage(amount);
        return true;
    }

    void OnValidate()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider2D>();
        }
    }
}
