using Assets._Project.Scripts.Gameplay.GameTime;
using Assets._Project.Scripts.Gameplay.Tasks;
using Assets._Project.Scripts.Gameplay.Tasks.Data;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Levels
{
    public class ShiftDirector : MonoBehaviour
    {
        [Header("Shift Configuration")]
        [SerializeField] private ShiftSettings _currentShiftSettings;
        [SerializeField] private int _initialTasksCount = 3;

        private ITimeService _timeService;
        private IQuestGenerator _questGenerator;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(ITimeService timeService, IQuestGenerator questGenerator, SignalBus signalBus)
        {
            _timeService = timeService;
            _questGenerator = questGenerator;
            _signalBus = signalBus;
        }

        private void Start()
        {
            if (_currentShiftSettings == null)
            {
                Debug.LogError("[ShiftDirector] Не назначен ShiftSettings! Смена сломана.");
                return;
            }

            // Подписываемся на терминал
            _signalBus.Subscribe<ShiftStartRequestedSignal>(OnShiftStartRequested);
            _signalBus.Subscribe<ShiftEndRequestedSignal>(OnShiftEndRequested);

            Debug.Log("[ShiftDirector] Сцена загружена. Жду пробития в терминале...");
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<ShiftStartRequestedSignal>(OnShiftStartRequested);
            _signalBus.Unsubscribe<ShiftEndRequestedSignal>(OnShiftEndRequested);

            if (_timeService != null && _timeService.IsShiftActive)
            {
                _timeService.EndShift();
            }
        }

        private void OnShiftStartRequested()
        {
            if (_timeService.IsShiftActive) return;

            Debug.Log($"[ShiftDirector] СТАРТ СМЕНЫ: День {_currentShiftSettings.DayNumber}");

            _timeService.StartShift(_currentShiftSettings);
            _questGenerator.InitializeShift(_currentShiftSettings);
            _questGenerator.FillInitialTasks(_initialTasksCount);
        }

        private void OnShiftEndRequested()
        {
            if (!_timeService.IsShiftActive) return;

            Debug.Log("[ShiftDirector] КОНЕЦ СМЕНЫ. Считаем бабки и переходим к результатам.");

            // Тормозим таймер
            _timeService.EndShift();

            // TODO: Вызов окна статистики, начисление денег за овертайм и загрузка сцены Дома
        }

        // ==========================================
        // DEBUG SECTION (NaughtyAttributes)
        // ==========================================
        [Button("Debug: Форсировать СТАРТ смены")]
        private void DebugStartShift() => _signalBus.Fire<ShiftStartRequestedSignal>();

        [Button("Debug: Выдать 1 задачу")]
        private void DebugForceGenerateTask() => _questGenerator.TryGenerateTask();

        [Button("Debug: Промотать 1 час (60 мин)")]
        private void DebugAdvanceOneHour() => _timeService.AddTaskTime(60);
    }
}
