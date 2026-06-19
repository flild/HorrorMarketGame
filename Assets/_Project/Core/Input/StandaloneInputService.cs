using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core.Input
{
    // Реализация инпута, которая живет пока включен объект или сцена
    public class StandaloneInputService : IInputService, IDisposable
    {
        private readonly GameInput _input;

        public Vector2 MoveInput => _input.Player.Move.ReadValue<Vector2>();
        public Vector2 LookInput => _input.Player.Look.ReadValue<Vector2>();
        public bool IsSprinting => _input.Player.Sprint.IsPressed();

        // Читаем триггер для прыжка (срабатывает 1 раз за нажатие)
        public bool IsJumping => _input.Player.Jump.triggered;
        // Читаем удержание для приседа
        public bool IsCrouching => _input.Player.Crouch.IsPressed();

        public event Action OnPauseTriggered;
        public event Action OnDropTriggered;
        public event Action OnInteractStarted;
        public event Action OnInteractCanceled;
        public event Action OnPhoneToggled;


        public StandaloneInputService()
        {
            _input = new GameInput();
            _input.Player.TogglePause.performed += HandlePauseInput;
            _input.UI.TogglePause.performed += HandlePauseInput;
            _input.Player.Drop.performed += _ => OnDropTriggered?.Invoke();
            _input.Player.Interact.started += _ => OnInteractStarted?.Invoke();
            _input.Player.Interact.canceled += _ => OnInteractCanceled?.Invoke();

            _input.Player.TogglePhone.performed += HandlePhoneInput;
            _input.UI.TogglePhone.performed += HandlePhoneInput;

            Enable();
        }
        public void SetUIMode(bool isUIActive)
        {
            if (isUIActive)
            {
                // Отрубаем управление персонажем, врубаем управление UI
                _input.Player.Disable();
                _input.UI.Enable();
            }
            else
            {
                // Наоборот
                _input.UI.Disable();
                _input.Player.Enable();
            }
        }

        private void HandlePauseInput(InputAction.CallbackContext context)
        {
            OnPauseTriggered?.Invoke();
        }

        private void HandlePhoneInput(InputAction.CallbackContext context)
        {
            OnPhoneToggled?.Invoke();
        }

        public string GetBindingName(string actionName)
        {
            // Ищем экшен по имени (например "Interact")
            var action = _input.asset.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"[InputService] Не найден экшен с именем: {actionName}");
                return "[?]";
            }

            // Берем отображаемое имя текущего бинда. 
            // DontIncludeInteractions убирает лишний мусор вроде "Hold" или "Press" из названия
            return $"[{action.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontIncludeInteractions)}]";
        }

        public void Enable() => _input.Player.Enable();
        public void Disable() => _input.Player.Disable();

        public void Dispose()
        {
            _input.Player.TogglePause.performed -= HandlePauseInput;
            _input.UI.TogglePause.performed -= HandlePauseInput;
            _input.Player.Drop.performed -= _ => OnDropTriggered?.Invoke();

            _input.Player.Interact.started -= _ => OnInteractStarted?.Invoke();
            _input.Player.Interact.canceled -= _ => OnInteractCanceled?.Invoke();

            _input.Player.TogglePhone.performed -= HandlePhoneInput;
            _input.UI.TogglePhone.performed -= HandlePhoneInput;

            Disable();
            _input.Dispose();
        }
    }
}
