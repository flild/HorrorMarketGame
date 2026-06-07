using UnityEngine;

public struct ItemAddedSignal
{
    public string ItemId;
    public int Amount;
}

public struct ItemRemovedSignal
{
    public string ItemId;
    public int Amount;
}

public struct EquipmentChangedSignal
{
    public ItemDefinition NewItem; 
    public GameObject EquippedInstance; 
}
public struct ToolActionSignal
{
    public string ToolId;      // Например, "mop"
    public bool IsActive;      // true - начали мыть, false - закончили
}