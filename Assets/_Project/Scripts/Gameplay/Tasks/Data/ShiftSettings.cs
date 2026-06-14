using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Tasks.Data
{
    [CreateAssetMenu(fileName = "Day_1_Settings", menuName = "Project/Levels/Shift Settings")]
    public class ShiftSettings : ScriptableObject
    {
        [Header("General")]
        [SerializeField, Range(1, 7)] private int _dayNumber = 1;

        [Header("Time Progression (Phase 1 - Task Driven)")]
        [Tooltip("Квота минут, после которой включается реальное время (10 часов = 600 минут)")]
        [SerializeField] private int _targetMinutesForPhaseOne = 600;

        [Header("Survival Phase (Phase 2 - Real Time)")]
        [Tooltip("Длительность одного игрового часа в реальных секундах (по GDD: 5 минут = 300 сек)")]
        [SerializeField] private float _realSecondsPerGameHour = 300f;

        [Header("Economy")]
        [Tooltip("Сколько платят за одну реальную минуту овертайма")]
        [SerializeField] private float _overtimePayRatePerMinute = 10f;
        [Tooltip("Штраф за потерю сознания (неоплаченные часы)")]
        [SerializeField] private float _faintPenalty = 500f;

        [Header("Tasks Pool")]
        [Tooltip("Пул доступных задач на эту смену с их весами")]
        [SerializeField] private List<TaskSpawnData> _availableTasks = new();

        [Header("Threat & AI")]
        [Tooltip("Множитель агрессивности окружения на эту смену")]
        [SerializeField, Range(0.1f, 3f)] private float _paranoiaMultiplier = 1f;

        // Публичные свойства только для чтения (иммутабельность данных в рантайме)
        public int DayNumber => _dayNumber;
        public int TargetMinutesForPhaseOne => _targetMinutesForPhaseOne;
        public float RealSecondsPerGameHour => _realSecondsPerGameHour;
        public float OvertimePayRatePerMinute => _overtimePayRatePerMinute;
        public float FaintPenalty => _faintPenalty;
        public IReadOnlyList<TaskSpawnData> AvailableTasks => _availableTasks;
        public float ParanoiaMultiplier => _paranoiaMultiplier;
    }

    [Serializable]
    public struct TaskSpawnData
    {
        public TaskDefinition Task;

        [Tooltip("Вес для рандомизатора (чем больше число относительно других, тем чаще выпадает задача)")]
        [Range(1, 100)] public int SpawnWeight;

        [Tooltip("Максимум таких задач за смену (0 = безлимитно)")]
        public int MaxQuotaPerShift;
    }
}
