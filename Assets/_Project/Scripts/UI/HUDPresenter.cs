using System;
using UnityEngine;
using Zenject;

public class HUDPresenter : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly HUDWindow _hudWindow;

    // Внедряем зависимости. Zenject сам передаст сюда SignalBus и наш HUDWindow со сцены
    public HUDPresenter(SignalBus signalBus, HUDWindow hudWindow)
    {
        _signalBus = signalBus;
        _hudWindow = hudWindow;
    }

    public void Initialize()
    {
        // Подписываемся на сигнал от луча из глаз
        _signalBus.Subscribe<InteractableFocusSignal>(OnInteractableFocus);
    }

    public void Dispose()
    {
        // Отписываемся, чтобы не было утечек памяти
        _signalBus.Unsubscribe<InteractableFocusSignal>(OnInteractableFocus);
    }

    private void OnInteractableFocus(InteractableFocusSignal signal)
    {
        if (signal.IsFocused)
        {
            _hudWindow.ShowInteractionPrompt(signal.PromptText);
        }
        else
        {
            _hudWindow.HideInteractionPrompt();
        }
    }
}