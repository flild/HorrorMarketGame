using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory
{
    public class PlayerHandsView : MonoBehaviour
    {
        [Header("Holding Settings")]
        [SerializeField] private Transform _holdPoint;

        private SignalBus _signalBus;
        private GameObject _spawnedToolInstance;
        private Transform _previousCarryableParent;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Start()
        {
            // Подписываемся на изменения в руках
            _signalBus.Subscribe<EquipmentChangedSignal>(OnEquipmentChanged);
        }

        private void OnDestroy()
        {
            _signalBus?.TryUnsubscribe<EquipmentChangedSignal>(OnEquipmentChanged);
        }

        private void OnEquipmentChanged(EquipmentChangedSignal signal)
        {
            // Если пришел пустой предмет — значит, руки освободились
            if (signal.NewItem == null)
            {
                ClearHands();
                return;
            }

            // Очищаем то, что было в руках до этого
            ClearHands();

            switch (signal.NewItem.Type)
            {
                case ItemType.Equippable:
                    // Для инструментов (швабра, сканер) спавним префаб из SO
                    if (signal.NewItem.ViewPrefab != null)
                    {
                        _spawnedToolInstance = Instantiate(signal.NewItem.ViewPrefab, _holdPoint);
                        ResetTransform(_spawnedToolInstance.transform);
                    }
                    break;

                case ItemType.Carryable:
                    // Для тяжелых физических объектов (коробка) берем уже существующий объект сцены
                    if (signal.EquippedInstance != null)
                    {
                        Transform carryableTransform = signal.EquippedInstance.transform;

                        // Запоминаем старый парент (на случай если коробка лежала в паллете)
                        _previousCarryableParent = carryableTransform.parent;

                        // Удочеряем объект к точке в руках
                        carryableTransform.SetParent(_holdPoint);
                        ResetTransform(carryableTransform);
                    }
                    break;
            }
        }

        private void ClearHands()
        {
            // 1. Очищаем спавненные инструменты
            if (_spawnedToolInstance != null)
            {
                Destroy(_spawnedToolInstance);
                _spawnedToolInstance = null;
            }

            // 2. Если мы держали физический объект, сервис рук уже вернул ему физику.
            // Наша задача здесь — просто отвязать его от иерархии камеры.
            if (_holdPoint.childCount > 0)
            {
                for (int i = _holdPoint.childCount - 1; i >= 0; i--)
                {
                    Transform child = _holdPoint.GetChild(i);

                    // Возвращаем в корень сцены или на старый парент
                    child.SetParent(_previousCarryableParent);

                    // Чуть-чуть пинаем объект вперед, чтобы он не застрял в текстурах игрока при сбросе
                    if (child.TryGetComponent<Rigidbody>(out var rb))
                    {
                        rb.AddForce(_holdPoint.forward * 2f, ForceMode.VelocityChange);
                    }
                }
            }

            _previousCarryableParent = null;
        }

        private void ResetTransform(Transform target)
        {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
        }
    }
}
