using Project.Core.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class WindowService : IWindowService, IInitializable, IDisposable
{
    private readonly DiContainer _container;
    private readonly IInputService _inputService;
    private readonly Stack<WindowBase> _windowStack = new Stack<WindowBase>();

    public bool IsAnyWindowOpen => _windowStack.Count > 0;

    public WindowService(DiContainer container, IInputService inputService)
    {
        _container = container;
        _inputService = inputService;
    }

    public void Initialize()
    {
        // Подписываемся на глобальный ивент паузы/назад
        _inputService.OnPauseTriggered += HandlePauseOrBack;
    }

    public void Dispose()
    {
        // Отписываемся
        _inputService.OnPauseTriggered -= HandlePauseOrBack;
    }

    private void HandlePauseOrBack()
    {
        if (IsAnyWindowOpen)
        {
            CloseTopWindow();
        }
        else
        {
            OpenWindow<PauseWindow>();
        }
    }

    public void ToggleWindow<T>() where T : WindowBase
    {
        var window = _container.TryResolve<T>();

        if (window == null)
        {
            Debug.LogError($"[WindowService] Окно {typeof(T).Name} не забинжено!");
            return;
        }

        // Если окно сейчас самое верхнее в стеке — закрываем его
        if (_windowStack.Count > 0 && _windowStack.Peek() == window)
        {
            CloseTopWindow();
        }
        // Если окна вообще нет в стеке — открываем
        else if (!_windowStack.Contains(window))
        {
            OpenWindow<T>();
        }
    }
    public void OpenWindow<T>() where T : WindowBase
    {
        var window = _container.TryResolve<T>();

        if (window == null)
        {
            Debug.LogError($"[WindowService] Окно {typeof(T).Name} не забинжено в Zenject!");
            return;
        }

        if (_windowStack.Count > 0 && _windowStack.Peek() == window) return;

        window.Show();
        _windowStack.Push(window);
        UpdateGameState();
    }

    public void CloseTopWindow()
    {
        if (_windowStack.Count == 0) return;

        var window = _windowStack.Pop();
        window.Hide();
        UpdateGameState();
    }

    public void CloseAllWindows()
    {
        while (_windowStack.Count > 0)
        {
            var window = _windowStack.Pop();
            window.Hide();
        }
        UpdateGameState();
    }

    private void UpdateGameState()
    {
        bool shouldPauseTime = _windowStack.Any(w => w.StopsTime);
        bool shouldUnlockCursor = _windowStack.Any(w => w.UnlocksCursor);

        Time.timeScale = shouldPauseTime ? 0f : 1f;

        Cursor.lockState = shouldUnlockCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = shouldUnlockCursor;

        _inputService.SetUIMode(shouldUnlockCursor);
    }
}