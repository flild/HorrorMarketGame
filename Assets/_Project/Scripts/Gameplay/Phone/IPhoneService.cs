using Assets._Project.Scripts.Gameplay.Tasks.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets._Project.Scripts.Gameplay.Phone
{
    public interface IPhoneService
    {
        IReadOnlyList<PhoneTaskData> CurrentTasks { get; }

        void AddNewTask(TaskDefinition task);
        void SetTaskState(string taskId, NightTaskState newState);
        void ClearAllTasks();
    }
}
