using Project.Core.Input;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterController))]
public class PlayerView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _headTransform; // Пустышка для камеры

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _sprintSpeed = 5.5f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Look")]
    [SerializeField] private float _mouseSensitivity = 15f;
    [SerializeField] private float _maxLookAngle = 80f;

    private IInputService _input;
    private float _verticalVelocity;
    private float _xRotation = 0f; // Угол наклона головы

    [Inject]
    public void Construct(IInputService input)
    {
        _input = input;
    }

    private void Start()
    {
        // Лочим курсор, чтобы не улетал на второй монитор
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
        Vector2 lookInput = _input.LookInput * (_mouseSensitivity * Time.deltaTime);

        // Вращаем голову вверх-вниз (ось X)
        _xRotation -= lookInput.y;
        _xRotation = Mathf.Clamp(_xRotation, -_maxLookAngle, _maxLookAngle);
        _headTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // Вращаем всё тело игрока влево-вправо (ось Y)
        transform.Rotate(Vector3.up * lookInput.x);
    }

    private void HandleMovement()
    {
        // Читаем инпут
        Vector2 moveInput = _input.MoveInput;
        float currentSpeed = _input.IsSprinting ? _sprintSpeed : _walkSpeed;

        // Переводим 2D инпут в 3D вектор направления относительно поворота игрока
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Гравитация
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; // Прижимаем к полу, чтобы не прыгал на спусках
        }
        _verticalVelocity += _gravity * Time.deltaTime;

        // Итоговый вектор
        Vector3 finalMovement = moveDirection * currentSpeed;
        finalMovement.y = _verticalVelocity;

        // Двигаем
        _characterController.Move(finalMovement * Time.deltaTime);
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        // Вырубаем контроллер, иначе Юнити проигнорит смену позиции
        _characterController.enabled = false;

        transform.position = position;
        transform.rotation = rotation;

        // Сбрасываем наклон камеры, чтобы игрок не смотрел в пол после спавна
        _xRotation = 0f;
        _headTransform.localRotation = Quaternion.identity;

        // Врубаем обратно
        _characterController.enabled = true;
    }
}