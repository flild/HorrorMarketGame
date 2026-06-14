using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory
{
    public class EquipmentService : IEquipmentService
    {
        private readonly SignalBus _signalBus;

        public ItemDefinition CurrentItem { get; private set; }
        public GameObject CurrentInstance { get; private set; }
        public bool IsHandsBusy => CurrentItem != null;

        public EquipmentService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Equip(ItemDefinition item, GameObject worldObject = null)
        {
            if (item == null) return;

            // Теперь мы ЖЕСТКО требуем, чтобы при поднятии предмета передавался инстанс со сцены
            if (worldObject == null)
            {
                Debug.LogError($"[Equipment] Попытка взять предмет '{item.DisplayName}' без указания физического объекта со сцены!");
                return;
            }

            if (IsHandsBusy)
            {
                Unequip();
            }

            CurrentItem = item;
            CurrentInstance = worldObject; // Сохраняем реальную швабру/коробку

            // Вырубаем физику (через наш новый ItemPhysicsController)
            TogglePhysics(CurrentInstance, false);

            Debug.Log($"[Equipment] Взяли в руки: {item.DisplayName}");

            _signalBus.Fire(new EquipmentChangedSignal
            {
                NewItem = CurrentItem,
                EquippedInstance = CurrentInstance
            });
        }

        public void Unequip()
        {
            if (!IsHandsBusy) return;

            Debug.Log($"[Equipment] Освободили руки от: {CurrentItem.DisplayName}");

            // Запоминаем объект, так как сейчас мы обнулим ссылки
            var instanceToDrop = CurrentInstance;

            CurrentItem = null;
            CurrentInstance = null;

            // 1. Посылаем сигнал. PlayerHandsView моментально отработает его 
            // и ОТВЯЖЕТ швабру от иерархии игрока (вызовет SetParent).
            _signalBus.Fire(new EquipmentChangedSignal
            {
                NewItem = null,
                EquippedInstance = null
            });

            // 2. Только ТЕПЕРЬ, когда швабра свободна, возвращаем ей коллайдеры и гравитацию.
            if (instanceToDrop != null)
            {
                TogglePhysics(instanceToDrop, true);

                // 3. Даем пинок вперед, используя вектор взгляда главной камеры
                if (instanceToDrop.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddForce(Camera.main.transform.forward * 3f, ForceMode.VelocityChange);
                }
            }
        }

        private void TogglePhysics(GameObject obj, bool enable)
        {
            // Теперь сервис просто просит сам предмет изменить свое состояние
            if (obj.TryGetComponent<ItemPhysicsController>(out var physics))
            {
                physics.SetPhysicsState(enable);
            }
            else
            {
                Debug.LogWarning($"[Equipment] На объекте {obj.name} нет ItemPhysicsController! Физика не отключена.");
            }
        }
    }
}
