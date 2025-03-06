using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// A class that stores the data for a single item in the inventory.
    /// This is used exclusively for drawing UI rather than actual inventory item logic.
    /// </summary>
    public class BogInventoryUIItem : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private BogInventoryItem Item;
        private bool IsHeld;
        private static BogInventoryPopupMenu popupMenuInstance;
        private BogInventoryBase Inventory;
        private int ItemIndex;
        private float HoldTimer;

        private static BogInventoryUIItem SelectedItemForMove = null; // Keep a static reference so we do not need to pass the item around and have multiple popup windows occur

        [Tooltip("The image that will be rendered as the inventory item.")]
        [SerializeField] protected Image ItemIcon;
        [Tooltip("The text that will render the quantity of the item.")]
        [SerializeField] protected TextMeshProUGUI ItemQuantityText;
        [Tooltip("The image that will be rendered as the selection indicator.")]
        [SerializeField] protected Image SelectionIndicator;
        [Tooltip("The image that will be rendered as the hover indicator.")]
        [SerializeField] protected Image HoverIndicator;
        [Tooltip("The image that will be rendered as the moving indicator.")]
        [SerializeField] protected Image MoveIndicator;
        [Tooltip("The time required to hold the item before the popup menu appears.")]
        [SerializeField] protected float HoldTime;
        [Tooltip("The popup menu prefab that will be used to display the item options.")]
        [SerializeField] protected BogInventoryPopupMenu popupMenuPrefab;

        public void SelectForMove()
        {
            SelectedItemForMove = this;
            if (MoveIndicator != null)
            {
                MoveIndicator.enabled = true;
            }
        }

        public void SetInventory(BogInventoryBase inventory)
        {
            Inventory = inventory;
        }

        /// <summary>
        /// Simple helper class to destroy the popup menu.
        /// </summary>
        public void DestroyPopupMenu()
        {
            if (popupMenuInstance != null)
            {
                Destroy(popupMenuInstance.gameObject);
            }
        }

        public virtual void SetItem(BogInventorySlot newSlot, int index)
        {
            if (newSlot != null)
            {
                Item = newSlot.Item;
                Debug.Log(Item.IsLocked);
                ItemIcon.sprite = Item.IsLocked ? Item.LockedIcon : Item.UnlockedIcon;
                ItemQuantityText.text = newSlot.Quantity > 1 ? newSlot.Quantity.ToString() : ""; // Show count if >1
            }

            ItemIndex = index;
            SetSelected(false);
        }

        public virtual void SetSelected(bool isSelected)
        {
            if (SelectionIndicator != null)
            {
                SelectionIndicator.enabled = isSelected;
                Inventory.GetCurrentlySelectedItem();
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // If the menu is open this may get in the way so we just simply close if a click is recieved on any item
            DestroyPopupMenu();

            if (SelectedItemForMove != null && SelectedItemForMove != this)
            {
                Inventory.MoveItem(SelectedItemForMove.ItemIndex, ItemIndex);
                SelectedItemForMove = null;
            }
            else
            {
                Inventory.SelectedItemIndex = ItemIndex;
                Inventory.DrawInventory();
            }

        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            IsHeld = true;
            HoldTimer = 0;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            IsHeld = false;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (HoverIndicator != null)
            {
                HoverIndicator.enabled = true;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (HoverIndicator != null)
            {
                HoverIndicator.enabled = false;
            }
        }

        private void Update()
        {
            if (IsHeld)
            {
                HoldTimer += Time.deltaTime;
                if (HoldTimer >= HoldTime)
                {
                    ShowPopupMenu();
                    IsHeld = false;
                }
            }
        }

        private void ShowPopupMenu()
        {
            // If there is a menu already and the user requires another then destroy the old one
            if (popupMenuInstance != null)
            {
                DestroyPopupMenu();
            }

            Inventory.SelectedItemIndex = ItemIndex;

            if (popupMenuPrefab != null)
            {
                popupMenuInstance = Instantiate(popupMenuPrefab, transform.position, Quaternion.identity, Inventory?.PopupMenuTransform);
                popupMenuInstance.SetupMenu(Item, this, Inventory);
            }
        }
    }
}
