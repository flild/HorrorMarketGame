using Project.Core.Input;
using Project.Core.Settings;
using Project.Player;
using UnityEngine;
using Zenject;

namespace Project.Installers
{
    public class SceneGameplayInstaller : MonoInstaller
    {
        [SerializeField] private PlayerSettings playerSettings; // Drag&Drop ассет настроек сюда

        public override void InstallBindings()
        {
            // 1. Биндим настройки
            Container.BindInstance(playerSettings).AsSingle();

            // 2. Биндим Input Service как Single, чтобы он жил всю сцену
            Container.BindInterfacesAndSelfTo<StandaloneInputService>().AsSingle().NonLazy();

            // 3. Биндим Игрока, если он уже есть на сцене (самый простой способ для соло-разработки)
            // Ищем PlayerView на сцене
            Container.Bind<PlayerView>().FromComponentInHierarchy().AsSingle();


        }
    }
}
