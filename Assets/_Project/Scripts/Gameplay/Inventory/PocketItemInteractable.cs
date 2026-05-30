using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
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

        [Inject]
        public void Construct(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        // Динамически формируем текст на основе данных из SO
        public override string InteractionPrompt => $"Взять {_itemData.DisplayName} [E]";

        public override void Interact()
        {
            base.Interact();

            _inventory.AddItem(_itemData.Id, _amount);

            // Ключи пропадают из мира навсегда
            Destroy(gameObject);
        }
    }
}
