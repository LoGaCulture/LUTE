//using BogGames.Tools.Inventory;
//using UnityEngine;

//public class GenericContainer : MonoBehaviour
//{
//    [SerializeField] protected BogInventoryBase inventory;

//    protected Animator animator;
//    protected BogInventoryItem[] itemList;

//    protected virtual void Start()
//    {
//        animator = GetComponent<Animator>();
//        itemList = GetComponents<BogInventoryItem>();
//    }

//    public virtual void AddContents(BogInventoryItem[] items, Animator animator)
//    {
//        itemList = items;
//        this.animator = animator;
//    }

//    public virtual void OpenContainer()
//    {
//        TriggerOpeningAnimation();
//        PickContainerContents();
//    }

//    public virtual void TriggerOpeningAnimation()
//    {
//        if (animator == null)
//        {
//            return;
//        }
//        animator.SetTrigger("Open");
//    }

//    protected virtual void PickContainerContents()
//    {
//        if (itemList.Length == 0 || inventory == null)
//        {
//            return;
//        }
//        foreach (BogInventoryItem item in itemList)
//        {
//            inventory.AddItem(item);
//        }
//    }
//}
