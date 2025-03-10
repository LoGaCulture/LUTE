using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// A class that is often attached to a gameobject so that it can reference scene objects for specific unity actions.
    /// If there is an inventory item provided then the unity action will be called when the item is used.
    /// The event is unsubscribed if/when the item is depleted o removed from the inventory.
    /// </summary>
    public class InventoryItemLUTEMonoRef : MonoBehaviour
    {
        [Tooltip("The inventory item to reference")]
        [SerializeField] protected InventoryItemLUTE inventoryItem;

        [Tooltip("The unity action to call when the inventory item is used")]
        [SerializeField] protected UnityEngine.Events.UnityEvent unityAction;

        private void OnEnable()
        {
            if (inventoryItem != null)
            {
                BogInventorySignals.OnInventoryItemUsed += OnItemUse;
                BogInventorySignals.OnInventoryItemRemoved += OnItemRemoved;
                BogInventorySignals.OnInventoryReset += OnInventoryReset;
            }
        }

        private void OnDisable()
        {
            if (inventoryItem != null)
            {
                BogInventorySignals.OnInventoryItemUsed -= OnItemUse;
                BogInventorySignals.OnInventoryItemRemoved -= OnItemRemoved;
                BogInventorySignals.OnInventoryReset -= OnInventoryReset;
            }
        }

        protected virtual void OnItemUse(BogInventoryItem item) { }

        protected virtual void OnItemRemoved(BogInventoryItem item)
        {
            if (item != null)
            {
                BogInventorySignals.OnInventoryItemUsed -= OnItemUse;
            }
        }

        protected virtual void OnInventoryReset(BogInventoryBase inventory)
        {
            if (inventoryItem != null)
            {
                BogInventorySignals.OnInventoryItemUsed -= OnItemUse;
            }
        }
    }
}
