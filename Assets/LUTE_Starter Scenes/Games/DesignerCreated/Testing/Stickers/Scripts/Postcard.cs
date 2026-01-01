using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class to hold information and methods for a postcard in the sticker game system
/// </summary>
public class Postcard : MonoBehaviour
{
    [Header("Postcard Details")]
    [Tooltip("The name of the postcard")]
    [SerializeField] protected string postcardName;
    [Tooltip("The description of the postcard")]
    [SerializeField] protected string postcardDescription;
    [Tooltip("The creator of the postcard")]
    [SerializeField] protected string postcardCreator;
    [Tooltip("The total number of stickers that can be on this postcard")]
    [SerializeField] protected int totalStickers;
    [Header("Components")]
    [Tooltip("The text that will be displayed for the postcard name")]
    [SerializeField] protected TMP_InputField nameText;
    [Tooltip("The text that will be displayed for the postcard description")]
    [SerializeField] protected TMP_InputField descriptionText;
    [Tooltip("The text that will be displayed for the postcard creator")]
    [SerializeField] protected TMP_InputField creatorText;
    [SerializeField] protected Animator anim;
    [SerializeField] protected RectTransform binIcon;
    //[Header("Feedbacks")]
    //[Tooltip("Feedback to be played upon sticker being deleted")]
    //[SerializeField] protected MMFeedbacks deleteStickerFeedback;
    //[Tooltip("Feedback to be played when loading postcard")]
    //[SerializeField] protected MMFeedbacks loadPostcardFeedback;
    //[Tooltip("Feedback to be played when discarding postcard")]
    //[SerializeField] protected MMFeedbacks discardFeedback;
    //[Tooltip("Feedback to be played when submitting postcard")]
    //[SerializeField] protected MMFeedbacks submitPostcardFeedback;
    //[Tooltip("Feedback to be played when flipping postcard")]
    //[SerializeField] protected MMFeedbacks flipPostcardFeedback;
    //[Tooltip("Feedback to be played when hovering over bin")]
    //[SerializeField] protected MMFeedbacks binHoverFeedback;
    //[Tooltip("Feedback to be played when achievement has been unlocked on this postcard")]
    //[SerializeField] protected MMFeedbacks achievementUnlockedFeedback;

    protected List<PostcardVar.StickerVar> stickerVars = new List<PostcardVar.StickerVar>();
    // The stickers that are on this postcard
    [HideInInspector]
    public List<Sticker> stickers = new List<Sticker>();
    // The layout group that the stickers will be placed in
    private Transform stickerCanvas;
    // Whether the postcard has been discarded recently
    private bool isDiscarded;
    // Whether the postcard has been flipped
    private bool isFlipped;

    public static List<Postcard> activePostcards = new List<Postcard>();

    public string PostcardName { get { return postcardName; } set { postcardName = value; } }
    public string PostcardDesc { get { return postcardDescription; } set { postcardDescription = value; } }
    public string PostcardCreator { get { return postcardCreator; } set { postcardCreator = value; } }
    public int TotalStickers { get { return totalStickers; } set { totalStickers = value; } }
    public List<Sticker> PostcardStickers { get { return stickers; } set { stickers = value; } }
    public List<PostcardVar.StickerVar> StickerVars { get { return stickerVars; } set { stickerVars = value; } }

    private StickerManager manager;

    public StickerManager Manager { get { return manager; } }

    /// <summary>
    /// Returns the progress of the postcard (how many stickers are on it and their type based on the StickerManager achievement list)
    private int PostcardProgress()
    {
        return 0;
    }

    public Postcard SavePostcard(Postcard newPostcard, Postcard savedPostcard)
    {
        if (newPostcard == null)
            return null;
        if (savedPostcard == null)
            return null;

        newPostcard.postcardName = savedPostcard.postcardName;
        newPostcard.postcardDescription = savedPostcard.postcardDescription;
        newPostcard.postcardCreator = savedPostcard.postcardCreator;
        newPostcard.totalStickers = savedPostcard.totalStickers;

        var originalStickers = savedPostcard.stickers;
        var manager = FindObjectOfType<StickerManager>();

        foreach (Sticker sticker in originalStickers)
        {
            var newSticker = manager.AddStickerComponent();
            newSticker.Initialise(sticker);
            newPostcard.stickers.Add(newSticker);
        }

        return newPostcard;
    }

    public static Postcard ActivePostcard { get; set; }

    protected virtual void Awake()
    {
        if (!activePostcards.Contains(this))
        {
            activePostcards.Add(this);
        }

        manager = FindObjectOfType<StickerManager>();
    }

    protected virtual void OnDestroy()
    {
        activePostcards.Remove(this);
    }

