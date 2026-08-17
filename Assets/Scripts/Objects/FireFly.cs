using UnityEngine;

public class FireFly : MonoBehaviour
{
    [Header("Movement Settings")]
    public GameObject[] targetPosition;
    public float moveSpeed = 2f;

    private int currentTargetIndex = 0;
    private int totalTargets;

    void Start()
    {
        totalTargets = targetPosition.Length;
    }

    void Update()
    {
        MoveTowardsTarget();
    }

    void MoveTowardsTarget()
    {
        if (totalTargets == 0) return;

        Vector2 currentPosition = transform.position;
        Vector2 targetPos = targetPosition[currentTargetIndex].transform.position;

        // Move towards the target position
        transform.position = Vector2.MoveTowards(currentPosition, targetPos, moveSpeed * Time.deltaTime);

        // Check if the firefly has reached the target position
        if (Vector2.Distance(currentPosition, targetPos) < 0.1f)
        {
            if (currentTargetIndex == totalTargets - 1)
            {
                // If it's the last target, reset to the first target
                transform.position = targetPosition[0].transform.position;
                currentTargetIndex = 1;
            }
            else
            {
                // Move to the next target position
                currentTargetIndex++;
            }
        }
    }
}
