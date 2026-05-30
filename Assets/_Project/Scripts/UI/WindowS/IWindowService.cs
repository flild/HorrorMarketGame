public interface IWindowService
{
    void OpenWindow<T>() where T : WindowBase;
    void CloseTopWindow();
    void CloseAllWindows();
    bool IsAnyWindowOpen { get; }
}
