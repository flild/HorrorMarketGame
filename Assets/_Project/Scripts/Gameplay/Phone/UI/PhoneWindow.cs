using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Phone.UI
{
    public class PhoneWindow : WindowBase
    {
        [Header("UI References")]
        [SerializeField] private Transform _tasksContainer;
        [SerializeField] private PhoneTaskItemView _taskPrefab;

        // Пул объектов, чтобы не дергать GC каждый раз при сигнале
        private readonly List<PhoneTaskItemView> _activeItems = new();

        public override bool StopsTime => false;
        public override bool UnlocksCursor => true;

        public void RenderTasks(IReadOnlyList<PhoneTaskData> tasks)
        {
            // 1. Скрываем все текущие плашки
            foreach (var item in _activeItems)
            {
                item.gameObject.SetActive(false);
            }

            // 2. Включаем или создаем нужные под количество задач
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
                viewItem.Setup(tasks[i]);
            }
        }
    }
}
