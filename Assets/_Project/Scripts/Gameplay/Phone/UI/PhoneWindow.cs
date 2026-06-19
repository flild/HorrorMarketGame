using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Phone.UI
{
    public class PhoneWindow : WindowBase
    {
        [Header("UI References")]
        [SerializeField] private Transform _tasksContainer; // Content внутри ScrollView
        [SerializeField] private PhoneTaskItemView _taskPrefab;

        private readonly List<PhoneTaskItemView> _activeItems = new();

        public override bool StopsTime => false;
        public override bool UnlocksCursor => true;

        // Структура для передачи готовых данных от Презентера
        public struct TaskUIData
        {
            public string Title;
            public string Description;
            public bool IsCompleted;
        }

        public void RenderTasks(IReadOnlyList<TaskUIData> tasks)
        {
            // 1. Скрываем все плашки (возвращаем в пул)
            foreach (var item in _activeItems)
            {
                item.gameObject.SetActive(false);
            }

            // 2. Достаем или создаем нужное количество
            for (int i = 0; i < tasks.Count; i++)
            {
                PhoneTaskItemView viewItem;

                if (i < _activeItems.Count)
                {
                    viewItem = _activeItems[i];
                }
                else
                {
                    viewItem = Instantiate(_taskPrefab, _tasksContainer);
                    _activeItems.Add(viewItem);
                }

                viewItem.gameObject.SetActive(true);
                viewItem.Setup(tasks[i].Title, tasks[i].Description, tasks[i].IsCompleted);
            }
        }
    }
}
