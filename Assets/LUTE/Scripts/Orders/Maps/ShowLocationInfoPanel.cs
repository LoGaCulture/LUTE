//using UnityEngine;

//namespace LoGaCulture.LUTE
//{
//    [OrderInfo("Maps", "Show Location Info Panel", "Show the location info panel for the current location")]
//    public class ShowLocationInfoPanel : Order
//    {
//        [Tooltip("The location to show the info panel for - default is node location or event handler location")]
//        [SerializeField] protected LocationData customLocation;
//        [Tooltip("The prefab to use for the location info panel - default is stored in 'Resources/Prefabs'")]
//        [SerializeField] protected LocationInfoPanel customPanelPrefab;
//        public override void OnEnter()
//        {
//            if (customPanelPrefab != null)
//            {
//                LocationInfoPanel.CustomLocationPrefab = customPanelPrefab;
//            }

//            LocationInfoPanel panel = LocationInfoPanel.GetLocationInfoPanel();
//            bool hasInfo = false;
//            if (panel != null)
//            {
//                if (customLocation.Value != null)
//                {
//                    panel.SetLocationInfo(customLocation.Value);
//                    hasInfo = true;
//                }
//                else
//                {
//                    var locInfo = ParentNode.NodeLocation;
//                    if (locInfo == null)
//                    {
//                        LocationClickEventHandler handler = ParentNode._EventHandler as LocationClickEventHandler;
//                        if (handler != null)
//                        {
//                            locInfo = handler.Location.locationRef;
//                        }
//                    }
//                    if (locInfo != null)
//                    {
//                        panel.SetLocationInfo(locInfo.Value);
//                        hasInfo = true;
//                    }
//                }
//                if (hasInfo)
//                {
//                    panel.ToggleMenu();
//                }
//            }
//            Continue();
//        }

//        public override string GetSummary()
//        {
//            string location = customLocation.Value != null ? customLocation.Value.Name : "current location";
//            return $"Show location info panel for {location}";
//        }
//    }
//}
