using System;
using UnityEngine;

public enum SpeakerType
{
    Player,
    Npc
}

[Serializable]
public class DialogueLine
{
    public SpeakerType speaker;
    public string text;
}