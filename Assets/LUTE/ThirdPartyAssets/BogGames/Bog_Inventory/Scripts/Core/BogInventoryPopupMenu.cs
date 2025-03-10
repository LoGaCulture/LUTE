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


            // Various checks to determine the behaviour of the popup menu buttons 
            // Derives the behaviour from the actual item
            if (newItem.IsLocked && !newItem.UseWhenLocked)
            {
                useButton.interactable = false;
            }
            else
            {
                useButton.interactable = true;
            }

            if (newItem.CanDrop)
            {
                dropButton.interactable = true;
            }
            else
            {
                dropButton.interactable = false;
            }

            if (newItem.CanMove)
            {
                moveButton.interactable = true;
            }
            else
            {
                moveButton.interactable = false;
            }

            useButton.onClick.AddListener(() => { newItem.Use(); if (newItem.ConsumeOnUse) inventory.RemoveItem(newItem); Destroy(this.gameObject); });
            moveButton.onClick.AddListener(() => { parentUIItem.SelectForMove(); Destroy(this.gameObject); });
            dropButton.onClick.AddListener(() => { newItem.Drop(); inventory.RemoveItem(newItem); Destroy(this.gameObject); });
        }
    }
}
