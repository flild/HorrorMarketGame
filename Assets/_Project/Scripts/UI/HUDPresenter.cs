using Assets._Project.Core;
using Assets._Project.Scripts.Gameplay.Inventory.Interfaces; // Путь к твоему интерфейсу
using System;
using UnityEngine;
using Zenject;

public class HUDPresenter : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly HUDWindow _hudWindow;
    private readonly ILocalizationService _localization;

    // Внедряем сервис локализации
    public HUDPresenter(SignalBus signalBus, HUDWindow hudWindow, ILocalizationService localization)
    {
        _signalBus = signalBus;
        _hudWindow = hudWindow;
        _localization = localization;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<InteractableFocusSignal>(OnInteractableFocus);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<InteractableFocusSignal>(OnInteractableFocus);
    }

    private void OnInteractableFocus(InteractableFocusSignal signal)
    {
        if (signal.IsFocused && signal.FocusedObject != null)
        {
            // Берем ключ и параметры из объекта
            PromptData prompt = signal.FocusedObject.InteractionPrompt;

            // Превращаем ключ в переведенный текст
            string localizedText = _localization.GetText(prompt.Key, prompt.Args);

            _hudWindow.ShowInteractionPrompt(localizedText);
        }
        else
        {
            _hudWindow.HideInteractionPrompt();
        }
    }
}