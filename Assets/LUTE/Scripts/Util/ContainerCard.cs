//using BogGames.Tools.Inventory;
//using LoGaCulture.LUTE;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;

//public class ContainerCard : MonoBehaviour
//{
//    [Tooltip("Whether this card actually requires a key")]
//    [SerializeField] protected bool requiresKey = true;
//    [Tooltip("The key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory")]
//    [SerializeField] protected string keyID;
//    [Tooltip("The method that should be triggered when the key is used")]
//    [SerializeField] protected UnityEvent keyAction;
//    [Tooltip("If this is false, the card won't be activable ")]
//    [SerializeField] protected bool activable = true;
//    [Tooltip("The delay (in seconds) after an activation during which the card can't be activated")]
//    [SerializeField] protected float delayBetweenUses = 0f;
//    [Tooltip("if this is set to false, your number of activations will be MaxNumberOfActivations")]
//    [SerializeField] protected bool unlimitedActivations = true;
//    [Header("Audio")]
//    [Tooltip("Sound to play when the zone gets activated")]
//    [SerializeField] protected AudioClip activationSound;
//    [Tooltip("Sound to play when the zone tries to get activated but can't")]
//    [SerializeField] protected AudioClip deniedSound;
//    [Tooltip("Close the card once opened")]
//    [SerializeField] protected bool closeOnUse = true;
//    [Tooltip("Whether to show a prompt when the player is opening the container")]
//    [SerializeField] protected bool showPrompt = true;
//    [Tooltip("Info to display on the prompt when the player tries to open container without a key")]
//    [SerializeField] protected string promptInfoError;
//    [Tooltip("Info to display on the prompt when the player tries to open container which is already opened")]
//    [SerializeField] protected string promptInfoOpened;
//    [Tooltip("How long the prompt info will fade for")]
//    [SerializeField] protected float promptFadeDuration = 0.2f;
//    [Tooltip("Colour of the prompt text")]
//    [SerializeField] protected Color promptColor = Color.red;
//    [Tooltip("Whether to hide this card if player is not within a radius of this card")]
//    [SerializeField] protected bool hideIfPlayerNotNearby = false;
//    [Tooltip("The location to spawn this card")]
//    [SerializeField] protected LocationVariable spawnLocation;
//    [Tooltip("The animation clip to play when opening the card")]
//    [SerializeField] protected AnimationClip openAnim;
//    [Tooltip("The animation clip to play when closing the card")]
//    [SerializeField] protected AnimationClip closeAnim;

//    protected List<int> keyList;
//    protected float lastActivationTimestamp;
//    protected int numberOfActivationsLeft;
//    protected Animator buttonPromptAnimator;
//    protected TextMeshProUGUI promptText;
//    protected ButtonPrompt buttonPrompt;
//    protected bool isOpen = false;

//    [HideInInspector]
//    public bool hasBeenSetup = false;

//    protected static List<ContainerCard> activeContainerCards = new List<ContainerCard>();
//    public static ContainerCard ActiveContainerCard { get; set; }

//    private LocationVariable locationVar;
//    private CanvasGroup canvasGroup;
//    private string promptInfoText;

//    protected virtual void Awake()
//    {
//        if (!activeContainerCards.Contains(this))
//        {
//            activeContainerCards.Add(this);
//        }
//    }

//    protected virtual void OnDestroy()
//    {
//        activeContainerCards.Remove(this);
//    }

//    protected virtual void Start()
//    {
//        keyList = new List<int>();
//        buttonPrompt = GetComponentInChildren<ButtonPrompt>();
//        if (buttonPrompt != null)
//        {
//            buttonPromptAnimator = buttonPrompt.GetComponent<Animator>();
//            promptText = buttonPrompt.GetComponentInChildren<TextMeshProUGUI>();
//        }
//    }

//    protected virtual void Update()
//    {
//        if (hasBeenSetup)
//        {
//            if (hideIfPlayerNotNearby && canvasGroup != null)
//            {
//                GetComponent<Canvas>().enabled = HideOnPlayer();
//                canvasGroup.alpha = HideOnPlayer() ? 1f : 0f;
//            }
//            if (!showPrompt)
//            {
//                if (buttonPrompt != null)
//                {
//                    buttonPrompt.gameObject.SetActive(false);
//                }

