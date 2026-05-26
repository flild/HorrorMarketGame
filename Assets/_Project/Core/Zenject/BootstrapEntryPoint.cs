using UnityEngine;
using Zenject;
using System.Threading.Tasks;

public class BootstrapEntryPoint : MonoBehaviour
{
    private SceneLoaderService _sceneLoader;

    [Inject]
    public void Construct(SceneLoaderService sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    private async void Start()
    {
        Debug.Log("[Bootstrap] Инициализация систем...");

        // Тут в будущем будет загрузка конфигов, проверка сохранений, инит аналитики и т.д.
        await Task.Delay(500); // Имитация тяжелой загрузки

        // Допустим, мы решаем запустить Главное меню
        Debug.Log("[Bootstrap] Загрузка Главного Меню...");
        await _sceneLoader.LoadCoreSceneAsync("MainMenu");

        // Либо, если грузим игру:
        // await _sceneLoader.LoadCoreSceneAsync("GameplayCore");
        // await _sceneLoader.LoadLocationAdditivelyAsync("Store_Day1");
    }
}

