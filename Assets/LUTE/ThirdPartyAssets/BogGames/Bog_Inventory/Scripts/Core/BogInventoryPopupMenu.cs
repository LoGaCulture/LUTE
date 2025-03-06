using UnityEngine;
using UnityEngine.UI;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// A simple class that defines the parameters for a popup menu.
    /// This is often used in conjunction with the BogInventoryUIItem class where the menu has a list of options for specific items.
    /// </summary>
    public class BogInventoryPopupMenu : MonoBehaviour
    {
        [SerializeField] protected Button useButton;
        [SerializeField] protected Button moveButton;
        [SerializeField] protected Button dropButton;

        public virtual void SetupMenu(BogInventoryItem newItem, BogInventoryUIItem parentUIItem, BogInventoryBase inventory = null)
        {
            // If there is no item then we cannot use, drop or move it
            if (newItem == null)
            {
                useButton.interactable = false;
                moveButton.interactable = false;
                dropButton.interactable = false;
                return;
            }

            if (inventory == null)
            {
                dropButton.interactable = false;
            }

            // If the item is locked then we cannot use it
            // But we can drop or move it if we have an inventory
            if (newItem.IsLocked)
            {
                useButton.interactable = false;
            }
            else
            {
                useButton.interactable = true;
            }

            useButton.onClick.AddListener(() => { newItem.Use(); if (newItem.ConsumeOnUse) inventory.RemoveItem(newItem); Destroy(this.gameObject); });
            moveButton.onClick.AddListener(() => { parentUIItem.SelectForMove(); Destroy(this.gameObject); });
            dropButton.onClick.AddListener(() => { newItem.Drop(); inventory.RemoveItem(newItem); Destroy(this.gameObject); });
        }
    }
}
