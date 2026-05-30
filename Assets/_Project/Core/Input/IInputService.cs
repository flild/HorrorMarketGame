using System;
using UnityEngine;
namespace Project.Core.Input
{
    public interface IInputService
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsSprinting { get; }
        bool IsJumping { get; }
        bool IsCrouching { get; }

        void SetUIMode(bool isUIActive);

        event Action OnPauseTriggered;
        event Action OnDropTriggered;
        event Action OnInteractTriggered;
        void Enable();
        void Disable();
    }
}
