using Project.Core.Input;
using UnityEngine;
using Zenject;

public class CoreInstaller : MonoInstaller
{
    // Ссылка на префаб или объект игрока на сцене
    //[SerializeField] private PlayerController playerInstance;

    public override void InstallBindings()
    {
        // Биндим игрока. Теперь кто угодно сможет его запросить через [Inject]
        Container.Bind<PlayerView>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
        // Тут же потом забиндишь UI, инвентарь и т.д.
        Container.BindInterfacesTo<WindowService>().AsSingle();

        SignalBusInstaller.Install(Container);

        // Биндим View (Они должны лежать на Canvas в сцене GameplayCore)
        Container.Bind<HUDWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PauseWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SettingsWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PhoneWindow>().FromComponentInHierarchy().AsSingle();

        // Биндим логику (Презентеры и хендлеры)
        Container.BindInterfacesTo<PausePresenter>().AsSingle();

        // Декларируем наши сигналы
        Container.DeclareSignal<InteractableFocusSignal>();
        Container.DeclareSignal<ReadNoteSignal>();

        // Биндим менеджер уровней, который будет рулить загрузкой локаций
        Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle();

        Container.BindInterfacesAndSelfTo<HUDPresenter>().AsSingle();
    }
}