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
            Debug.Log($"[Equipment] Попытка экипировать предмет: {item?.DisplayName ?? "null"}");
            if (item == null) return;

            // Если руки уже заняты, сначала сбрасываем старый предмет
            if (IsHandsBusy)
            {
                Unequip();
            }

            CurrentItem = item;

            if (item.Type == ItemType.Carryable)
            {
                if (worldObject == null)
                {
                    Debug.LogError($"[Equipment] Попытка взять Carryable предмет '{item.DisplayName}' без указания физического объекта со сцены!");
                    return;
                }

                CurrentInstance = worldObject;
                // Отключаем физику, чтобы объект не улетел и не толкал игрока
                TogglePhysics(CurrentInstance, false);
            }
            else if (item.Type == ItemType.Equippable)
            {
                // Для инструментов (швабра) логика инстанцирования ложится на HandsView,
                // либо мы можем подготовить инстанс здесь, если у нас есть пул.
                // Пока просто фиксируем, что нам нужен префаб.
                CurrentInstance = null;
            }

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

            if (CurrentItem.Type == ItemType.Carryable && CurrentInstance != null)
            {
                // Возвращаем физику объекту, который выкинули
                TogglePhysics(CurrentInstance, true);

                // Небольшой пинок вперед, чтобы коробка не падала строго под ноги
                // (Реализуем позже через HandsView или прямо тут, если добавим ссылку на трансформ игрока)
            }

            CurrentItem = null;
            CurrentInstance = null;

            _signalBus.Fire(new EquipmentChangedSignal
            {
                NewItem = null,
                EquippedInstance = null
            });
        }

        private void TogglePhysics(GameObject obj, bool enable)
        {
            if (obj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = !enable;
                rb.detectCollisions = enable;
            }

            // Отключаем коллайдеры, чтобы объект в руках не триггерил рейкасты и коллизии
            var colliders = obj.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                // Если это триггер взаимодействия, его можно не трогать, но основные отключаем
                if (!col.isTrigger)
                {
                    col.enabled = enable;
                }
            }
        }
    }
}
