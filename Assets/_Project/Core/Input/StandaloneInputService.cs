using System;
using UnityEngine;

namespace Project.Core.Input
{
    // Реализация инпута, которая живет пока включен объект или сцена
    public class StandaloneInputService : IInputService, IDisposable
    {
        private readonly GameInput _input;

        public Vector2 MoveInput => _input.Player.Move.ReadValue<Vector2>();
        public Vector2 LookInput => _input.Player.Look.ReadValue<Vector2>();
        public bool IsSprinting => _input.Player.Sprint.IsPressed();

        public StandaloneInputService()
        {
            _input = new GameInput();
            Enable(); // По умолчанию включаем
        }

        public void Enable() => _input.Player.Enable();
        public void Disable() => _input.Player.Disable();

        public void Dispose()
        {
            Disable();
            _input.Dispose();

        }
    }
}
