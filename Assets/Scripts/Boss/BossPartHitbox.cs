using UnityEngine;

public class BossPartHitbox : MonoBehaviour
{
    [Header("Part Settings")]
    [SerializeField] BossWeakPointType weakPointType;
    [SerializeField] Collider2D hitboxCollider;

    [Header("Visual Feedback")]
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] Color activeColor = new Color(1f, 0.4f, 0.4f, 1f);
    [SerializeField] Color inactiveColor = Color.white;
    [SerializeField] GameObject activeMarker;
    [SerializeField] GameObject inactiveMarker;

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
