using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public Transform player;

    DialogueData currentDialogue;

    Transform npc;

    int index;

    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueData dialogue, Transform npcTransform)
    {
        currentDialogue = dialogue;
        npc = npcTransform;
        index = 0;

        ShowCurrentLine();
    }

    public void Next()
    {
        index++;

        if(index >= currentDialogue.lines.Length)
        {
            DialogueUI.Instance.Hide();
            return;
        }

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.lines[index];

        Transform target =
            line.speaker == SpeakerType.Player ? player : npc;

        DialogueUI.Instance.Show(line.text, target);
    }
}