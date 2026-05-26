using UnityEngine;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Пока биндим только загрузчик сцен. Позже сюда добавим SaveSystem и прочее.
        Container.BindInterfacesAndSelfTo<SceneLoaderService>().AsSingle();
    }
}