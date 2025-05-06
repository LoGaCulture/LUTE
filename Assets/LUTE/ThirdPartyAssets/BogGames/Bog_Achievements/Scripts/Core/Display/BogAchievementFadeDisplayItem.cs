using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BogGames.Tools.Achievements
{
    /// <summary>
    /// A class used to hold relevant information used to display an achievement, typically in a fading manner.
    /// Add this to a prefab containing the relevant elements listed below.
    /// </summary>
	[AddComponentMenu("Bog Games/Tools/Achievements/BogAchievementFadeDisplayItem")]
    public class BogAchievementFadeDisplayItem : MonoBehaviour
    {
        [Tooltip("The background image to display when the achievement is locked.")]
        [SerializeField] protected Image lockedBackground;
        [Tooltip("The background image to display when the achievement is unlocked.")]
        [SerializeField] protected Image unlockedBackground;
        [Tooltip("The icon image to display for the achievement.")]
        [SerializeField] protected Image icon;
        [Tooltip("The title text to display for the achievement.")]
        [SerializeField] protected TextMeshProUGUI title;
        [Tooltip("The description text to display for the achievement.")]
        [SerializeField] protected TextMeshProUGUI description;
        [Tooltip("The progress bar to display for the achievement.")]
        [SerializeField] protected Slider progressBarDisplay;

        public Image Icon { get { return icon; } set { icon = value; } }
        public TextMeshProUGUI Title { get { return title; } set { title = value; } }
        public TextMeshProUGUI Description { get { return description; } set { description = value; } }
        public Slider ProgressBarDisplay { get { return progressBarDisplay; } set { progressBarDisplay = value; } }
        public string AchievementID { get; set; }

        public void FadeCanvasGroup(CanvasGroup target, float duration, float targetAlpha, bool unscaled = true)
        {
            StartCoroutine(FadeCanvasGroupIEnum(target, duration, targetAlpha, unscaled));
        }

        public IEnumerator FadeCanvasGroupIEnum(CanvasGroup target, float duration, float targetAlpha, bool unscaled = true)
        {
            if (target == null)
            {
                target = GetComponent<CanvasGroup>();
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
            if (targetAlpha == 0f)
            {
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// Used when called from the achievement canvas class to help setup items in the list.
        /// </summary>
        /// <param name="achievement"></param>
        public virtual void SetAchievementInMenu(BogAchievement achievement)
        {
            AchievementID = achievement.AchievementID;
            title.text = achievement.Title;
            description.text = achievement.Description;
            if (achievement.AchievementType == AchievementType.Progressive)
            {
                progressBarDisplay.value = (float)achievement.ProgressCurrent / (float)achievement.ProgressTarget;
                progressBarDisplay.gameObject.SetActive(true);
            }
            else
            {
                progressBarDisplay.gameObject.SetActive(false);
            }

            if (achievement.UnlockedStatus)
            {
                lockedBackground.enabled = false;
                unlockedBackground.enabled = true;
                icon.sprite = achievement.UnlockedImage;
            }
            else
            {
                lockedBackground.enabled = true;
                unlockedBackground.enabled = false;
            }
        }

        /// <summary>
        /// Unlocks the relevant item in the achievement list.
        /// </summary>
        /// <param name="achievement"></param>
        public virtual void UnlockAchievementInMenu(BogAchievement achievement)
        {
            if (achievement.UnlockedStatus == true)
            {
                lockedBackground.enabled = false;
                unlockedBackground.enabled = true;
                icon.sprite = achievement.UnlockedImage;
            }

            if (achievement.AchievementType == AchievementType.Progressive)
            {
                progressBarDisplay.value = (float)achievement.ProgressCurrent / (float)achievement.ProgressTarget;
                progressBarDisplay.gameObject.SetActive(true);
            }
            else
            {
                progressBarDisplay.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Progress the relevant achievement progress in the list
        /// </summary>
        /// <param name="achievement"></param>
        public virtual void UpdateAchievementProgressInMenu(BogAchievement achievement)
        {
            if (achievement.AchievementType == AchievementType.Progressive)
            {
                progressBarDisplay.value = (float)achievement.ProgressCurrent / (float)achievement.ProgressTarget;
                progressBarDisplay.gameObject.SetActive(true);
            }
            else
            {
                progressBarDisplay.gameObject.SetActive(false);
            }
        }
    }
}
