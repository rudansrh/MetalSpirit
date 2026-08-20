using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossWeakPointManager : MonoBehaviour
{
    [Header("Weak Point Settings")]
    [SerializeField] float weakPointOpenDuration = 5f;                                      // 약점 포인트 활성화 시간
    [SerializeField] List<BossPartHitbox> weakPointHitboxes = new List<BossPartHitbox>();   // 약점 포인트 Hitbox 목록

    [Header("Light Image")]
    [SerializeField] GameObject[] weakPointImage; // 약점 포인트 UI 이미지 (L U R D)

    BossController bossController;      // 보스 컨트롤러 참조
    BossPartHitbox currentWeakPoint;    // 현재 활성화된 약점 포인트
    Coroutine weakPointCycleCoroutine;  // 약점 포인트 순환 코루틴
    float remainingOpenTime;            // 현재 활성화된 약점 포인트의 남은 시간

    public BossWeakPointType CurrentWeakPointType =>    // 현재 활성화된 약점 포인트 유형 반환
        currentWeakPoint != null ? currentWeakPoint.WeakPointType : BossWeakPointType.LeftArm;

    void Reset()
    {
        CacheWeakPoints();
    }

    void Awake()
    {
        CacheWeakPoints();
        BindHitboxes();
    }

    void Start()
    {
        if (currentWeakPoint == null || weakPointImage == null) return;

        for (int i = 0; i < weakPointImage.Length; i++)
        {
            if (weakPointImage[i] == null) continue;

            weakPointImage[i].SetActive(false);
        }
    }

    void Update()
    {
        remainingOpenTime = Mathf.Max(0f, remainingOpenTime - Time.deltaTime);
    }

    void CacheWeakPoints()
    {
        if (weakPointHitboxes.Count > 0)
        {
            return;
        }

        weakPointHitboxes.AddRange(GetComponentsInChildren<BossPartHitbox>(true));
    }

    void BindHitboxes()
    {
        for (int i = 0; i < weakPointHitboxes.Count; i++)
        {
            BossPartHitbox hitbox = weakPointHitboxes[i];
            if (hitbox == null)
            {
                continue;
            }

            hitbox.Configure(this);
            hitbox.SetWeakPointActive(false);
        }
    }

    public void Begin(BossController controller)
    {
        bossController = controller;
        BindHitboxes();
        StartCycle();
    }

    public void StopCycle()
    {
        if (weakPointCycleCoroutine != null)
        {
            StopCoroutine(weakPointCycleCoroutine);
            weakPointCycleCoroutine = null;
        }

        SetCurrentWeakPoint(null);
    }

    public bool IsWeakPointActive(BossWeakPointType type)
    {
        return currentWeakPoint != null && currentWeakPoint.WeakPointType == type;
    }

    public void ForceSetWeakPoint(BossWeakPointType type)
    {
        for (int i = 0; i < weakPointHitboxes.Count; i++)
        {
            BossPartHitbox hitbox = weakPointHitboxes[i];
            if (hitbox != null && hitbox.WeakPointType == type)
            {
                SetCurrentWeakPoint(hitbox);
                remainingOpenTime = weakPointOpenDuration;
                return;
            }
        }
    }

    void StartCycle()
    {
        StopCycle();

        if (weakPointHitboxes.Count == 0) return;

        weakPointCycleCoroutine = StartCoroutine(WeakPointCycleRoutine());
    }

    IEnumerator WeakPointCycleRoutine()
    {
        while (bossController != null && bossController.IsBattleActive && !bossController.IsDefeated)
        {
            BossPartHitbox nextWeakPoint = GetRandomWeakPoint();
            SetCurrentWeakPoint(nextWeakPoint);
            remainingOpenTime = weakPointOpenDuration;

            float elapsed = 0f;
            while (elapsed < weakPointOpenDuration)
            {
                if (bossController == null || !bossController.IsBattleActive || bossController.IsDefeated)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    BossPartHitbox GetRandomWeakPoint()
    {
        List<BossPartHitbox> candidates = new List<BossPartHitbox>();

        for (int i = 0; i < weakPointHitboxes.Count; i++)
        {
            BossPartHitbox hitbox = weakPointHitboxes[i];
            if (hitbox == null)
            {
                continue;
            }

            if (weakPointHitboxes.Count > 1 && hitbox == currentWeakPoint)
            {
                continue;
            }

            candidates.Add(hitbox);
        }

        if (candidates.Count == 0)
        {
            return currentWeakPoint;
        }

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    void SetCurrentWeakPoint(BossPartHitbox nextWeakPoint)
    {
        currentWeakPoint = nextWeakPoint;

        for (int i = 0; i < weakPointHitboxes.Count; i++)
        {
            BossPartHitbox hitbox = weakPointHitboxes[i];
            if (hitbox == null)
            {
                continue;
            }

            hitbox.SetWeakPointActive(hitbox == currentWeakPoint);
        }

        if (weakPointImage == null) return;

        // 모든 이미지 비활성화
        for (int i = 0; i < weakPointImage.Length; i++)
        {
            if (weakPointImage[i] == null) continue;
            weakPointImage[i].SetActive(false);
        }

        // 현재 약점 이미지 활성화
        if (currentWeakPoint != null)
        {
            weakPointImage[(int)currentWeakPoint.WeakPointType].SetActive(true);

            AudioManager.instance?.PlaySfx(AudioManager.Sfx.ElevatorDing); //***

            Debug.Log($"Current Weak Point: {currentWeakPoint.WeakPointType}");
        }
    }
}