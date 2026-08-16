using UnityEngine;
using UnityEngine.Events;

public class ObjectiveCounter : MonoBehaviour
{
    [Header("Objective Settings")]
    [SerializeField] int requiredCount = 1;
    [SerializeField] bool completeOnlyOnce = true;
    [SerializeField] bool deactivateOnComplete = false;

    [Header("Events")]
    [SerializeField] UnityEvent onProgressed;
    [SerializeField] UnityEvent onCompleted;

    int currentCount;
    bool isCompleted;

    public int CurrentCount => currentCount;
    public int RequiredCount => requiredCount;
    public bool IsCompleted => isCompleted;

    public void ReportProgress()
    {
        ReportProgress(1);
    }

    public void ReportProgress(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (completeOnlyOnce && isCompleted)
        {
            return;
        }

        currentCount += amount;
        onProgressed?.Invoke();

        if (currentCount < Mathf.Max(1, requiredCount))
        {
            return;
        }

        CompleteObjective();
    }

    [ContextMenu("Complete Objective")]
    public void CompleteObjective()
    {
        if (completeOnlyOnce && isCompleted)
        {
            return;
        }

        currentCount = Mathf.Max(currentCount, Mathf.Max(1, requiredCount));
        isCompleted = true;
        onCompleted?.Invoke();

        if (deactivateOnComplete)
        {
            gameObject.SetActive(false);
        }
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        currentCount = 0;
        isCompleted = false;
    }
}
