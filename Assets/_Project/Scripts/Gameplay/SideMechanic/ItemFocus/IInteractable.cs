using UnityEngine;

public interface IInteractable
{

    string InteractionPrompt { get; }

    // Вызывается, когда игрок навел прицел
    void OnFocus();

    // Вызывается, когда игрок убрал прицел
    void OnLoseFocus();

    // Само действие (нажатие ЛКМ или 'E')
    void Interact();

    // Задел на будущее для квестов: принудительная подсветка полки
    // colorState - например, 0 = дефолт, 1 = квестовый (желтый), 2 = ошибка (красный)
    void ForceHighlight(bool state, int colorState = 0);
}
