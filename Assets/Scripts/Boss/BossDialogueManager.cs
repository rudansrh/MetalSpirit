using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossDialogueManager : MonoBehaviour
{
    public static BossDialogueManager Instance;

    [SerializeField] Transform player;
    [SerializeField] string submitActionName = "Next";

    DialogueData currentDialogue;
    Transform speakerTransform;
    PlayerInput playerInput;
    int index;
    bool isTalking;

    public event Action<DialogueData> DialogueStarted;
    public event Action<DialogueData> DialogueEnded;

    public bool IsTalking => isTalking;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CachePlayerReferences();
    }

    void Update()
    {
        InputAction submitAction = GetSubmitAction();
        if (submitAction == null)
        {
            return;
        }

        if (!submitAction.WasPressedThisFrame())
        {
            return;
        }

        if (DialogueUI.Instance.IsTyping)
        {
            DialogueUI.Instance.FinishTyping();
            return;
        }

        Next();
    }

    public void StartDialogue(DialogueData dialogue, Transform npcTransform)
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0)
        {
            Debug.LogWarning("Cannot start boss dialogue because no lines were provided.");
            return;
        }

        currentDialogue = dialogue;
        speakerTransform = npcTransform;
        isTalking = true;
        index = 0;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = false;
            PlayerController.Instance.isTalking = true;
        }

        DialogueStarted?.Invoke(currentDialogue);
        ShowCurrentLine();
    }

    public void Next()
    {
        if (!isTalking)
        {
            return;
        }

        index++;
        if (index >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.lines[index];
        Transform target = line.speaker == SpeakerType.Player ? player : speakerTransform;
        DialogueUI.Instance.Show(line, target);
    }

    void EndDialogue()
    {
        DialogueData finishedDialogue = currentDialogue;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Hide();
        }

        currentDialogue = null;
        speakerTransform = null;
        isTalking = false;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = true;
            PlayerController.Instance.isTalking = false;
        }

        DialogueEnded?.Invoke(finishedDialogue);
    }

    void CachePlayerReferences()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        if (player == null)
        {
            player = PlayerController.Instance.transform;
        }

        if (playerInput == null)
        {
            playerInput = PlayerController.Instance.GetComponent<PlayerInput>();
        }
    }

    InputAction GetSubmitAction()
    {
        CachePlayerReferences();

        if (playerInput == null || playerInput.actions == null || string.IsNullOrWhiteSpace(submitActionName))
        {
            return null;
        }

        return playerInput.actions[submitActionName];
    }
}
