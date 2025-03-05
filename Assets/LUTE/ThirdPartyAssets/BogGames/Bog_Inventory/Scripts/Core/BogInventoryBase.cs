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
        private List<BogInventoryItem> items;

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

        public Transform PopupMenuTransform { get { return popupMenuTransform; } }

        protected virtual void Awake()
        {
            items = new List<BogInventoryItem?>(new BogInventoryItem?[inventoryWidth * inventoryHeight]);
        }

        /// <summary>
        /// Helper class that will popuplate the inventory then show it to screen.
        /// </summary>
        public virtual void ShowInventory()
        {
            DrawInventory();
            inventoryCanvas?.FadeInventoryCanvas();
        }

        public virtual void AddItem(BogInventoryItem item)
        {
            int emptyIndex = items.IndexOf(null);
            if (emptyIndex != -1)
            {
                BogInventorySignals.DoInventoryItemAdded(item);

                items[emptyIndex] = item;
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
            }
        }

        public virtual void RemoveItem(BogInventoryItem item)
        {
            int itemIndex = items.IndexOf(item);
            if (itemIndex != -1)
            {
                items[itemIndex] = null;
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
            }
        }

        public virtual void UseItem(int index)
        {
            if (index >= 0 && index < items.Count && items[index] != null)
            {
                items[index]?.Use();
            }
        }

        public virtual void MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex >= 0 && fromIndex < items.Count && toIndex >= 0 && toIndex < items.Count)
            {
                BogInventorySignals.DoInventoryItemMoved(items[fromIndex]);

                (items[fromIndex], items[toIndex]) = (items[toIndex], items[fromIndex]);
                SelectedItemIndex = toIndex;
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
            }
        }

        public virtual void DrawInventory()
        {
            inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
        }

        public int GetItemIndex(BogInventoryItem item)
        {
            return items.IndexOf(item);
        }
    }
}
