using System;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    public enum BogItemType
    {
        Default,
        Consumable,
        Equipment,
        Misc,
        Character
    }

    /// <summary>
    /// A base class for all inventory items which handles storing of item data and basic item actions.
    /// Often used in conjunction with BogBaseInventory class.
    /// </summary>
    [Serializable]
    public class BogInventoryItem : ScriptableObject
    {
        [SerializeField] protected string itemID;
        [Tooltip("The type of item this is. Used for sorting and filtering.")]
        [SerializeField] protected BogItemType itemType;
        [Tooltip("The name of the item.")]
        [SerializeField] protected string itemName;
        [Tooltip("A short description of the item.")]
        [TextArea(3, 10)]
        [SerializeField] protected string itemDescription;
        [Tooltip("Whether this item is locked or not.")]
        [SerializeField] protected bool isLocked;
        [Tooltip("Allow this item to be used, even if it is locked.")]
        [SerializeField] protected bool useWhenLocked;
        [Tooltip("Whether to consume this item when used.")]
        [SerializeField] protected bool consumeOnUse;
        [Tooltip("Whether the item can be dropped.")]
        [SerializeField] protected bool canDrop = true;
        [Tooltip("Whether the item can be moved in the inventory.")]
        [SerializeField] protected bool canMove = true;
        [Tooltip("How many of this item can be stacked; default is 1.")]
        [SerializeField] protected int maxStackSize = 1;

        [Header("SFX and VFX")]
        [Tooltip("The icon to display when the item is unlocked.")]
        [SerializeField] protected Sprite unlockedIcon;
        [Tooltip("The icon to display when the item is locked.")]
        [SerializeField] protected Sprite lockedIcon;
        [Tooltip("The sound to play when the item is used.")]
        [SerializeField] protected AudioClip useSound;
        [Tooltip("The sound to play when the item is dropped.")]
        [SerializeField] protected AudioClip dropSound;
        [Tooltip("The sound to play when the item is moved.")]
        [SerializeField] protected AudioClip moveSound;
        [Tooltip("The sound to play when an item is unlocked.")]
        [SerializeField] protected AudioClip unlockSound;
        [Tooltip("The sound to play when an item is locked.")]
        [SerializeField] protected AudioClip lockSound;
        [Tooltip("The sound to play when an item is added to the inventory.")]
        [SerializeField] protected AudioClip addSound;

        public string ItemID { get { return itemID; } }
        public string ItemName { get { return itemName; } }
        public bool IsLocked { get { return isLocked; } set { isLocked = value; } }
        public bool UseWhenLocked { get { return useWhenLocked; } }
        public string ItemDescription { get { return itemDescription; } }
        public int MaxStackSize { get { return maxStackSize; } }
        public bool ConsumeOnUse { get { return consumeOnUse; } }
        public bool CanDrop { get { return canDrop; } }
        public bool CanMove { get { return canMove; } }
        public Sprite UnlockedIcon { get { return unlockedIcon; } }
        public Sprite LockedIcon { get { return lockedIcon; } }
        public AudioClip UseSound { get { return useSound; } }
        public AudioClip DropSound { get { return dropSound; } }
        public AudioClip MoveSound { get { return moveSound; } }
        public AudioClip UnlockedSound { get { return unlockSound; } }
        public AudioClip LockedSound { get { return lockSound; } }
        public AudioClip AddSound { get { return addSound; } }

        public virtual void Use()
        {
            BogInventorySignals.DoInventoryItemUsed(this);
        }
        public virtual void Drop()
        {
            BogInventorySignals.DoInventoryItemRemoved(this);
        }

        public virtual void UnlockItem()
        {
            BogInventorySignals.DoInventoryItemUnlocked(this);
            isLocked = false;
        }

        public virtual void LockItem()
        {
            BogInventorySignals.DoInventoryItemLocked(this);
            isLocked = true;
        }
    }
}
