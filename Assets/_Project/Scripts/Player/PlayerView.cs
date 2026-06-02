using Project.Core.Input;
using Project.Core.Settings; // <-- Добавили твой неймспейс
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _headTransform;
    
    // Заменяем кучу разрозненных переменных на одну ссылку на твой конфиг
    [Header("Settings")]
    [SerializeField] private PlayerSettings _settings;

    private IInputService _input;
    private float _verticalVelocity;
    private float _xRotation = 0f;

    [Inject]
    public void Construct(IInputService input)
    {
        _input = input;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        // Берем сенсу из конфига
        Vector2 lookInput = _input.LookInput * _settings.mouseSensitivity;

        _xRotation -= lookInput.y;
        
        // Камера: вверх - это отрицательный угол, вниз - положительный. 
        // Используем твои верхний и нижний лимиты.
        _xRotation = Mathf.Clamp(_xRotation, -_settings.upperLookLimit, _settings.lowerLookLimit);
        
        _headTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = Vector2.ClampMagnitude(_input.MoveInput, 1f);
        
        // Берем скорости из конфига
        float currentSpeed = _input.IsSprinting ? _settings.sprintSpeed : _settings.walkSpeed;

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0)
                _verticalVelocity = -2f;

            if (_input.IsJumping)
            {
                // Захардкодил высоту прыжка в 1, потому что в твоем конфиге ее пока нет.
                // Захочешь — добавь public float jumpHeight = 1f; в PlayerSettings.
                _verticalVelocity = Mathf.Sqrt(1f * -2f * _settings.gravity);
            }
        }

        // Гравитация из конфига
        _verticalVelocity += _settings.gravity * Time.deltaTime;

        Vector3 finalMovement = moveDirection * currentSpeed;
        finalMovement.y = _verticalVelocity;

        _characterController.Move(finalMovement * Time.deltaTime);
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        _characterController.enabled = false;
        transform.position = position;
        transform.rotation = rotation;
        _xRotation = 0f;
        _headTransform.localRotation = Quaternion.identity;
        _characterController.enabled = true;
    }
}