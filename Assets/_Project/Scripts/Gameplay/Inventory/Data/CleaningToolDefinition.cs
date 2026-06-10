using Assets._Project.Scripts.Gameplay.Inventory.Interactables;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets._Project.Scripts.Gameplay.Inventory.Data
{
    [CreateAssetMenu(fileName = "NewCleaningToolDef", menuName = "Project/Inventory/Items/Cleaning Tool Definition")]
    public class CleaningToolDefinition : ToolItemDefinition, ICleaningTool
    {
        // Базовые поля ViewPrefab и WorldPrefab он унаследует от ToolItemDefinition.
        // А сюда можно пихать статы, специфичные только для уборки.

        [field: Header("Cleaning Stats")]
        [field: SerializeField, Tooltip("Множитель скорости мытья. Улучшенная швабра может мыть быстрее.")]
        public float WashSpeedMultiplier { get; private set; } = 1f;
    }
}
