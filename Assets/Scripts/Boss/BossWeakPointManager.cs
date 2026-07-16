using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossWeakPointManager : MonoBehaviour
{
    [Header("Weak Point Settings")]
    [SerializeField] float weakPointOpenDuration = 5f;
    [SerializeField] List<BossPartHitbox> weakPointHitboxes = new List<BossPartHitbox>();

    [Header("UI")]
    [SerializeField] Text weakPointText;
    [SerializeField] string weakPointTextPrefix = "Weak Point";

    BossController bossController;
    BossPartHitbox currentWeakPoint;
    Coroutine weakPointCycleCoroutine;
    float remainingOpenTime;

    public BossWeakPointType CurrentWeakPointType =>
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

    void Update()
    {
        if (currentWeakPoint == null || weakPointText == null)
        {
            return;
        }

        remainingOpenTime = Mathf.Max(0f, remainingOpenTime - Time.deltaTime);
        weakPointText.text =
            $"{weakPointTextPrefix}: {GetDisplayName(currentWeakPoint.WeakPointType)} ({remainingOpenTime:0.0}s)";
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

        if (weakPointHitboxes.Count == 0)
        {
            return;
        }

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

        if (weakPointText == null)
        {
            return;
        }

        weakPointText.text = currentWeakPoint == null
            ? $"{weakPointTextPrefix}: None"
            : $"{weakPointTextPrefix}: {GetDisplayName(currentWeakPoint.WeakPointType)} ({weakPointOpenDuration:0.0}s)";
    }

    string GetDisplayName(BossWeakPointType type)
    {
        switch (type)
        {
            case BossWeakPointType.LeftArm:
                return "Left Arm";
            case BossWeakPointType.RightArm:
                return "Right Arm";
            case BossWeakPointType.UpperBody:
                return "Upper Body";
            case BossWeakPointType.LowerBody:
                return "Lower Body";
            default:
                return type.ToString();
        }
    }
}
