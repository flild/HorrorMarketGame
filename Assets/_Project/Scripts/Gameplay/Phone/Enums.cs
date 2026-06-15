using Assets._Project.Scripts.Gameplay.Tasks.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets._Project.Scripts.Gameplay.Phone
{
    public enum NightTaskState
    {
        Active,
        Completed,
        Failed
    }

    public class PhoneTaskData
    {
        public TaskDefinition Definition { get; }
        public NightTaskState State { get; set; }
        public int CurrentProgress { get; set; } // Добавили поле

        public PhoneTaskData(TaskDefinition definition)
        {
            Definition = definition;
            State = NightTaskState.Active;
            CurrentProgress = 0;
        }
    }

    // Сигнал для UI, что список задач изменился (добавили, выполнили или провалили)
    public struct PhoneTasksUpdatedSignal { }

    // Сигнал появления нового сообщения (спам/новости из GDD)
    public struct PhoneMessageReceivedSignal
    {
        public string Sender;
        public string Text;
    }
    public struct PlayerActionSignal
    {
        public string ActionId;
        public int Amount; // Обычно 1, но вдруг ты добавишь уборку разом 5 банок
    }
}
