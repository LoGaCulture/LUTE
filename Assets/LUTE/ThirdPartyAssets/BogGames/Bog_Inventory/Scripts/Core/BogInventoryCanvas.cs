using System.Collections.Generic;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// A class that handles the drawing of an inventory.
    /// Often called from the base inventory classes and requires specific Inventory UI Items to render correctly.
    /// Rather than build these "slots" we force users to provide their own versions to reduce complexity and allow for customisation.
    /// </summary>
    public class BogInventoryCanvas : MonoBehaviour
    {
        [Tooltip("The grid layout object that is used to draw the inventory.")]
        [SerializeField] protected Transform gridLayout;
        [Tooltip("The inventory item prefab that is used to draw empty slots")]
        [SerializeField] protected BogInventoryUIItem emptyItemPrefab;

        public virtual void DrawInventory(List<BogInventoryItem?> items, int selectedIndex, BogInventoryBase currentInventory)
        {
            foreach (Transform child in gridLayout)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < items.Count; i++)
            {
                BogInventoryUIItem newItem = Instantiate(emptyItemPrefab, gridLayout);
                newItem.SetInventory(currentInventory);
                if (items[i] != null)
                {
                    newItem.SetItem(items[i], i);
                }
                if (i == selectedIndex)
                {
                    newItem.SetSelected(true);
                }
            }
        }
    }
}
