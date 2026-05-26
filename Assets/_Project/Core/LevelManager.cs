using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class LevelManager : IInitializable
{
    private readonly SceneLoaderService _sceneLoader;
    private string _currentLocation = string.Empty;

    // Инжектим наш глобальный загрузчик сцен из 1 этапа
    public LevelManager(SceneLoaderService sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    // Вызовется автоматически, когда GameplayCore загрузится
    public async void Initialize()
    {
        Debug.Log("[LevelManager] Ядро загружено. Грузим первую локацию...");

        // Позже здесь ты будешь читать сохраненки и понимать, какой день грузить.
        // Сейчас жестко грузим первый день магазина.
        await LoadLocation("Store_Day1");
    }

    public async Task LoadLocation(string locationName)
    {
        // Выгружаем старую локацию, если она была (например, игрок ушел из дома в магазин)
        if (!string.IsNullOrEmpty(_currentLocation))
        {
            await _sceneLoader.UnloadLocationAsync(_currentLocation);
        }

        // Грузим новую поверх ядра
        await _sceneLoader.LoadLocationAdditivelyAsync(locationName);
        _currentLocation = locationName;

        Debug.Log($"[LevelManager] Локация {locationName} успешно загружена и пришита к Ядру.");
    }
}
