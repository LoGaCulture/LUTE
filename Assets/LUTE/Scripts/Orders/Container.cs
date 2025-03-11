//using MoreMountains.InventoryEngine;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;

//[OrderInfo("Adventure",
//             "Container",
//             "A container that can hold items. Can be locked or unlocked with a 'key'.")]
//[AddComponentMenu("")]
//public class Container : Order
//{
//    [Tooltip("Will spawn this prefab rather than the generic container prefab. Ideally this prefab inherits from ContainerCard")]
//    [SerializeField] protected ContainerCard setContainerCard;
//    [Tooltip("Whether this card actually requires a key")]
//    [Header("Key Settings")]
//    [SerializeField] protected bool requiresKey = true;
//    [Tooltip("The key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory")]
//    [SerializeField] protected string keyID;
//    [Tooltip("The method that should be triggered when the key is used")]
//    [SerializeField] protected UnityEvent keyAction = null;
//    [Tooltip("If this is false, the card won't be activable ")]
//    [SerializeField] protected bool activable = true;
//    [Tooltip("The delay (in seconds) after an activation during which the card can't be activated")]
//    [SerializeField] protected float delayBetweenUses = 0f;
//    [Tooltip("if this is set to false, your number of activations will be MaxNumberOfActivations")]
//    [SerializeField] protected bool unlimitedActivations = true;
//    [Tooltip("Close the card once opened")]
//    [SerializeField] protected bool closeOnUse = true;
//    [Header("Audio")]
//    [Tooltip("Sound to play when the zone gets activated")]
//    [SerializeField] protected AudioClip activationSound;
//    [Tooltip("Sound to play when the zone tries to get activated but can't")]
//    [SerializeField] protected AudioClip deniedSound;
//    [Header("Prompts")]
//    [Tooltip("Whether to show a prompt when the player is opening the container")]
//    [SerializeField] protected bool showPrompt = true;
//    [Tooltip("Info to display on the prompt when the player opens a container without a key")]
//    [SerializeField] protected string promptInfoError;
//    [Tooltip("Info to display on the prompt when the player opens a container which is already open")]
//    [SerializeField] protected string promptInfoOpened = "Already opened";
//    [Tooltip("How long the prompt info will fade for")]
//    [SerializeField] protected float promptFadeDuration = 0.2f;
//    [Tooltip("Colour of the prompt text")]
//    [SerializeField] protected Color promptColor = Color.red;
//    [Header("Locations")]
//    [Tooltip("Whether to hide this card if player is not within a radius of this card")]
//    [SerializeField] protected bool hideIfPlayerNotNearby = false;
//    [Tooltip("The location to spawn this card")]
//    [SerializeField] protected LocationVariable spawnLocation;
//    [Header("Animation")]
//    [Tooltip("The animation clip to play when opening the card")]
//    [SerializeField] protected AnimationClip openAnim;
//    [Tooltip("The animation clip to play when closing the card")]
//    [SerializeField] protected AnimationClip closeAnim;
//    [Header("Items")]
//    [Tooltip("The items that will be picked up on use")]
//    [SerializeField] protected List<InventoryItem> itemsToPickup = new List<InventoryItem>();
//    [HideInInspector]
//    [SerializeField] protected List<int> itemsQuantities = new List<int>();

//    public override void OnEnter()
//    {
//        ContainerCard.CreateNewCard(requiresKey, keyID, keyAction, activable, delayBetweenUses, unlimitedActivations, closeOnUse, activationSound, deniedSound, itemsToPickup, itemsQuantities, promptInfoError, promptFadeDuration, promptColor, hideIfPlayerNotNearby, spawnLocation, GetEngine(), setContainerCard, openAnim, closeAnim, promptInfoOpened, showPrompt);
//        Continue();
//    }

//    public override string GetSummary()
//    {
//        string summary = "Opens a container ";
//        summary += requiresKey ? "that requires " + keyID + " to open" : "that does not require a key to open";
//        return summary;
//    }
//}
