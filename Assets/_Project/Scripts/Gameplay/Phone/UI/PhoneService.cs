using System.Collections.Generic;
using System.Linq;
using Assets._Project.Scripts.Gameplay.Tasks.Data;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Phone.UI
{
    public class PhoneService : IPhoneService
    {
        private readonly SignalBus _signalBus;
        private readonly List<PhoneTaskData> _currentTasks = new();

        public IReadOnlyList<PhoneTaskData> CurrentTasks => _currentTasks;

        public PhoneService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void AddNewTask(TaskDefinition task)
        {
            if (_currentTasks.Any(t => t.Definition.Id == task.Id && t.State == NightTaskState.Active))
            {
                Debug.LogWarning($"[PhoneService] Задача {task.Id} уже активна. Пропускаем дубликат.");
                return;
            }

            _currentTasks.Add(new PhoneTaskData(task));

            // Кидаем сигнал, чтобы UI перерисовался и, возможно, проиграл звук уведомления
            _signalBus.Fire<PhoneTasksUpdatedSignal>();
        }

        public void SetTaskState(string taskId, NightTaskState newState)
        {
            var task = _currentTasks.FirstOrDefault(t => t.Definition.Id == taskId && t.State == NightTaskState.Active);

            if (task != null)
            {
                task.State = newState;
                _signalBus.Fire<PhoneTasksUpdatedSignal>();
            }
        }

        public void ClearAllTasks()
        {
            _currentTasks.Clear();
            _signalBus.Fire<PhoneTasksUpdatedSignal>();
        }
    }
}
