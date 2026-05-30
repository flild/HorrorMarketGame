using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Inventory.Interfaces
{
    public interface IEquipmentService
    {
        ItemDefinition CurrentItem { get; }
        GameObject CurrentInstance { get; }
        bool IsHandsBusy { get; }

        /// <summary>
        /// Взять предмет в руки.
        /// </summary>
        /// <param name="item">Данные предмета</param>
        /// <param name="worldObject">Физический объект со сцены (только для типа Carryable)</param>
        void Equip(ItemDefinition item, GameObject worldObject = null);

        /// <summary>
        /// Выбросить или убрать текущий предмет из рук.
        /// </summary>
        void Unequip();
    }
}
