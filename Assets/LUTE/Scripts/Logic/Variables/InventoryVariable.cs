//using System.Collections;
//using System.Collections.Generic;
//using MoreMountains.InventoryEngine;
//using UnityEngine;

//[VariableInfo("", "Inventory")]
//[AddComponentMenu("")]
//[System.Serializable]
//public class InventoryVariable : BaseVariable<Inventory>
//{
//    public override bool SupportsArithmetic(SetOperator setOperator)
//    {
//        return false;
//    }

//    public override bool Evaluate(ComparisonOperator comparisonOperator, object value)
//    {
//        var item = (InventoryItem)value;
//        var items = Value.InventoryContains(item.ItemID);
//        bool result = false;
//        switch (comparisonOperator)
//        {
//            case ComparisonOperator.Equals:
//                result = items.Count >= 1;
//                break;
//            case ComparisonOperator.NotEquals:
//                result = items.Count <= 0;
//                break;
//        }
//        return result;
//    }
//}

////Container for Inventory variables ref
//[System.Serializable]
//public struct InventoryData
//{
//    [SerializeField]
//    [VariableProperty("<Value>", typeof(InventoryVariable))]
//    public InventoryVariable inventoryRef;
//    [SerializeField]
//    public Inventory inventoryVal;
//    [SerializeField]
//    public InventoryItem item;

//    public InventoryData(Inventory val, InventoryItem item)
//    {
//        inventoryVal = val;
//        inventoryRef = null;
//        this.item = item;
//    }

//    public static implicit operator Inventory(InventoryData inventoryData)
//    {
//        return inventoryData.Value;
//    }

//    [SerializeField]
//    public Inventory Value
//    {
//        get { return (inventoryRef == null) ? inventoryVal : inventoryRef.Value; }
//        set
//        {
//            if (inventoryRef == null)
//            {
//                inventoryVal = value;
//            }
//            else
//            {
//                inventoryRef.Value = value;
//            }
//        }
//    }

//    public string GetDescription()
//    {
//        // if (inventoryRef == null)
//        // {
//        //     return inventoryVal.ToString();
//        // }
//        // else
//        // {
//        //     return inventoryRef.Key;
//        // }

//        return "";
//    }
//}
