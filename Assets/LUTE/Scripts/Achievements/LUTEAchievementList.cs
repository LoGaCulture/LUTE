using BogGames.Tools.Achievements;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Inherited class from BogAchievementList that utilises the save and singleton management system to load and reset achievements.
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementList", menuName = "LUTE/Achievements/Achievement List")]
    public class LUTEAchievementList : BogAchievementList
    {
        public override void ResetAchievements()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Debug.LogFormat("BogAchievements: Achievements Reset");
                LogaManager.Instance.BogAchievementsManager.ResetAchievements();
            }
            else
            {
                Debug.LogWarning("BogAchievementRules Warning: Must be playing the game to reset achievements");
            }
#endif        
        }
    }
}
