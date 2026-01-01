namespace LoGaCulture.LUTE
{
    //[CreateAssetMenu(fileName = "PostcardAchievementList", menuName = "LUTE/Postcard Achievement List")]
    //[ExecuteAlways]
    //public class PostcardAchievementList : MMAchievementList
    //{
    //    public List<PostcardAchievement> PostcardAchievements = new List<PostcardAchievement>();

    //    public override void ResetAchievements()
    //    {
    //        base.ResetAchievements();
    //        foreach (PostcardAchievement achievement in PostcardAchievements)
    //        {
    //            achievement.ProgressCurrent = 0;
    //            achievement.UnlockedStatus = false;
    //        }
    //    }

    //    public bool CheckPostcardAchievement(Postcard postcard)
    //    {
    //        foreach (PostcardAchievement achievement in PostcardAchievements)
    //        {
    //            if (!achievement.UnlockedStatus)
    //            {
    //                bool allRulesMet = true;
    //                foreach (CustomAchievement rule in achievement.rules)
    //                {
    //                    if (!CheckRule(rule, postcard))
    //                    {
    //                        allRulesMet = false;
    //                        break;
    //                    }
    //                }

    //                if (allRulesMet)
    //                {
    //                    var engine = postcard.Manager.engine;
    //                    achievement.SetEngine(engine);
    //                    achievement.UnlockAchievement();
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    private bool CheckRule(CustomAchievement rule, Postcard postcard)
    //    {
    //        bool rulesMet = false;

    //        // Check sticker rules
    //        if (rule.stickerType == StickerManager.StickerType.None)
    //        {
    //            rulesMet = postcard.stickers.Count >= rule.totalStickersRequired;
    //        }
    //        else
    //        {
    //            int matchingStickersCount = postcard.stickers.Count(s => s.StickerType == rule.stickerType);
    //            rulesMet = postcard.stickers.Count >= rule.totalStickersRequired && matchingStickersCount >= rule.totalStickersRequired;
    //        }

    //        // If sticker rules are met, check text rules
    //        if (rulesMet)
    //        {
    //            bool CheckTextRule(string text, string contains, string doesNotContain)
    //            {
    //                return (string.IsNullOrEmpty(contains) || text.Contains(contains)) &&
    //                       (string.IsNullOrEmpty(doesNotContain) || !text.Contains(doesNotContain));
    //            }

    //            if (rule.checkNameText && !CheckTextRule(postcard.PostcardName, rule.nameTextContains, rule.nameTextDoesNotContain) ||
    //                rule.checkAuthorText && !CheckTextRule(postcard.PostcardCreator, rule.authorTextContains, rule.authorTextDoesNotContain) ||
    //                rule.checkDescText && !CheckTextRule(postcard.PostcardDesc, rule.descTextContains, rule.descTextDoesNotContain))
    //            {
    //                rulesMet = false;
    //            }
    //        }

    //        return rulesMet;
    //    }
    //}
}