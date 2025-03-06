using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory",
    "Lock Inventory Item",
    "Locks an item in the inventory; if multiple items share the same ID then all get locked.")]
    public class LockInventoryItem : Order
    {
        [Tooltip("The inventory assoicated with this item.")]
        [SerializeField] protected BogInventoryBase inventory;
        [Tooltip("The Item to lock")]
        [HideInInspector]
        [SerializeField] protected BogInventoryItem item;

        public override void OnEnter()
        {
            if (item == null || inventory == null)
            {
                Continue();
                return;
            }

            inventory.LockItem(item);
            Continue();
        }

        public override string GetSummary()
        {
            return item != null ? "     Locking " + " " + item.ItemName : "Error: No item set";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
