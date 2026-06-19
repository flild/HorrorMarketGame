using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Project.Core.Input;
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
        private IInputService _inputService; // Добавили

        [Inject]
        public void Construct(IEquipmentService equipment, IInputService inputService)
        {
            _equipment = equipment;
            _inputService = inputService;
        }

        public override PromptData InteractionPrompt
        {
            get
            {
                string interactBind = _inputService.GetBindingName("Interact");
                return new PromptData("ui_prompt_take", _itemData.DisplayNameKey, interactBind);
            }
        }

        public override void Interact()
        {
            Debug.Log($"interact with {_itemData.DisplayNameKey}");
            base.Interact();

            // Передаем дату и ссылку на сам физический объект в сервис рук
            _equipment.Equip(_itemData, gameObject);
        }
    }
}
