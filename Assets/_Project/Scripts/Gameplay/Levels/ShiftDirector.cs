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

        // Zenject сам вкинет зависимости в этот MonoBehaviour при загрузке сцены, 
        [Inject]
        public void Construct(ITimeService timeService, IQuestGenerator questGenerator)
        {
            _timeService = timeService;
            _questGenerator = questGenerator;
        }

        private void Start()
        {
            if (_currentShiftSettings == null)
            {
                Debug.LogError("[ShiftDirector] Не назначен ShiftSettings! Смена не запустится.");
                return;
            }

            StartShift();
        }

        private void StartShift()
        {
            Debug.Log($"[ShiftDirector] Запуск смены: День {_currentShiftSettings.DayNumber}");

            // Инициализируем мозг и таймер настройками конкретного дня
            _timeService.StartShift(_currentShiftSettings);
            _questGenerator.InitializeShift(_currentShiftSettings);

            // Выдаем стартовые задачи
            _questGenerator.FillInitialTasks(_initialTasksCount);
        }

        private void OnDestroy()
        {
            // Если мы выгружаем локацию (например, игрок умер или закончил смену),
            // обязательно глушим таймер, чтобы он не тикал в пустоту
            if (_timeService != null && _timeService.IsShiftActive)
            {
                _timeService.EndShift();
            }
        }

        // ==========================================
        // DEBUG SECTION (NaughtyAttributes)
        // ==========================================

        [Button("Debug: Выдать 1 задачу")]
        private void DebugForceGenerateTask()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Только в Play Mode!");
                return;
            }

            bool success = _questGenerator.TryGenerateTask();
            if (!success)
            {
                Debug.LogWarning("[ShiftDirector Debug] Не удалось выдать задачу (квоты исчерпаны или пул пуст).");
            }
        }

        [Button("Debug: Промотать 1 час (60 мин)")]
        private void DebugAdvanceOneHour()
        {
            if (!Application.isPlaying) return;

            _timeService.AddTaskTime(60);
            Debug.Log("[ShiftDirector Debug] Искусственно добавлен 1 час.");
        }

        [Button("Debug: Завершить фазу рутины (Сразу 04:00)")]
        private void DebugSkipToSurvivalPhase()
        {
            if (!Application.isPlaying) return;

            // Накидываем время с запасом, чтобы TimeService принудительно уперся в лимит фазы 1
            _timeService.AddTaskTime(_currentShiftSettings.TargetMinutesForPhaseOne);
            Debug.Log("[ShiftDirector Debug] Время промотано до начала фазы выживания.");
        }
    }
}
