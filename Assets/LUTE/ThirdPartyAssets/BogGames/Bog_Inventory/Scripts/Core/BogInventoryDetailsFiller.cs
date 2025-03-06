using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// Helper class that uses the inventory stored to fill in details about the currently selected item.
    /// Ensure you have a nice looking "empty" inventory detail panel to show when no item is selected.
    /// </summary>
    public class BogInventoryDetailsFiller : MonoBehaviour
    {
        [Tooltip("The image renderer for the item icon.")]
        [SerializeField] private UnityEngine.UI.Image itemIcon;
        [Tooltip("The text object for the item name.")]
        [SerializeField] private TMPro.TextMeshProUGUI itemName;
        [Tooltip("The text object for the item description.")]
        [SerializeField] private TMPro.TextMeshProUGUI itemDescription;
        [Tooltip("If there is no item selected, display this image.")]
        [SerializeField] private Sprite emptyIcon;

        private void Awake()
        {
            BogInventorySignals.OnInventoryItemSelected += OnInventoryItemSelected;
        }
        private void OnDestroy()
        {
            BogInventorySignals.OnInventoryItemSelected -= OnInventoryItemSelected;
        }

        private void OnInventoryItemSelected(BogInventoryItem item)
        {
            // If there is no item or the item is locked, reset the details so we cannot see them
            // One could choose not to do this and show the locked item details
            if (item == null || item.IsLocked)
            {
                ResetDetails();
            }
            else
            {
                itemIcon.sprite = item.IsLocked ? item.LockedIcon : item.UnlockedIcon;
                itemName.text = item.ItemName;
                itemDescription.text = item.ItemDescription;
            }
        }

        private void ResetDetails()
        {
            itemIcon.sprite = emptyIcon != null ? emptyIcon : null;
            itemName.text = "";
            itemDescription.text = "";
        }
    }
}
