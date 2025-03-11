//using MoreMountains.InventoryEngine;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace LoGaCulture.LUTE
//{
//    public class StickerCollectionMenu : MonoBehaviour
//    {
//        [SerializeField] protected Inventory mainInventory;
//        [SerializeField] protected Transform stickerGroup;

//        protected LTDescr fadeTween; //Used for fading menu

//        protected static bool menuActive = false;

//        private CanvasGroup canvasGroup;
//        private List<StickerCollectionMenuItem> stickerItems = new List<StickerCollectionMenuItem>();


//        // this needs to be done with resources loading rather than asset database
//        //public static T[] GetAllInstances<T>() where T : ScriptableObject
//        //{
//        //    string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name); //FindAssets uses tags check documentation for more info
//        //    T[] instances = new T[guids.Length];
//        //    for (int i = 0; i < guids.Length; i++) //probably could get optimized
//        //    {
//        //        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
//        //        instances[i] = AssetDatabase.LoadAssetAtPath<T>(path);
//        //    }

//        //    // This may be the ideal solution - could split this into editor code (above) and runtime code (below)
//        //    //var list = Resources.FindObjectsOfTypeAll<T>();
//        //    //return list;

//        //    return instances;
//        //}

//        protected virtual void Start()
//        {
//            canvasGroup = GetComponentInChildren<CanvasGroup>();
//            if (!menuActive)
//            {
//                canvasGroup.alpha = 0.0f;
//                canvasGroup.interactable = false;
//                canvasGroup.blocksRaycasts = false;
//            }
//        }

//        public void ToggleMenu()
//        {
//            if (mainInventory == null) return;


//            var items = new List<InventoryItem>();
//            //if (items.Length <= 0) return;

//            //if (stickerGroup == null) return;

//            //if (canvasGroup == null) return;

//            //if (fadeTween != null)
//            //{
//            //    LeanTween.cancel(fadeTween.id, true);
//            //    fadeTween = null;
//            //}

//            foreach (var item in items)
//            {
//                bool unlocked = false;
//                if (stickerItems.Contains(stickerItems.Where(x => x.ItemID == item.ItemID).FirstOrDefault())) continue;
//                var stickerCollectionMenuItem = Instantiate(Resources.Load<GameObject>("Prefabs/StickerCollectionItem"), stickerGroup).GetComponent<StickerCollectionMenuItem>();
//                if (stickerCollectionMenuItem == null) continue;

//                stickerCollectionMenuItem.Initialise(item.ItemName, item.Icon, item.ItemID);
//                stickerItems.Add(stickerCollectionMenuItem);
//                // Check if the item is in the inventory
//                List<int> list = mainInventory.InventoryContains(item.ItemID);

//                if (list.Count > 0)
//                {

//                    unlocked = true;
//                }
//                stickerCollectionMenuItem.SetUnlocked(unlocked);
//            }

//            // Fade the canvas group in using lean tween
//            if (menuActive)
//            {
//                //Fade menu out
//                LeanTween.value(canvasGroup.gameObject, canvasGroup.alpha, 0f, 0.4f)
//        .setEase(LeanTweenType.easeOutQuint)
//        .setOnUpdate((t) =>
//        {
//            canvasGroup.alpha = t;
//        }).setOnComplete(() =>
//        {
//            canvasGroup.alpha = 0f;
//            canvasGroup.interactable = false;
//            canvasGroup.blocksRaycasts = false;
//        });
//            }
//            else
//            {
//                //Fade menu in
//                LeanTween.value(canvasGroup.gameObject, canvasGroup.alpha, 1f, 0.4f)
//        .setEase(LeanTweenType.easeOutQuint)
//        .setOnUpdate((t) =>
//        {
//            canvasGroup.alpha = t;
//        }).setOnComplete(() =>
//        {
//            canvasGroup.alpha = 1f;
//            canvasGroup.interactable = true;
//            canvasGroup.blocksRaycasts = true;
//        });
//            }
//            menuActive = !menuActive;
//        }
//    }
//}
