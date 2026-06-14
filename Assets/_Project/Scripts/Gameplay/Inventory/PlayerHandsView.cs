using Assets._Project.Scripts.Gameplay.Inventory.Data;
using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory
{
    public class PlayerHandsView : MonoBehaviour
    {
        [Header("Holding Settings")]
        [SerializeField] private Transform _holdPoint;
        [SerializeField] private Transform _toolPoint;

        private SignalBus _signalBus;
        private GameObject _spawnedToolInstance;
        private Transform _previousCarryableParent;
        private DiContainer _container;

        [Inject]
        public void Construct(SignalBus signalBus, DiContainer container) // <-- ДОБАВИЛ
        {
            _signalBus = signalBus;
            _container = container;
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
            if (signal.NewItem == null || signal.EquippedInstance == null)
            {
                ClearHands();
                return;
            }

            ClearHands();

            Transform instanceTransform = signal.EquippedInstance.transform;

            // Запоминаем откуда взяли, чтобы потом туда вернуть
            _previousCarryableParent = instanceTransform.parent;

            // Разводим логику позиционирования
            if (signal.NewItem.Type == ItemType.Equippable)
            {
                // Швабры и инструменты крепим в ToolPoint
                instanceTransform.SetParent(_toolPoint);
                ResetTransform(instanceTransform);

                // Инициализируем логику инструмента
                if (signal.EquippedInstance.TryGetComponent<IToolVisual>(out var toolVisual))
                {
                    toolVisual.Initialize(signal.NewItem.Id);
                }
            }
            else if (signal.NewItem.Type == ItemType.Carryable)
            {
                // Коробки крепим в HoldPoint
                instanceTransform.SetParent(_holdPoint);
                ResetTransform(instanceTransform);
            }
        }

        private void ClearHands()
        {
            // Мы больше не удаляем префабы, потому что мы их больше не спавним.
            // Нужно просто отвязать объект от рук и кинуть обратно в мир.

            if (_toolPoint.childCount > 0)
                DropChild(_toolPoint);

            if (_holdPoint.childCount > 0)
                DropChild(_holdPoint);

            _previousCarryableParent = null;
        }

        private void DropChild(Transform point)
        {
            for (int i = point.childCount - 1; i >= 0; i--)
            {
                Transform child = point.GetChild(i);

                if (_previousCarryableParent != null)
                {
                    child.SetParent(_previousCarryableParent);
                }
                else
                {
                    child.SetParent(null);
                    SceneManager.MoveGameObjectToScene(child.gameObject, SceneManager.GetActiveScene());
                }
            }
        }

        private void ResetTransform(Transform target)
        {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
        }
    }
}
