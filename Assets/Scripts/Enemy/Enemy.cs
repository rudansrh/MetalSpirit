using UnityEngine;

public abstract class Enemy : MonoBehaviour, IEnemyDamageReceiver
{
    [SerializeField] protected float facingDirection = -1f;

    public bool isPossessed = false;
    public GameObject nearbyEnemy;

    protected bool isDying = false;
    protected EnemyAnimationController animationController;

    protected void InitializeEnemyBase()
    {
        animationController = GetComponent<EnemyAnimationController>();
        facingDirection = transform.localScale.x < 0f ? 1f : -1f;
        UpdateFacingVisual();
    }

    public void SetPossessed(bool possessed)
    {
        if (isPossessed == possessed)
        {
            return;
        }

        isPossessed = possessed;
        animationController?.TriggerStun();
    }

    protected void UpdateFacingVisual()
    {
        Vector3 localScale = transform.localScale;
        float absX = Mathf.Abs(localScale.x);
        localScale.x = facingDirection > 0f ? -absX : absX;
        transform.localScale = localScale;
    }

    public abstract void Attacked(float playerDamage);
}
