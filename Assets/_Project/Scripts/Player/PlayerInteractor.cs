using Project.Core.Input;
using UnityEngine;
using Zenject;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _interactionDistance = 2.5f;
    [SerializeField] private LayerMask _interactableLayerMask;
    private SignalBus _signalBus;

    private IInputService _input;
    private IInteractable _currentInteractable;

    [Inject]
    public void Construct(IInputService input, SignalBus signalBus)
    {
        _input = input;
        _signalBus = signalBus;
    }

    private void Update()
    {
        HandleRaycast();
        HandleInput();
    }

    private void HandleRaycast()
    {
        // ДЕБАГ: Рисуем красный луч в окне Scene. В самой игре (Game) его не видно.
        Debug.DrawRay(_cameraTransform.position, _cameraTransform.forward * _interactionDistance, Color.red);

        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _interactionDistance, _interactableLayerMask))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                if (_currentInteractable != interactable)
                {
                    ClearFocus();
                    _currentInteractable = interactable;
                    _currentInteractable.OnFocus();
                    _signalBus.Fire(new InteractableFocusSignal { IsFocused = true, PromptText = "Взять ключи [E]" });
                }
                return;
            }
        }

        ClearFocus();
    }

    private void HandleInput()
    {
        // Если мы на что-то смотрим и кнопка нажата в этот кадр
        if (_currentInteractable != null && _input.IsInteracting)
        {
            _currentInteractable.Interact();
        }
    }

    private void ClearFocus()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.OnLoseFocus();
            _currentInteractable = null;
            _signalBus.Fire(new InteractableFocusSignal { IsFocused = false });
        }
    }
}
