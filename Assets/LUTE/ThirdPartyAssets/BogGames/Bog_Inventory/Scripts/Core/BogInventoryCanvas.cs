using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// A class that handles the drawing of an inventory.
    /// Often called from the base inventory classes and requires specific Inventory UI Items to render correctly.
    /// Rather than build these "slots" we force users to provide their own versions to reduce complexity and allow for customisation.
    /// </summary>
    public class BogInventoryCanvas : MonoBehaviour
    {
        [Tooltip("The grid layout object that is used to draw the inventory.")]
        [SerializeField] protected Transform gridLayout;
        [Tooltip("The inventory item prefab that is used to draw empty slots")]
        [SerializeField] protected BogInventoryUIItem emptyItemPrefab;
        [Tooltip("The main canvas group that is used to fade the inventory in and out.")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [Tooltip("How long to fade the menu in and out for.")]
        [SerializeField] protected float fadeDuration = 0.5f;
        [Tooltip("The audio clip to play when the inventory is opened.")]
        [SerializeField] protected AudioClip openSound;
        [Tooltip("The audio clip to play when the inventory is closed.")]
        [SerializeField] protected AudioClip closeSound;

        private bool inventoryOpen = false;

        public AudioClip OpenSound { get { return openSound; } }
        public AudioClip CloseSound { get { return closeSound; } }

        public virtual void DrawInventory(List<BogInventorySlot?> items, int selectedIndex, BogInventoryBase currentInventory)
        {
            foreach (Transform child in gridLayout)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < items.Count; i++)
            {
                BogInventoryUIItem newItem = Instantiate(emptyItemPrefab, gridLayout);
                newItem.SetInventory(currentInventory);
                newItem.SetItem(items[i], i);
                if (i == selectedIndex)
                {
                    newItem.SetSelected(true);
                }
            }
        }

        public virtual void FadeInventoryCanvas()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                return;
            }

            if (inventoryOpen)
            {
                BogInventorySignals.DoInventoryMenuClosed(this);

                StartCoroutine(FadeCanvasGroupIEnum(canvasGroup, fadeDuration, 0f));
                inventoryOpen = false;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                BogInventorySignals.DoInventoryMenuOpened(this);

                StartCoroutine(FadeCanvasGroupIEnum(canvasGroup, fadeDuration, 1f));
                inventoryOpen = true;
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
