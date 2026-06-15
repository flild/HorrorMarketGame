using Assets._Project.Scripts.Gameplay.GameTime;
using Assets._Project.Scripts.Gameplay.Inventory;
using Assets._Project.Scripts.Gameplay.Phone;
using Assets._Project.Scripts.Gameplay.Phone.UI;
using Assets._Project.Scripts.Gameplay.Tasks;
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
        Container.BindInterfacesTo<ShiftTimeService>().AsSingle();

        // Декларация сигналов инвентаря
        Container.DeclareSignal<ItemAddedSignal>();
        Container.DeclareSignal<ItemRemovedSignal>();
        Container.DeclareSignal<EquipmentChangedSignal>();
        Container.DeclareSignal<ToolActionSignal>();
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


        //Time
        Container.DeclareSignal<TimeTickSignal>();
        Container.DeclareSignal<ShiftPhaseChangedSignal>();


        // Сигналы телефона
        Container.DeclareSignal<PhoneTasksUpdatedSignal>();
        Container.DeclareSignal<PhoneMessageReceivedSignal>();

        // Сервисы
        Container.BindInterfacesTo<PhoneService>().AsSingle();

        // Презентер телефона (Window уже есть в твоем коде)
        Container.BindInterfacesAndSelfTo<PhonePresenter>().AsSingle();

        // Декларируем новый сигнал
        Container.DeclareSignal<PlayerActionSignal>();

        // Биндим QuestTracker (он IInitializable/IDisposable, так что используем Interfaces)
        Container.BindInterfacesTo<QuestTracker>().AsSingle();
    }
}