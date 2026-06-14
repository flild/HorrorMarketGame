using Assets._Project.Scripts.Gameplay.Tasks.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Time
{
    public class ShiftTimeService : ITimeService, ITickable
    {
        private readonly SignalBus _signalBus;

        private ShiftSettings _currentSettings;
        private ShiftPhase _currentPhase;
        private bool _isShiftActive;

        // Внутренние счетчики
        private int _totalMinutesElapsed;
        private int _absoluteTimeMinutes; // 18:00 = 1080
        private float _realTimeAccumulator;

        // Константы
        private const int ShiftStartMinutes = 1080; // 18:00
        private const int PhaseOneEndMinutes = 1680; // 04:00 (1080 + 10 часов)
        private const int ShiftEndMinutes = 1800;    // 06:00 (1080 + 12 часов)

        public ShiftPhase CurrentPhase => _currentPhase;
        public bool IsShiftActive => _isShiftActive;

        public ShiftTimeService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void StartShift(ShiftSettings settings)
        {
            _currentSettings = settings;
            _totalMinutesElapsed = 0;
            _absoluteTimeMinutes = ShiftStartMinutes;
            _realTimeAccumulator = 0f;

            _currentPhase = ShiftPhase.TaskDriven;
            _isShiftActive = true;

            FireTimeUpdate();
            _signalBus.Fire(new ShiftPhaseChangedSignal { NewPhase = _currentPhase });
        }

        public void EndShift()
        {
            _isShiftActive = false;
        }

        public void AddTaskTime(int minutesToAdvance)
        {
            if (!_isShiftActive || _currentPhase != ShiftPhase.TaskDriven)
                return;

            AdvanceTime(minutesToAdvance);
        }

        // Вызывается Zenject'ом каждый кадр (замена Update)
        public void Tick()
        {
            if (!_isShiftActive || _currentPhase == ShiftPhase.TaskDriven)
                return;

            // Считаем, сколько реальных секунд нужно для одной игровой минуты
            float realSecondsPerGameMinute = _currentSettings.RealSecondsPerGameHour / 60f;

            _realTimeAccumulator += UnityEngine.Time.deltaTime;

            if (_realTimeAccumulator >= realSecondsPerGameMinute)
            {
                int minutesToAdd = Mathf.FloorToInt(_realTimeAccumulator / realSecondsPerGameMinute);
                _realTimeAccumulator -= minutesToAdd * realSecondsPerGameMinute;

                AdvanceTime(minutesToAdd);
            }
        }

        private void AdvanceTime(int minutes)
        {
            _totalMinutesElapsed += minutes;
            _absoluteTimeMinutes += minutes;

            CheckPhaseTransitions();
            FireTimeUpdate();
        }

        private void CheckPhaseTransitions()
        {
            // Переход из рутины в сурвайвл (04:00)
            if (_currentPhase == ShiftPhase.TaskDriven && _absoluteTimeMinutes >= PhaseOneEndMinutes)
            {
                // Защита от перескока: фиксируем время ровно на 04:00, если задача дала слишком много минут
                _absoluteTimeMinutes = PhaseOneEndMinutes;
                _currentPhase = ShiftPhase.Survival;
                _signalBus.Fire(new ShiftPhaseChangedSignal { NewPhase = _currentPhase });
            }
            // Переход из сурвайвла в овертайм (06:00)
            else if (_currentPhase == ShiftPhase.Survival && _absoluteTimeMinutes >= ShiftEndMinutes)
            {
                _currentPhase = ShiftPhase.Overtime;
                _signalBus.Fire(new ShiftPhaseChangedSignal { NewPhase = _currentPhase });
            }
        }

        private void FireTimeUpdate()
        {
            // Форматируем для UI. Берем остаток от 24 часов (1440 минут)
            int displayHours = (_absoluteTimeMinutes / 60) % 24;
            int displayMinutes = _absoluteTimeMinutes % 60;
            string formatted = $"{displayHours:D2}:{displayMinutes:D2}";

            _signalBus.Fire(new TimeTickSignal
            {
                TotalMinutesElapsed = _totalMinutesElapsed,
                FormattedTime = formatted
            });
        }
    }
}
