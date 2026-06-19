using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDef", menuName = "Project/Inventory/Basic Item")]
public abstract class ItemDefinition : ScriptableObject
{
    [field: Header("Basic Info")]
    [field: SerializeField, Tooltip("Уникальный текстовый ID (например: 'key_warehouse', 'box_cans')")]
    public string Id { get; private set; }

    [field: SerializeField, Tooltip("Название для UI")]
    public string DisplayNameKey { get; private set; }

    [field: SerializeField]
    public ItemType Type { get; private set; }
}
