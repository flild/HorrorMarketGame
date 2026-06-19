using Assets._Project.Core;
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
        private readonly SignalBus _signalBus;
        private readonly IPhoneService _phoneService;
        private readonly ILocalizationService _localization;
        private readonly PhoneWindow _view;

        public PhonePresenter(
            SignalBus signalBus,
            IPhoneService phoneService,
            ILocalizationService localization,
            PhoneWindow view)
        {
            _signalBus = signalBus;
            _phoneService = phoneService;
            _localization = localization;
            _view = view;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<PhoneTasksUpdatedSignal>(OnTasksUpdated);

            // Если язык поменялся во время игры - перерисовываем телефон
            _signalBus.Subscribe<LanguageChangedSignal>(OnTasksUpdated);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<PhoneTasksUpdatedSignal>(OnTasksUpdated);
            _signalBus.Unsubscribe<LanguageChangedSignal>(OnTasksUpdated);
        }

        private void OnTasksUpdated()
        {
            var uiDataList = new List<PhoneWindow.TaskUIData>();

            foreach (var task in _phoneService.CurrentTasks)
            {
                // Переводим ключи
                string locName = _localization.GetText(task.Definition.DisplayNameKey);
                string locDesc = _localization.GetText(task.Definition.PhoneDescriptionKey);

                // Форматируем прогресс, если нужно сделать больше 1 действия
                if (task.Definition.RequiredAmount > 1)
                {
                    locName = $"{locName} [{task.CurrentProgress}/{task.Definition.RequiredAmount}]";
                }

                uiDataList.Add(new PhoneWindow.TaskUIData
                {
                    Title = locName,
                    Description = locDesc,
                    IsCompleted = task.State == NightTaskState.Completed
                });
            }

            _view.RenderTasks(uiDataList);
        }
    }
}
