using System;
using Zenject;
using UnityEngine;

public class PausePresenter : IInitializable, IDisposable
{
    private readonly PauseWindow _window;
    private readonly IWindowService _windowService;
    private readonly SceneLoaderService _sceneLoader;

    public PausePresenter(PauseWindow window, IWindowService windowService, SceneLoaderService sceneLoader)
    {
        _window = window;
        _windowService = windowService;
        _sceneLoader = sceneLoader;
    }

    public void Initialize()
    {
        // Подписываемся на клики
        _window.ContinueButton.onClick.AddListener(OnContinueClicked);
        _window.SettingsButton.onClick.AddListener(OnSettingsClicked);
        _window.ExitButton.onClick.AddListener(OnExitClicked);
    }

    public void Dispose()
    {
        // Обязательно отписываемся, чтобы не словить утечки памяти при смене сцен
        _window.ContinueButton.onClick.RemoveListener(OnContinueClicked);
        _window.SettingsButton.onClick.RemoveListener(OnSettingsClicked);
        _window.ExitButton.onClick.RemoveListener(OnExitClicked);
    }

    private void OnContinueClicked()
    {
        // Просто закрываем верхнее окно (Паузу)
        _windowService.CloseTopWindow();
    }

    private void OnSettingsClicked()
    {
        // Открываем настройки поверх паузы
        _windowService.OpenWindow<SettingsWindow>();
    }

    private async void OnExitClicked()
    {
        Debug.Log("[Pause] Выход в главное меню...");

        // Сбрасываем время на нормальное перед сменой сцен, 
        // иначе Главное меню загрузится замороженным
        Time.timeScale = 1f;

        // Закрываем все окна, чтобы очистить стек WindowService
        _windowService.CloseAllWindows();

        // Грузим Главное меню (LoadSceneMode.Single автоматически выгрузит GameplayCore)
        await _sceneLoader.LoadCoreSceneAsync("MainMenu");
    }
}