//            }
//        }
//    }

//    public static ContainerCard GetContainerCard()
//    {
//        if (ActiveContainerCard == null)
//        {
//            ContainerCard containerCard = null;
//            if (activeContainerCards.Count > 0)
//            {
//                containerCard = activeContainerCards[0];
//            }
//            if (containerCard != null)
//            {
//                ActiveContainerCard = containerCard;
//            }
//            if (ActiveContainerCard == null)
//            {
//                GameObject containerObj = Resources.Load<GameObject>("Prefabs/ContainerCard");
//                if (containerObj != null)
//                {
//                    GameObject go = Instantiate(containerObj) as GameObject;
//                    go.name = "ContainerCard";
//                    ActiveContainerCard = go.GetComponent<ContainerCard>();
//                }
//            }
//        }

//        return ActiveContainerCard;
//    }

//    public static void CreateNewCard(bool requiresKey, string keyID, UnityEvent keyEvent, bool activable, float delayBetweenUses, bool unlimitedActivations, bool closeOnUse, AudioClip openFeedback, AudioClip closeFeedback, List<BogInventoryItem> itemsToPickup = null, List<int> itemsQuantities = null, string promtText = "", float promptFadeDuration = 0.2f, Color promptColor = new Color(), bool hideOnPlayer = false, LocationVariable locationVariable = null, BasicFlowEngine engine = null, ContainerCard customPrefab = null, AnimationClip openAnim = null, AnimationClip closeAnim = null, string promtTextOpened = null, bool showPrompt = true)
//    {
//        GameObject go = null;

//        if (customPrefab != null)
//            go = Instantiate(customPrefab.gameObject) as GameObject;
//        else
//        {
//            GameObject containerObj = Resources.Load<GameObject>("Prefabs/ContainerCard");
//            if (containerObj != null)
//                go = Instantiate(containerObj) as GameObject;
//        }
//        go.name = "ContainerCard";

//        var containerCard = go.GetComponent<ContainerCard>();

//        containerCard.requiresKey = requiresKey;
//        containerCard.keyID = keyID;
//        if (keyEvent.GetPersistentEventCount() > 0) // Check if keyEvent is not null
//            containerCard.keyAction = keyEvent;
//        containerCard.activable = activable;
//        containerCard.delayBetweenUses = delayBetweenUses;
//        containerCard.unlimitedActivations = unlimitedActivations;
//        containerCard.closeOnUse = closeOnUse;
//        containerCard.activationSound = openFeedback;
//        containerCard.deniedSound = closeFeedback;
//        containerCard.promptInfoError = promtText;
//        containerCard.promptInfoOpened = promtTextOpened;
//        containerCard.promptFadeDuration = promptFadeDuration;
//        containerCard.promptColor = promptColor;
//        containerCard.hideIfPlayerNotNearby = hideOnPlayer;
//        if (openAnim != null)
//            containerCard.openAnim = openAnim;
//        if (closeAnim != null)
//            containerCard.closeAnim = closeAnim;
//        containerCard.showPrompt = showPrompt;
//        containerCard.locationVar = locationVariable;

//        var itemContainerObj = containerCard.GetComponentInChildren<GenericContainer>();
//        AnimatorOverrideController animator = new AnimatorOverrideController(itemContainerObj.GetComponent<Animator>().runtimeAnimatorController);
//        itemContainerObj.GetComponent<Animator>().runtimeAnimatorController = animator;

