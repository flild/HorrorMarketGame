using Assets._Project.Core;
using Assets._Project.Scripts.Gameplay.GameTime;
using Assets._Project.Scripts.Gameplay.Inventory.Interfaces; // Путь к твоему интерфейсу
using System;
using UnityEngine;
using Zenject;

public class HUDPresenter : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly HUDWindow _hudWindow;
    private readonly ILocalizationService _localization;

    public HUDPresenter(SignalBus signalBus, HUDWindow hudWindow, ILocalizationService localization)
    {
        _signalBus = signalBus;
        _hudWindow = hudWindow;
        _localization = localization;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<InteractableFocusSignal>(OnInteractableFocus);

        // Подписываемся на тики времени смены
        _signalBus.Subscribe<TimeTickSignal>(OnTimeTick);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<InteractableFocusSignal>(OnInteractableFocus);
        _signalBus.Unsubscribe<TimeTickSignal>(OnTimeTick);
    }

    private void OnInteractableFocus(InteractableFocusSignal signal)
    {
        if (signal.IsFocused && signal.FocusedObject != null)
        {
            PromptData prompt = signal.FocusedObject.InteractionPrompt;
            string localizedText = _localization.GetText(prompt.Key, prompt.Args);
            _hudWindow.ShowInteractionPrompt(localizedText);
        }
        else
        {
            _hudWindow.HideInteractionPrompt();
        }
    }

    private void OnTimeTick(TimeTickSignal signal)
    {
        // Передаем строку времени (например "18:30") во вьюху
        _hudWindow.UpdateClock(signal.FormattedTime);
    }
}