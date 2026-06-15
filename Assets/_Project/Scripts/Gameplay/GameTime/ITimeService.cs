using Assets._Project.Scripts.Gameplay.Tasks.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets._Project.Scripts.Gameplay.GameTime
{
    public interface ITimeService
    {
        ShiftPhase CurrentPhase { get; }
        bool IsShiftActive { get; }

        /// <summary>
        /// Инициализация параметров конкретного дня
        /// </summary>
        void StartShift(ShiftSettings settings);

        /// <summary>
        /// Вызывается менеджером задач при завершении интерактива
        /// </summary>
        void AddTaskTime(int minutesToAdvance);

        void EndShift();
    }
}
