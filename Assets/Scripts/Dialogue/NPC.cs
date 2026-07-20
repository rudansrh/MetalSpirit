using UnityEngine;

public class NPC : MonoBehaviour
{
    public DialogueData dialogue;

    public void Talk()
    {
        DialogueManager.Instance.StartDialogue(dialogue, transform);
    }
}