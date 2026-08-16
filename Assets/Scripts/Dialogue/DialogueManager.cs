using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Transform player;

    DialogueData currentDialogue;
    Transform npc;
    InputAction submitAction;

    int index;
    bool isTalking = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CachePlayerReferences();
    }

    private void Update()
    {
        if (submitAction == null)
        {
            CachePlayerReferences();
        }

        if (submitAction == null)
        {
            return;
        }

        if (submitAction.WasPressedThisFrame())
        {
            if (DialogueUI.Instance.IsTyping)
            {
                DialogueUI.Instance.FinishTyping();
            }
            else
            {
                Next();
            }
        }
    }

    public void StartDialogue(DialogueData dialogue, Transform npcTransform)
    {
        currentDialogue = dialogue;
        npc = npcTransform;
        isTalking = true;
        index = 0;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.isTalking = true;
        }

        ShowCurrentLine();
    }

    public void Next()
    {
        if (!isTalking) return;

        index++;
        if(index >= currentDialogue.lines.Length)
        {
            DialogueUI.Instance.Hide();
            isTalking = false;
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.canMove = true;
                PlayerController.Instance.isTalking = false;
            }
            return;
        }

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.lines[index];
        Transform target = line.speaker == SpeakerType.Player ? player : npc;
        DialogueUI.Instance.Show(line, target);
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

        if (submitAction == null)
        {
            PlayerInput playerInput = PlayerController.Instance.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                submitAction = playerInput.actions["Next"];
            }
        }
    }
}
