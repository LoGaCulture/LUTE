using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// Simple class that represents a single slot in the inventory.
    /// Contains the item and the quantity of that item.
    /// </summary>
    public class BogInventorySlot
    {
        [Tooltip("The item that is in the slot.")]
        [SerializeField] protected BogInventoryItem item;
        [Tooltip("The quantity of the item in the slot.")]
        [SerializeField] protected int quantity;

        public BogInventoryItem Item { get { return item; } set { item = value; } }
        public int Quantity { get { return quantity; } set { quantity = value; } }

        public BogInventorySlot(BogInventoryItem item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }
}
