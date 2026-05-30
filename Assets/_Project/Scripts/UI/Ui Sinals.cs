using UnityEngine;

public struct InteractableFocusSignal
{
    public bool IsFocused;
    public string PromptText;
}

// Задел на будущее для записок
public struct ReadNoteSignal
{
    public string LocalizationKey; // Ключ для перевода
}