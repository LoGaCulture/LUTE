using System.Collections.Generic;
using UnityEngine;

namespace BogGames.Tools.Achievements
{
    /// <summary>
    /// Responsible for managing the achievements in the game.
    /// Handles changing state of achievements and saving or loading them to file using the LUTE saving system (AKA BogSaveManager).
    /// </summary>
    public class BogAchievementsManager : MonoBehaviour
    {
        private BogAchievementList currentAchievementList;
        private List<BogAchievement> currentAchievements;
        private BogAchievement currentAchievement;
        private string achievementsListID;

        public List<BogAchievement> CurrentAchievements { get { return currentAchievements; } }

        /// <summary>
        /// This method is required to fill the achievement list from the BogAchievementList scriptable object.
        /// Within LUTE, this is called if the user has allowed saving/loading profile for achievements.
        /// The actual load call is derived from the SaveManager class
        /// </summary>
        public void LoadAchievementList(BogAchievementList achievementList)
        {
            currentAchievements = new List<BogAchievement>();

            if (achievementList == null)
            {
                return;
            }

            currentAchievementList = achievementList;

            // Store the ID for future save purposes
            achievementsListID = achievementList.AchievementsListID;

            foreach (BogAchievement achievement in achievementList.Achievements)
            {
                currentAchievements.Add(achievement.Copy());
            }
        }

        /// <summary>
        /// Unlocks the specified achievement (if found).
        /// </summary>
        /// <param name="achievementID">Achievement I.</param>
        public void UnlockAchievement(string achievementID)
        {
            currentAchievement = CheckAchievementExists(achievementID);
            if (currentAchievement != null)
            {
                currentAchievement.UnlockAchievement();
            }
        }

        /// <summary>
        /// Locks the specified achievement (if found).
        /// </summary>
        /// <param name="achievementID">Achievement ID.</param>
        public void LockAchievement(string achievementID)
        {
            currentAchievement = CheckAchievementExists(achievementID);
            if (currentAchievement != null)
            {
                currentAchievement.LockAchievement();
            }
        }

        /// <summary>
        /// Adds progress to the specified achievement (if found).
        /// </summary>
        /// <param name="achievementID">Achievement ID.</param>
        /// <param name="newProgress">New progress.</param>
        public void AddProgress(string achievementID, int newProgress)
        {
            currentAchievement = CheckAchievementExists(achievementID);
            if (currentAchievement != null)
            {
                currentAchievement.AddProgress(newProgress);
            }
        }

        /// <summary>
        /// Sets the progress of the specified achievement (if found) to the specified progress.
        /// </summary>
        /// <param name="achievementID">Achievement ID.</param>
        /// <param name="newProgress">New progress.</param>
        public void SetProgress(string achievementID, int newProgress)
        {
            currentAchievement = CheckAchievementExists(achievementID);
            if (currentAchievement != null)
            {
                currentAchievement.SetProgress(newProgress);
            }
        }

        /// <summary>
        /// <returns>The achievement corresponding to the searched ID if found, otherwise null.</returns>
        /// <param name="searchedID">Searched I.</param>
        /// </summary>
        private BogAchievement CheckAchievementExists(string searchedID)
        {
            if (currentAchievements == null || currentAchievements.Count <= 0)
            {
                return null;
            }

            foreach (BogAchievement achievement in currentAchievements)
            {
                if (achievement.AchievementID == searchedID)
                {
                    return achievement;
                }
            }

            return null;
        }

        #region Serialisation
        /// <summary>
        /// Removes saved data and resets all achievements from a list.
        /// Typically called from an editor button or debug menu.
        /// </summary>
        /// <param name="listID">The ID of the achievement list to reset.</param>
        public virtual void ResetAchievements(string listID)
        {
            if (currentAchievements != null)
            {
                foreach (BogAchievement achievement in currentAchievements)
                {
                    achievement.SetProgress(0);
                    achievement.LockAchievement();
                }
            }
            BogAchievementSignals.DoAchievementsReset();
        }

        public virtual void ResetAchievements()
        {
            ResetAchievements(achievementsListID);
        }

        public virtual void ExtractSerialisedBogAchievementsData(BogAchievementsData serialisedAchievements)
        {
            // Extract the achievement data from the save file and update our list of achievements
            // This call is derived from the SaveManager class

            if (serialisedAchievements == null)
                return;

            for (int i = 0; i < currentAchievements.Count; i++)
            {
                for (int j = 0; j < serialisedAchievements.Achievements.Length; j++)
                {
                    if (currentAchievements[i].AchievementID == serialisedAchievements.Achievements[j].AchievementID)
                    {
                        currentAchievements[i].ProgressCurrent = serialisedAchievements.Achievements[j].ProgressCurrent;
                        currentAchievements[i].UnlockedStatus = serialisedAchievements.Achievements[j].UnlockedStatus;
                    }
                }
            }
            #endregion
        }
    }
}
