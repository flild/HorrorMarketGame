using Assets._Project.Scripts.Gameplay.Tasks.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets._Project.Scripts.Gameplay.Tasks
{
    public interface IQuestGenerator
    {
        /// <summary>
        /// Инициализация параметров генератора для текущей смены
        /// </summary>
        void InitializeShift(ShiftSettings settings);

        /// <summary>
        /// Пытается сгенерировать и выдать одну задачу. 
        /// Возвращает true, если задача успешно выдана.
        /// </summary>
        bool TryGenerateTask();

        /// <summary>
        /// Заполняет телефон начальным пулом задач
        /// </summary>
        void FillInitialTasks(int amount);
    }
}