    public static Postcard GetPostcard(string name, string description, StickerItem sticker, string creator, bool checkName = true)
    {
        if (ActivePostcard == null)
        {
            Postcard postcard = null;
            if (activePostcards.Count > 0)
            {
                postcard = activePostcards[0];
            }
            if (postcard != null)
            {
                ActivePostcard = postcard;
            }
            if (ActivePostcard == null)
            {
                // Create a new postcard 
                var postCardPrefab = Resources.Load<GameObject>("Prefabs/BlankPostcard");
                if (postCardPrefab != null)
                {
                    var postcardGO = Instantiate(postCardPrefab).GetComponent<Postcard>();
                    if (checkName)
                        name = postcardGO.GetUniquePostcardName(name);
                    postcardGO.name = name;
                    postcardGO.postcardName = name;
                    postcardGO.postcardDescription = description;
                    postcardGO.postcardCreator = creator;
                    postcardGO.stickerCanvas = postcardGO.GetComponentInChildren<GridLayoutGroup>().transform;
                    postcardGO.SetPostcardText(name, description, creator);
                    ActivePostcard = postcardGO;
                    return postcardGO;
                }
            }
        }
        if (ActivePostcard.isDiscarded)
        {
            if (checkName)
                name = ActivePostcard.GetUniquePostcardName(name);
            ActivePostcard.SetPostcardBase(name, description, creator);
            ActivePostcard.SetPostcardText(name, description, creator);
        }
        return ActivePostcard;
    }

    // Returns a new postcard name that is guaranteed not to clash with any existing Postcards.
    public virtual string GetUniquePostcardName(string originalKey, Postcard ignorePostcard = null)
    {
        int suffix = 0;
        string baseKey = originalKey.Trim();

        // No empty keys allowed
        if (baseKey.Length == 0)
        {
            baseKey = LogaConstants.DefaultPostcardName;
        }

        var engine = manager.engine;

        var post = engine.GetComponents<Postcard>();

        string key = baseKey;
        while (true)
        {
            bool collision = false;
            for (int i = 0; i < post.Length; i++)
            {
                var postcard = post[i];
                if (postcard == ignorePostcard || postcard.PostcardName == null)
                {
                    continue;
                }
                if (postcard.PostcardName.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    collision = true;
                    suffix++;
                    key = baseKey + " " + suffix;
                }
            }

            if (!collision)
            {
                return key;
            }
        }
    }

    public static Postcard SetStickers(Postcard postcard, bool checkName = false)
    {
        //foreach(var oldPostcard in activePostcards)
        //{
        //    oldPostcard.Discard(null);
        //}
        //if(ActivePostcard)
        //    ActivePostcard.Discard(null);

        Postcard activePostcard = GetPostcard(postcard.postcardName, postcard.postcardDescription, null, postcard.postcardCreator, checkName);

        if (activePostcard == null)
            return null;


        foreach (var sticker in postcard.stickerVars)
        {
            activePostcard.AddSticker(sticker);
        }

        activePostcard.SetActive(true);
        //activePostcard.loadPostcardFeedback?.PlayFeedbacks();

        return activePostcard;
    }

    public void SetPostcardBase(string name, string desc, string creator)
    {
        if (ActivePostcard != null)
        {
            ActivePostcard.name = name;
            ActivePostcard.postcardName = name;
            ActivePostcard.postcardDescription = desc;
            ActivePostcard.postcardCreator = creator;
        }

        ActivePostcard.isDiscarded = false;
    }

    public void SetPostcardText(string name, string desc, string creator)
    {
        if (nameText != null)
        {
            nameText.text = name;
        }
        if (descriptionText != null)
        {
            descriptionText.text = desc;
        }
        if (creatorText != null)
        {
            creatorText.text = creator;
        }
    }

