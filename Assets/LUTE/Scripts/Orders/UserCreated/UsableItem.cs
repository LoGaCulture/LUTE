//using MoreMountains.InventoryEngine;
//using UnityEngine;

//[OrderInfo("Items",
//              "Usable Item",
//              "Creates an item which will either show a card to use an item or use it when location is met")]
//[AddComponentMenu("")]
//public class UsableItem : Order
//{
//    [Tooltip("The sound to play when the item gets used")]
//    [SerializeField] protected AudioClip useSound;
//    [Tooltip("Whether or not to show the item card (if false the item wil be used automatically)")]
//    [SerializeField] protected bool showCard = true;
//    [Tooltip("Where this item will be placed on the map")]
//    [SerializeField] protected LocationData itemLocation;
//    [Tooltip("The item that will actually be used")]
//    [SerializeField] protected InventoryItem item;

//    public override void OnEnter()
//    {
//        LocationItemUsable.CreateItem(null, item, useSound, showCard, itemLocation);
//        Continue();
//    }

//    public override string GetSummary()
//    {
//        if (item == null)
//        {
//            return "Error: No item reference";
//        }
//        return "Use " + item.ItemName;
//    }

//    public override Color GetButtonColour()
//    {
//        return new Color32(184, 210, 235, 255);
//    }
//}