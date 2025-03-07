using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory",
    "Remove Item from Inventory",
    "Removes an item from the inventory.")]
    public class RemoveItemFromInventory : Order
    {
        [Tooltip("The inventory to remove the item from")]
        [SerializeField] protected BogInventoryBase inventory;
        [Tooltip("The Item to remove from the inventory")]
        [HideInInspector]
        [SerializeField] protected BogInventoryItem item;
        [Tooltip("The amount of the item to remove from the inventory")]
        [SerializeField] protected int amount = 1;

        public override void OnEnter()
        {
            if (item == null || amount <= 0 || inventory == null)
            {
                Continue();
                return;
            }

            inventory.RemoveItemEditor(item, amount);
            Continue();
        }

        public override string GetSummary()
        {
            return item != null ? "     Removing " + amount + " " + item.ItemName + "(s)" : "Error: No item set";
        }
        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
