using UnityEngine;
using UnityEngine.Events;

public class Password : MonoBehaviour, IInteractable
{
    const float SoulBlockerSkin = 0.01f;

    [Header("Password Settings")]
    [SerializeField] string correctPassword = "";
    [SerializeField] string successMessage = "암호가 일치합니다.";
    [SerializeField] string failureMessage = "암호가 틀렸습니다.";
    [SerializeField] float reamainTime = 1.5f;
    [SerializeField] bool stayUnlockedAfterSuccess = true;
    [SerializeField] UnityEvent onPasswordMatched;

    private string purpose = "비밀번호 입력";
    public string Purpose => purpose;

    bool isUnlocked;
    Collider2D triggerCollider;
    Collider2D soulBlockingCollider;

    public int MaxInputLength => string.IsNullOrEmpty(correctPassword) ? 8 : correctPassword.Length;
    public string DefaultMessage => "암호를 입력하세요. (E / ESC 닫기)";
    public float ReamainTime => reamainTime;

    void Awake()
    {
        CacheColliders();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        CacheColliders();
    }
#endif

    public void Interact(GameObject interactor)
    {
        if (PlayerController.Instance.GetComponent<PlayerAbilityManager>().isSoul) return;
        if (stayUnlockedAfterSuccess && isUnlocked)
        {
            gameObject.SetActive(false);
            Debug.Log($"{name}: 이미 해제된 암호입니다.");
            return;
        }

        if (PasswordUIManager.Instance == null)
        {
            Debug.LogWarning("PasswordUIManager가 씬에 없습니다.");
            return;
        }

        PasswordUIManager.Instance.Open(this, interactor);
        PlayerController.Instance.isUIopen = true;
    }

    public bool Validate(string input, out string resultMessage)
    {
        if (input == correctPassword)
        {
            resultMessage = successMessage;
            Debug.Log($"{name}: {resultMessage}");

            if (stayUnlockedAfterSuccess)
            {
                isUnlocked = true;
            }

            onPasswordMatched?.Invoke();
            return true;
        }

        resultMessage = failureMessage;
        Debug.Log($"{name}: {resultMessage}");
        return false;
    }

    public void CompleteSuccessfulInteraction()
    {
        if (!stayUnlockedAfterSuccess)
        {
            return;
        }

        gameObject.SetActive(false);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!TryGetSoulPlayer(other, out Rigidbody2D playerRigidbody, out Collider2D playerCollider))
        {
            return;
        }

        ResolveSoulOverlap(playerRigidbody, playerCollider);
    }

    void CacheColliders()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        triggerCollider = null;
        soulBlockingCollider = null;

        foreach (Collider2D currentCollider in colliders)
        {
            if (currentCollider == null)
            {
                continue;
            }

            if (currentCollider.isTrigger)
            {
                triggerCollider ??= currentCollider;
                continue;
            }

            soulBlockingCollider ??= currentCollider;
        }

        if (soulBlockingCollider == null)
        {
            soulBlockingCollider = triggerCollider;
        }
    }

    bool TryGetSoulPlayer(Collider2D other, out Rigidbody2D playerRigidbody, out Collider2D playerCollider)
    {
        playerRigidbody = null;
        playerCollider = null;

        if (other == null)
        {
            return false;
        }

        PlayerAbilityManager abilityManager = other.GetComponent<PlayerAbilityManager>();
        if (abilityManager == null)
        {
            abilityManager = other.GetComponentInParent<PlayerAbilityManager>();
        }

        if (abilityManager == null || !abilityManager.isSoul)
        {
            return false;
        }

        playerRigidbody = other.attachedRigidbody;
        if (playerRigidbody == null)
        {
            playerRigidbody = abilityManager.GetComponent<Rigidbody2D>();
        }

        playerCollider = other;
        if (playerCollider == null && playerRigidbody != null)
        {
            playerCollider = playerRigidbody.GetComponent<Collider2D>();
        }

        return playerRigidbody != null && playerCollider != null;
    }

    void ResolveSoulOverlap(Rigidbody2D playerRigidbody, Collider2D playerCollider)
    {
        Collider2D blockerCollider = soulBlockingCollider != null ? soulBlockingCollider : triggerCollider;
        if (blockerCollider == null || playerRigidbody == null || playerCollider == null)
        {
            return;
        }

        Bounds blockerBounds = blockerCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;

        float overlapX = Mathf.Min(playerBounds.max.x, blockerBounds.max.x) - Mathf.Max(playerBounds.min.x, blockerBounds.min.x);
        float overlapY = Mathf.Min(playerBounds.max.y, blockerBounds.max.y) - Mathf.Max(playerBounds.min.y, blockerBounds.min.y);

        if (overlapX <= 0f || overlapY <= 0f)
        {
            return;
        }

        Vector2 push = Vector2.zero;
        Vector2 blockerCenter = blockerBounds.center;

        if (overlapX <= overlapY)
        {
            float directionX = Mathf.Sign(playerBounds.center.x - blockerCenter.x);
            if (Mathf.Approximately(directionX, 0f))
            {
                directionX = playerRigidbody.linearVelocity.x > 0f ? -1f : 1f;
            }

            push = new Vector2((overlapX + SoulBlockerSkin) * directionX, 0f);
        }
        else
        {
            float directionY = Mathf.Sign(playerBounds.center.y - blockerCenter.y);
            if (Mathf.Approximately(directionY, 0f))
            {
                directionY = playerRigidbody.linearVelocity.y > 0f ? -1f : 1f;
            }

            push = new Vector2(0f, (overlapY + SoulBlockerSkin) * directionY);
        }

        playerRigidbody.position += push;

        Vector2 adjustedVelocity = playerRigidbody.linearVelocity;
        if (!Mathf.Approximately(push.x, 0f) && adjustedVelocity.x * push.x < 0f)
        {
            adjustedVelocity.x = 0f;
        }

        if (!Mathf.Approximately(push.y, 0f) && adjustedVelocity.y * push.y < 0f)
        {
            adjustedVelocity.y = 0f;
        }

        playerRigidbody.linearVelocity = adjustedVelocity;
    }

    [ContextMenu("Reset Unlock State")]
    public void ResetUnlockState()
    {
        isUnlocked = false;
    }
}