//        if (animator != null)
//        {
//            if (containerCard.openAnim != null)
//            {
//                animator["ChestOpen"] = containerCard.openAnim;
//            }
//            if (containerCard.closeAnim != null)
//                animator["ChestClosed"] = containerCard.closeAnim;
//        }
//        if (itemContainerObj != null && itemsToPickup.Count > 0)
//        {
//            for (int i = 0; i < itemsToPickup.Count; i++)
//            {
//                var itemPickerComp = itemContainerObj.gameObject.AddComponent<ItemPicker>();
//                itemPickerComp.Item = itemsToPickup[i];
//                if (itemsQuantities != null && itemsQuantities.Count > i)
//                {
//                    itemPickerComp.Quantity = itemsQuantities[i];
//                }
//                else
//                {
//                    itemPickerComp.Quantity = 1;
//                }
//            }
//            itemContainerObj.AddContents(itemContainerObj.GetComponents<ItemPicker>(), itemContainerObj.GetComponent<Animator>());
//        }
//        containerCard.canvasGroup = containerCard.GetComponent<CanvasGroup>();
//        containerCard.hasBeenSetup = true;

//        if (containerCard.isOpen)
//        {
//            itemContainerObj = containerCard.GetComponentInChildren<GenericContainer>();
//            itemContainerObj.TriggerOpeningAnimation();
//        }
//    }

//    public virtual void SetupCard(bool requiresKey, string keyID, UnityEvent keyEvent, bool activable, float delayBetweenUses, bool unlimitedActivations, bool closeOnUse, AudioClip openFeedback, AudioClip closeFeedback, List<ItemPicker> itemsToPickup = null, List<int> itemsQuantities = null, string promtText = "", float promptFadeDuration = 0.2f, Color promptColor = new Color(), bool hideOnPlayer = false, string locationVariable = "", BasicFlowEngine engine = null)
//    {
//        if (!ActiveContainerCard.hasBeenSetup)
//        {
//            ActiveContainerCard.requiresKey = requiresKey;
//            ActiveContainerCard.keyID = keyID;
//            if (keyEvent.GetPersistentEventCount() > 0) // Check if keyEvent is not null
//                ActiveContainerCard.keyAction = keyEvent;
//            ActiveContainerCard.activable = activable;
//            ActiveContainerCard.delayBetweenUses = delayBetweenUses;
//            ActiveContainerCard.unlimitedActivations = unlimitedActivations;
//            ActiveContainerCard.closeOnUse = closeOnUse;
//            ActiveContainerCard.activationSound = openFeedback;
//            ActiveContainerCard.deniedSound = closeFeedback;
//            ActiveContainerCard.promptInfoError = promtText;
//            // ActiveContainerCard.promptInfoOpened = promtTextOpened;
//            ActiveContainerCard.promptFadeDuration = promptFadeDuration;
//            ActiveContainerCard.promptColor = promptColor;
//            ActiveContainerCard.hideIfPlayerNotNearby = hideOnPlayer;
//            if (!string.IsNullOrEmpty(locationVariable) && engine != null)
//            {
//                // ActiveContainerCard.spawnLocation = locationVariable;
//                var locations = engine.GetComponents<LocationVariable>();
//                for (int i = 0; i < locations.Length; i++)
//                {
//                    var locString = locationVariable;

//                    if (locations[i].Scope == VariableScope.Global && locations[i].Value.Position == locString)
//                    {
//                        ActiveContainerCard.locationVar = locations[i];
//                    }
//                }
//            }
//            var itemContainerObj = ActiveContainerCard.GetComponentInChildren<GenericContainer>();
//            if (itemContainerObj != null && itemsToPickup.Count > 0)
//            {
//                for (int i = 0; i < itemsToPickup.Count; i++)
//                {
//                    var itemPickerComp = itemContainerObj.gameObject.AddComponent<ItemPicker>();
//                    itemPickerComp.Item = itemsToPickup[i].Item;
//                    if (itemsQuantities != null && itemsQuantities.Count > i)
//                    {
//                        itemPickerComp.Quantity = itemsQuantities[i];
//                    }
//                    else
//                    {
//                        itemPickerComp.Quantity = 1;
//                    }
//                }
//                itemContainerObj.AddContents(itemContainerObj.GetComponents<ItemPicker>(), itemContainerObj.GetComponent<Animator>());
//            }
//            ActiveContainerCard.canvasGroup = ActiveContainerCard.GetComponent<CanvasGroup>();
//            ActiveContainerCard.hasBeenSetup = true;
//        }
//        else
//        {
//            if (ActiveContainerCard.isOpen)
//            {
//                var itemContainerObj = ActiveContainerCard.GetComponentInChildren<GenericContainer>();
//                itemContainerObj.TriggerOpeningAnimation();
//            }
//        }
//    }

