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
    private IInteractable _currentInteractable; // То, на что мы смотрим
    private IInteractable _interactingObject;// То, что мы сейчас "зажали/держим"

    [Inject]
    public void Construct(IInputService input, SignalBus signalBus)
    {
        _input = input;
        _signalBus = signalBus;
    }

    private void Start()
    {
        _input.OnInteractStarted += HandleInteractStart;
        _input.OnInteractCanceled += HandleInteractEnd;
    }

    private void HandleRaycast()
    {
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
                    _signalBus.Fire(new InteractableFocusSignal { IsFocused = true, PromptText = _currentInteractable.InteractionPrompt });
                }
                return;
            }
        }

        ClearFocus();
    }


    private void HandleInteractStart()
    {
        if (_currentInteractable != null)
        {
            _interactingObject = _currentInteractable;
            _interactingObject.Interact();
        }
    }

    private void HandleInteractEnd()
    {
        if (_interactingObject != null)
        {
            _interactingObject.EndInteract();
            _interactingObject = null;
        }
    }
    private void ClearFocus()
    {
        if (_currentInteractable != null)
        {
            // Жестко прерываем взаимодействие, если отвернулись в процессе "зажатия"
            if (_interactingObject == _currentInteractable)
            {
                HandleInteractEnd();
            }

            _currentInteractable.OnLoseFocus();
            _currentInteractable = null;
            _signalBus.Fire(new InteractableFocusSignal { IsFocused = false });
        }
    }
    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.OnInteractStarted -= HandleInteractStart;
            _input.OnInteractCanceled -= HandleInteractEnd;
        }
    }

    private void Update()
    {
        // В апдейте остается ТОЛЬКО рейкаст
        HandleRaycast();
    }

}
