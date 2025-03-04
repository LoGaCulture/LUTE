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
        public BogInventoryItem testItem;

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
            AddItem(testItem);
        }

        public virtual void AddItem(BogInventoryItem item)
        {
            int emptyIndex = items.IndexOf(null);
            if (emptyIndex != -1)
            {
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
                (items[fromIndex], items[toIndex]) = (items[toIndex], items[fromIndex]);
                inventoryCanvas?.DrawInventory(items, SelectedItemIndex, this);
                // Play sound here using signals and LUTE Inventory sound manager (move sound)
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
