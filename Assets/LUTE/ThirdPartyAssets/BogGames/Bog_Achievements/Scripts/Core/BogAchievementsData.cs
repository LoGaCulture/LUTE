using System.Collections.Generic;
using UnityEngine;

namespace BogGames.Tools.Achievements
{
    /// <summary>
    /// A serializable class used to store an achievement into a save file
    /// </summary>
    [System.Serializable]
    public class SerialisedBogAchievement
    {
        [SerializeField] private string achievementID;
        [SerializeField] private bool unlockedStatus;
        [SerializeField] private int progressCurrent;

        public string AchievementID { get { return achievementID; } set { achievementID = value; } }
        public bool UnlockedStatus { get { return unlockedStatus; } set { unlockedStatus = value; } }
        public int ProgressCurrent { get { return progressCurrent; } set { progressCurrent = value; } }

        /// <summary>
        /// Initialises a new instance of the <see cref="BogGames.Tools.SerialisedBogAchievement"/> class.
        /// </summary>  
        /// <param name="achievementID">Achievement ID.</param>
        /// <param name="unlockedStatus">If set to <c>true</c> unlocked status.</param>
        /// <param name="progressCurrent">Progress current.</param>
        public SerialisedBogAchievement(string achievementID, bool unlockedStatus, int progressCurrent)
        {
            AchievementID = achievementID;
            UnlockedStatus = unlockedStatus;
            ProgressCurrent = progressCurrent;
        }
    }

    /// <summary>
    /// Serialisable container for encoding the state of achievements.
    /// Handles the encoding and decoding of data derived from the achievement manager or other managers that have access to achievement data.
    ///</summary>
    [System.Serializable]
    public class BogAchievementsData
    {
        [SerializeField] private SerialisedBogAchievement[] achievements;

        public SerialisedBogAchievement[] Achievements { get { return achievements; } set { achievements = value; } }

        public static BogAchievementsData Encode(List<BogAchievement> currentAchievements)
        {
            var achievementData = new BogAchievementsData();

            achievementData.achievements = new SerialisedBogAchievement[currentAchievements.Count];

            for (int i = 0; i < currentAchievements.Count; i++)
            {
                achievementData.achievements[i] = new SerialisedBogAchievement(currentAchievements[i].AchievementID, currentAchievements[i].UnlockedStatus, currentAchievements[i].ProgressCurrent);
            }

            return achievementData;
        }

        /// <summary>
        /// Extracts the serialised achievements into our achievements array if the achievements ID match.
        /// </summary>
        /// <param name="achievementsData">Serialized achievements.</param>
        public static void Decode(BogAchievementsData achievementsData, BogAchievementsManager bogAchievementsManager)
        {
            if (bogAchievementsManager == null)
            {
                Debug.LogError("BogAchievementsManager is null. Cannot decode achievements data.");
                return;
            }

            if (achievementsData == null)
            {
                Debug.LogError("Achievements data is null. Cannot decode achievements data.");
                return;
            }

            bogAchievementsManager.ExtractSerialisedBogAchievementsData(achievementsData);
        }
    }
}
