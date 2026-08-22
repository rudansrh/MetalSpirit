using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossArenaController : MonoBehaviour
{
    const string WallTag = "Wall";
    const string WallLayerName = "Wall";

    [Header("Platform Cycle")]
    [SerializeField] float platformCycleInterval = 5f;                          // 발판 생성/파괴 주기
    [SerializeField] bool cyclePlatformsInPhase1 = true;                        // 페이즈 1에서 발판 순환 여부
    [SerializeField] bool cyclePlatformsInPhase2 = true;                        // 페이즈 2에서 발판 순환 여부
    [SerializeField] bool hidePlatformsInPhase2 = true;                         // 페이즈 2에서 기본 발판 숨김 여부
    [SerializeField] List<GameObject> platformSetA = new List<GameObject>();    // 발판 세트 A
    [SerializeField] List<GameObject> platformSetB = new List<GameObject>();    // 발판 세트 B
    // [SerializeField] List<GameObject> bonusPlatforms = new List<GameObject>();

    [Header("Phase 2 Debris")]
    [SerializeField] GameObject debrisObjectPrefab;                             // 실제 떨어질 잔해 프리팹
    [SerializeField] float debrisSpawnInterval = 2f;                            // 잔해 주기
    [SerializeField] float debrisWarningDuration = 1f;                          // 잔해 경고 표시 시간
    [SerializeField] float debrisFallSpeed = 8f;                                // 잔해 낙하 속도
    [SerializeField] float debrisDamage = 15f;                                  // 잔해 피해량
    [SerializeField] Vector2 landedDebrisHeightRange = new Vector2(0.5f, 2f);   // 피했을 때 바닥 기준 추가 높이 범위
    [SerializeField] List<Transform> debrisSpawnPoints = new List<Transform>(); // 잔해 생성 지점 목록
    [SerializeField] GameObject debrisWarningIndicatorPrefab;                   // 잔해 경고 표시 프리팹
    [SerializeField] GameObject debrisImpactEffectPrefab;                       // 잔해 충돌 효과 프리팹

    [Header("Shake")]
    [SerializeField] bool allowShake = true;            // 흔들림 허용 여부
    [SerializeField] Transform shakeTarget;             // 흔들림 대상 (카메라)
    [SerializeField] float shakeAmount = 0.15f;         // 흔들림 강도
    [SerializeField] float shakeTickInterval = 0.1f;    // 흔들림 갱신 간격
    [SerializeField] Collider2D BossAreaSave;

    BossController bossController;                      // 보스 컨트롤러 참조
    Coroutine platformCycleCoroutine;                   // 발판 순환 코루틴
    Coroutine debrisCoroutine;                          // 잔해 생성 코루틴
    Coroutine shakeCoroutine;                           // 흔들림 코루틴
    bool usingPlatformSetA = true;                      // 현재 사용 중인 발판 세트 (A 또는 B)
    Vector3 originalShakeLocalPosition;                 // 흔들림 대상의 원래 로컬 위치
    readonly List<GameObject> spawnedArenaObjects = new List<GameObject>();

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

        BossAreaSave.enabled = false;
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

        ClearSpawnedArenaObjects();
        ResetShakePosition();
    }

    public void SetAllPlatformsActive(bool isActive)
    {
        SetPlatformGroup(platformSetA, isActive);
        SetPlatformGroup(platformSetB, isActive);
    }

    public void SetAllowShake(bool allow)
    {
        allowShake = allow;
        if (!allowShake)
        {
            ResetShakePosition();
        }
    }

    void HandlePhaseChanged(BossPhase phase)
    {
        if (phase == BossPhase.Phase2)
        {
            if (hidePlatformsInPhase2)
            {
                SetAllPlatformsActive(false);
            }

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

            ClearSpawnedArenaObjects();
            ResetShakePosition();
        }
    }

    void StartPlatformLoop()
    {
        if (bossController != null && bossController.CurrentPhase == BossPhase.Phase2 && hidePlatformsInPhase2)
        {
            SetAllPlatformsActive(false);
        }
        else
        {
            ApplyPlatformState(usingPlatformSetA);
        }

        platformCycleCoroutine = StartCoroutine(PlatformCycleRoutine());
    }

    IEnumerator PlatformCycleRoutine()
    {
        while (bossController != null && bossController.IsBattleActive && !bossController.IsDefeated)
        {
            if (bossController.CurrentPhase == BossPhase.Phase2 && hidePlatformsInPhase2)
            {
                SetAllPlatformsActive(false);
                yield return new WaitForSeconds(platformCycleInterval);
                continue;
            }

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
            if (bossController.CurrentPhase != BossPhase.Phase2 || debrisSpawnPoints.Count == 0 || debrisObjectPrefab == null)
            {
                yield return null;
                continue;
            }

            ClearSpawnedArenaObjects();

            Transform spawnPoint = debrisSpawnPoints[Random.Range(0, debrisSpawnPoints.Count)];
            if (spawnPoint == null)
            {
                yield return new WaitForSeconds(debrisSpawnInterval);
                continue;
            }

            if (!TryGetDebrisLandingData(spawnPoint.position, out Vector3 fallTargetPosition, out float floorY))
            {
                yield return new WaitForSeconds(debrisSpawnInterval);
                continue;
            }

            GameObject warningInstance = SpawnWarningIndicator(spawnPoint.position, fallTargetPosition);
            yield return new WaitForSeconds(debrisWarningDuration);

            if (warningInstance != null)
            {
                Destroy(warningInstance);
            }

            yield return RunDebrisFallRoutine(spawnPoint.position, fallTargetPosition, floorY);
            yield return new WaitForSeconds(debrisSpawnInterval);
        }
    }

    bool TryGetDebrisLandingData(Vector3 spawnPosition, out Vector3 fallTargetPosition, out float floorY)
    {
        fallTargetPosition = spawnPosition;
        floorY = spawnPosition.y;

        RaycastHit2D[] hits = Physics2D.RaycastAll(spawnPosition, Vector2.down, 50f);
        RaycastHit2D selectedHit = default;
        bool foundHit = false;
        int wallLayer = LayerMask.NameToLayer(WallLayerName);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            bool isWallLayer = wallLayer >= 0 && hitCollider.gameObject.layer == wallLayer;
            bool isWallTag = hitCollider.CompareTag(WallTag);
            if (!isWallLayer && !isWallTag)
            {
                continue;
            }

            selectedHit = hits[i];
            foundHit = true;
            break;
        }

        if (!foundHit)
        {
            return false;
        }

        float halfHeight = GetDebrisHalfHeight();
        floorY = selectedHit.point.y;
        fallTargetPosition = new Vector3(spawnPosition.x, floorY + halfHeight, spawnPosition.z);
        return true;
    }

    float GetDebrisHalfHeight()
    {
        if (debrisObjectPrefab == null)
        {
            return 0.5f;
        }

        if (debrisObjectPrefab.TryGetComponent<BoxCollider2D>(out var boxCollider))
        {
            return boxCollider.size.y * Mathf.Abs(debrisObjectPrefab.transform.localScale.y) * 0.5f;
        }

        if (debrisObjectPrefab.TryGetComponent<Collider2D>(out var collider2D))
        {
            return collider2D.bounds.extents.y;
        }

        return 0.5f;
    }

    GameObject SpawnWarningIndicator(Vector3 spawnPosition, Vector3 targetPosition)
    {
        if (debrisWarningIndicatorPrefab == null)
        {
            return null;
        }

        Vector3 indicatorPosition = (spawnPosition + targetPosition) * 0.5f;
        GameObject warningInstance = Instantiate(debrisWarningIndicatorPrefab, indicatorPosition, Quaternion.identity);
        RegisterSpawnedArenaObject(warningInstance);

        Vector3 warningScale = warningInstance.transform.localScale;
        warningScale.y = Mathf.Abs(spawnPosition.y - targetPosition.y);
        warningInstance.transform.localScale = warningScale;

        return warningInstance;
    }

    IEnumerator RunDebrisFallRoutine(Vector3 spawnPosition, Vector3 fallTargetPosition, float floorY)
    {
        GameObject fallingDebris = Instantiate(debrisObjectPrefab, spawnPosition, Quaternion.identity);
        RegisterSpawnedArenaObject(fallingDebris);

        BossDebrisObject debrisObject = fallingDebris.GetComponent<BossDebrisObject>();
        if (debrisObject == null)
        {
            debrisObject = fallingDebris.AddComponent<BossDebrisObject>();
        }

        debrisObject.Initialize(debrisDamage);

        while (fallingDebris != null)
        {
            if (debrisObject.WasResolved)
            {
                if (debrisObject.PlayerWasHit)
                {
                    SpawnDebrisImpactEffect(fallingDebris.transform.position);
                    UnregisterSpawnedArenaObject(fallingDebris);
                    Destroy(fallingDebris);
                    yield break;
                }

                break;
            }

            fallingDebris.transform.position = Vector3.MoveTowards(
                fallingDebris.transform.position,
                fallTargetPosition,
                debrisFallSpeed * Time.deltaTime);

            if (Vector3.Distance(fallingDebris.transform.position, fallTargetPosition) <= 0.01f)
            {
                break;
            }

            yield return null;
        }

        if (fallingDebris == null)
        {
            yield break;
        }

        UnregisterSpawnedArenaObject(fallingDebris);
        Destroy(fallingDebris);

        AudioManager.instance?.PlaySfx(AudioManager.Sfx.FloorDown); //***

        float halfHeight = GetDebrisHalfHeight();
        float randomHeight = Random.Range(landedDebrisHeightRange.x, landedDebrisHeightRange.y);
        Vector3 landedPosition = new Vector3(
            fallTargetPosition.x,
            floorY + halfHeight + randomHeight,
            fallTargetPosition.z);

        GameObject landedDebris = Instantiate(debrisObjectPrefab, landedPosition, Quaternion.identity);
        RegisterSpawnedArenaObject(landedDebris);
        // ActivateBonusPlatform();
        SpawnDebrisImpactEffect(fallTargetPosition);
    }

    void SpawnDebrisImpactEffect(Vector3 position)
    {
        if (debrisImpactEffectPrefab == null)
        {
            return;
        }

        GameObject impactEffect = Instantiate(debrisImpactEffectPrefab, position, Quaternion.identity);
        RegisterSpawnedArenaObject(impactEffect);
        Destroy(impactEffect, 1.5f);
    }

    void RegisterSpawnedArenaObject(GameObject spawnedObject)
    {
        if (spawnedObject == null)
        {
            return;
        }

        spawnedArenaObjects.Add(spawnedObject);
    }

    void UnregisterSpawnedArenaObject(GameObject spawnedObject)
    {
        if (spawnedObject == null)
        {
            return;
        }

        spawnedArenaObjects.Remove(spawnedObject);
    }

    void ClearSpawnedArenaObjects()
    {
        for (int i = spawnedArenaObjects.Count - 1; i >= 0; i--)
        {
            GameObject spawnedObject = spawnedArenaObjects[i];
            if (spawnedObject != null)
            {
                spawnedObject.SetActive(false);
                Destroy(spawnedObject);
            }
        }

        spawnedArenaObjects.Clear();
    }

    /* void ActivateBonusPlatform()
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
    } */

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

            Gizmos.DrawLine(point.position, point.position + Vector3.down * 6f);
        }
    }
}

public class BossDebrisObject : MonoBehaviour
{
    float damage;
    Collider2D debrisCollider;

    public bool WasResolved { get; private set; }
    public bool PlayerWasHit { get; private set; }

    public void Initialize(float debrisDamage)
    {
        damage = debrisDamage;

        if (debrisCollider == null)
        {
            debrisCollider = GetComponent<Collider2D>();
        }

        if (debrisCollider != null)
        {
            debrisCollider.isTrigger = true;
        }

        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D == null)
        {
            rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        }

        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.angularVelocity = 0f;

        WasResolved = false;
        PlayerWasHit = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (WasResolved)
        {
            return;
        }

        if (collision.TryGetComponent<PlayerController>(out var playerController))
        {
            if (playerController.isInvincibility)
            {
                return;
            }
        }

        if (collision.TryGetComponent<PlayerAbilityManager>(out var playerAbility))
        {
            if (playerAbility.isSoul)
            {
                return;
            }
        }

        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, DamageType.Normal);
            PlayerWasHit = true;
            WasResolved = true;
        }
    }
}
