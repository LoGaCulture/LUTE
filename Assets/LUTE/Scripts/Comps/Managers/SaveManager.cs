using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_5_3_OR_NEWER

/// <summary>
/// SaveManager is a class that manages the saving and loading of the game.
/// WebGL requires extra js so we use playerprefs
/// webplayer does not implement systemio so requires playerprefs
/// </summary>

public class SaveManager : MonoBehaviour
{
    public enum SaveProfile
    {
        SaveAll,
        EngineData,
        BogAchievementData,
        BogInventoryData,
        None
    }


    protected static SaveHistory saveHistory = new SaveHistory();

    public static string SAVE_DIRECTORY { get { return Application.persistentDataPath + "/LUTESaves/"; } }

    private static string GetFullFilePath(string saveDataKey)
    {
        return SAVE_DIRECTORY + saveDataKey + ".json";
    }

    protected virtual bool ReadSaveHistory(string saveDataKey)
    {
        var historyData = string.Empty;
#if UNITY_WEBPLAYER || UNITY_WEBGL
        historyData = PlayerPrefs.GetString(saveDataKey);
#else
        var filePath = GetFullFilePath(saveDataKey);
        if (System.IO.File.Exists(filePath))
        {
            historyData = System.IO.File.ReadAllText(filePath);
        }
#endif // UNITY_WEBPLAYER || UNITY_WEBGL
        if (!string.IsNullOrEmpty(historyData))
        {
            var tempSaveHistory = JsonUtility.FromJson<SaveHistory>(historyData);
            if (tempSaveHistory != null)
            {
                saveHistory = tempSaveHistory;
                return true;
            }
        }
        return false;
    }

    protected virtual bool WriteSaveHistory(string saveDataKey)
    {
        var historyData = JsonUtility.ToJson(saveHistory, true);
        if (!string.IsNullOrEmpty(historyData))
        {
#if UNITY_WEBPLAYER || UNITY_WEBGL
            PlayerPrefs.SetString(saveDataKey, historyData);
            PlayerPrefs.Save();
#else
            var filePath = GetFullFilePath(saveDataKey);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            fileInfo.Directory.Create();

            System.IO.File.WriteAllText(filePath, historyData);
#endif // UNITY_WEBPLAYER || UNITY_WEBGL
            return true;
        }
        return false;
    }

    /// <summary>
    /// Executes the node based on save point key in the following order:
    /// 1. Save point loaded using event handler with matching save point key.
    /// 2. First save point order found with matching key; will execute the next order in the list.
    /// 3. Any label in a node with matching key; will execute the next order in the list.
    /// </summary>
    /// <param name="savePointKey"></param>
    protected virtual void ExecuteNodes(string savePointKey)
    {
        // Fire any matching SavePointLoaded event handler with matching save key
        SavePointLoaded.NotifyEventHandlers(savePointKey);

        // Execute any node containing a save point order with matching save key, with resume on load set to true
        var savePoints = Object.FindObjectsByType<SavePoint>(FindObjectsSortMode.None);
        for (int i = 0; i < savePoints.Length; i++)
        {
            var savePoint = savePoints[i];
            if (savePoint.ResumeOnLoad &&
                string.Compare(savePoint.SavePointKey, savePointKey, true) == 0)
            {
                int index = savePoint.OrderIndex;
                var node = savePoint.ParentNode;
                var engine = savePoint.GetEngine();
                engine.ExecuteNode(node, index + 1);

                // Should only be one save point with the same key
                break;
            }
        }
    }

    /// Start executing the first save found with isStartingPoint set to true
    protected virtual void ExecuteStartNode()
    {
        var savePoints = Object.FindObjectsByType<SavePoint>(FindObjectsSortMode.None);
        for (int i = 0; i < savePoints.Length; i++)
        {
            var savePoint = savePoints[i];
            if (savePoint.IsStartingPoint)
            {
                var node = savePoint.ParentNode;
                var engine = savePoint.GetEngine();
                engine.ExecuteNode(node, savePoint.OrderIndex);
                break;
            }
        }
    }

