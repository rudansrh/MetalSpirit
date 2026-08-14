using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (lines == null)
        {
            return;
        }

        bool changed = false;

        foreach (DialogueLine line in lines)
        {
            if (line != null && line.TryUpgradeLegacyData())
            {
                changed = true;
            }
        }

        if (changed)
        {
            EditorUtility.SetDirty(this);
        }
    }
#endif
}