    public virtual void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }

    public Sticker AddSticker(StickerItem sticker)
    {
        if (sticker == null)
            return null;
        if (stickerCanvas == null)
            return null;
        // Find the sticker prefab in the resources
        var stickerPrefab = Resources.Load<GameObject>("Prefabs/BlankSticker");
        if (stickerPrefab == null)
            return null;
        // If the postcard is limited by the number of stickers it can have, check if the limit has been reached
        if (totalStickers > 0 && stickers.Count >= totalStickers)
            return null;
        // Instantiate the sticker prefab and set its parent to the sticker canvas
        var stickerInstance = Instantiate(stickerPrefab, stickerCanvas).GetComponent<Sticker>();
        // Use sticker class to set image and name etc of the sticker (i.e., initialise the sticker)
        stickerInstance.Initialise(sticker);
        if (binIcon != null)
            stickerInstance.SetBinIcon(binIcon);
        stickerInstance.SetPostcard(this);

        // Add the sticker to the list of stickers
        stickers.Add(stickerInstance);

        return stickerInstance;
    }

    public Sticker AddSticker(Sticker sticker)
    {
        if (sticker == null)
            return null;
        if (stickerCanvas == null)
            return null;
        // Find the sticker prefab in the resources
        var stickerPrefab = Resources.Load<GameObject>("Prefabs/BlankSticker");
        if (stickerPrefab == null)
            return null;
        // Instantiate the sticker prefab and set its parent to the sticker canvas
        var stickerInstance = Instantiate(stickerPrefab, stickerCanvas).GetComponent<Sticker>();
        // Use sticker class to set image and name etc of the sticker (i.e., initialise the sticker)
        stickerInstance.Initialise(sticker);

        // Add the sticker to the list of stickers
        stickers.Add(stickerInstance);

        return stickerInstance;
    }

    public Sticker AddSticker(PostcardVar.StickerVar sticker)
    {
        if (sticker == null)
            return null;
        if (stickerCanvas == null)
            return null;
        // Find the sticker prefab in the resources
        var stickerPrefab = Resources.Load<GameObject>("Prefabs/BlankSticker");
        if (stickerPrefab == null)
            return null;
        // Instantiate the sticker prefab and set its parent to the sticker canvas
        var stickerInstance = Instantiate(stickerPrefab, stickerCanvas).GetComponent<Sticker>();
        // Use sticker class to set image and name etc of the sticker (i.e., initialise the sticker)
        stickerInstance.Initialise(sticker);
        if (binIcon != null)
            stickerInstance.SetBinIcon(binIcon);
        stickerInstance.SetPostcard(this);

        // Add the sticker to the list of stickers
        stickers.Add(stickerInstance);

        return stickerInstance;
    }

    public Sticker RemoveSticker(Sticker sticker)
    {
        if (sticker == null)
            return null;
        stickers.Remove(sticker);
        Destroy(sticker.gameObject);
        // Play feedback
        //deleteStickerFeedback?.PlayFeedbacks();
        return sticker;
    }

    public void SubmitDesign()
    {
        manager.SubmitDesign(ActivePostcard);
        //PostcardAchievementList list = MMAchievementManager.currentList as PostcardAchievementList;
        //if (list != null)
        //{
        //    if(list.CheckPostcardAchievement(this))
        //    {
        //        achievementUnlockedFeedback?.PlayFeedbacks();
        //    }
        //    else
        //        submitPostcardFeedback?.PlayFeedbacks();
        //}
        //else
        //    submitPostcardFeedback?.PlayFeedbacks();
        Discard(false);
    }

    public void Discard(bool playFedback = true)
    {
        isDiscarded = true;
        foreach (var sticker in stickers)
        {
            Destroy(sticker.gameObject);
        }
        stickers.Clear();
        postcardName = "";
        postcardDescription = "";

        if (isFlipped)
        {
            FlipPostcard();
            stickerCanvas.transform.Rotate(0, -180, 0);
        }

        //if(playFedback)
        //        discardFeedback?.PlayFeedbacks();
        ActivePostcard?.SetActive(false);
    }

    public void FlipPostcard()
    {
        if (nameText == null || descriptionText == null || creatorText == null)
        {
            return;
        }

        var descriptorGroup = nameText.transform.parent.GetComponent<CanvasGroup>();
        if (descriptorGroup == null)
        {
            return;
        }

        if (!isFlipped)
        {
            if (anim != null)
            {
                anim.SetTrigger("Flip");
            }

            foreach (var sticker in stickers)
            {
                sticker.FlipSticker();
            }
            descriptorGroup.alpha = 1;
            descriptorGroup.blocksRaycasts = true;
            descriptorGroup.interactable = true;
            isFlipped = true;
        }
        else
        {
            if (anim != null)
            {
                anim.SetTrigger("Unflip");
            }

            foreach (var sticker in stickers)
            {
                sticker.FlipSticker();
            }
            descriptorGroup.alpha = 0;
            descriptorGroup.blocksRaycasts = false;
            descriptorGroup.interactable = false;
            isFlipped = false;
        }

        //flipPostcardFeedback?.PlayFeedbacks();
    }

    public void UpdatePostcardName(string name)
    {
        if (ActivePostcard)
            ActivePostcard.postcardName = name;
    }
    public void UpdatePostcardDesc(string desc)
    {
        if (ActivePostcard)
            ActivePostcard.PostcardDesc = desc;
    }
    public void UpdatePostcardCreator(string creator)
    {
        if (ActivePostcard)
            ActivePostcard.PostcardCreator = creator;
    }

    public void PlayBinFeedback()
    {
        //binHoverFeedback?.PlayFeedbacks();
    }
    public void StopBinFeedback()
    {
        //binHoverFeedback?.StopFeedbacks();
    }
}