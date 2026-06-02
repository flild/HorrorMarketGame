using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Inventory.Data
{
    [CreateAssetMenu(fileName = "NewProductDef", menuName = "Project/Inventory/Items/Product Definition")]
    public class ProductItemDefinition : ItemDefinition
    {
        [field: Header("Product Settings")]
        [field: SerializeField, Tooltip("Префаб самой банки/яблока для спавна на полке")]
        public GameObject ShelfPrefab { get; private set; }

    }
}
