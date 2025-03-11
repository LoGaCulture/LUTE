//using LoGaCulture.LUTE;
//using MoreMountains.InventoryEngine;
//using System;
//using System.Collections.Generic;
//using UnityEngine;


//[CreateAssetMenu(fileName = "ContainerItem", menuName = "LUTE/Inventory/ContainerItem", order = 0)]
//[Serializable]
///// <summary>
///// Container item class, to use when you want an objec that can itself be a container in your inventory
///// </summary>
//public class ContainerItem : InventoryItem
//{
//    [Header("Key Settings")]
//    [Tooltip("Whether this card actually requires a key")]
//    [SerializeField] protected bool requiresKey = true;
//    [Tooltip("The key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory")]
//    [SerializeField] protected string keyID;
//    [Tooltip("The delay (in seconds) after an activation during which the card can't be activated")]
//    [SerializeField] protected float delayBetweenUses = 0f;
//    [Tooltip("if this is set to false, your number of activations will be MaxNumberOfActivations")]
//    [SerializeField] protected bool unlimitedActivations = true;
//    [Tooltip("Whether to show a prompt when the player is opening the container")]
//    [SerializeField] protected bool showPrompt = true;
//    [Header("Key Sounds")]
//    [Tooltip("Feedback to play when the zone gets activated")]
//    [SerializeField] protected AudioClip activationSound;
//    [Tooltip("Feedback to play when the zone tries to get activated but can't")]
//    [SerializeField] protected AudioClip deniedSound;
//    [Tooltip("Sprite to show when the container has been opened")]
//    [SerializeField] protected Sprite openSprite;
//    [SerializeField] protected InventoryItem[] items;
//    [SerializeField] protected int[] quantities;

//    protected bool isOpen = false;
//    protected float lastActivationTimestamp;
//    protected int numberOfActivationsLeft;
//    protected List<int> keyList = new List<int>();
//    protected Inventory inventory;
//    protected static List<ItemPicker> itemPickers = new List<ItemPicker>();

//    public override bool Pick(string playerID)
//    {
//        base.Pick(playerID);
//        itemPickers.Clear();
//        inventory = TargetInventory("Player1");
//        for (int i = 0; i < items.Length; i++)
//        {
//            var itemPicker = inventory.gameObject.AddComponent<ItemPicker>();
//            itemPicker.Item = items[i];
//            itemPicker.Quantity = quantities[i];
//            itemPickers.Add(itemPicker);
//        }
//        return true;
//    }

//    public override bool Use(string playerID)
//    {
//        TriggerOpening();
//        if (isOpen)
//            return true;
//        else
//            return false;
//    }

//    protected virtual void TriggerOpening()
//    {
//        if (!CheckNumberOfUses())
//        {
//            PromptError();
//            return;
//        }

//        inventory = TargetInventory("Player1");

//        if (inventory == null)
//        {
//            return;
//        }

//        if (isOpen)
//        {
//            PromptOpenError();
//            return;
//        }

//        if (requiresKey)
//        {
//            keyList.Clear();
//            keyList = inventory.InventoryContains(keyID);
//            if (keyList.Count <= 0)
//            {
//                //No key was found
//                PromptError();
//                return;
//            }
//            else
//            {
//                inventory.UseItem(keyID);
//            }
//        }

//        //If we have to this stage then the key has been found and used
//        //We can now trigger the key action
//        if (activationSound)
//        {
//            LogaManager.Instance.SoundManager.PlaySound(activationSound, 1.0f);
//        }
//        Icon = openSprite;
//        TriggerKeyAction();
//        isOpen = true;
//    }

//    protected virtual void TriggerKeyAction()
//    {
//        for (int i = 0; i < itemPickers.Count; i++)
//        {
//            itemPickers[i].Pick();
//        }
//    }

//    public virtual void PromptError()
//    {
//        if (deniedSound)
//        {
//            LogaManager.Instance.SoundManager.PlaySound(deniedSound, 1.0f);
//        }
//        if (showPrompt)
//        {
//            //do stuff to show an error
//        }
//    }

//    public virtual void PromptOpenError()
//    {
//        if (deniedSound)
//        {
//            LogaManager.Instance.SoundManager.PlaySound(deniedSound, 1.0f);
//        }
//        if (showPrompt)
//        {
//            //do stuff to show an error
//        }
//    }

//    public virtual bool CheckNumberOfUses()
//    {
//        if (!Usable)
//        {
//            return false;
//        }

//        if (Time.time - lastActivationTimestamp < delayBetweenUses)
//        {
//            return false;
//        }

//        if (unlimitedActivations)
//        {
//            return true;
//        }

//        if (numberOfActivationsLeft == 0)
//        {
//            return false;
//        }

//        if (numberOfActivationsLeft > 0)
//        {
//            return true;
//        }
//        return false;
//    }

//    public virtual void AddItems(InventoryItem item, int quantity)
//    {
//        for (int i = 0; i < items.Length; i++)
//        {
//            if (items[i] == item)
//            {
//                quantities[i] += quantity;
//                return;
//            }
//        }
//        inventory = TargetInventory("Player1");
//        var itemPicker = inventory.gameObject.AddComponent<ItemPicker>();
//        itemPicker.Item = item;
//        itemPicker.Quantity = quantity;
//        itemPickers.Add(itemPicker);
//        Array.Resize(ref items, items.Length + 1);
//        Array.Resize(ref quantities, quantities.Length + 1);
//        items[items.Length - 1] = item;
//        quantities[quantities.Length - 1] = quantity;
//    }
//}