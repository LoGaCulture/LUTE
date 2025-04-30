using BogGames.Tools.Achievements;
using BogGames.Tools.Inventory;
using LoGaCulture.LUTE;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// This component encodes and decodes a list of game objects to be saved for each Save Point.
/// To extend the save system to handle other data types, just modify or subclass this component.
public class SaveData : MonoBehaviour
{
    protected const string EngineDataKey = "EngineData";
    protected const string LogKey = "LogData";
    protected const string AchievementDataKey = "AchievementData";
    protected const string InventoryDataKey = "InventoryData";

    [Tooltip("List of engine objects in which its variables will be encoded and saved - only integer is currently supported")]
    [SerializeField] protected List<BasicFlowEngine> engines = new List<BasicFlowEngine>();

    public virtual void Encode(List<SaveDataItem> saveDataItems, bool settingsOnly, SaveManager.SaveProfile newProfile)
    {
        switch (newProfile)
        {
            case SaveManager.SaveProfile.EngineData:
                SaveEngineData(saveDataItems, settingsOnly);
                break;
            case SaveManager.SaveProfile.BogAchievementData:
                SaveAchievementData(saveDataItems);
                break;
            case SaveManager.SaveProfile.BogInventoryData:
                SaveInventoryData(saveDataItems);
                break;
            case SaveManager.SaveProfile.SaveAll:
                SaveEngineData(saveDataItems, settingsOnly);
                SaveAchievementData(saveDataItems);
                SaveInventoryData(saveDataItems);
                break;
        }
    }

    // Saves the engine and log data to the save data items list
    private void SaveEngineData(List<SaveDataItem> saveDataItems, bool settingsOnly)
    {
        for (int i = 0; i < engines.Count; i++)
        {
            var engine = engines[i];
            var engineData = EngineData.Encode(engine, settingsOnly);

            var saveDataItem = SaveDataItem.Create(EngineDataKey, JsonUtility.ToJson(engineData));
            saveDataItems.Add(saveDataItem);

            var logData = SaveDataItem.Create(LogKey, LogaManager.Instance.SaveLog.GetJsonHistory());
            saveDataItems.Add(logData);
        }
    }

    private void SaveAchievementData(List<SaveDataItem> saveDataItems)
    {
        var achievementData = BogAchievementsData.Encode(LogaManager.Instance.BogAchievementsManager.CurrentAchievements);

        var saveDataItem = SaveDataItem.Create(AchievementDataKey, JsonUtility.ToJson(achievementData));
        saveDataItems.Add(saveDataItem);
    }

    private void SaveInventoryData(List<SaveDataItem> saveDataItems)
    {
        var inventoryItems = BogInventoryBase.items.Where(x => x != null).ToList();
        if (inventoryItems == null)
        {

            return;
        }

        var inventoryData = BogInventoryData.Encode(inventoryItems);
        var saveDataItem = SaveDataItem.Create(InventoryDataKey, JsonUtility.ToJson(inventoryData));
        saveDataItems.Add(saveDataItem);
    }

    public virtual void Decode(List<SaveDataItem> saveDataItems)
    {
        for (int i = 0; i < saveDataItems.Count; i++)
        {
            var saveDataItem = saveDataItems[i];

            if (saveDataItem == null)
            {
                continue;
            }

            if (saveDataItem.Type == EngineDataKey)
            {
                var engineData = JsonUtility.FromJson<EngineData>(saveDataItem.Data);
                if (engineData == null)
                {
                    Debug.LogError("Engine data is null so failed to decode engine data");
                    continue;
                }

                EngineData.Decode(engineData);
            }

            if (saveDataItem.Type == LogKey)
            {
                LogaManager.Instance.SaveLog.LoadLogData(saveDataItem.Data);
            }

            if (saveDataItem.Type == InventoryDataKey)
            {
                var inventoryData = JsonUtility.FromJson<BogInventoryData>(saveDataItem.Data);
                if (inventoryData == null)
                {
                    continue;
                }

                BogInventoryData.Decode(inventoryData, null);
            }

            if (saveDataItem.Type == AchievementDataKey)
            {
                var achievementData = JsonUtility.FromJson<BogAchievementsData>(saveDataItem.Data);
                if (achievementData == null)
                {
                    // Using a return statement here would break the loop which is fine as we have reached the end of the list
                    // However this is left open for future save data files
                    continue;
                }

                BogAchievementsData.Decode(achievementData, LogaManager.Instance.BogAchievementsManager);
            }
        }
    }
}
