using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Cysharp.Threading.Tasks; // Обязательный using для UniTask

public class SceneLoaderService
{
    private readonly ZenjectSceneLoader _zenjectSceneLoader;

    public SceneLoaderService(ZenjectSceneLoader zenjectSceneLoader)
    {
        _zenjectSceneLoader = zenjectSceneLoader;
    }

    // Загрузка корневой сцены (например, Главного меню или GameplayCore)
    public async UniTask LoadCoreSceneAsync(string sceneName)
    {
        // ZenjectSceneLoader сам прокинет зависимости в новую сцену
        await _zenjectSceneLoader.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToUniTask();
    }

    // Загрузка локации ПОВЕРХ ядра (твои магазины и дома)
    public async UniTask LoadLocationAdditivelyAsync(string sceneName)
    {
        await _zenjectSceneLoader.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask();
    }

    public async UniTask UnloadLocationAsync(string sceneName)
    {
        var op = SceneManager.UnloadSceneAsync(sceneName);
        if (op != null)
        {
            // Вместо костыльного цикла с Task.Yield просто ждем операцию через UniTask
            await op.ToUniTask();
        }
    }
}