using UnityEngine;

public class NPC : MonoBehaviour
{
    public DialogueData dialogue;
    [SerializeField] private DialogueData armsDialogue;
    [SerializeField] private DialogueData fullBodyDialogue;

    public void Talk()
    {
        DialogueData selectedDialogue = ResolveDialogue();

        if (selectedDialogue == null || DialogueManager.Instance == null)
        {
            return;
        }

        DialogueManager.Instance.StartDialogue(selectedDialogue, transform);
    }

    private DialogueData ResolveDialogue()
    {
        PlayerProgressionManager progressionManager = PlayerController.Instance != null
            ? PlayerController.Instance.GetComponent<PlayerProgressionManager>()
            : null;

        if (progressionManager == null)
        {
            return dialogue;
        }

        return progressionManager.EffectiveUnlockedStage switch
        {
            PlayerStage.FullBody => fullBodyDialogue != null ? fullBodyDialogue : armsDialogue != null ? armsDialogue : dialogue,
            PlayerStage.Arms => armsDialogue != null ? armsDialogue : dialogue,
            _ => dialogue
        };
    }
}
