//using TMPro;
//using UnityEngine;

//namespace LoGaCulture.LUTE
//{
//    public class LocationInfoPanel : ObjectInfoPanel
//    {
//        [Tooltip("The default image to display when the location is undiscovered")]
//        [SerializeField] protected Sprite defaultImage;
//        [Tooltip("The tint to apply to the image when the location is partially discovered")]
//        [SerializeField] protected Color partialTint;
//        [SerializeField] protected TextMeshProUGUI statusText;
//        [Range(0.1f, 1.0f)]
//        [Tooltip("The length of the partial text to display as a percent")]
//        [SerializeField] protected float partialTextLength = 0.3f;
//        [SerializeField] protected string defaultTitle = "Unknown Landmark";

//        private ObjectInfo ObjectInfo;
//        private LUTELocationInfo LocationInfo;

//        protected override void Awake()
//        {
//            ActiveLocationInfoPanel = this;
//        }
//        protected override void OnDestroy()
//        {
//            ActiveLocationInfoPanel = null;
//        }

//        protected void Update()
//        {
//            // Grouping the null checks for clarity
//            if (LocationInfo == null || infoImage == null || titleText == null || bodyText == null || statusText == null)
//            {
//                return;
//            }

//            // Common data setup
//            string title = defaultTitle;
//            string status = string.Empty;
//            Sprite sprite = defaultImage;
//            Color color = Color.white;
//            string body = string.Empty;

//            switch (LocationInfo._LocationStatus)
//            {
//                case LUTELocationInfo.LocationStatus.Unvisited:
//                    status = "Status: Undiscovered";
//                    body = string.Empty;
//                    break;

//                case LUTELocationInfo.LocationStatus.Visited:
//                    //sprite = ObjectInfo.ObjectIcon;
//                    sprite = LocationInfo.LocationImage;
//                    color = partialTint;
//                    //title = ObjectInfo.ObjectName;
//                    title = LocationInfo.DisplayName;
//                    status = "Status: Partially Discovered";

//                    // Ensure fullText is valid before substring calculation
//                    //string fullText = ObjectInfo.ShortDescription ?? string.Empty;
//                    string fullText = LocationInfo.Description ?? string.Empty;
//                    int lengthToShow = (int)(fullText.Length * partialTextLength);
//                    lengthToShow = Mathf.Clamp(lengthToShow, 0, fullText.Length); // Ensure valid substring length
//                    body = fullText.Substring(0, lengthToShow) + "...";
//                    break;

//                case LUTELocationInfo.LocationStatus.Completed:
//                    //sprite = ObjectInfo.ObjectIcon;
//                    sprite = LocationInfo.LocationImage;
//                    //title = ObjectInfo.ObjectName;
//                    title = LocationInfo.DisplayName;
//                    status = "Status: Fully Discovered";
//                    //body = ObjectInfo.ShortDescription;
//                    body = LocationInfo.Description;
//                    break;
//            }

//            // Applying the calculated values
//            infoImage.sprite = sprite;
//            infoImage.color = color;
//            titleText.text = title;
//            statusText.text = status;
//            bodyText.text = body;
//        }

//        public static LocationInfoPanel GetLocationInfoPanel()
//        {
//            if (ActiveLocationInfoPanel == null)
//            {
//                GameObject prefab = Resources.Load<GameObject>("Prefabs/LocationInfoPanel");
//                if (CustomLocationPrefab != null)
//                {
//                    prefab = CustomLocationPrefab.gameObject;
//                }
//                if (prefab != null)
//                {
//                    GameObject go = Instantiate(prefab);
//                    go.SetActive(false);
//                    go.name = "LocationInfoPanel";
//                    ActiveLocationInfoPanel = go.GetComponent<LocationInfoPanel>();
//                }
//            }
//            return ActiveLocationInfoPanel;
//        }

//        public override void SetInfo(ObjectInfo info)
//        {
//            if (info == null)
//                return;

//            // If there is no object info or the new object info is not the current object info
//            if (!ObjectInfo || ObjectInfo.ObjectName != info.ObjectName)
//            {
//                ObjectInfo = info;
//            }
//        }

//        public void SetLocationInfo(LUTELocationInfo info)
//        {
//            if (info == null)
//                return;

//            // If there is no location info or the new location info is not the current location info
//            if (!LocationInfo || LocationInfo.infoID != info.infoID)
//            {
//                LocationInfo = info;
//            }
//            //if (LocationInfo.objectInfo != null)
//            //{
//            //    SetInfo(LocationInfo.objectInfo);
//            //}
//        }
//    }
//}