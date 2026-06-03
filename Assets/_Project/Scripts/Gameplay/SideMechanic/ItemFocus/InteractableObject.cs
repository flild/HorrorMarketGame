using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Events")]
    [Tooltip("Сюда можно вешать логику прямо из инспектора (например, дернуть аниматор двери)")]
    public UnityEvent OnInteractEvent;

    [Header("UI")]
    [SerializeField] protected string _promptText = "Взаимодействовать [E]";
    // Слои для шейдера обводки
    private const int DefaultLayer = 0;
    private const int OutlineLayer = 9;

    private bool _isForceHighlighted = false;

    public virtual string InteractionPrompt => _promptText;
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

        gameObject.layer = targetLayer;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = targetLayer;
        }
    }

    // Добавляем пустую виртуальную реализацию, 
    // чтобы обычным предметам (ключам) не нужно было это переопределять.
    public virtual void EndInteract()
    {
    }
}