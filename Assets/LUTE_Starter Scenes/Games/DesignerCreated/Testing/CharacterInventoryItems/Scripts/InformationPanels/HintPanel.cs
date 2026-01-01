using System.Collections;
using TMPro;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Inherited from information panel to be used as a panel that displays a hint relating to various mini-games.
    /// Often called from inventory systems.
    /// </summary>
    public class HintPanel : InformationPanel
    {
        [Tooltip("The main canvas group that is used to fade the panel in and out.")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [Tooltip("The hint text component that is rendered to the screen.")]
        [SerializeField] protected TextMeshProUGUI hintText;
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
        public void SetInformation(string text)
        {
            if (hintText == null)
            {
                hintText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (hintText == null)
            {
                return;
            }

            hintText.text = text;
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
