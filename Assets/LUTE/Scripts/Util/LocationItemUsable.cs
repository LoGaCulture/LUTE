//using LoGaCulture.LUTE;
//using MoreMountains.InventoryEngine;
//using UnityEngine;
//using UnityEngine.UI;

//public class LocationItemUsable : MonoBehaviour
//{
//    [SerializeField] protected Image itemImage;
//    protected AudioClip feedback;
//    protected bool showCard;
//    protected LocationData location;
//    protected InventoryItem item;
//    protected Sprite imageIcon;

//    private bool itemUsed = false;
//    private Canvas canvas;
//    private bool isSetup = false;

//    protected virtual void Start()
//    {
//        canvas = GetComponent<Canvas>();
//    }

//    protected virtual void Update()
//    {
//        if (isSetup)
//        {
//            if (CheckLocation())
//            {
//                //If we are at the location and we are NOT showing the card but have not picked up the item then we can pickup item
//                if (!itemUsed)
//                {
//                    if (!showCard)
//                    {
//                        UseItem();
//                    }
//                }
//            }

//            bool canShowCard = CheckLocation() && !itemUsed && showCard;
//            itemImage.sprite = item.Icon;
//            canvas.enabled = canShowCard;
//        }
//    }

//    public static LocationItemUsable CreateItem(LocationItemPickup customPrefab, InventoryItem item, AudioClip useFeedback, bool card, LocationData location)
//    {
//        GameObject go = null;
//        if (customPrefab != null)
//            go = Instantiate(customPrefab.gameObject) as GameObject;
//        else
//        {
//            GameObject containerObj = Resources.Load<GameObject>("Prefabs/UseItemCard");
//            if (containerObj != null)
//                go = Instantiate(containerObj) as GameObject;
//        }
//        go.name = "UsableItem_" + item.ItemName;

//        var itemContainer = go.GetComponent<LocationItemUsable>();
//        itemContainer.feedback = useFeedback;
//        itemContainer.showCard = card;
//        itemContainer.location = location;
//        itemContainer.item = item;

//        itemContainer.isSetup = true;

//        return itemContainer;
//    }

//    public virtual void UseItem(string playerID = "Player1")
//    {
//        if (item != null && !itemUsed)
//        {
//            itemUsed = true;
//            if (feedback)
//            {
//                LogaManager.Instance.SoundManager.PlaySound(feedback, 1.0f);
//            }
//            item.Use(playerID); //should actually find the inventory properly or the player ID properly
//        }

//    }

//    private bool CheckLocation()
//    {
//        return location.locationRef.Evaluate(ComparisonOperator.Equals, null);
//    }
//}
