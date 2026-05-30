using UnityEngine.UI;
using UnityEngine;

public class PauseWindow : WindowBase
{
    public override bool StopsTime => true;
    public override bool UnlocksCursor => true;

    [Header("Buttons")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _exitButton;

    // Отдаем ссылки наружу для Презентера
    public Button ContinueButton => _continueButton;
    public Button SettingsButton => _settingsButton;
    public Button ExitButton => _exitButton;
}