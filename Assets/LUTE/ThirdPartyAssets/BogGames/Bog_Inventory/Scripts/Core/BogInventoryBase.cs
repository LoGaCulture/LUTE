using System.Collections.Generic;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// Base class for the inventory system.
    /// Handles storing of inventory data and basic inventory actions.
    /// Often used in conjunction with BogInventoryItem class and the signalling system for Inventories.
    /// </summary>
    [System.Serializable]
    public class BogInventoryBase : MonoBehaviour
    {
        [SerializeField] protected int inventoryID;
        [Tooltip("The width of the inventory.")]
        [SerializeField] protected int inventoryWidth;
        [Tooltip("The height of the inventory.")]
        [SerializeField] protected int inventoryHeight;
        [Tooltip("The inventory canvas object that is used to draw the inventory.")]
        [SerializeField] protected BogInventoryCanvas inventoryCanvas;
        [Tooltip("The parent that the popup menu will attach to.")]
        [SerializeField] protected Transform popupMenuTransform;

        // The actual list of items
        public static List<BogInventorySlot> items;

        [HideInInspector]
        public int SelectedItemIndex = -1;

        public int InventoryID { get { return inventoryID; } }
        public BogInventoryCanvas BogInventoryCanvas { get { return inventoryCanvas; } }
        public Transform PopupMenuTransform { get { return popupMenuTransform; } }

        protected virtual void Awake()
        {
            items = new List<BogInventorySlot?>(new BogInventorySlot?[inventoryWidth * inventoryHeight]);
        }

        /// <summary>
        /// Helper class that will popuplate the inventory then show it to screen.
        /// </summary>
        public virtual void ShowInventory()
        {
            DrawInventory();
            inventoryCanvas?.FadeInventoryCanvas();
        }

        public virtual void AddItem(BogInventoryItem item, int amount = 1)
        {
            // Try stacking into existing slots first
            for (int i = 0; i < items.Count; i++)
            {
                BogInventorySlot? slot = items[i];

                if (slot != null && slot.Item.ItemID == item.ItemID && slot.Item.MaxStackSize > 1)
                {
                    int spaceLeft = slot.Item.MaxStackSize - slot.Quantity;
                    int amountToAdd = Mathf.Min(spaceLeft, amount);

                    slot.Quantity += amountToAdd;
                    slot.SlotIndex = i;
                    amount -= amountToAdd;

                    if (amount <= 0)
                    {
                        inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
                        return;
                    }
                }
            }

            // If there's remaining amount, place it into new empty slots
            while (amount > 0)
            {
                int emptyIndex = items.FindIndex(slot => slot == null);
                if (emptyIndex == -1)
                {
                    Debug.LogWarning("No more space in inventory!");
                    return;
                }

                int amountToPlace = Mathf.Min(amount, item.MaxStackSize);
                BogInventorySlot newSlot = new BogInventorySlot(item, amountToPlace, emptyIndex);

                items[emptyIndex] = newSlot;
                amount -= amountToPlace;

            }

            BogInventorySignals.DoInventoryItemAdded(item);
            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public virtual void AddItem(BogInventoryItem item)
        {
            var amount = 1;
            // Try stacking into existing slots first
            for (int i = 0; i < items.Count; i++)
            {
                BogInventorySlot? slot = items[i];

                if (slot != null && slot.Item.ItemID == item.ItemID && slot.Item.MaxStackSize > 1)
                {
                    int spaceLeft = slot.Item.MaxStackSize - slot.Quantity;
                    int amountToAdd = Mathf.Min(spaceLeft, amount);

                    slot.Quantity += amountToAdd;
                    slot.SlotIndex = i;
                    amount -= amountToAdd;

                    if (amount <= 0)
                    {
                        inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
                        return;
                    }
                }
            }

            // If there's remaining amount, place it into new empty slots
            while (amount > 0)
            {
                int emptyIndex = items.FindIndex(slot => slot == null);
                if (emptyIndex == -1)
                {
                    Debug.LogWarning("No more space in inventory!");
                    return;
                }

                int amountToPlace = Mathf.Min(amount, item.MaxStackSize);
                BogInventorySlot newSlot = new BogInventorySlot(item, amountToPlace, emptyIndex);

                items[emptyIndex] = newSlot;
                amount -= amountToPlace;

            }

            BogInventorySignals.DoInventoryItemAdded(item);
            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public virtual void InsertItem(BogInventorySlot slot, int amount = 1)
        {
            int targetIndex = slot.SlotIndex;

            // Ensure target index is within valid range
            if (targetIndex < 0 || targetIndex >= items.Count)
            {
                Debug.LogWarning($"Invalid item index {targetIndex} for insertion.");
                return;
            }

            // If the slot is already occupied, warn and do nothing
            if (items[targetIndex] != null)
            {
                Debug.LogWarning($"Slot {targetIndex} is already occupied. Cannot insert item {slot.Item.ItemID}.");
                return;
            }

            // Insert item at the exact index with the given quantity
            items[targetIndex] = new BogInventorySlot(slot.Item, amount, targetIndex);

            // Redraw inventory UI
            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public virtual void RemoveItem(BogInventoryItem item, int quantity = 1)
        {
            int itemIndex = SelectedItemIndex;
            if (itemIndex != -1 && items[itemIndex] != null)
            {
                BogInventorySlot slot = items[itemIndex];

                if (slot.Item.ItemID == item.ItemID)
                {
                    slot.Quantity -= quantity;

                    if (slot.Quantity <= 0)
                    {
                        items[itemIndex] = null;
                    }
                }

                BogInventorySignals.DoInventoryItemRemoved(item);
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
            }
        }

        public virtual void RemoveItem(BogInventoryItem item)
        {
            int quantity = 1;

            int itemIndex = items.FindIndex(slot => slot != null && slot.Item.ItemID == item.ItemID);

            if (itemIndex != -1 && items[itemIndex] != null)
            {
                BogInventorySlot slot = items[itemIndex];

                if (slot.Item.ItemID == item.ItemID)
                {
                    slot.Quantity -= quantity;

                    if (slot.Quantity <= 0)
                    {
                        items[itemIndex] = null;
                    }
                }

                BogInventorySignals.DoInventoryItemRemoved(item);
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
            }
        }

        public virtual void RemoveItemEditor(BogInventoryItem item, int quantity = 1)
        {
            int itemIndex = items.FindIndex(slot => slot != null && slot.Item.ItemID == item.ItemID);

            if (itemIndex != -1 && items[itemIndex] != null)
            {
                BogInventorySlot slot = items[itemIndex];

                if (slot.Item.ItemID == item.ItemID)
                {
                    slot.Quantity -= quantity;

                    if (slot.Quantity <= 0)
                    {
                        items[itemIndex] = null;
                    }
                }

                BogInventorySignals.DoInventoryItemRemoved(item);
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
            }
        }

        public virtual void UseItem(int index)
        {
            if (index >= 0 && index < items.Count && items[index] != null)
            {
                items[index]?.Item.Use();
            }

            if (--items[index].Quantity <= 0)
            {
                items[index] = null; // Remove item if empty
            }

            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public virtual void MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex >= 0 && fromIndex < items.Count && toIndex >= 0 && toIndex < items.Count)
            {
                if (items[fromIndex] == null) return; // Ensure fromSlot is valid

                BogInventorySlot fromSlot = items[fromIndex];
                BogInventorySlot? toSlot = items[toIndex];

                if (toSlot != null && fromSlot.Item.ItemID == toSlot.Item.ItemID && fromSlot.Item.MaxStackSize > 1)
                {
                    int spaceLeft = fromSlot.Item.MaxStackSize - toSlot.Quantity;
                    int amountToMove = Mathf.Min(spaceLeft, fromSlot.Quantity);

                    if (amountToMove > 0)
                    {
                        // Stack the items together if possible
                        toSlot.Quantity += amountToMove;
                        fromSlot.Quantity -= amountToMove;

                        if (fromSlot.Quantity <= 0)
                        {
                            items[fromIndex] = null;
                        }
                    }
                    else if (fromSlot.Quantity != toSlot.Quantity)
                    {
                        // Swap if the items are the same but have different quantities
                        (items[fromIndex], items[toIndex]) = (items[toIndex], items[fromIndex]);

                        // Update both item indexes after swapping
                        if (items[fromIndex] != null) items[fromIndex].SlotIndex = fromIndex;
                        if (items[toIndex] != null) items[toIndex].SlotIndex = toIndex;
                    }
                }
                else if (toSlot == null || fromSlot.Item.ItemID != toSlot.Item.ItemID)
                {
                    // Swap only if items are different
                    (items[fromIndex], items[toIndex]) = (items[toIndex], items[fromIndex]);

                    // Update both item indexes after swapping
                    if (items[fromIndex] != null) items[fromIndex].SlotIndex = fromIndex;
                    if (items[toIndex] != null) items[toIndex].SlotIndex = toIndex;
                }

                BogInventorySignals.DoInventoryItemMoved(items[toIndex].Item);

                SelectedItemIndex = toIndex;
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
            }
        }

        public virtual void UnlockItem(BogInventoryItem item)
        {
            int itemIndex = items.FindIndex(slot => slot != null && slot.Item.ItemID == item.ItemID);

            if (itemIndex != -1)
            {
                BogInventorySlot slot = items[itemIndex];

                if (slot != null && slot.Item != null && slot.Item.ItemID == item.ItemID)
                {
                    slot.Item.IsLocked = false;
                    slot.Item.UnlockItem();
                }
            }

            BogInventorySignals.DoInventoryItemUnlocked(item);
            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public virtual void LockItem(BogInventoryItem item)
        {
            int itemIndex = items.FindIndex(slot => slot != null && slot.Item.ItemID == item.ItemID);

            if (itemIndex != -1)
            {
                BogInventorySlot slot = items[itemIndex];

                if (slot != null && slot.Item != null && slot.Item.ItemID == item.ItemID)
                {
                    slot.Item.IsLocked = true;
                    slot.Item.LockItem();
                }
            }

            BogInventorySignals.DoInventoryItemLocked(item);
            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public virtual bool InventoryContains(BogInventoryItem item)
        {
            bool contains = false;
            foreach (var slot in items)
            {
                if (slot != null && slot.Item.ItemID == item.ItemID)
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }

        public virtual void UnlockRandomItem()
        {
            if (items == null || items.Count == 0)
                return;

            int startIndex = Random.Range(0, items.Count);
            int index = startIndex;

            do
            {
                BogInventorySlot slot = items[index];

                if (slot != null && slot.Item != null && slot.Item.IsLocked)
                {
                    slot.Item.IsLocked = false;
                    slot.Item.UnlockItem();

                    BogInventorySignals.DoInventoryItemUnlocked(slot.Item);
                    inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
                    return;
                }

                index = (index + 1) % items.Count; // Move to the next item, wrapping around
            } while (index != startIndex); // Stop if we've looped through all items
        }

        public virtual void DrawInventory()
        {
            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public BogInventoryItem GetCurrentlySelectedItem()
        {
            if (SelectedItemIndex >= 0 && SelectedItemIndex < items.Count)
            {
                if (items[SelectedItemIndex] != null)
                {
                    BogInventorySignals.DoInventoryItemSelected(items[SelectedItemIndex].Item);
                    return items[SelectedItemIndex].Item;
                }

            }
            BogInventorySignals.DoInventoryItemSelected(null);
            return null;
        }

        #region Serialisation

        /// <summary>
        /// Resets the inventory to its default state.
        /// Will clear all items and reset the inventory size to the one provdided in inspector.
        /// </summary>
        public virtual void ResetInventory()
        {
            if (Application.isPlaying)
            {
                items = new List<BogInventorySlot?>(new BogInventorySlot?[inventoryWidth * inventoryHeight]);
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);

                BogInventorySignals.DoInventoryReset(this);
                Debug.Log("Inventory reset.");
            }
        }

        /// <summary>
        /// Extracts the serialised data from save file and updates the inventory list.
        /// </summary>
        /// <param name="serialisedItems"></param>
        public virtual void ExtractSerialisedBogInventoryData(BogInventoryData serialisedItems)
        {
            if (serialisedItems == null)
                return;

            for (int i = 0; i < serialisedItems.InventoryItems.Length; i++)
            {
                var serialisedItem = serialisedItems.InventoryItems[i];

                if (serialisedItem != null)
                {
                    InsertItem(serialisedItem.BogInventoryItem, serialisedItem.BogInventoryItem.Quantity);
                }
            }
        }
        #endregion
    }
}