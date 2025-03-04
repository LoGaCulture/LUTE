using System;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    public enum BogItemType
    {
        Default,
        Consumable,
        Equipment,
        Misc
    }

    /// <summary>
    /// A base class for all inventory items which handles storing of item data and basic item actions.
    /// Often used in conjunction with BogBaseInventory class.
    /// </summary>
    [CreateAssetMenu(fileName = "New Bog Inventory Item", menuName = "Bog Inventory/Inventory Item")]
    [Serializable]
    public class BogInventoryItem : ScriptableObject
    {
        [SerializeField] protected int itemID;
        [Tooltip("The type of item this is. Used for sorting and filtering.")]
        [SerializeField] protected BogItemType itemType;
        [Tooltip("The name of the item.")]
        [SerializeField] protected string itemName;
        [Tooltip("A short description of the item.")]
        [SerializeField] protected string itemDescription;
        [Tooltip("The icon to display when the item is unlocked.")]
        [SerializeField] protected Sprite unlockedIcon;
        [Tooltip("The icon to display when the item is locked.")]
        [SerializeField] protected Sprite lockedIcon;
        [Tooltip("The sound to play when the item is used.")]
        [SerializeField] protected AudioClip useSound; // replace with other audio library if needed
        [Tooltip("The sound to play when the item is moved inside the inventory.")]
        [SerializeField] protected AudioClip moveSound; // replace with other audio library if needed

        public Sprite UnlockedIcon { get { return unlockedIcon; } }
        public AudioClip MoveSound { get { return moveSound; } }

        public virtual void Use()
        {
            // play sound
        }
        public virtual void Drop()
        {

        }
    }
}
