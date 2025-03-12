//using MoreMountains.InventoryEngine;
//using MoreMountains.Tools;
//using UnityEngine;

//namespace LoGaCulture.LUTE
//{
//    [OrderInfo("Alberto",
//             "ShowInfoPanel",
//             "Will open an info panel and fill the content with the information object provided.")]
//    [AddComponentMenu("")]
//    public class ShowInfoPanelOrder : Order
//    {
//        [Tooltip("The info related to fill the panel")]
//        [SerializeField] protected ObjectInfo objectInfo;
//        [Tooltip("The corresponding item to add to the inventory - can be empty")]
//        [SerializeField] protected HistoryInventoryItem historyInventoryItem;
//        [Tooltip("Can be specific with what inventory to add the item to if need be - will find the default inventory if using this.")]
//        [SerializeField] protected Inventory inventory;

//        private ItemPicker itemPicker;

//        public override void OnEnter()
//        {
//            ObjectInfoPanel newPanel = ObjectInfoPanel.GetInfoPanel();
//            if (newPanel != null && objectInfo != null)
//            {
//                newPanel.SetInfo(objectInfo);
//                newPanel.ToggleMenu();
//            }
//            if (historyInventoryItem != null)
//            {
//                if (inventory == null)
//                    inventory = historyInventoryItem.TargetInventory("Player1");
//                if (inventory != null)
//                {
//                    if (inventory.InventoryContains(historyInventoryItem.ItemID).Count <= 0)
//                    {
//                        //we need a serialised item to actually add to the inventory so we must create it
//                        itemPicker = GetEngine().gameObject.AddComponent<ItemPicker>();
//                        itemPicker.Item = historyInventoryItem;
//                        //we add the item to the inventory
//                        itemPicker.Quantity = 1;
//                        Invoke("Add", 0.0f);
//                    }
//                }
//            }
//            Continue();
//        }

//        private void Add()
//        {
//            itemPicker.Pick();
//            MMGameEvent.Trigger("Save");
//        }

//        public override string GetSummary()
//        {
//            if (objectInfo == null)
//            {
//                return "Error: please provide some historical information";
//            }
//            return "Show info panel with " + objectInfo.name;
//        }

//        public override Color GetButtonColour()
//        {
//            return base.GetButtonColour();
//        }
//    }
//}
