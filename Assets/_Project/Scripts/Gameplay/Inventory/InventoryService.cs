using Assets._Project.Scripts.Gameplay.Inventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets._Project.Scripts.Gameplay.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
        private readonly SignalBus _signalBus;

        // Zenject сам прокинет SignalBus при создании
        public InventoryService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void AddItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return;

            if (_items.ContainsKey(itemId))
                _items[itemId] += amount;
            else
                _items[itemId] = amount;

            Debug.Log($"[Inventory] Добавлен предмет: {itemId} (x{amount}). Всего: {_items[itemId]}");

            // Кидаем сигнал, чтобы UI мог показать плашку "Получен ключ"
            _signalBus.Fire(new ItemAddedSignal { ItemId = itemId, Amount = amount });
        }

        public void RemoveItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return;

            if (_items.ContainsKey(itemId))
            {
                _items[itemId] -= amount;
                if (_items[itemId] <= 0)
                {
                    _items.Remove(itemId);
                }

                _signalBus.Fire(new ItemRemovedSignal { ItemId = itemId, Amount = amount });
            }
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            return _items.TryGetValue(itemId, out int currentAmount) && currentAmount >= amount;
        }

        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            return _items.TryGetValue(itemId, out int amount) ? amount : 0;
        }
    }
}
