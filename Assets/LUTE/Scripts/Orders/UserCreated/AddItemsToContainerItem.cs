//using MoreMountains.InventoryEngine;
//using System.Linq;
//using UnityEngine;

//[OrderInfo("Container",
//              "Add Items To Container",
//              "Adds an item of specific quantity to an exisiting container item")]
//[AddComponentMenu("")]
//public class AddItemsToContainerItem : Order
//{
//    [Tooltip("The item to add ")]
//    [SerializeField] protected InventoryItem item;
//    [Tooltip("The quantity of the item to add")]
//    [SerializeField] protected int quantity;
//    [Tooltip("The container item to add the item (s) to")]
//    [SerializeField] protected string containerItemName;

//    public override void OnEnter()
//    {
//        if (item == null)
//        {
//            Continue();
//            return;
//        }

//        var inventory = item.TargetInventory("Player1");
//        if (inventory == null)
//        {
//            Continue();
//            return;
//        }

//        var containerItem = inventory.Content.ToList().Find(x => x.ItemName == containerItemName) as ContainerItem;

//        if (containerItem == null)
//        {
//            Continue();
//            return;
//        }

//        if (quantity <= 0)
//        {
//            Continue();
//            return;
//        }

//        containerItem.AddItems(item, quantity);

//        Continue();
//    }

//    public override string GetSummary()
//    {
//        return "";
//    }
//}