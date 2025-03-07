using System.Collections.Generic;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// A serialisable class used to store an inventory item into a save file
    /// </summary>
    [System.Serializable]
    public class SerialisedBogInventoryItem
    {
        [SerializeField] private BogInventorySlot bogInventoryItem;

        public BogInventorySlot BogInventoryItem { get { return bogInventoryItem; } set { bogInventoryItem = value; } }

        /// <summary>
        /// Initialises a new instance of the <see cref="BogGames.Tools.Inventory.SerialisedBogInventoryItem"/> class.
        /// </summary>
        /// <param name="bogInventoryItem"></param>
        public SerialisedBogInventoryItem(BogInventorySlot bogInventoryItem)
        {
            BogInventoryItem = bogInventoryItem;
        }
    }

    /// <summary>
    /// Serialisable container for encoding the state of inventory items.
    /// Handles the encoding and decoding of data derived from the inventory manager or other managers that have access to inventory data.
    /// </summary>
    [System.Serializable]
    public class BogInventoryData
    {
        [SerializeField] private SerialisedBogInventoryItem[] inventoryItems;

        public SerialisedBogInventoryItem[] InventoryItems { get { return inventoryItems; } set { inventoryItems = value; } }

        /// <summary>
        /// Inserts the current inventory items into the serialised inventory items array.
        /// </summary>
        /// <param name="currentInventoryItems"></param>
        /// <returns></returns>
        public static BogInventoryData Encode(List<BogInventorySlot> currentInventoryItems)
        {
            var inventoryData = new BogInventoryData();

            inventoryData.inventoryItems = new SerialisedBogInventoryItem[currentInventoryItems.Count];

            for (int i = 0; i < currentInventoryItems.Count; i++)
            {
                inventoryData.inventoryItems[i] = new SerialisedBogInventoryItem(currentInventoryItems[i]);
            }

            return inventoryData;
        }

        /// <summary>
        /// Extracts the serialised inventory items into our inventory items array.
        /// </summary>
        /// <param name="inventoryData"></param>
        /// <param name="bogInventoryManager"></param>
        public static void Decode(BogInventoryData inventoryData, BogInventoryBase bogInventoryManager)
        {
            if (inventoryData == null)
            {
                Debug.LogWarning("No inventory data to decode.");
                return;
            }

            if (bogInventoryManager == null)
            {
                bogInventoryManager = GameObject.FindFirstObjectByType<BogInventoryBase>();
            }

            if (bogInventoryManager == null)
            {
                Debug.LogWarning("No inventory manager to decode to.");
                return;
            }

            bogInventoryManager.ExtractSerialisedBogInventoryData(inventoryData);
        }
    }
}
