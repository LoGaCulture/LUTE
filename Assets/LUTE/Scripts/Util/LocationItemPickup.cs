//using LoGaCulture.LUTE;
//using MoreMountains.InventoryEngine;
//using UnityEngine;
//using UnityEngine.UI;

////An object that has an item picker attached - will pickup the item if the location variable returns true OR show the item card which allows pickups
//public class LocationItemPickup : MonoBehaviour
//{
//    [SerializeField] protected Image itemImage;
//    protected AudioClip feedback;
//    protected bool showPrompt;
//    protected bool showCard;
//    protected LocationVariable location;
//    protected ItemPicker item;
//    protected int quantity;
//    protected Sprite imageIcon;
//    protected Node parentNode;

//    private bool itemPickedUp = false;
//    private ButtonPrompt buttonPrompt;
//    private Canvas canvas;
//    private bool isSetup = false;

//    protected virtual void Start()
//    {
//        canvas = GetComponent<Canvas>();
//        buttonPrompt = GetComponentInChildren<ButtonPrompt>(); //add a fun animation to the prompt
//    }

//    protected virtual void Update()
//    {
//        if (isSetup)
//        {
//            if (CheckLocation())
//            {
//                //If we are at the location and we are NOT showing the card but have not picked up the item then we can pickup item
//                if (!itemPickedUp)
//                {
//                    if (!showCard)
//                    {
//                        PickupItem();
//                    }
//                }
//            }

//            bool canShowCard = CheckLocation() && !itemPickedUp && showCard;
//            if (canShowCard)
//            {
//                canvas.enabled = true;
//                if (showPrompt)
//                {
//                    buttonPrompt.PromptText.text = quantity.ToString();
//                    itemImage.sprite = item.Item.Icon;
//                    buttonPrompt.Show();
//                }
//                else
//                    buttonPrompt.Hide();
//            }
//            else
//            {
//                canvas.enabled = false;
//            }
//        }
//    }

//    public static LocationItemPickup CreateItem(LocationItemPickup customPrefab, InventoryItem item, int quantity, AudioClip pickupSound, bool prompt, bool card, LocationVariable location, Node parentNode = null)
//    {
//        GameObject go = null;
//        if (customPrefab != null)
//            go = Instantiate(customPrefab.gameObject) as GameObject;
//        else
//        {
//            GameObject containerObj = Resources.Load<GameObject>("Prefabs/ItemCard");
//            if (containerObj != null)
//                go = Instantiate(containerObj) as GameObject;
//        }
//        go.name = "ContainerItem_" + item.ItemName;

//        var itemContainer = go.GetComponent<LocationItemPickup>();
//        itemContainer.feedback = pickupSound;
//        itemContainer.showPrompt = prompt;
//        itemContainer.showCard = card;
//        itemContainer.location = location;

//        var newItem = itemContainer.gameObject.AddComponent<ItemPicker>();
//        newItem.Item = item;
//        newItem.Quantity = quantity;
//        itemContainer.item = newItem;
//        itemContainer.quantity = quantity;

//        if (parentNode != null)
//            itemContainer.parentNode = parentNode;

//        itemContainer.isSetup = true;

//        return itemContainer;
//    }

//    public virtual void PickupItem()
//    {
//        if (item != null && !itemPickedUp)
//        {
//            itemPickedUp = true;
//            if (feedback)
//            {
//                LogaManager.Instance.SoundManager.PlaySound(feedback, 1.0f);
//            }
//            item.Pick();
//        }

//    }

//    private bool CheckLocation()
//    {
//        if (parentNode != null && parentNode.ShouldCancel)
//            return false;
//        return location.Evaluate(ComparisonOperator.Equals, null); //check to see if node also executing?
//    }
//}