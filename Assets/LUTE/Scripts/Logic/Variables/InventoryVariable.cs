using BogGames.Tools.Inventory;
using UnityEngine;

[VariableInfo("", "Inventory")]
[AddComponentMenu("")]
[System.Serializable]
public class InventoryVariable : BaseVariable<BogInventoryBase>
{
    public override bool SupportsArithmetic(SetOperator setOperator)
    {
        return false;
    }

    public override bool Evaluate(ComparisonOperator comparisonOperator, object value)
    {
        var item = (BogInventoryItem)value;

        bool result = Value.InventoryContains(item);
        return result;
    }
}

//Container for Inventory variables ref
[System.Serializable]
public struct InventoryData
{
    [SerializeField]
    [VariableProperty("<Value>", typeof(InventoryVariable))]
    public InventoryVariable inventoryRef;
    [SerializeField]
    public BogInventoryBase inventoryVal;
    [SerializeField]
    public BogInventoryItem item;

    public InventoryData(BogInventoryBase val, BogInventoryItem item)
    {
        inventoryVal = val;
        inventoryRef = null;
        this.item = item;
    }

    public static implicit operator BogInventoryBase(InventoryData inventoryData)
    {
        return inventoryData.Value;
    }

    [SerializeField]
    public BogInventoryBase Value
    {
        get { return (inventoryRef == null) ? inventoryVal : inventoryRef.Value; }
        set
        {
            if (inventoryRef == null)
            {
                inventoryVal = value;
            }
            else
            {
                inventoryRef.Value = value;
            }
        }
    }

    public string GetDescription()
    {
        //if (inventoryRef == null)
        //{
        //    return inventoryVal.ToString();
        //}
        //else
        //{
        //    return inventoryRef.Key;
        //}
        return "";
    }
}
