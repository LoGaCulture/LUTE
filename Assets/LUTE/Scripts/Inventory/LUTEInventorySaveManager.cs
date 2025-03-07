using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Listens out for Bog's inventory signals and processes them.
    /// For the most part, Bog's inventory system handles the inventory but this class is used to save the inventory as a serialised file.
    /// </summary>
    public class LUTEInventorySaveManager : MonoBehaviour
    {
        public string SavePointKey
        {
            get
            {
                return "LUTEInventoryList";
            }
        }

        public string SavePointDescription
        {
            get
            {
                return "LUTEInventoryList" + " - " + BogInventoryBase.items.Count + System.DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");
            }
        }

        protected virtual void Awake()
        {
            BogInventorySignals.OnInventoryItemUsed += OnItemUpdated;
            BogInventorySignals.OnInventoryItemUnlocked += OnItemUpdated;
            BogInventorySignals.OnInventoryItemLocked += OnItemUpdated;
            BogInventorySignals.OnInventoryItemAdded += OnItemUpdated;
            BogInventorySignals.OnInventoryItemRemoved += OnItemUpdated;
            BogInventorySignals.OnInventoryItemMoved += OnItemUpdated;
            BogInventorySignals.OnInventoryReset += OnInventoryReset;
        }

        protected virtual void OnDisable()
        {
            BogInventorySignals.OnInventoryItemUsed -= OnItemUpdated;
            BogInventorySignals.OnInventoryItemUnlocked -= OnItemUpdated;
            BogInventorySignals.OnInventoryItemLocked -= OnItemUpdated;
            BogInventorySignals.OnInventoryItemAdded -= OnItemUpdated;
            BogInventorySignals.OnInventoryItemRemoved -= OnItemUpdated;
            BogInventorySignals.OnInventoryItemMoved -= OnItemUpdated;
            BogInventorySignals.OnInventoryReset -= OnInventoryReset;
        }

        protected virtual void OnItemUpdated(BogInventoryItem item)
        {
            LogaManager.Instance.SaveManager.AddSavePoint(SavePointKey, SavePointDescription, false, SaveManager.SaveProfile.BogInventoryData);
        }

        protected virtual void OnInventoryReset(BogInventoryBase inventory)
        {
            LogaManager.Instance.SaveManager.AddSavePoint(SavePointKey, SavePointDescription, false, SaveManager.SaveProfile.BogInventoryData);
        }
    }
}
