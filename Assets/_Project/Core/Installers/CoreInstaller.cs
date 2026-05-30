using Assets._Project.Scripts.Gameplay.Inventory;
using Project.Core.Input;
using UnityEngine;
using Zenject;

public class CoreInstaller : MonoInstaller
{
    // Ссылка на префаб или объект игрока на сцене
    //[SerializeField] private PlayerController playerInstance;

    public override void InstallBindings()
    {
        // Базовое ядро
        Container.Bind<PlayerView>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
        Container.BindInterfacesTo<WindowService>().AsSingle();

        // Инициализация шины сигналов
        SignalBusInstaller.Install(Container);

        // --- СИСТЕМА ИНВЕНТАРЯ И РУК ---
        Container.BindInterfacesTo<InventoryService>().AsSingle();
        Container.BindInterfacesTo<EquipmentService>().AsSingle();

        // Декларация сигналов инвентаря
        Container.DeclareSignal<ItemAddedSignal>();
        Container.DeclareSignal<ItemRemovedSignal>();
        Container.DeclareSignal<EquipmentChangedSignal>();
        // --------------------------------

        // UI Окна (Должны лежать на Canvas в GameplayCore)
        Container.Bind<HUDWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PauseWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SettingsWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PhoneWindow>().FromComponentInHierarchy().AsSingle();

        // Презентеры и хендлеры
        Container.BindInterfacesTo<PausePresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<HUDPresenter>().AsSingle();

        // Старые сигналы
        Container.DeclareSignal<InteractableFocusSignal>();
        Container.DeclareSignal<ReadNoteSignal>();

        // Менеджер уровней
        Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle();
    }
}