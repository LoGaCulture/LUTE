using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(StickerManager))]
public class StickerMenu : MonoBehaviour
{
    [SerializeField] protected GameObject postcardButton;
    [SerializeField] protected Transform buttonLayout;
    //[Tooltip("Feedback to be played when closing this menu")]
    //[SerializeField] protected MMFeedbacks closeFeedback;
    //[Tooltip("Feedback to be played when opening this menu")]
    //[SerializeField] protected MMFeedbacks openFeedback;

    private StickerManager stickerManager;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Awake()
    {
        stickerManager = GetComponent<StickerManager>();
    }

    public void ShowPostcardMenu()
    {
        if (stickerManager == null)
            return;

        var engine = stickerManager.engine;
        if (engine == null)
            return;

        var postcards = engine.Postcards;
        if (postcards == null || postcards.Count <= 0)
            return;

        if (postcardButton == null || buttonLayout == null)
            return;

        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) return;
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) return;

        // If there are no buttons then we must be showig the menu
        if (spawnedButtons.Count <= 0)
        {
            // Spawn or use a bunch of buttons on this guy
            for (int i = 0; i < postcards.Count; i++)
            {
                // If there are no stickers then we can't show this postcard
                if (postcards[i].StickerVars.Count <= 0)
                    continue;

                int index = i;

                var newButton = Instantiate(postcardButton, buttonLayout);
                var button = newButton.GetComponentInChildren<UnityEngine.UI.Button>();
                button.onClick.AddListener(() => stickerManager.LoadPostCard(index));
                button.onClick.AddListener(() => ClearMenu(canvasGroup, canvas));

                UnityEngine.UI.Image image = null;
                foreach (Transform t in newButton.transform)
                {
                    image = t.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                        break;
                }

                // Set the button image to first sticker
                if (image != null)
                {
                    var firstSticker = postcards[i].StickerVars[0].Image;
                    image.sprite = firstSticker;
                }

                var buttonName = newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                if (buttonName != null)
                {
                    var postcardName = engine.Postcards[i].PostcardName;
                    buttonName.text = postcardName;
                }

                TextMeshProUGUI descText = null;
                foreach (Transform t in newButton.transform)
                {
                    TextMeshProUGUI tmpComponent = t.GetComponent<TextMeshProUGUI>();
                    if (tmpComponent != null && tmpComponent != buttonName)
                    {
                        descText = tmpComponent;
                        break;
                    }
                }

                if (descText != null)
                {
                    var postcardDesc = engine.Postcards[i].PostcardDesc;
                    descText.text = postcardDesc;
                }

                spawnedButtons.Add(newButton);
            }

            //openFeedback?.PlayFeedbacks();
            canvasGroup.alpha = 1.0f;
            canvas.enabled = true;

        }
        else
        {
            ClearMenu(canvasGroup, canvas);
            //closeFeedback?.PlayFeedbacks();
        }
    }

    private void ClearMenu(CanvasGroup canvasGroup, Canvas canvas)
    {
        if (canvasGroup == null)
            return;
        // If there are buttons then we must destroy them and hide this menu
        foreach (var button in spawnedButtons)
        {
            Destroy(button.gameObject);
        }
        spawnedButtons.Clear();

        canvasGroup.alpha = 0.0f;
        canvas.enabled = false;
    }
}
