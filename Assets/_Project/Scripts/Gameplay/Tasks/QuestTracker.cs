using Assets._Project.Scripts.Gameplay.Phone;
using Assets._Project.Scripts.Gameplay.GameTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Tasks
{
    public class QuestTracker : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IPhoneService _phoneService;
        private readonly ITimeService _timeService;

        public QuestTracker(SignalBus signalBus, IPhoneService phoneService, ITimeService timeService)
        {
            _signalBus = signalBus;
            _phoneService = phoneService;
            _timeService = timeService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<PlayerActionSignal>(OnPlayerActionFired);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerActionSignal>(OnPlayerActionFired);
        }

        private void OnPlayerActionFired(PlayerActionSignal signal)
        {
            // Ищем все активные таски, которые ждут именно этот ActionId
            var activeTasks = _phoneService.CurrentTasks
                .Where(t => t.State == NightTaskState.Active && t.Definition.TargetActionId == signal.ActionId)
                .ToList();

            foreach (var task in activeTasks)
            {
                task.CurrentProgress += signal.Amount;

                // Кидаем сигнал, чтобы UI обновил циферки прогресса (если захочешь их выводить)
                _signalBus.Fire<PhoneTasksUpdatedSignal>();

                if (task.CurrentProgress >= task.Definition.RequiredAmount)
                {
                    CompleteTask(task);
                }
            }
        }

        private void CompleteTask(PhoneTaskData task)
        {
            Debug.Log($"[QuestTracker] Задача выполнена: {task.Definition.DisplayName}");

            _phoneService.SetTaskState(task.Definition.Id, NightTaskState.Completed);

            // Двигаем скрытое время смены!
            _timeService.AddTaskTime(task.Definition.TimeRewardMinutes);
        }
    }
}
