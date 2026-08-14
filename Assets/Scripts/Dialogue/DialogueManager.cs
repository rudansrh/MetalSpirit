using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Transform player;

    DialogueData currentDialogue;
    Transform npc;
    [SerializeField]InputAction submitAction;

    int index;
    bool isTalking = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = PlayerController.Instance.transform;
        submitAction = PlayerController.Instance.GetComponent<PlayerInput>().actions["Next"];
    }

    private void Update()
    {
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
        PlayerController.Instance.isTalking = true;

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
            PlayerController.Instance.canMove = true;
            PlayerController.Instance.isTalking = false;
            return;
        }

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.lines[index];
        Transform target = line.speaker == SpeakerType.Player ? player : npc;
        DialogueUI.Instance.Show(line.BuildRichText(), target);
    }
}
