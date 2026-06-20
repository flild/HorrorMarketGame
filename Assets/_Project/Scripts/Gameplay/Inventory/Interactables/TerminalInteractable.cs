using Assets._Project.Scripts.Gameplay.GameTime;
using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Project.Core.Input;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Interactables
{
    public class TerminalInteractable : InteractableObject, IInteractable
    {
        private ITimeService _timeService;
        private IInputService _inputService;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(ITimeService timeService, IInputService inputService, SignalBus signalBus)
        {
            _timeService = timeService;
            _inputService = inputService;
            _signalBus = signalBus;
        }

        public override PromptData InteractionPrompt
        {
            get
            {
                string interactBind = _inputService.GetBindingName("Interact");

                // Если смена еще не идет — предлагаем начать
                if (!_timeService.IsShiftActive)
                {
                    return new PromptData("ui_prompt_clock_in", interactBind);
                }

                // Если смена уже идет — предлагаем закончить
                return new PromptData("ui_prompt_clock_out", interactBind);
            }
        }

        public override void Interact()
        {
            if (_timeService == null) return;

            // Блокируем спам кнопкой
            base.Interact();

            if (!_timeService.IsShiftActive)
            {
                Debug.Log("[Terminal] Игрок пробился на смену.");
                _signalBus.Fire<ShiftStartRequestedSignal>();
            }
            else
            {
                Debug.Log("[Terminal] Игрок выбился с терминала. Конец смены.");
                _signalBus.Fire<ShiftEndRequestedSignal>();
            }
        }
    }
}