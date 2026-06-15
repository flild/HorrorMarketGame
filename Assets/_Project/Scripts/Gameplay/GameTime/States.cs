using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.GameTime
{
    public enum ShiftPhase
    {
        TaskDriven = 0, // Время идет только от задач (первые 10 часов)
        Survival = 1,   // Таймер тикает сам (последние 2 часа до 06:00)
        Overtime = 2    // После 06:00, таймер тикает сам, начисляется бонус к деньгам
    }

    public struct TimeTickSignal
    {
        public int TotalMinutesElapsed;
        public string FormattedTime; // Формат "18:00" для UI
    }

    public struct ShiftPhaseChangedSignal
    {
        public ShiftPhase NewPhase;
    }
}