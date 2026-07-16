using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArenaController : MonoBehaviour
{
    [Header("Platform Cycle")]
    [SerializeField] float platformCycleInterval = 5f;
    [SerializeField] bool cyclePlatformsInPhase1;
    [SerializeField] bool cyclePlatformsInPhase2 = true;
    [SerializeField] List<GameObject> platformSetA = new List<GameObject>();
    [SerializeField] List<GameObject> platformSetB = new List<GameObject>();
    [SerializeField] List<GameObject> bonusPlatforms = new List<GameObject>();

    [Header("Phase 2 Debris")]
    [SerializeField] float debrisSpawnInterval = 2f;
    [SerializeField] float debrisWarningDuration = 1f;
    [SerializeField] Vector2 debrisImpactSize = new Vector2(1.5f, 1.5f);
    [SerializeField] float debrisDamage = 15f;
    [SerializeField] List<Transform> debrisSpawnPoints = new List<Transform>();
    [SerializeField] GameObject debrisWarningIndicatorPrefab;
    [SerializeField] GameObject debrisImpactEffectPrefab;

    [Header("Shake")]
    [SerializeField] Transform shakeTarget;
    [SerializeField] float shakeAmount = 0.15f;
    [SerializeField] float shakeTickInterval = 0.1f;

    BossController bossController;
    Coroutine platformCycleCoroutine;
    Coroutine debrisCoroutine;
    Coroutine shakeCoroutine;
    bool usingPlatformSetA = true;
    Vector3 originalShakeLocalPosition;

    void Awake()
    {
        if (shakeTarget == null && Camera.main != null)
        {
            shakeTarget = Camera.main.transform;
        }

        if (shakeTarget != null)
        {
            originalShakeLocalPosition = shakeTarget.localPosition;
        }
    }

    public void Begin(BossController controller)
    {
        StopArenaLoop();

        bossController = controller;
        bossController.OnPhaseChanged -= HandlePhaseChanged;
        bossController.OnPhaseChanged += HandlePhaseChanged;

        StartPlatformLoop();
        HandlePhaseChanged(bossController.CurrentPhase);
    }

    public void StopArenaLoop()
    {
        if (bossController != null)
        {
            bossController.OnPhaseChanged -= HandlePhaseChanged;
        }

        if (platformCycleCoroutine != null)
        {
            StopCoroutine(platformCycleCoroutine);
            platformCycleCoroutine = null;
        }

        if (debrisCoroutine != null)
        {
            StopCoroutine(debrisCoroutine);
            debrisCoroutine = null;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        ResetShakePosition();
    }

    void HandlePhaseChanged(BossPhase phase)
    {
        if (phase == BossPhase.Phase2)
        {
            if (debrisCoroutine == null)
            {
                debrisCoroutine = StartCoroutine(DebrisRoutine());
            }

            if (shakeCoroutine == null)
            {
                shakeCoroutine = StartCoroutine(ShakeRoutine());
            }
        }
        else
        {
            if (debrisCoroutine != null)
            {
                StopCoroutine(debrisCoroutine);
                debrisCoroutine = null;
            }

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }

            ResetShakePosition();
        }
    }

    void StartPlatformLoop()
    {
        ApplyPlatformState(usingPlatformSetA);
        platformCycleCoroutine = StartCoroutine(PlatformCycleRoutine());
    }

    IEnumerator PlatformCycleRoutine()
    {
        while (bossController != null && bossController.IsBattleActive && !bossController.IsDefeated)
        {
            bool shouldCycle =
                bossController.CurrentPhase == BossPhase.Phase1 ? cyclePlatformsInPhase1 : cyclePlatformsInPhase2;

            if (shouldCycle)
            {
                usingPlatformSetA = !usingPlatformSetA;
                ApplyPlatformState(usingPlatformSetA);
            }

            yield return new WaitForSeconds(platformCycleInterval);
        }
    }

    void ApplyPlatformState(bool enableSetA)
    {
        SetPlatformGroup(platformSetA, enableSetA);
        SetPlatformGroup(platformSetB, !enableSetA);
    }

    void SetPlatformGroup(List<GameObject> platforms, bool isActive)
    {
        for (int i = 0; i < platforms.Count; i++)
        {
            GameObject platform = platforms[i];
            if (platform != null)
            {
                platform.SetActive(isActive);
            }
        }
    }

    IEnumerator DebrisRoutine()
    {
        while (bossController != null && bossController.IsBattleActive && !bossController.IsDefeated)
        {
            if (bossController.CurrentPhase != BossPhase.Phase2 || debrisSpawnPoints.Count == 0)
            {
                yield return null;
                continue;
            }

            Transform spawnPoint = debrisSpawnPoints[Random.Range(0, debrisSpawnPoints.Count)];
            if (spawnPoint == null)
            {
                yield return new WaitForSeconds(debrisSpawnInterval);
                continue;
            }

            GameObject warningInstance = null;
            if (debrisWarningIndicatorPrefab != null)
            {
                warningInstance = Instantiate(debrisWarningIndicatorPrefab, spawnPoint.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(debrisWarningDuration);

            if (warningInstance != null)
            {
                Destroy(warningInstance);
            }

            bool playerHit = ResolveDebrisImpact(spawnPoint.position);
            if (!playerHit)
            {
                ActivateBonusPlatform();
            }

            yield return new WaitForSeconds(debrisSpawnInterval);
        }
    }

    bool ResolveDebrisImpact(Vector2 impactPosition)
    {
        if (debrisImpactEffectPrefab != null)
        {
            Destroy(Instantiate(debrisImpactEffectPrefab, impactPosition, Quaternion.identity), 1.5f);
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(impactPosition, debrisImpactSize, 0f);
        bool playerHit = false;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || !hit.CompareTag("Player"))
            {
                continue;
            }

            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(debrisDamage, DamageType.Normal);
                playerHit = true;
            }
        }

        return playerHit;
    }

    void ActivateBonusPlatform()
    {
        for (int i = 0; i < bonusPlatforms.Count; i++)
        {
            GameObject platform = bonusPlatforms[i];
            if (platform != null && !platform.activeSelf)
            {
                platform.SetActive(true);
                return;
            }
        }
    }

    IEnumerator ShakeRoutine()
    {
        if (shakeTarget == null)
        {
            yield break;
        }

        while (bossController != null && bossController.IsBattleActive && !bossController.IsDefeated)
        {
            if (bossController.CurrentPhase != BossPhase.Phase2)
            {
                yield return null;
                continue;
            }

            shakeTarget.localPosition = originalShakeLocalPosition +
                                        (Vector3)Random.insideUnitCircle * shakeAmount;
            yield return new WaitForSeconds(shakeTickInterval);
        }

        ResetShakePosition();
    }

    void ResetShakePosition()
    {
        if (shakeTarget != null)
        {
            shakeTarget.localPosition = originalShakeLocalPosition;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < debrisSpawnPoints.Count; i++)
        {
            Transform point = debrisSpawnPoints[i];
            if (point == null)
            {
                continue;
            }

            Gizmos.DrawWireCube(point.position, debrisImpactSize);
        }
    }
}