    protected virtual void LoadSaveGame(string saveDataKey, bool customPoint, string customPointID)
    {
        if (ReadSaveHistory(saveDataKey))
        {
            saveHistory.ClearRewoundSavePoints();

            if (!customPoint)
                saveHistory.LoadLastSavePoint();
            else
                saveHistory.LoadSavePoint(customPointID);
        }
    }

    // Scene loading in Unity is asynchronous so we need to take care to avoid race conditions. 
    // The following callbacks tell us when a scene has been loaded and when 
    // a saved game has been loaded. We delay taking action until the next 
    // frame (via a delegate) so that we know for sure which case we're dealing with.

    protected System.Action loadAction;

    protected virtual void OnEnable()
    {
        SaveManagerSignals.OnSavePointLoaded += OnSavePointLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected virtual void OnDisable()
    {
        SaveManagerSignals.OnSavePointLoaded -= OnSavePointLoaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected virtual void OnSavePointLoaded(string savePointKey)
    {
        var key = savePointKey;
        loadAction = () => ExecuteNodes(key);
    }

    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
        {
            return;
        }

        // We first assume that this is a 'normal' scene load rather than a saved game being loaded.
        // If we subsequently receive a notification that a saved game was loaded then the load action 
        // set here will be overridden by the OnSavePointLoaded callback above.

        if (loadAction == null)
        {
            loadAction = ExecuteStartNode;
        }
    }

    protected virtual void Start()
    {
        // on scene load may not be called so we call it here when the manager starts up
        if (loadAction == null)
            loadAction = ExecuteStartNode;
    }

    protected virtual void Update()
    {
        if (loadAction != null)
        {
            loadAction();
            loadAction = null;
        }
    }

    public string StartScene { get; set; }

    public virtual int TotalSavePoints { get { return saveHistory.TotalSavePoints; } }

    public virtual int TotalRewoundSavePoints { get { return saveHistory.TotalRewoundSavePoints; } }

    public virtual void SaveGame(string saveDataKey)
    {
        WriteSaveHistory(saveDataKey);
    }

    public void Load(string saveDataKey, bool customPoint = false, string customPointID = "")
    {
        var key = saveDataKey;
        loadAction = () => LoadSaveGame(key, customPoint, customPointID);
    }

    public static void DeleteSave(string saveDataKey)
    {
#if UNITY_WEBPLAYER || UNITY_WEBGL
        PlayerPrefs.DeleteKey(saveDataKey);
        PlayPrefs.Save();
#else
        var fullPath = GetFullFilePath(saveDataKey);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);

        }
#endif // UNITY_WEBPLAYER || UNITY_WEBGL
    }
    public bool HasSaveData(string saveDataKey)
    {
#if UNITY_WEBPLAYER || UNITY_WEBGL
        return PlayerPrefs.HasKey(saveDataKey);
#else
        var fullPath = GetFullFilePath(saveDataKey);
        return System.IO.File.Exists(fullPath);
#endif // UNITY_WEBPLAYER || UNITY_WEBGL
    }
    public virtual void AddSavePoint(string savePointKey, string savePointDescription, bool settingsOnly, SaveProfile newProfile = SaveProfile.EngineData)
    {
        saveHistory.AddSavePoint(savePointKey, savePointDescription, settingsOnly, newProfile);
        SaveManagerSignals.DoSavePointAdded(savePointKey, savePointDescription);
    }

    public virtual void RewindSavePoint()
    {
        if (saveHistory.TotalSavePoints > 0)
        {
            //Cannot rewind as we are at the first save point
            if (saveHistory.TotalSavePoints > 1)
            {
                saveHistory.Rewind();
            }
            saveHistory.LoadLastSavePoint();
        }
    }

    public virtual void FastForwardSavePoint()
    {
        if (saveHistory.TotalRewoundSavePoints > 0)
        {
            saveHistory.FastForward();
            saveHistory.LoadLastSavePoint();
        }
    }

    public virtual void ClearHistory()
    {
        saveHistory.Clear();
    }

    public virtual string GetDebugInfo()
    {
        return saveHistory.GetDebugInfo();
    }
}
#endif // UNITY_5_3_OR_NEWER