using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using Project.Core.Input;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory
{
    public class PocketItemInteractable : InteractableObject, IInteractable
    {
        [Header("Item Settings")]
        [SerializeField] private ItemDefinition _itemData;
        [SerializeField] private int _amount = 1;

        private IInventoryService _inventory;
        private IInputService _inputService; // Добавили

        [Inject]
        public void Construct(IInventoryService inventory, IInputService inputService)
        {
            _inventory = inventory;
            _inputService = inputService;
        }

        // Аналогично
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
            base.Interact();

            _inventory.AddItem(_itemData.Id, _amount);

            // Ключи пропадают из мира навсегда
            Destroy(gameObject);
        }
    }
}
