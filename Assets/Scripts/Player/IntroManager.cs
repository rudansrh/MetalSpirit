using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera introCamera;
    [SerializeField] private SpriteRenderer fadeImage;
    [SerializeField] private DialogueData playerDialogue;
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private Transform npcTarget;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "LegZone";

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    [Header("Camera Settings")]
    [SerializeField] private float firstZoomSize = 3.5f;
    [SerializeField] private float closeZoomSize = 2.2f;
    [SerializeField] private float firstZoomDuration = 4f;
    [SerializeField] private float closeZoomDuration = 1.2f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float waypointPause = 0.2f;
    [SerializeField] private float fallDistance = 8f;
    [SerializeField] private float fallDuration = 1.4f;
    [SerializeField] private List<Vector2> movementRoute = new()
    {
        new Vector2(0f, -3f),
        new Vector2(-1f, -3f),
        new Vector2(1f, -3f),
        new Vector2(0f, -3f)
    };

    [Header("Dialogue Settings")]
    [SerializeField] private float postZoomDialogueDelay = 0.35f;

    private IntroPlayerController introPlayerController;
    private float initialCameraSize;

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void Start()
    {
        if (player == null || introCamera == null || fadeImage == null || playerDialogue == null || dialogueUI == null)
        {
            Debug.LogError("IntroManager: 필수 참조가 비어 있어 인트로를 시작할 수 없습니다.");
            enabled = false;
            return;
        }

        initialCameraSize = introCamera.orthographicSize;
        introPlayerController = GetOrCreateIntroPlayerController();
        introPlayerController.PrepareForIntro(GetRoutePoint(0));
        dialogueUI.Hide();

        SetFadeAlpha(1f);
        UpdateFadeOverlayTransform();
        StartCoroutine(PlayIntroSequence());
    }

    private void LateUpdate()
    {
        UpdateFadeOverlayTransform();
    }

    private void AutoAssignReferences()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (introCamera == null)
        {
            introCamera = Camera.main;
        }

        if (fadeImage == null)
        {
            fadeImage = FindSpriteRendererByName("FadeImage");
        }

        if (dialogueUI == null)
        {
            dialogueUI = FindAnyObjectByType<DialogueUI>();
        }

        if (playerDialogue == null && player != null && player.TryGetComponent<NPC>(out var npc))
        {
            playerDialogue = npc.dialogue;
        }

        if (npcTarget == null)
        {
            npcTarget = player;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        yield return FadeToAlpha(0f, fadeInDuration);
        yield return new WaitForSeconds(0.2f);

        yield return RunMoveAndZoomSequence();
        yield return new WaitForSeconds(0.2f);

        yield return PlayDialogueLines(0, Mathf.Min(3, playerDialogue.lines.Length));

        if (playerDialogue.lines.Length > 3)
        {
            dialogueUI.Hide();

            yield return ZoomToPlayer(closeZoomSize, closeZoomDuration);
            yield return new WaitForSeconds(postZoomDialogueDelay);
            yield return PlayDialogueLines(3, playerDialogue.lines.Length);
        }

        dialogueUI.Hide();

        yield return AnimateFall();
        yield return FadeToAlpha(1f, fadeOutDuration);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator RunMoveAndZoomSequence()
    {
        for (int i = 1; i < movementRoute.Count; i++)
        {
            yield return introPlayerController.MoveTo(GetRoutePoint(i), moveSpeed);
            yield return new WaitForSeconds(waypointPause);
        }

        yield return ZoomToPlayer(firstZoomSize, firstZoomDuration);
        introPlayerController.SetIdleAnimation();
    }

    private IEnumerator PlayDialogueLines(int startIndex, int endExclusive)
    {
        if (dialogueUI == null)
        {
            yield break;
        }

        for (int i = startIndex; i < endExclusive; i++)
        {
            DialogueLine line = playerDialogue.lines[i];
            Transform target = ResolveSpeakerTarget(line.speaker);
            dialogueUI.Show(line, target);

            yield return WaitForAdvanceRelease();
            yield return WaitForDialogueAdvance();
        }
    }

    private IEnumerator AnimateFall()
    {
        yield return introPlayerController.Fall(fallDistance, fallDuration);
    }

    private IEnumerator ZoomToPlayer(float targetSize, float duration)
    {
        Vector3 startPosition = introCamera.transform.position;
        Vector3 endPosition = new Vector3(player.position.x, player.position.y, startPosition.z);

        yield return ZoomCamera(startPosition, endPosition, introCamera.orthographicSize, targetSize, duration);
    }

    private IEnumerator ZoomCamera(Vector3 startPosition, Vector3 endPosition, float startSize, float endSize, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            introCamera.transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);
            introCamera.orthographicSize = Mathf.Lerp(startSize, endSize, easedProgress);

            yield return null;
        }

        introCamera.transform.position = endPosition;
        introCamera.orthographicSize = endSize;
        onComplete?.Invoke();
    }

    private Transform ResolveSpeakerTarget(SpeakerType speaker)
    {
        if (speaker == SpeakerType.Player || npcTarget == null)
        {
            return player;
        }

        return npcTarget;
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadeImage.color = color;
    }

    private void UpdateFadeOverlayTransform()
    {
        if (fadeImage == null || introCamera == null)
        {
            return;
        }

        fadeImage.transform.position = new Vector3(introCamera.transform.position.x, introCamera.transform.position.y, 0f);

        if (fadeImage.sprite == null)
        {
            return;
        }

        float worldHeight = introCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * introCamera.aspect;
        Vector2 spriteSize = fadeImage.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        fadeImage.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
    }

    private IEnumerator WaitForDialogueAdvance()
    {
        while (true)
        {
            if (WasAdvancePressed())
            {
                if (dialogueUI.IsTyping)
                {
                    dialogueUI.FinishTyping();
                }
                else
                {
                    break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator WaitForAdvanceRelease()
    {
        yield return null;

        while (IsAdvanceHeld())
        {
            yield return null;
        }
    }

    private bool WasAdvancePressed()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return false;
        }

        return keyboard.spaceKey.wasPressedThisFrame
            || keyboard.enterKey.wasPressedThisFrame
            || keyboard.numpadEnterKey.wasPressedThisFrame;
    }

    private bool IsAdvanceHeld()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return false;
        }

        return keyboard.spaceKey.isPressed
            || keyboard.enterKey.isPressed
            || keyboard.numpadEnterKey.isPressed;
    }

    private Vector3 GetRoutePoint(int index)
    {
        if (movementRoute == null || movementRoute.Count == 0)
        {
            return new Vector3(0f, -3f, player != null ? player.position.z : 0f);
        }

        Vector2 point = movementRoute[Mathf.Clamp(index, 0, movementRoute.Count - 1)];
        float z = player != null ? player.position.z : 0f;
        return new Vector3(point.x, point.y, z);
    }

    private SpriteRenderer FindSpriteRendererByName(string objectName)
    {
        SpriteRenderer[] spriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer.name == objectName)
            {
                return renderer;
            }
        }

        return null;
    }

    private IntroPlayerController GetOrCreateIntroPlayerController()
    {
        if (player == null)
        {
            return null;
        }

        IntroPlayerController controller = player.GetComponent<IntroPlayerController>();

        if (controller == null)
        {
            controller = player.gameObject.AddComponent<IntroPlayerController>();
        }

        return controller;
    }

    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        fadeImage.enabled = true;

        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float nextAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetFadeAlpha(nextAlpha);
            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }
}
