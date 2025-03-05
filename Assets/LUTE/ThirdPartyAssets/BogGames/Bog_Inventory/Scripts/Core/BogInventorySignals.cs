namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// Class used to send signals when inventory items are used, added, removed, or moved.
    /// Also keeps track of when the inventory is opened/closed.
    /// Helps to keep track of inventory actions and then take appropriate actions (such as playing sounds).
    /// </summary>
    public class BogInventorySignals
    {
        /// <summary>
        /// Inventory Item used; sent when an item is used.
        /// </summary>
        public static event InventoryItemUsed OnInventoryItemUsed;
        public delegate void InventoryItemUsed(BogInventoryItem item);
        public static void DoInventoryItemUsed(BogInventoryItem item) { OnInventoryItemUsed?.Invoke(item); }

        /// <summary>
        /// Inventory Item unlocked; sent when an item is unlocked.
        /// </summary>
        public static event InventoryItemUnlocked OnInventoryItemUnlocked;
        public delegate void InventoryItemUnlocked(BogInventoryItem item);
        public static void DoInventoryItemUnlocked(BogInventoryItem item) { OnInventoryItemUnlocked?.Invoke(item); }

        /// <summary>
        /// Inventory Item locked; sent when an item is locked.
        /// </summary>
        public static event InventoryItemLocked OnInventoryItemLocked;
        public delegate void InventoryItemLocked(BogInventoryItem item);
        public static void DoInventoryItemLocked(BogInventoryItem item) { OnInventoryItemLocked?.Invoke(item); }

        /// <summary>
        /// Inventory Item added signal; sent when an item is added to the inventory.
        /// </summary>
        public static event InventoryItemAddedHandler OnInventoryItemAdded;
        public delegate void InventoryItemAddedHandler(BogInventoryItem item);
        public static void DoInventoryItemAdded(BogInventoryItem item) { OnInventoryItemAdded?.Invoke(item); }

        /// <summary>
        /// Inventory Item removed signal; sent when an item is removed from the inventory.
        /// </summary>
        public static event InventoryItemRemovedHandler OnInventoryItemRemoved;
        public delegate void InventoryItemRemovedHandler(BogInventoryItem item);
        public static void DoInventoryItemRemoved(BogInventoryItem item) { OnInventoryItemRemoved?.Invoke(item); }

        /// <summary>
        /// Inventory Item moved signal; sent when an item is moved within the inventory.
        /// </summary>
        public static event InventoryItemMovedHandler OnInventoryItemMoved;
        public delegate void InventoryItemMovedHandler(BogInventoryItem item);
        public static void DoInventoryItemMoved(BogInventoryItem item) { OnInventoryItemMoved?.Invoke(item); }

        /// <summary>
        /// Inventory opened signal; sent when the inventory is opened.
        /// </summary>
        public static event IventoryMenuOpenedHandler OnInventoryMenuOpened;
        public delegate void IventoryMenuOpenedHandler();
        public static void DoInventoryMenuOpened(BogInventoryCanvas canvas) { OnInventoryMenuOpened?.Invoke(); }

        /// <summary>
        /// Inventory closed signal; sent when the inventory is closed.
        /// </summary>
        public static event InventoryMenuClosedHandler OnInventoryMenuClosed;
        public delegate void InventoryMenuClosedHandler();
        public static void DoInventoryMenuClosed(BogInventoryCanvas canvas) { OnInventoryMenuClosed?.Invoke(); }
    }
}