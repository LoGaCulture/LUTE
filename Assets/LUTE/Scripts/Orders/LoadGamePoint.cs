using LoGaCulture.LUTE;
using UnityEngine;
using UnityEngine.SceneManagement;

[OrderInfo(
    "Saving",
    "Load Game",
    "Loads game from either default or custom save file and will load to latest save point or to specific save point with custom key.")]
public class LoadGamePoint : Order
{
    [Tooltip("The key to load the game from - if none is provided then we use default")]
    [SerializeField] protected string saveKey = LogaConstants.DefaultSaveDataKey;
    [Tooltip("If true, load the game from this specific point when using the save data file provided")]
    [SerializeField] protected bool loadCustomPoint = false;
    [Tooltip("If loading from specific point, provide the key that you wish to load; this should match the save point ID exactly.")]
    [SerializeField] protected string customKey = string.Empty;
    [Tooltip("If set to true and the loading fails then the node moves to next order in the list - often this will be calling next node and returning to the prior node (such as a menu).")]
    [SerializeField] protected bool continueToNextOrder = true;

    public override void OnEnter()
    {
        var saveManager = LogaManager.Instance.SaveManager;

        saveManager.StartScene = SceneManager.GetActiveScene().name;

        if (!HandleSaveDataLoad(saveManager))
        {
            if (continueToNextOrder)
            {
                Continue();
            }
        }
    }

    private bool HandleSaveDataLoad(SaveManager saveManager)
    {
        if (!saveManager.HasSaveData(saveKey))
        {
            return false;
        }

        if (loadCustomPoint && !string.IsNullOrEmpty(customKey))
        {
            saveManager.Load(saveKey, true, customKey);
        }
        else
        {
            saveManager.Load(saveKey);
        }

        return true;
    }

    public override string GetSummary()
    {
        string summary = "Load game from ";
        if (saveKey != LogaConstants.DefaultSaveDataKey)
        {
            summary += saveKey;
        }
        else
        {
            summary += "default save data";
        }
        if (loadCustomPoint)
        {
            summary += "custom point: " + customKey;
        }
        return summary;

    }

    public override Color GetButtonColour()
    {
        return new Color32(235, 191, 217, 255);
    }
}
