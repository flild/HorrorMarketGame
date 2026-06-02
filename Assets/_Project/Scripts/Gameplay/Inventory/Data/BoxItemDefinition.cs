using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Inventory.Data
{
    [CreateAssetMenu(fileName = "NewBoxDef", menuName = "Project/Inventory/Items/Box Definition")]
    public class BoxItemDefinition : ItemDefinition
    {
        [field: Header("Box Contents")]
        [field: SerializeField, Tooltip("Что лежит внутри?")]
        public ProductItemDefinition ContentItem { get; private set; }

        [field: SerializeField, Tooltip("Сколько штук в полной коробке")]
        public int MaxCapacity { get; private set; } = 10;
    }
}
