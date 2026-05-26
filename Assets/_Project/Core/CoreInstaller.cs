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
        // Container.Bind<PlayerController>().FromInstance(playerInstance).AsSingle();
        Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
        // Тут же потом забиндишь UI, инвентарь и т.д.
        // Container.Bind<UIManager>().FromComponentInHierarchy().AsSingle();

        // Биндим менеджер уровней, который будет рулить загрузкой локаций
        //Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle();
    }
}