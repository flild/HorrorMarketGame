using System;
using System.Collections.Generic;
using System.Text;

namespace Assets._Project.Scripts.Gameplay.Inventory.Interfaces
{
    public interface IInventoryService
    {
        void AddItem(string itemId, int amount = 1);
        void RemoveItem(string itemId, int amount = 1);
        bool HasItem(string itemId, int amount = 1);
        int GetItemCount(string itemId);
    }
}
