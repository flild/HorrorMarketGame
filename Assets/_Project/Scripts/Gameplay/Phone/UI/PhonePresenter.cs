using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Phone.UI
{
    public class PhonePresenter : IInitializable, IDisposable
    {
        private readonly IPhoneService _phoneService;
        private readonly SignalBus _signalBus;
        private readonly PhoneWindow _view;

        public PhonePresenter(IPhoneService phoneService, SignalBus signalBus, PhoneWindow view)
        {
            _phoneService = phoneService;
            _signalBus = signalBus;
            _view = view;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<PhoneTasksUpdatedSignal>(OnTasksUpdated);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<PhoneTasksUpdatedSignal>(OnTasksUpdated);
        }

        private void OnTasksUpdated()
        {
            // Передаем ответственность за отрисовку во View
            _view.RenderTasks(_phoneService.CurrentTasks);
        }
    }
}
