using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory
{
    public class CarryableInteractable : InteractableObject, IInteractable
    {
        [Header("Equipment Settings")]
        [SerializeField] private ItemDefinition _itemData;

        private IEquipmentService _equipment;

        [Inject]
        public void Construct(IEquipmentService equipment)
        {
            _equipment = equipment;
        }

        public override string InteractionPrompt => $"Взять {_itemData.DisplayName} [E]";

        public override void Interact()
        {
            Debug.Log($"interact with {_itemData.DisplayName}");
            base.Interact();

            // Передаем дату и ссылку на сам физический объект в сервис рук
            _equipment.Equip(_itemData, gameObject);
        }
    }
}
