using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LandfillEndingSequence : MonoBehaviour
{
    const string EndingImageObjectName = "EndingImage";
    const string FinalFadeImageObjectName = "FadeImage2";

    [Header("Scene References")]
    [SerializeField] Transform player;
    [SerializeField] Camera introCamera;
    [SerializeField] SpriteRenderer fadeImage;
    [SerializeField] SpriteRenderer finalFadeImage;
    [SerializeField] DialogueData playerDialogue;
    [SerializeField] DialogueUI dialogueUI;
    [SerializeField] Transform npcTarget;
    [SerializeField] GameObject endingImageObject;

    [Header("Scene Transition")]
    [SerializeField] string nextSceneName = "Start";

    [Header("Fade Settings")]
    [SerializeField] float fadeInDuration = 2f;
    [SerializeField] float fadeOutDuration = 1.5f;
    [SerializeField] float endingImageFlashDuration = 1f;

    [Header("Ending Timing")]
    [SerializeField] float firstFadeInStartDelay = 2f;
    [SerializeField] float dialogueStartDelay = 3f;
    [SerializeField] float postDialogueDelay = 0.2f;
    [SerializeField] float postFlashDelay = 0.2f;

    [Header("Mid Dialogue Camera Beat")]
    [SerializeField] int dialogueCameraBeatTriggerLineIndex = 3;
    [SerializeField] Vector3 dialogueCameraBeatOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] float dialogueCameraBeatUpDuration = 0.35f;
    [SerializeField] float dialogueCameraBeatHoldDuration = 0.1f;
    [SerializeField] float dialogueCameraBeatReturnDuration = 0.4f;

    [Header("Camera Settings")]
    [SerializeField] Vector3 endingCameraTargetPosition = new Vector3(0f, 5f, -10f);
    [SerializeField] float endingZoomOutMultiplier = 2f;
    [SerializeField] float endingCameraMoveDuration = 3f;
    [SerializeField] float postZoomOutDelay = 2f;

    float initialCameraSize;
    PlayerController playerController;

    void Awake()
    {
        AutoAssignReferences();
    }

    void Start()
    {
        if (player == null || introCamera == null || fadeImage == null || playerDialogue == null || dialogueUI == null)
        {
            Debug.LogError("LandfillEndingSequence: 필수 참조가 비어 있어 엔딩 시퀀스를 시작할 수 없습니다.");
            enabled = false;
            return;
        }

        playerController = PlayerController.Instance;
        initialCameraSize = introCamera.orthographicSize;

        if (player != null)
        {
            Vector3 playerPosition = player.position;
            playerPosition.x = 0f;
            player.position = playerPosition;
        }

        if (endingImageObject != null)
        {
            endingImageObject.SetActive(false);
        }

        dialogueUI.Hide();
        SetPlayerLocked(true);
        SetFadeAlpha(1f);
        SetFinalFadeAlpha(0f);
        UpdateFadeOverlayTransform();
        StartCoroutine(PlayEndingSequence());
    }

    void LateUpdate()
    {
        UpdateFadeOverlayTransform();
    }

    void AutoAssignReferences()
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

        if (finalFadeImage == null)
        {
            finalFadeImage = FindSpriteRendererByName(FinalFadeImageObjectName);
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

        if (endingImageObject == null)
        {
            SpriteRenderer endingImageRenderer = FindSpriteRendererByName(EndingImageObjectName);
            if (endingImageRenderer != null)
            {
                endingImageObject = endingImageRenderer.gameObject;
            }
        }
    }

    IEnumerator PlayEndingSequence()
    {
        yield return new WaitForSeconds(firstFadeInStartDelay);

        yield return FadeToAlpha(0f, fadeInDuration);

        if (dialogueStartDelay > 0f)
        {
            yield return new WaitForSeconds(dialogueStartDelay);
        }

        yield return PlayDialogueLines();

        if (postDialogueDelay > 0f)
        {
            yield return new WaitForSeconds(postDialogueDelay);
        }

        yield return PlayEndingImageFlash();

        if (postFlashDelay > 0f)
        {
            yield return new WaitForSeconds(postFlashDelay);
        }

        yield return MoveCameraToEnding();

        if (postZoomOutDelay > 0f)
        {
            yield return new WaitForSeconds(postZoomOutDelay);
        }

        yield return FadeFinalToAlpha(1f, fadeOutDuration);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator PlayDialogueLines()
    {
        if (dialogueUI == null || playerDialogue == null || playerDialogue.lines == null)
        {
            yield break;
        }

        for (int i = 0; i < playerDialogue.lines.Length; i++)
        {
            DialogueLine line = playerDialogue.lines[i];
            Transform target = ResolveSpeakerTarget(line != null ? line.speaker : SpeakerType.Player);
            dialogueUI.Show(line, target);

            yield return WaitForAdvanceRelease();
            yield return WaitForDialogueAdvance();

            if (i == dialogueCameraBeatTriggerLineIndex)
            {
                dialogueUI.Hide();
                yield return PlayDialogueCameraBeat();
            }
        }

        dialogueUI.Hide();
    }

    IEnumerator PlayEndingImageFlash()
    {
        float duration = Mathf.Max(0f, endingImageFlashDuration);
        if (duration <= 0f)
        {
            yield break;
        }

        float halfDuration = duration * 0.5f;
        yield return FadeToAlpha(1f, halfDuration);

        if (endingImageObject != null)
        {
            endingImageObject.SetActive(true);
        }

        yield return FadeToAlpha(0f, halfDuration);
    }

    IEnumerator MoveCameraToEnding()
    {
        if (introCamera == null)
        {
            yield break;
        }

        Vector3 startPosition = introCamera.transform.position;
        Vector3 targetPosition = endingCameraTargetPosition;
        targetPosition.z = startPosition.z;

        float targetSize = initialCameraSize * Mathf.Max(1f, endingZoomOutMultiplier);
        yield return ZoomCamera(startPosition, targetPosition, introCamera.orthographicSize, targetSize, endingCameraMoveDuration);
    }

    IEnumerator PlayDialogueCameraBeat()
    {
        if (introCamera == null)
        {
            yield break;
        }

        Vector3 startPosition = introCamera.transform.position;
        Vector3 liftedPosition = startPosition + dialogueCameraBeatOffset;
        liftedPosition.z = startPosition.z;

        yield return MoveCamera(startPosition, liftedPosition, dialogueCameraBeatUpDuration);

        if (dialogueCameraBeatHoldDuration > 0f)
        {
            yield return new WaitForSeconds(dialogueCameraBeatHoldDuration);
        }

        yield return MoveCamera(liftedPosition, startPosition, dialogueCameraBeatReturnDuration);
    }

    IEnumerator MoveCamera(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        if (introCamera == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            introCamera.transform.position = endPosition;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            introCamera.transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);
            yield return null;
        }

        introCamera.transform.position = endPosition;
    }

    IEnumerator ZoomCamera(Vector3 startPosition, Vector3 endPosition, float startSize, float endSize, float duration)
    {
        if (duration <= 0f)
        {
            introCamera.transform.position = endPosition;
            introCamera.orthographicSize = endSize;
            yield break;
        }

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
    }

    Transform ResolveSpeakerTarget(SpeakerType speaker)
    {
        if (speaker == SpeakerType.Player || npcTarget == null)
        {
            return player;
        }

        return npcTarget;
    }

    void SetPlayerLocked(bool locked)
    {
        if (playerController == null)
        {
            return;
        }

        playerController.canMove = !locked;
        playerController.StopMovement();
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadeImage.color = color;
        fadeImage.enabled = color.a > 0f;
    }

    void SetFinalFadeAlpha(float alpha)
    {
        if (finalFadeImage == null)
        {
            return;
        }

        Color color = finalFadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        finalFadeImage.color = color;
        finalFadeImage.enabled = color.a > 0f;
    }

    void UpdateFadeOverlayTransform()
    {
        if (introCamera == null)
        {
            return;
        }

        UpdateFadeSpriteTransform(fadeImage);
        UpdateFadeSpriteTransform(finalFadeImage);
    }

    IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        if (fadeImage == null)
        {
            yield break;
        }

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

    IEnumerator FadeFinalToAlpha(float targetAlpha, float duration)
    {
        if (finalFadeImage == null)
        {
            yield break;
        }

        finalFadeImage.enabled = true;

        float startAlpha = finalFadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float nextAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetFinalFadeAlpha(nextAlpha);
            yield return null;
        }

        SetFinalFadeAlpha(targetAlpha);
    }

    void UpdateFadeSpriteTransform(SpriteRenderer targetFadeImage)
    {
        if (targetFadeImage == null || introCamera == null || targetFadeImage.sprite == null)
        {
            return;
        }

        targetFadeImage.transform.position = new Vector3(introCamera.transform.position.x, introCamera.transform.position.y, 0f);

        float worldHeight = introCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * introCamera.aspect;
        Vector2 spriteSize = targetFadeImage.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        targetFadeImage.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
    }

    IEnumerator WaitForDialogueAdvance()
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

    IEnumerator WaitForAdvanceRelease()
    {
        yield return null;

        while (IsAdvanceHeld())
        {
            yield return null;
        }
    }

    bool WasAdvancePressed()
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

    bool IsAdvanceHeld()
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

    SpriteRenderer FindSpriteRendererByName(string objectName)
    {
        SpriteRenderer[] spriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null && renderer.name == objectName)
            {
                return renderer;
            }
        }

        return null;
    }
}
