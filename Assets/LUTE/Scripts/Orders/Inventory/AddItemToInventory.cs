using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory",
        "Add Item To Inventory",
        "Adds an item to the inventory")]
    public class AddItemToInventory : Order
    {
        [Tooltip("The inventory to add the item to")]
        [SerializeField] protected BogInventoryBase inventory;
        [Tooltip("The Item to add to the inventory")]
        [HideInInspector]
        [SerializeField] protected BogInventoryItem item;
        [Tooltip("The amount of the item to add to the inventory")]
        [SerializeField] protected int amount = 1;
        [Tooltip("If true, the item will be added if it already exists in the inventory")]
        [SerializeField] protected bool addIfAlreadyExists = true;

        public override void OnEnter()
        {
            if (item == null || amount <= 0 || inventory == null)
            {
                Continue();
                return;
            }

            inventory.AddItem(item, amount, addIfAlreadyExists);
            Continue();
        }

        public override string GetSummary()
        {
            return item != null ? "     Adding " + amount + " " + item.ItemName + "(s)" : "Error: No item set";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
