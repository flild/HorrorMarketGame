using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Inventory.Data
{
    [CreateAssetMenu(fileName = "NewToolDef", menuName = "Project/Inventory/Items/Tool Definition")]
    public class ToolItemDefinition : ItemDefinition
    {
        [field: Header("Visuals")]
        [field: SerializeField, Tooltip("Префаб, который появится в руках игрока (швабра, сканер).")]
        public GameObject ViewPrefab { get; private set; }

        [field: SerializeField, Tooltip("Префаб для выкидывания предмета из рук обратно на пол (если надо).")]
        public GameObject WorldPrefab { get; private set; }
    }
}
