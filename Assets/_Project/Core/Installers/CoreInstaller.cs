using Assets._Project.Core;
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
    public override void InstallBindings()
    {
        // 1. Шина сигналов всегда ставится первой
        SignalBusInstaller.Install(Container);
        DeclareSignals();

        // 2. Базовые и системные сервисы
        BindCoreServices();

        // 3. Геймплейные механики и системы
        BindGameplayServices();

        // 4. Слой пользовательского интерфейса
        BindUIServices();
    }

    private void DeclareSignals()
    {
        // --- Взаимодействие и Игрок ---
        Container.DeclareSignal<InteractableFocusSignal>();
        Container.DeclareSignal<PlayerActionSignal>();
        Container.DeclareSignal<ReadNoteSignal>();

        // --- Инвентарь и Инструменты ---
        Container.DeclareSignal<ItemAddedSignal>();
        Container.DeclareSignal<ItemRemovedSignal>();
        Container.DeclareSignal<EquipmentChangedSignal>();
        Container.DeclareSignal<ToolActionSignal>();

        // --- Система Времени ---
        Container.DeclareSignal<TimeTickSignal>();
        Container.DeclareSignal<ShiftPhaseChangedSignal>();
        Container.DeclareSignal<ShiftStartRequestedSignal>();
        Container.DeclareSignal<ShiftEndRequestedSignal>();

        // --- Телефон и Квесты ---
        Container.DeclareSignal<PhoneTasksUpdatedSignal>();
        Container.DeclareSignal<PhoneMessageReceivedSignal>();

        // --- Системные ---
        Container.DeclareSignal<LanguageChangedSignal>();
    }

    private void BindCoreServices()
    {
        // View игрока персистентен на корневой сцене
        Container.Bind<PlayerView>().FromComponentInHierarchy().AsSingle();

        Container.BindInterfacesTo<StandaloneInputService>().AsSingle();
        Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle();
        Container.BindInterfacesTo<LocalizationService>().AsSingle();
        Container.BindInterfacesTo<WindowService>().AsSingle();
    }

    private void BindGameplayServices()
    {
        Container.BindInterfacesTo<ShiftTimeService>().AsSingle();
        Container.BindInterfacesTo<InventoryService>().AsSingle();
        Container.BindInterfacesTo<EquipmentService>().AsSingle();

        Container.BindInterfacesTo<PhoneInputHandler>().AsSingle();
        Container.BindInterfacesTo<PhoneService>().AsSingle();

        Container.BindInterfacesTo<QuestTracker>().AsSingle();
        Container.BindInterfacesTo<QuestGenerator>().AsSingle();
    }

    private void BindUIServices()
    {
        // Окна (View - ищутся на Canvas)
        Container.Bind<HUDWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PauseWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<SettingsWindow>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PhoneWindow>().FromComponentInHierarchy().AsSingle();

        // Презентеры (Controller)
        Container.BindInterfacesAndSelfTo<HUDPresenter>().AsSingle();
        Container.BindInterfacesTo<PausePresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<PhonePresenter>().AsSingle();
    }
}