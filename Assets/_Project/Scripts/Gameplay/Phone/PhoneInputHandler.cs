using Assets._Project.Scripts.Gameplay.Phone.UI;
using Project.Core.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Phone
{
    public class PhoneInputHandler : IInitializable, IDisposable
    {
        private readonly IInputService _inputService;
        private readonly IWindowService _windowService;

        public PhoneInputHandler(IInputService inputService, IWindowService windowService)
        {
            _inputService = inputService;
            _windowService = windowService;
        }

        public void Initialize()
        {
            _inputService.OnPhoneToggled += OnPhoneToggled;
        }

        public void Dispose()
        {
            _inputService.OnPhoneToggled -= OnPhoneToggled;
        }

        private void OnPhoneToggled()
        {
            _windowService.ToggleWindow<PhoneWindow>();
        }
    }
}
