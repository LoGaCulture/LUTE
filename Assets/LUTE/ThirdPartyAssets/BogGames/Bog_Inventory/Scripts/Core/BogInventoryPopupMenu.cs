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

        private BogInventoryUIItem ParentUIItem;
        private BogInventoryItem ParentInventoryItem;

        public virtual void SetupMenu(BogInventoryItem newItem, BogInventoryUIItem parentUIItem)
        {
            ParentInventoryItem = newItem;
            ParentUIItem = parentUIItem;

            useButton.onClick.AddListener(() => { newItem.Use(); });
            moveButton.onClick.AddListener(() => ParentUIItem.SelectForMove());
            dropButton.onClick.AddListener(() => newItem.Drop());

        }
    }
}
