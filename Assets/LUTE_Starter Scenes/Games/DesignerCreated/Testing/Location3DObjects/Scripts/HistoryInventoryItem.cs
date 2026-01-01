//using MoreMountains.InventoryEngine;
//using UnityEngine;

//namespace LoGaCulture.LUTE
//{

//    [CreateAssetMenu(fileName = "HistoryItem", menuName = "LUTE/Inventory/HistoryItem", order = 0)]
//    [System.Serializable]
//    public class HistoryInventoryItem : InventoryItem
//    {
//        [Tooltip("The historical info that this item represents")]
//        [SerializeField] protected ObjectInfo historicalInfo;

//        public ObjectInfo HistoricalInfo { get { return historicalInfo; } set { historicalInfo = value; } }

//        public override bool Use(string playerID)
//        {
//            ObjectInfoPanel newPanel = ObjectInfoPanel.GetInfoPanel();
//            if (newPanel != null && historicalInfo != null)
//            {
//                newPanel.SetInfo(historicalInfo);
//                newPanel.ToggleMenu();

//                InventoryInputManager inventoryInputManager = this.TargetInventory(playerID).GetComponentInChildren<InventoryInputManager>();
//                if (inventoryInputManager != null)
//                {
//                    inventoryInputManager.ToggleInventory();
//                }
//            }

//            return true;
//        }
//    }
//}