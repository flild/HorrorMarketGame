using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using System.Linq;

public class LevelManager : IInitializable
{
    private readonly SceneLoaderService _sceneLoader;
    private readonly PlayerView _player;
    private string _currentLocation = string.Empty;

    // Добавили инжект PlayerView, чтобы LevelManager мог его двигать
    public LevelManager(SceneLoaderService sceneLoader, PlayerView player)
    {
        _sceneLoader = sceneLoader;
        _player = player;
    }

    public async void Initialize()
    {
        Debug.Log("[LevelManager] Ядро загружено. Грузим первую локацию...");

        // Для теста грузим магазин и спавним у главной двери
        await LoadLocation("Store_Day1", SpawnLocationId.StoreMainDoor);
    }

    public async Task LoadLocation(string locationName, SpawnLocationId spawnId)
    {
        if (!string.IsNullOrEmpty(_currentLocation))
        {
            await _sceneLoader.UnloadLocationAsync(_currentLocation);
        }

        // Грузим новую сцену
        await _sceneLoader.LoadLocationAdditivelyAsync(locationName);
        _currentLocation = locationName;

        // После загрузки сцены ищем точку спавна
        var spawnPoints = Object.FindObjectsByType<SpawnPoint>();
        var targetSpawn = spawnPoints.FirstOrDefault(p => p.LocationId == spawnId);

        if (targetSpawn != null)
        {
            _player.Teleport(targetSpawn.Position, targetSpawn.Rotation);
            Debug.Log($"[LevelManager] Игрок телепортирован на {spawnId}");
        }
        else
        {
            // Если забыл расставить точки - игрок просто останется где был
            Debug.LogError($"[LevelManager] Точка спавна {spawnId} не найдена в сцене {locationName}!");
        }
    }
}