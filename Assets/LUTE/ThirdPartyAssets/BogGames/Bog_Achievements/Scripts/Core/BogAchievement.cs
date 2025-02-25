using System;
using UnityEngine;

namespace BogGames.Tools.Achievements
{
    /// <summary>
    /// This achievement system supports two simple types of achievements:
    /// Simple (do something > get achievement)
    /// Progress based (jump X times, collect X items, etc).
    /// This class is used to store relevant information and general methods for achievements.
    /// </summary>
    public enum AchievementType { Simple, Progressive }

    [Serializable]
    public class BogAchievement : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("The unique identifier for this achievement.")]
        [SerializeField] protected string achievementID;
        [Tooltip("The type of achievement.")]
        [SerializeField] protected AchievementType achievementType;
        [Tooltip("If true, the achievement won't be displayed in the achievement list.")]
        [SerializeField] protected bool hiddenAchievement;
        [Tooltip("If true, the achievement has been unlocked.")]
        [SerializeField] protected bool unlockedStatus;

        [Header("Description")]
        [Tooltip("The achievement's name/title.")]
        [SerializeField] protected string title;
        [Tooltip("The achievement's description.")]
        [SerializeField] protected string description;

        [Header("Visuals and SFX")]
        [Tooltip("The image to display while this achievement is locked.")]
        [SerializeField] protected Sprite lockedImage;
        [Tooltip("The image to display when the achievement is unlocked.")]
        [SerializeField] protected Sprite unlockedImage;
        // Uses LUTE built-in sound manager but can be replaced with generic audio source or third party audio mangagement.
        [Tooltip("The sound to play when the achievement is unlocked.")]
        [SerializeField] protected AudioClip unlockedSound;
        [Tooltip("The sound to play when displaying this achievement and progress has been made (but not unlocked).")]
        [SerializeField] protected AudioClip progressSound;

        [Header("Progress Status")]
        [Tooltip("The amount of progress needed to unlock this achievement.")]
        [SerializeField] protected int progressTarget;
        [Tooltip("The current amount of progress made on this achievement.")]
        [SerializeField] protected int progressCurrent;

        public string AchievementID { get { return achievementID; } }
        public string Title { get { return title; } }
        public string Description { get { return description; } }
        public Sprite UnlockedImage { get { return unlockedImage; } }
        public AudioClip UnlockedSound { get { return unlockedSound; } }
        public AudioClip ProgressSound { get { return progressSound; } }
        public AchievementType AchievementType { get { return achievementType; } }
        public bool UnlockedStatus { get { return unlockedStatus; } set { unlockedStatus = value; } }
        public int ProgressCurrent { get { return progressCurrent; } set { progressCurrent = value; } }
        public int ProgressTarget { get { return progressTarget; } }

        /// <summary>
        /// Unlocks the achievement and will save the current achievements using LUTE's built-in save system.
        /// Try replacing the saving with player prefs if abstracting this class.
        /// Sends signals to the achievement signal manager that will often be caught by the achievement display class.
        /// </summary>
        public virtual void UnlockAchievement()
        {
            // If the achievement has already been unlocked, we do nothing and exit.
            if (unlockedStatus)
            {
                return;
            }

            unlockedStatus = true;

            // Display Achievement if permitted via achievement signals.
            // One could make this optional if desired but we always choose to display achievements.
            BogAchievementSignals.DoAchievementUnlocked(this);
        }

        /// <summary>
        /// Lock current Achievement.
        /// </summary>
        public virtual void LockAchievement()
        {
            // If the achievement has already been locked, we do nothing and exit.
            if (!unlockedStatus)
            {
                return;
            }

            unlockedStatus = false;
        }

        /// <summary>
        /// Add specified value to the current progress.
        /// </summary>
        /// <param name="newProgress">New progress.</param>
        public virtual void AddProgress(int newProgress)
        {
            progressCurrent += newProgress;
            EvaluateProgress();
        }

        /// <summary>
        /// Sets the progress to the value passed in parameter.
        /// </summary>
        /// <param name="newProgress">New progress.</param>
        public virtual void SetProgress(int newProgress)
        {
            progressCurrent = newProgress;
            EvaluateProgress();
        }

        /// <summary>
        /// Evaluates the current progress of the achievement, and unlocks it if needed.
        /// </summary>
        protected virtual void EvaluateProgress()
        {
            if (progressCurrent >= progressTarget)
            {
                progressCurrent = progressTarget;
                UnlockAchievement();
            }
            else if (progressCurrent > 0)
            {
                // We send signals out to the achievement signal manager for achievement changed state.
                // Mostly used for displaying visuals of the acheivement progress.
                BogAchievementSignals.DoAchievementProgress(this);
            }
        }

        /// <summary>
        /// Copies this achievement (useful when loading from a scriptable object list)
        /// </summary>
        public virtual BogAchievement Copy()
        {
            BogAchievement clone = ScriptableObject.CreateInstance<BogAchievement>();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this), clone);
            // we use Json utility to store a copy of our achievement - not a reference
            //clone = JsonUtility.FromJsonOverwrite<BogAchievement>(JsonUtility.ToJson(this));
            return clone;
        }
    }
}