//    public virtual void TriggerButtonAction(Inventory inventory = null)
//    {
//        if (!CheckNumberOfUses())
//        {
//            PromptError();
//            return;
//        }
//        if (inventory == null)
//        {
//            //find inventory in the scene
//            inventory = FindFirstObjectByType<Inventory>();
//        }
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
//        if (activationSound != null)
//        {
//            LogaManager.Instance.SoundManager.PlaySound(activationSound, 1.0f);
//        }

//        TriggerKeyAction();
//        isOpen = true;
//        if (closeOnUse)
//        {
//            this.gameObject.SetActive(false);
//        }
//    }

//    public virtual void TriggerButtonAction()
//    {
//        if (!CheckNumberOfUses())
//        {
//            PromptError();
//            return;
//        }
//        var inventory = FindFirstObjectByType<Inventory>();

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
//        if (activationSound != null)
//        {
//            LogaManager.Instance.SoundManager.PlaySound(activationSound, 1.0f);
//        }
//        TriggerKeyAction();
//        isOpen = true;
//        if (closeOnUse)
//        {
//            this.gameObject.SetActive(false);
//        }
//    }

//    protected virtual void TriggerKeyAction()
//    {
//        if (keyAction != null)
//        {
//            keyAction.Invoke();
//        }
//    }

//    public virtual void PromptError()
//    {
//        if (showPrompt)
//        {
//            if (buttonPromptAnimator != null)
//            {
//                buttonPromptAnimator.SetTrigger("Error");
//            }
//            if (deniedSound != null)
//            {
//                LogaManager.Instance.SoundManager.PlaySound(deniedSound, 1.0f);
//            }
//            if (promptText != null && !string.IsNullOrEmpty(promptInfoError))
//            {
//                promptInfoText = promptInfoError;
//                StopAllCoroutines();
//                StartCoroutine(FadePromptInfo());
//            }
//        }
//    }

//    public virtual void PromptOpenError()
//    {
//        if (showPrompt)
//        {
//            if (buttonPromptAnimator != null)
//            {
//                buttonPromptAnimator.SetTrigger("Error");
//            }
//            if (deniedSound != null)
//            {
//                LogaManager.Instance.SoundManager.PlaySound(deniedSound, 1.0f);
//            }
//            if (promptText != null && !string.IsNullOrEmpty(promptInfoOpened))
//            {
//                promptInfoText = promptInfoOpened;
//                StopAllCoroutines();
//                StartCoroutine(FadePromptInfo());
//            }
//        }
//    }

//    //Checks the remaining number of uses and eventual delay between uses and returns true if the zone can be activated.
//    public virtual bool CheckNumberOfUses()
//    {
//        if (!activable)
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

//    private IEnumerator FadePromptInfo()
//    {
//        if (promptText != null && !string.IsNullOrEmpty(promptInfoText))
//        {
//            promptText.text = promptInfoText;
//            promptText.color = new Color(promptText.color.r, promptText.color.g, promptText.color.b, 1f);
//            float t = 0f;
//            yield return new WaitForSeconds(.25f);
//            while (t < 1)
//            {
//                t += Time.deltaTime / promptFadeDuration;
//                promptText.color = new Color(promptText.color.r, promptText.color.g, promptText.color.b, Mathf.Lerp(1f, 0f, t));
//                yield return null;
//            }
//            yield return new WaitForSeconds(promptFadeDuration);
//            //reset values for next time
//            promptText.text = "";
//            promptText.color = new Color(promptText.color.r, promptText.color.g, promptText.color.b, 0f);
//        }
//    }

//    private bool HideOnPlayer()
//    {
//        return locationVar.Evaluate(ComparisonOperator.Equals, null);
//    }
//}
