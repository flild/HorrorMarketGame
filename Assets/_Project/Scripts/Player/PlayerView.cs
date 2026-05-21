using Project.Core.Input;
using Project.Core.Settings;
using UnityEngine;
using Zenject;

namespace Project.Player
{
    // Этот компонент вешается на рут Player 
    public class PlayerView : MonoBehaviour
    {
        [Header("Required References")]
        // SerializeField используется для Zenject Binding 'FromComponent' или прямого drag-and-drop в эдиторе
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform headTransform; // Объект "Head" для вращения по X (pitch)

        // Зависимости, получаемые через Inject
        private IInputService _inputService;
        private PlayerSettings _settings;

        // Локальные переменные состояния
        private float _verticalRotation; // Текущий угол наклона головы
        private Vector3 _velocity; // Вектор вертикальной скорости (гравитация)

        // Метод инжекции зависимостей (вызывается Zenject при старте сцены)
        [Inject]
        public void Construct(IInputService inputService, PlayerSettings settings)
        {
            _inputService = inputService;
            _settings = settings;
        }

        private void Start()
        {
            // Валидация: Проверяем, что ссылки настроены корректно в эдиторе
            ValidateReferences();

            // Настройка курсора
            LockCursor();
        }

        private void Update()
        {
            // Для соло-разработки Update допустим, но в идеале логику 
            // движения выносят в ITickable через Zenject
            HandleLookRotation();
            HandleMovement();
        }

        private void ValidateReferences()
        {
            if (characterController == null)
                Debug.LogError($"CharacterController not assigned on {gameObject.name} (PlayerView)", gameObject);

            if (headTransform == null)
                Debug.LogError($"Head Transform not assigned on {gameObject.name} (PlayerView)", gameObject);
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // --- Логика Вращения (Look) ---
        private void HandleLookRotation()
        {
            Vector2 lookInput = _inputService.LookInput * _settings.mouseSensitivity;

            // 1. Горизонтальное вращение: Вращаем КОРПУС (Player root) по оси Y
            transform.Rotate(Vector3.up * lookInput.x);

            // 2. Вертикальное вращение: Наклоняем ГОЛОВУ (Head) по оси X
            _verticalRotation -= lookInput.y;

            // Ограничиваем угол, чтобы игрок не видел свои пятки
            _verticalRotation = Mathf.Clamp(_verticalRotation, -_settings.lowerLookLimit, _settings.upperLookLimit);

            headTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);

            // Cinemachine Brain на Main Camera автоматически привяжется к положению headTransform
        }

        // --- Логика Перемещения (Move) ---
        private void HandleMovement()
        {
            Vector2 moveInput = _inputService.MoveInput;
            bool isSprinting = _inputService.IsSprinting;

            // Определяем текущую скорость из ScriptableObject настроек
            float targetSpeed = isSprinting ? _settings.sprintSpeed : _settings.walkSpeed;

            // Рассчитываем направление движения относительно ТЕКУЩЕГО поворота рута
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

            // Нормализуем, чтобы диагональное движение не было быстрее
            if (moveDirection.sqrMagnitude > 1) moveDirection.Normalize();

            // Обработка Гравитации
            HandleGravity();

            // Сборка финального вектора движения (Горизонталь + Вертикаль)
            Vector3 finalMoveCommand = moveDirection * (targetSpeed * Time.deltaTime) + _velocity * Time.deltaTime;

            // Передача команды CharacterController
            characterController.Move(finalMoveCommand);
        }

        private void HandleGravity()
        {
            // Проверка заземления (CharacterController.isGrounded обновляется после Move())
            if (characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Прижимная сила, чтобы не прыгал на спусках
                return;
            }

            // Применяем гравитацию
            _velocity.y += _settings.gravity * Time.deltaTime;
        }
    }
}