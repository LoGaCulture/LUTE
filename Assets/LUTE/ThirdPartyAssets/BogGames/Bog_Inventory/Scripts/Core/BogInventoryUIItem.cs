using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// A class that stores the data for a single item in the inventory.
    /// This is used exclusively for drawing UI rather than actual inventory item logic.
    /// </summary>
    public class BogInventoryUIItem : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private BogInventoryItem Item;
        private bool IsHeld;
        private BogInventoryPopupMenu popupMenuInstance;
        private BogInventoryBase Inventory;
        private int ItemIndex;
        private float HoldTimer;

        private static BogInventoryUIItem SelectedItemForMove = null;

        [SerializeField] protected Image ItemIcon;
        [SerializeField] protected Image SelectionIndicator;
        [SerializeField] protected float HoldTime;
        [SerializeField] protected BogInventoryPopupMenu popupMenuPrefab;

        public void SelectForMove()
        {
            SelectedItemForMove = this;
        }

        public void SetInventory(BogInventoryBase inventory)
        {
            Inventory = inventory;
        }

        public virtual void SetItem(BogInventoryItem newItem, int index)
        {
            Item = newItem;
            ItemIndex = index;
            ItemIcon.sprite = Item.UnlockedIcon;
            SetSelected(false);
        }

        public virtual void SetSelected(bool isSelected)
        {
            if (SelectionIndicator != null)
            {
                SelectionIndicator.enabled = isSelected;
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
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
            if (popupMenuInstance == null && popupMenuPrefab != null)
            {
                popupMenuInstance = Instantiate(popupMenuPrefab, transform.position, Quaternion.identity, Inventory?.PopupMenuTransform);
                popupMenuInstance.SetupMenu(Item, this);
            }
        }
    }
}
