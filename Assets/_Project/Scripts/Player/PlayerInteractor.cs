using Project.Core.Input;
using UnityEngine;
using Zenject;
using Assets._Project.Scripts.Gameplay.Inventory.Interfaces; // Добавили для IInteractable

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _interactionDistance = 2.5f;
    [SerializeField] private LayerMask _interactableLayerMask;
    private SignalBus _signalBus;

    private IInputService _input;
    private IInteractable _currentInteractable; // То, на что мы смотрим
    private IInteractable _interactingObject;   // То, что мы сейчас "зажали/держим"

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

                    // КИДАЕМ В ШИНУ ССЫЛКУ НА ОБЪЕКТ, А НЕ ТЕКСТ
                    _signalBus.Fire(new InteractableFocusSignal
                    {
                        IsFocused = true,
                        FocusedObject = _currentInteractable
                    });
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
            // Проверяем на жизнь и здесь, так как игрок может держать "E", 
            // пока объект уничтожается скриптом.
            if (_interactingObject as Object != null)
            {
                _interactingObject.EndInteract();
            }
            _interactingObject = null;
        }
    }

    private void ClearFocus()
    {
        if (_currentInteractable != null)
        {
            // Кастуем к Object, чтобы Unity правильно проверила, жив ли нативный объект
            bool isAlive = _currentInteractable as Object != null;

            // Жестко прерываем взаимодействие, если отвернулись в процессе "зажатия"
            if (_interactingObject == _currentInteractable)
            {
                HandleInteractEnd();
            }

            if (isAlive)
            {
                _currentInteractable.OnLoseFocus();
            }

            _currentInteractable = null;

            // Теперь код гарантированно доходит сюда, и плашка UI пропадает
            _signalBus.Fire(new InteractableFocusSignal
            {
                IsFocused = false,
                FocusedObject = null
            });
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