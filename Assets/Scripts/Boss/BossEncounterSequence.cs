using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class BossEncounterSequence : MonoBehaviour
{
    const float Phase2CameraZ = -10f;
    const string SceneFadeOverlayName = "BossSceneFadeOverlay";

    [Header("References")]
    [SerializeField] BossController bossController;
    [SerializeField] BossArenaController bossArenaController;
    [SerializeField] BossDialogueManager bossDialogueManager;
    [SerializeField] GameObject bossRoot;
    [SerializeField] GameObject bossHealthUiObject;
    [SerializeField] GameObject triggerElevatorAppear;
    [SerializeField] Transform elevatorRideTransform;
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

    [Header("Phase 2 Transition")]
    [SerializeField] Transform phase2FocusTarget;
    [SerializeField] Vector3 phase2FocusPosition = Vector3.zero;
    [SerializeField] float phase2ZoomSize = 3f;
    [SerializeField] float phase2ZoomDuration = 0.35f;
    [SerializeField] float phase2ShakeDuration = 1f;
    [SerializeField] float phase2ShakeMagnitude = 0.2f;
    [SerializeField] float phase2ReturnDuration = 0.35f;
    [SerializeField] bool enableArenaShakeInPhase2 = true;

    [Header("Boss Defeat")]
    [SerializeField] float bossDefeatDelay = 2f;
    [SerializeField] float bossFadeOutDuration = 1f;

    [Header("Elevator Escape")]
    [SerializeField] Vector3 elevatorMoveDirection = Vector3.up;
    [SerializeField] float elevatorMoveSpeed = 2f;
    [SerializeField] float elevatorRideDuration = 3f;
    [SerializeField] string elevatorExitSceneName = "Landfill";

    [Header("Scene Fade")]
    [SerializeField] float sceneFadeOutDuration = 1f;
    [SerializeField] int sceneFadeSortingOrder = 1000;

    [Header("Player Control")]
    [SerializeField] bool lockPlayerDuringSequence = true;

    [Header("Events")]
    [SerializeField] UnityEvent onSequenceStarted;
    [SerializeField] UnityEvent onBossActivated;
    [SerializeField] UnityEvent onDialogueFinished;
    [SerializeField] UnityEvent onBattleStarted;
    [SerializeField] UnityEvent onPhase2Started;
    [SerializeField] UnityEvent onBossDefeatedSequenceFinished;
    [SerializeField] UnityEvent onElevatorEscapeStarted;

    Coroutine sequenceCoroutine;
    Coroutine phase2TransitionCoroutine;
    Coroutine bossDefeatCoroutine;
    Coroutine elevatorEscapeCoroutine;
    bool hasEncounterStarted;
    bool dialogueFinished;
    bool phase2Started;
    SpriteRenderer[] cachedBossSpriteRenderers;
    Color[] cachedBossSpriteColors;
    Canvas sceneFadeCanvas;
    Image sceneFadeImage;

    void Reset()
    {
        CacheReferences();
    }

    void Awake()
    {
        CacheReferences();
        PrepareEncounterState();
        EnsureSceneFadeOverlay();
        SetSceneFadeAlpha(0f);
    }

    void OnEnable()
    {
        CacheReferences();
        SubscribeBossEvents();
    }

    void OnDisable()
    {
        UnsubscribeBossEvents();
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

        if (cachedBossSpriteRenderers == null || cachedBossSpriteRenderers.Length == 0)
        {
            cachedBossSpriteRenderers = bossRoot != null
                ? bossRoot.GetComponentsInChildren<SpriteRenderer>(true)
                : System.Array.Empty<SpriteRenderer>();
            CacheBossSpriteColors();
        }

        if (bossHealthUiObject == null && bossController != null)
        {
            Transform sliderTransform = bossController.transform.root.Find("Canvas/Slider_BossHealth");
            if (sliderTransform != null)
            {
                bossHealthUiObject = sliderTransform.gameObject;
            }
        }

        if (bossArenaController == null && bossController != null)
        {
            bossArenaController = bossController.GetComponentInChildren<BossArenaController>(true);
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

    void SubscribeBossEvents()
    {
        if (bossController == null)
        {
            return;
        }

        bossController.OnPhase2TransitionRequested -= HandlePhase2TransitionRequested;
        bossController.OnPhase2TransitionRequested += HandlePhase2TransitionRequested;
        bossController.OnBossDefeated -= HandleBossDefeated;
        bossController.OnBossDefeated += HandleBossDefeated;
    }

    void UnsubscribeBossEvents()
    {
        if (bossController == null)
        {
            return;
        }

        bossController.OnPhase2TransitionRequested -= HandlePhase2TransitionRequested;
        bossController.OnBossDefeated -= HandleBossDefeated;
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

        SetBossVisualAlpha(1f);
        SetBossHealthUiVisible(false);

        if (bossArenaController != null)
        {
            bossArenaController.SetAllowShake(false);
            bossArenaController.SetAllPlatformsActive(false);
        }

        if (triggerElevatorAppear != null)
        {
            triggerElevatorAppear.SetActive(false);
        }
    }

    void CacheBossSpriteColors()
    {
        if (cachedBossSpriteRenderers == null)
        {
            cachedBossSpriteColors = System.Array.Empty<Color>();
            return;
        }

        cachedBossSpriteColors = new Color[cachedBossSpriteRenderers.Length];
        for (int i = 0; i < cachedBossSpriteRenderers.Length; i++)
        {
            cachedBossSpriteColors[i] = cachedBossSpriteRenderers[i] != null
                ? cachedBossSpriteRenderers[i].color
                : Color.white;
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

        SetBossVisualAlpha(1f);

        if (bossController != null)
        {
            bossController.SuppressAutoStartBattle();
        }

        SetBossHealthUiVisible(false);

        if (bossArenaController != null)
        {
            bossArenaController.SetAllPlatformsActive(false);
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

        SetBossHealthUiVisible(true);

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

    void HandlePhase2TransitionRequested()
    {
        if (phase2TransitionCoroutine != null || phase2Started)
        {
            return;
        }

        phase2TransitionCoroutine = StartCoroutine(PlayPhase2TransitionSequence());
    }

    void HandleBossDefeated()
    {
        if (bossDefeatCoroutine != null)
        {
            return;
        }

        bossDefeatCoroutine = StartCoroutine(PlayBossDefeatSequence());
    }

    public void BeginElevatorEscape()
    {
        if (elevatorEscapeCoroutine != null)
        {
            return;
        }

        elevatorEscapeCoroutine = StartCoroutine(PlayElevatorEscapeSequence());
    }

    IEnumerator PlayPhase2TransitionSequence()
    {
        PlayerController player = PlayerController.Instance;
        SetPlayerLocked(player, true);
        SetBossHealthUiVisible(false);

        float carriedHealth = bossController != null ? bossController.CurrentHealth : 0f;

        yield return PlayPhase2CameraSequence();

        if (bossArenaController != null)
        {
            bossArenaController.SetAllowShake(enableArenaShakeInPhase2);
            bossArenaController.SetAllPlatformsActive(false);
        }

        if (bossController != null)
        {
            bossController.InitializePhase(BossPhase.Phase2, carriedHealth, true);
            bossController.StartBattle();
        }

        phase2Started = true;
        SetBossVisualAlpha(1f);
        SetBossHealthUiVisible(true);
        SetPlayerLocked(player, false);
        onPhase2Started?.Invoke();
        phase2TransitionCoroutine = null;
    }

    IEnumerator PlayBossDefeatSequence()
    {
        PlayerController player = PlayerController.Instance;
        SetPlayerLocked(player, true);
        SetBossHealthUiVisible(false);

        if (bossDefeatDelay > 0f)
        {
            yield return new WaitForSeconds(bossDefeatDelay);
        }

        yield return FadeBossOutSequence();

        if (bossRoot != null)
        {
            bossRoot.SetActive(false);
        }

        if (triggerElevatorAppear != null)
        {
            triggerElevatorAppear.SetActive(true);
        }

        SetPlayerLocked(player, false);
        onBossDefeatedSequenceFinished?.Invoke();
        bossDefeatCoroutine = null;
    }

    IEnumerator PlayElevatorEscapeSequence()
    {
        onElevatorEscapeStarted?.Invoke();

        PlayerController player = PlayerController.Instance;
        SetPlayerLocked(player, true);
        SetBossHealthUiVisible(false);

        Vector3 moveDirection = elevatorMoveDirection.sqrMagnitude > 0f ? elevatorMoveDirection.normalized : Vector3.up;
        Vector3 playerOffsetFromElevator = Vector3.zero;

        if (player != null && elevatorRideTransform != null)
        {
            playerOffsetFromElevator = player.transform.position - elevatorRideTransform.position;
        }

        float elapsed = 0f;
        while (elapsed < elevatorRideDuration)
        {
            elapsed += Time.deltaTime;
            Vector3 delta = moveDirection * elevatorMoveSpeed * Time.deltaTime;

            if (elevatorRideTransform != null)
            {
                elevatorRideTransform.position += delta;
            }

            if (player != null)
            {
                if (elevatorRideTransform != null)
                {
                    player.transform.position = elevatorRideTransform.position + playerOffsetFromElevator;
                }
                else
                {
                    player.transform.position += delta;
                }

                player.StopMovement();
            }

            yield return null;
        }

        if (!string.IsNullOrWhiteSpace(elevatorExitSceneName))
        {
            yield return FadeOutToScene(elevatorExitSceneName);
        }

        elevatorEscapeCoroutine = null;
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

    void SetBossHealthUiVisible(bool isVisible)
    {
        if (bossHealthUiObject != null)
        {
            bossHealthUiObject.SetActive(isVisible);
            return;
        }

        if (bossController != null)
        {
            bossController.SetBossHealthUiVisible(isVisible);
        }
    }

    IEnumerator PlayPhase2CameraSequence()
    {
        Camera sequenceCamera = Camera.main;
        if (sequenceCamera == null)
        {
            yield break;
        }

        cameraFollow followCamera = cameraFollow.Instance;
        bool restoreFollowState = false;

        if (followCamera != null)
        {
            restoreFollowState = followCamera.followTarget;
            followCamera.followTarget = false;
        }

        Vector3 originalPosition = sequenceCamera.transform.position;
        originalPosition.z = Phase2CameraZ;
        sequenceCamera.transform.position = originalPosition;
        float originalSize = sequenceCamera.orthographicSize;
        Vector3 focusPosition = ResolvePhase2FocusPosition();
        focusPosition.z = Phase2CameraZ;

        yield return MoveCamera(sequenceCamera, originalPosition, focusPosition, originalSize, phase2ZoomSize, phase2ZoomDuration);
        yield return ShakeCamera(sequenceCamera, focusPosition);
        yield return MoveCamera(sequenceCamera, sequenceCamera.transform.position, originalPosition, sequenceCamera.orthographicSize, originalSize, phase2ReturnDuration);

        sequenceCamera.transform.position = originalPosition;
        sequenceCamera.orthographicSize = originalSize;

        if (followCamera != null)
        {
            followCamera.followTarget = restoreFollowState;
        }
    }

    Vector3 ResolvePhase2FocusPosition()
    {
        if (phase2FocusTarget != null)
        {
            return phase2FocusTarget.position;
        }

        return phase2FocusPosition;
    }

    IEnumerator MoveCamera(Camera targetCamera, Vector3 startPosition, Vector3 endPosition, float startSize, float endSize, float duration)
    {
        if (targetCamera == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            targetCamera.transform.position = endPosition;
            targetCamera.orthographicSize = endSize;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            targetCamera.transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);
            targetCamera.orthographicSize = Mathf.Lerp(startSize, endSize, easedProgress);
            yield return null;
        }

        targetCamera.transform.position = endPosition;
        targetCamera.orthographicSize = endSize;
    }

    IEnumerator ShakeCamera(Camera targetCamera, Vector3 basePosition)
    {
        if (targetCamera == null || phase2ShakeDuration <= 0f || phase2ShakeMagnitude <= 0f)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < phase2ShakeDuration)
        {
            elapsed += Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * phase2ShakeMagnitude;
            targetCamera.transform.position = new Vector3(basePosition.x + offset.x, basePosition.y + offset.y, basePosition.z);
            yield return null;
        }

        targetCamera.transform.position = basePosition;
    }

    IEnumerator FadeBossOutSequence()
    {
        if (bossRoot == null)
        {
            yield break;
        }

        if (bossFadeOutDuration <= 0f)
        {
            SetBossVisualAlpha(0f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < bossFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / bossFadeOutDuration);
            SetBossVisualAlpha(Mathf.Lerp(1f, 0f, progress));
            yield return null;
        }

        SetBossVisualAlpha(0f);
    }

    void SetBossVisualAlpha(float alpha)
    {
        if (cachedBossSpriteRenderers == null || cachedBossSpriteColors == null)
        {
            return;
        }

        float clampedAlpha = Mathf.Clamp01(alpha);
        for (int i = 0; i < cachedBossSpriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = cachedBossSpriteRenderers[i];
            if (spriteRenderer == null)
            {
                continue;
            }

            Color baseColor = i < cachedBossSpriteColors.Length ? cachedBossSpriteColors[i] : spriteRenderer.color;
            Color color = spriteRenderer.color;
            color.r = baseColor.r;
            color.g = baseColor.g;
            color.b = baseColor.b;
            color.a = baseColor.a * clampedAlpha;
            spriteRenderer.color = color;
        }
    }

    IEnumerator FadeOutToScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            yield break;
        }

        if (sceneFadeOutDuration > 0f)
        {
            yield return FadeSceneOverlay(0f, 1f, sceneFadeOutDuration);
        }

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeSceneOverlay(float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0f)
        {
            SetSceneFadeAlpha(endAlpha);
            yield break;
        }

        EnsureSceneFadeOverlay();
        if (sceneFadeImage == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            UpdateSceneFadeOverlayTransform(Camera.main);
            SetSceneFadeAlpha(alpha);
            yield return null;
        }

        UpdateSceneFadeOverlayTransform(Camera.main);
        SetSceneFadeAlpha(endAlpha);
    }

    void EnsureSceneFadeOverlay()
    {
        if (sceneFadeCanvas != null && sceneFadeImage != null)
        {
            return;
        }

        Transform existingOverlay = transform.Find(SceneFadeOverlayName);
        if (existingOverlay != null)
        {
            sceneFadeCanvas = existingOverlay.GetComponent<Canvas>();
            sceneFadeImage = existingOverlay.GetComponentInChildren<Image>(true);
        }

        if (sceneFadeCanvas == null)
        {
            GameObject overlayObject = new GameObject(SceneFadeOverlayName);
            overlayObject.transform.SetParent(transform, false);
            sceneFadeCanvas = overlayObject.AddComponent<Canvas>();
            sceneFadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            sceneFadeCanvas.sortingOrder = sceneFadeSortingOrder;

            CanvasScaler scaler = overlayObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            overlayObject.AddComponent<GraphicRaycaster>();

            GameObject imageObject = new GameObject("FadeImage");
            imageObject.transform.SetParent(overlayObject.transform, false);
            sceneFadeImage = imageObject.AddComponent<Image>();

            RectTransform imageRect = sceneFadeImage.rectTransform;
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
        }

        if (sceneFadeCanvas == null || sceneFadeImage == null)
        {
            return;
        }

        sceneFadeCanvas.sortingOrder = sceneFadeSortingOrder;
        sceneFadeImage.color = new Color(0f, 0f, 0f, 0f);
        sceneFadeCanvas.gameObject.SetActive(false);
    }

    void UpdateSceneFadeOverlayTransform(Camera targetCamera)
    {
        if (sceneFadeCanvas != null)
        {
            sceneFadeCanvas.worldCamera = targetCamera;
        }
    }

    void SetSceneFadeAlpha(float alpha)
    {
        if (sceneFadeCanvas == null || sceneFadeImage == null)
        {
            return;
        }

        Color color = sceneFadeImage.color;
        color.r = 0f;
        color.g = 0f;
        color.b = 0f;
        color.a = Mathf.Clamp01(alpha);
        sceneFadeImage.color = color;
        sceneFadeCanvas.gameObject.SetActive(color.a > 0f);
    }
}
