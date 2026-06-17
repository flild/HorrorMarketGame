using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Events")]
    [Tooltip("Сюда можно вешать логику прямо из инспектора (например, дернуть аниматор двери)")]
    public UnityEvent OnInteractEvent;



    [Header("UI")]
    [Tooltip("Ключ локализации из CSV (например: ui_prompt_interact)")]
    [SerializeField] protected string _promptKey = "ui_prompt_interact";
    public virtual PromptData InteractionPrompt => new PromptData(_promptKey);

    // Слои для шейдера обводки
    private const int DefaultLayer = 0;
    private const int OutlineLayer = 9;

    private bool _isForceHighlighted = false;
    // Делаем методы virtual, чтобы ты мог отнаследоваться от этого класса, 
    // если понадобится сложная логика (например, класс DraggableBox : InteractableObject)
    public virtual void OnFocus()
    {
        if (!_isForceHighlighted) EnableOutline(true);
    }

    public virtual void OnLoseFocus()
    {
        if (!_isForceHighlighted) EnableOutline(false);
    }

    public virtual void Interact()
    {
        // Вызываем все подписки из инспектора
        OnInteractEvent?.Invoke();
    }

    public virtual void ForceHighlight(bool state, int colorState = 0)
    {
        _isForceHighlighted = state;
        EnableOutline(state);
    }

    protected virtual void EnableOutline(bool enable)
    {
        int targetLayer = enable ? OutlineLayer : DefaultLayer;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var rnd in renderers)
        {
            rnd.gameObject.layer = targetLayer;
        }
    }

    // Добавляем пустую виртуальную реализацию, 
    // чтобы обычным предметам (ключам) не нужно было это переопределять.
    public virtual void EndInteract()
    {
    }
}