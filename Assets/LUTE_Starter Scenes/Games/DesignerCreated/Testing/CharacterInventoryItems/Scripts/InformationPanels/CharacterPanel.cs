using System.Collections;
using TMPro;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Inherited from information panel to be used as a panel that displays a character's information.
    /// Often called from inventory systems and used to play various mini-games.
    /// </summary>
    public class CharacterPanel : InformationPanel
    {
        [Tooltip("The main canvas group that is used to fade the panel in and out.")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [Tooltip("The character portrait component that is rendered to the screen.")]
        [SerializeField] protected UnityEngine.UI.Image characterPortraitRenderer;
        [Tooltip("The character name component that is rendered to the screen.")]
        [SerializeField] protected TextMeshProUGUI characterNameText;
        [Tooltip("The character text component that is rendered to the screen.")]
        [SerializeField] protected TextMeshProUGUI detailText;
        [Tooltip("The button that is used to extend the functionality of this panel.")]
        [SerializeField] protected UnityEngine.UI.Button playButton;
        [Tooltip("How long to fade the menu in and out for.")]
        [SerializeField] protected float fadeDuration = 0.5f;

        public override void TogglePanel()
        {
            FadePanelCanvas();
        }

        /// <summary>
        /// Sets the information to be displayed on the panel.
        /// </summary>
        /// <param name="text"></param>
        public void SetInformation(Sprite characterPortrait, string characterName, string details, UnityEngine.Events.UnityEvent action)
        {
            if (characterPortraitRenderer == null)
            {
                characterPortraitRenderer = GetComponentInChildren<UnityEngine.UI.Image>();
            }
            if (characterPortraitRenderer == null || characterPortrait == null)
            {
                return;
            }

            if (characterNameText == null)
            {
                characterNameText = GetComponentInChildren<TextMeshProUGUI>();
            }
            if (characterNameText == null || characterName == null)
            {
                return;
            }
            if (detailText == null)
            {
                detailText = GetComponentInChildren<TextMeshProUGUI>();
            }
            if (detailText == null || string.IsNullOrEmpty(details))
            {
                return;
            }

            if (playButton == null)
            {
                playButton = GetComponentInChildren<UnityEngine.UI.Button>();
            }

            characterPortraitRenderer.sprite = characterPortrait;
            characterNameText.text = characterName;
            detailText.text = details;
            // We skip null checks for the button as it is optional.
            // Of course we must still check if it is null before adding a listener.
            if (playButton != null && action.GetPersistentEventCount() >= 1)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(action.Invoke);
                // Toggle the panel after the button has been invoked - this is fairly standard but you could make this optional if required
                playButton.onClick.AddListener(TogglePanel);
            }
        }

        private void FadePanelCanvas()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                return;
            }

            if (isPanelActive)
            {
                StartCoroutine(FadeCanvasGroupIEnum(canvasGroup, fadeDuration, 0f));
                isPanelActive = false;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                StartCoroutine(FadeCanvasGroupIEnum(canvasGroup, fadeDuration, 1f));
                isPanelActive = true;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        private IEnumerator FadeCanvasGroupIEnum(CanvasGroup target, float duration, float targetAlpha, bool unscaled = true)
        {
            if (target == null)
            {
                target = canvasGroup;
            }

            if (target == null)
            {
                yield break;
            }

            float currentAlpha = target.alpha;

            float t = 0f;
            while (t < 1.0f)
            {
                if (target == null)
                    yield break;

                float newAlpha = Mathf.SmoothStep(currentAlpha, targetAlpha, t);
                target.alpha = newAlpha;

                if (unscaled)
                {
                    t += Time.unscaledDeltaTime / duration;
                }
                else
                {
                    t += Time.deltaTime / duration;
                }

                yield return null;
            }

            target.alpha = targetAlpha;
        }
    }
}
