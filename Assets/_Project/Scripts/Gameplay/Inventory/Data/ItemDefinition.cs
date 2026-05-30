using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDef", menuName = "Project/Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [field: Header("Basic Info")]
    [field: SerializeField, Tooltip("Уникальный текстовый ID (например: 'key_warehouse', 'mop')")]
    public string Id { get; private set; }

    [field: SerializeField, Tooltip("Название для UI")]
    public string DisplayName { get; private set; }

    [field: SerializeField]
    public ItemType Type { get; private set; }

    [field: Header("Visuals")]
    [field: SerializeField, Tooltip("Префаб, который появится в руках игрока. Оставь пустым для PocketItem.")]
    public GameObject ViewPrefab { get; private set; }

    // Задел на будущее: префаб для выкидывания предмета из рук обратно на пол
    [field: SerializeField, Tooltip("Префаб физического объекта, если предмет можно выбросить.")]
    public GameObject WorldPrefab { get; private set; }
}
