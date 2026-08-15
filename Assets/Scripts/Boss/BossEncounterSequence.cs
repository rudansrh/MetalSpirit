using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-100)]
public class BossEncounterSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BossController bossController;
    [SerializeField] BossDialogueManager bossDialogueManager;
    [SerializeField] GameObject bossRoot;
    [SerializeField] DialogueData introDialogue;
    [SerializeField] Transform dialogueAnchor;

    [Header("Entrance")]
    [SerializeField] bool hideBossOnStart = true;
    [SerializeField] Transform entranceStartPoint;
    [SerializeField] Transform entranceEndPoint;
    [SerializeField] float entranceDuration = 1.25f;
    [SerializeField] AnimationCurve entranceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] float preDialogueDelay = 0.2f;
    [SerializeField] float postDialogueDelay = 0.2f;

    [Header("Player Control")]
    [SerializeField] bool lockPlayerDuringSequence = true;

    [Header("Events")]
    [SerializeField] UnityEvent onSequenceStarted;
    [SerializeField] UnityEvent onBossActivated;
    [SerializeField] UnityEvent onDialogueFinished;
    [SerializeField] UnityEvent onBattleStarted;

    Coroutine sequenceCoroutine;
    bool hasEncounterStarted;
    bool dialogueFinished;

    void Reset()
    {
        CacheReferences();
    }

    void Awake()
    {
        CacheReferences();
        PrepareEncounterState();
    }

    void CacheReferences()
    {
        if (bossController == null)
        {
            bossController = GetComponentInChildren<BossController>(true);
        }

        if (bossRoot == null && bossController != null)
        {
            bossRoot = bossController.gameObject;
        }

        if (bossDialogueManager == null)
        {
            bossDialogueManager = FindFirstObjectByType<BossDialogueManager>();
        }

        if (dialogueAnchor == null && bossController != null)
        {
            dialogueAnchor = bossController.transform;
        }
    }

    void PrepareEncounterState()
    {
        if (bossController != null)
        {
            bossController.SuppressAutoStartBattle();
        }

        if (hideBossOnStart && bossRoot != null)
        {
            bossRoot.SetActive(false);
        }
    }

    public void BeginEncounter()
    {
        if (hasEncounterStarted || sequenceCoroutine != null)
        {
            return;
        }

        sequenceCoroutine = StartCoroutine(PlayEncounterSequence());
    }

    public void BeginIntro()
    {
        BeginEncounter();
    }

    IEnumerator PlayEncounterSequence()
    {
        hasEncounterStarted = true;
        onSequenceStarted?.Invoke();

        PlayerController player = PlayerController.Instance;
        SetPlayerLocked(player, lockPlayerDuringSequence);

        if (bossRoot != null && !bossRoot.activeSelf)
        {
            bossRoot.SetActive(true);
        }

        if (bossController != null)
        {
            bossController.SuppressAutoStartBattle();
        }

        Transform bossTransform = dialogueAnchor != null
            ? dialogueAnchor
            : bossController != null ? bossController.transform : null;

        if (bossTransform != null && entranceStartPoint != null)
        {
            bossTransform.position = entranceStartPoint.position;
        }

        yield return RunEntranceAnimation(bossTransform);

        onBossActivated?.Invoke();

        if (preDialogueDelay > 0f)
        {
            yield return new WaitForSeconds(preDialogueDelay);
        }

        if (introDialogue != null && bossDialogueManager != null)
        {
            dialogueFinished = false;
            bossDialogueManager.DialogueEnded += HandleDialogueEnded;
            bossDialogueManager.StartDialogue(introDialogue, bossTransform);

            yield return new WaitUntil(() => dialogueFinished);

            bossDialogueManager.DialogueEnded -= HandleDialogueEnded;
            onDialogueFinished?.Invoke();
        }
        else
        {
            onDialogueFinished?.Invoke();
        }

        if (postDialogueDelay > 0f)
        {
            yield return new WaitForSeconds(postDialogueDelay);
        }

        if (bossController != null)
        {
            bossController.StartBattle();
        }

        SetPlayerLocked(player, false);
        onBattleStarted?.Invoke();
        sequenceCoroutine = null;
    }

    IEnumerator RunEntranceAnimation(Transform bossTransform)
    {
        if (bossTransform == null || entranceStartPoint == null || entranceEndPoint == null || entranceDuration <= 0f)
        {
            if (bossTransform != null && entranceEndPoint != null)
            {
                bossTransform.position = entranceEndPoint.position;
            }

            yield break;
        }

        Vector3 startPosition = entranceStartPoint.position;
        Vector3 endPosition = entranceEndPoint.position;
        float elapsed = 0f;

        while (elapsed < entranceDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / entranceDuration);
            float easedTime = entranceCurve != null ? entranceCurve.Evaluate(normalizedTime) : normalizedTime;
            bossTransform.position = Vector3.LerpUnclamped(startPosition, endPosition, easedTime);
            yield return null;
        }

        bossTransform.position = endPosition;
    }

    void HandleDialogueEnded(DialogueData _)
    {
        if (PlayerController.Instance != null && lockPlayerDuringSequence)
        {
            PlayerController.Instance.canMove = false;
            PlayerController.Instance.StopMovement();
        }

        dialogueFinished = true;
    }

    void SetPlayerLocked(PlayerController player, bool locked)
    {
        if (player == null || !lockPlayerDuringSequence)
        {
            return;
        }

        player.canMove = !locked;
        player.StopMovement();
    }
}
