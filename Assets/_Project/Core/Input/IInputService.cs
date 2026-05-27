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

        void Enable();
        void Disable();
    }
}
