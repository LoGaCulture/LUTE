using System.Collections.Generic;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// Base class for the inventory system.
    /// Handles storing of inventory data and basic inventory actions.
    /// Often used in conjunction with BogInventoryItem class and the signalling system for Inventories.
    /// </summary>
    public class BogInventoryBase : MonoBehaviour
    {
        // The actual list of items
        private List<BogInventorySlot> items;

        [SerializeField] protected int inventoryID;
        [Tooltip("The width of the inventory.")]
        [SerializeField] protected int inventoryWidth;
        [Tooltip("The height of the inventory.")]
        [SerializeField] protected int inventoryHeight;
        [Tooltip("The inventory canvas object that is used to draw the inventory.")]
        [SerializeField] protected BogInventoryCanvas inventoryCanvas;
        [Tooltip("The parent that the popup menu will attach to.")]
        [SerializeField] protected Transform popupMenuTransform;

        [HideInInspector]
        public int SelectedItemIndex = -1;

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
            for (int i = 0; i < items.Count; i++)
            {
                BogInventorySlot? slot = items[i];
                if (slot != null && slot.Item.ItemID == item.ItemID && slot.Item.MaxStackSize > 1)
                {
                    int spaceLeft = slot.Item.MaxStackSize - slot.Quantity;
                    int amountToAdd = Mathf.Min(spaceLeft, amount);
                    slot.Quantity += amountToAdd;
                    amount -= amountToAdd;

                    if (amount <= 0)
                    {
                        BogInventorySignals.DoInventoryItemAdded(item);

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

                BogInventorySignals.DoInventoryItemAdded(item);

                int amountToPlace = Mathf.Min(amount, item.MaxStackSize);
                items[emptyIndex] = new BogInventorySlot(item, amountToPlace);
                amount -= amountToPlace;
            }

            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public virtual void AddItem(BogInventoryItem item)
        {
            var amount = 1;
            for (int i = 0; i < items.Count; i++)
            {
                BogInventorySlot? slot = items[i];
                if (slot != null && slot.Item.ItemID == item.ItemID && slot.Item.MaxStackSize > 1)
                {
                    int spaceLeft = slot.Item.MaxStackSize - slot.Quantity;
                    int amountToAdd = Mathf.Min(spaceLeft, amount);
                    slot.Quantity += amountToAdd;
                    amount -= amountToAdd;

                    if (amount <= 0)
                    {
                        BogInventorySignals.DoInventoryItemAdded(item);

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

                BogInventorySignals.DoInventoryItemAdded(item);

                int amountToPlace = Mathf.Min(amount, item.MaxStackSize);
                items[emptyIndex] = new BogInventorySlot(item, amountToPlace);
                amount -= amountToPlace;
            }

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
                BogInventorySignals.DoInventoryItemMoved(items[fromIndex].Item);

                BogInventorySlot fromSlot = items[fromIndex];
                BogInventorySlot toSlot = items[toIndex];

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
                    }
                }
                else if (toSlot == null || fromSlot.Item.ItemID != toSlot.Item.ItemID)
                {
                    // Swap only if items are different
                    (items[fromIndex], items[toIndex]) = (items[toIndex], items[fromIndex]);
                }

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

            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
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
    }
}