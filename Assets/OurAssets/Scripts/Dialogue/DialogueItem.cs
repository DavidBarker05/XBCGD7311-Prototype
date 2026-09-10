using System;
using UnityEngine;

[Serializable]
public class SerializedDialogueItem
{
    public string name;
    public string text;
    public int fontSize;
    public int charactersPerSecond;

    public DialogueItem Deserialized => new DialogueItem() { Name = name, Text = text, FontSize = fontSize, CharactersPerSecond = charactersPerSecond };
}

[Serializable]
public class DialogueItem
{
    public string Name;
    [TextArea] public string Text;
    public int FontSize;
    public int CharactersPerSecond;

    public SerializedDialogueItem Serialized => new SerializedDialogueItem() { name = Name, text = Text, fontSize = FontSize, charactersPerSecond = CharactersPerSecond };
}