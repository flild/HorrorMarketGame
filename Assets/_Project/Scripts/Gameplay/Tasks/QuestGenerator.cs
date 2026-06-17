using Assets._Project.Scripts.Gameplay.Phone;
using Assets._Project.Scripts.Gameplay.Tasks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Tasks
{
    public class QuestGenerator : IQuestGenerator
    {
        private readonly IPhoneService _phoneService;

        private ShiftSettings _currentSettings;
        // Словарь для отслеживания, сколько раз мы уже выдали конкретную задачу за сегодня
        private readonly Dictionary<string, int> _issuedQuotas = new();

        public QuestGenerator(IPhoneService phoneService)
        {
            _phoneService = phoneService;
        }

        public void InitializeShift(ShiftSettings settings)
        {
            _currentSettings = settings;
            _issuedQuotas.Clear();
        }

        public void FillInitialTasks(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                TryGenerateTask();
            }
        }

        public bool TryGenerateTask()
        {
            if (_currentSettings == null)
            {
                Debug.LogError("[QuestGenerator] Генератор не инициализирован настройками смены!");
                return false;
            }

            var availableTasks = GetValidTasksForRoll();

            if (availableTasks.Count == 0)
            {
                Debug.Log("[QuestGenerator] Нет доступных задач для выдачи (квоты исчерпаны или все в работе).");
                return false;
            }

            var selectedTaskData = RollByWeight(availableTasks);
            IssueTask(selectedTaskData.Task);

            return true;
        }

        private List<TaskSpawnData> GetValidTasksForRoll()
        {
            var validTasks = new List<TaskSpawnData>();

            foreach (var spawnData in _currentSettings.AvailableTasks)
            {
                string taskId = spawnData.Task.Id;

                // 1. Проверяем квоту (если MaxQuotaPerShift == 0, значит безлимит)
                if (spawnData.MaxQuotaPerShift > 0)
                {
                    _issuedQuotas.TryGetValue(taskId, out int issuedCount);
                    if (issuedCount >= spawnData.MaxQuotaPerShift)
                    {
                        continue; // Лимит исчерпан
                    }
                }

                // 2. Проверяем дубликаты. Игроку незачем мыть пол в двух разных заданиях одновременно
                bool isAlreadyActive = _phoneService.CurrentTasks
                    .Any(t => t.State == NightTaskState.Active && t.Definition.Id == taskId);

                if (isAlreadyActive)
                {
                    continue;
                }

                validTasks.Add(spawnData);
            }

            return validTasks;
        }

        private TaskSpawnData RollByWeight(List<TaskSpawnData> validTasks)
        {
            int totalWeight = validTasks.Sum(t => t.SpawnWeight);
            int roll = UnityEngine.Random.Range(0, totalWeight);

            int currentWeight = 0;

            foreach (var taskData in validTasks)
            {
                currentWeight += taskData.SpawnWeight;
                if (roll < currentWeight)
                {
                    return taskData;
                }
            }

            // Fallback (теоретически недостижим при правильной математике)
            return validTasks.Last();
        }

        private void IssueTask(Data.TaskDefinition task)
        {
            // Увеличиваем счетчик выданных квот
            if (!_issuedQuotas.TryAdd(task.Id, 1))
            {
                _issuedQuotas[task.Id]++;
            }

            _phoneService.AddNewTask(task);
            Debug.Log($"[QuestGenerator] Сгенерирована задача: {task.Id}");
        }
    }
}
