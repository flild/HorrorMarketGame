using UnityEngine;

public class SettingsWindow : WindowBase
{
    // Окно настроек тоже должно стопать игру
    public override bool StopsTime => true;
    public override bool UnlocksCursor => true;
}
