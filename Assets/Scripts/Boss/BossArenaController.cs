using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArenaController : MonoBehaviour
{
    [Header("Platform Cycle")]
    [SerializeField] float platformCycleInterval = 5f;                          // 발판 생성/파괴 주기
    [SerializeField] bool cyclePlatformsInPhase1;                               // 페이즈 1에서 발판 순환 여부
    [SerializeField] bool cyclePlatformsInPhase2 = true;                        // 페이즈 2에서 발판 순환 여부
    [SerializeField] List<GameObject> platformSetA = new List<GameObject>();    // 발판 세트 A
    [SerializeField] List<GameObject> platformSetB = new List<GameObject>();    // 발판 세트 B
    [SerializeField] List<GameObject> bonusPlatforms = new List<GameObject>();  // 보너스 발판 (페이즈 2에서만 활성화)

    [Header("Phase 2 Debris")]
    [SerializeField] float debrisSpawnInterval = 2f;                            // 잔해 떨어지는 주기
    [SerializeField] float debrisWarningDuration = 1f;                          // 잔해 경고 표시 시간
    [SerializeField] Vector2 debrisImpactSize = new Vector2(1.5f, 1.5f);        // 잔해 충돌 크기
    [SerializeField] float debrisDamage = 15f;                                  // 잔해 피해량
    [SerializeField] List<Transform> debrisSpawnPoints = new List<Transform>(); // 잔해 생성 지점 목록
    [SerializeField] GameObject debrisWarningIndicatorPrefab;                   // 잔해 경고 표시 프리팹
    [SerializeField] GameObject debrisImpactEffectPrefab;                       // 잔해 충돌 효과 프리팹

    [Header("Shake")]
    [SerializeField] bool allowShake = true;            // 흔들림 허용 여부
    [SerializeField] Transform shakeTarget;             // 흔들림 대상 (카메라)
    [SerializeField] float shakeAmount = 0.15f;         // 흔들림 강도
    [SerializeField] float shakeTickInterval = 0.1f;    // 흔들림 갱신 간격

    BossController bossController;      // 보스 컨트롤러 참조
    Coroutine platformCycleCoroutine;   // 발판 순환 코루틴
    Coroutine debrisCoroutine;          // 잔해 생성 코루틴
    Coroutine shakeCoroutine;           // 흔들림 코루틴
    bool usingPlatformSetA = true;      // 현재 사용 중인 발판 세트 (A 또는 B)
    Vector3 originalShakeLocalPosition; // 흔들림 대상의 원래 로컬 위치

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

    // 발판 순환 코루틴: 페이즈에 따라 발판 세트를 주기적으로 전환
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

    // 발판 상태 적용: 현재 활성화할 발판 세트를 설정하고, 나머지 세트는 비활성화
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

    // 잔해 생성 코루틴: 페이즈 2에서 주기적으로 잔해를 생성하고, 플레이어에게 피해를 줄 수 있음
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

    // 잔해 충돌 처리: 충돌 영역 내 플레이어에게 피해를 주고, 충돌 효과를 생성
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

    // 보너스 발판 활성화: 플레이어가 잔해 충돌을 피했을 때, 비활성화된 보너스 발판 중 하나를 활성화
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
        if (shakeTarget == null || allowShake == false)
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
