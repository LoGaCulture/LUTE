using LoGaCulture.LUTE;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveMenu : MonoBehaviour
{
    [Tooltip("String used to store save games in player prefs. Use unique strings for each game.")]
    [SerializeField] protected string saveKey = LogaConstants.DefaultSaveDataKey;
    [Tooltip("Auto load the most recent save when entering play")]
    [SerializeField] protected bool loadOnStart = false;
    [Tooltip("Auto save the game after each Save Point Order is executed")]
    [SerializeField] protected bool autoSave = true;
    [Tooltip("Show all options in the save menu - if false, disables to use of player saving")]
    [SerializeField] protected bool showAllOptions = true;
    [Tooltip("Delete save game data from disk when game is restarted - useful for debugging")]
    [SerializeField] protected bool deleteOnRestart = false;
    [SerializeField] protected CanvasGroup saveMenuGroup;
    [SerializeField] protected Button saveMenuButton;
    [SerializeField] protected Button saveButton;
    [SerializeField] protected Button loadButton;
    [SerializeField] protected Button rewindButton;
    [SerializeField] protected Button forwardButton;
    [SerializeField] protected Button restartButton;
    [SerializeField] protected ScrollRect debugView;
    [SerializeField] protected bool dontDestroyOnLoad = true;

    protected static bool saveMenuActive = false;
    protected AudioSource menuAudioSource;
    protected LTDescr fadeTween; //Used for fading menu
    protected static SaveMenu instance; //Used for singleton
    protected static bool hasLoadedOnStart; //Used to prevent multiple loads on start
    protected static string defaultSave;

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (transform.parent == null)
        {
            if (dontDestroyOnLoad)
                GameObject.DontDestroyOnLoad(this);
        }
        else
        {
            Debug.LogError("SaveMenu should be a root object in the scene hierarchy otherwise it cannot be preserved across scenes.");
        }

        menuAudioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        if (!saveMenuActive)
        {
            saveMenuGroup.alpha = 0f;
        }

        var saveManager = LogaManager.Instance.SaveManager;

        if (string.IsNullOrEmpty(saveManager.StartScene))
        {
            saveManager.StartScene = SceneManager.GetActiveScene().name;
        }

        if (loadOnStart && !hasLoadedOnStart)
        {
            hasLoadedOnStart = true;

            if (saveManager.HasSaveData(saveKey))
            {
                saveManager.Load(saveKey);
            }
        }
    }

    protected virtual void Update()
    {
        var saveManager = LogaManager.Instance.SaveManager;

        bool showSaveLoadButtons = showAllOptions;
        if (saveButton.IsActive() != showSaveLoadButtons)
        {
            saveButton.gameObject.SetActive(showSaveLoadButtons);
            loadButton.gameObject.SetActive(showSaveLoadButtons);
        }

        if (showSaveLoadButtons)
        {
            if (saveButton != null)
            {
                // Don't allow saving if the game is in a state where saving is not allowed 
                // Don't allow saving unless there is one save point in the history - avoids loading a save with 0 points
                saveButton.interactable = saveManager.TotalSavePoints > 0 && saveMenuActive;
            }
            if (loadButton != null)
            {
                loadButton.interactable = saveManager.HasSaveData(saveKey) && saveMenuActive;
            }

            if (restartButton != null)
            {
                restartButton.interactable = saveMenuActive;
            }
            if (rewindButton != null)
            {
                rewindButton.interactable = saveManager.TotalSavePoints > 0 && saveMenuActive;
            }
            if (forwardButton != null)
            {
                forwardButton.interactable = saveManager.TotalSavePoints > 0 && saveMenuActive;
            }

            if (debugView.enabled)
            {
                var debugText = debugView.GetComponentInChildren<TextMeshProUGUI>();
                if (debugText != null)
                {
                    debugText.text = saveManager.GetDebugInfo();
                }
            }
        }
    }

    protected void OnEnable()
    {
        SaveManagerSignals.OnSavePointAdded += OnSavePointAdded;
    }
    protected void OnDisable()
    {
        SaveManagerSignals.OnSavePointAdded -= OnSavePointAdded;
    }

    protected virtual void OnSavePointAdded(string savePointKey, string savePointDesc)
    {
        var saveManager = LogaManager.Instance.SaveManager;
        if (autoSave && saveManager.TotalSavePoints > 0)
        {
            saveManager.SaveGame(saveKey);
        }
    }

    protected void PlayClickSound()
    {
        if (menuAudioSource != null && menuAudioSource.clip != null)
        {
            menuAudioSource.Play();
        }
    }

    public virtual string SaveKey
    {
        get { return saveKey; }
    }

    public void ToggleSaveMenu()
    {
        if (fadeTween != null)
        {
            LeanTween.cancel(fadeTween.id, true);
            fadeTween = null;
        }

        if (saveMenuActive)
        {
            //Fade menu out
            LeanTween.value(saveMenuGroup.gameObject, saveMenuGroup.alpha, 0f, 0.4f)
    .setEase(LeanTweenType.easeOutQuint)
    .setOnUpdate((t) =>
    {
        saveMenuGroup.alpha = t;
    }).setOnComplete(() =>
    {
        saveMenuGroup.alpha = 0f;
    });
        }
        else
        {
            //Fade menu in
            LeanTween.value(saveMenuGroup.gameObject, saveMenuGroup.alpha, 1f, 0.4f)
    .setEase(LeanTweenType.easeOutQuint)
    .setOnUpdate((t) =>
    {
        saveMenuGroup.alpha = t;
    }).setOnComplete(() =>
    {
        saveMenuGroup.alpha = 1f;
    });
        }
        saveMenuActive = !saveMenuActive;
    }

    public virtual void Save()
    {
        //used for pressing save button
        PlayClickSound();

        var saveManager = LogaManager.Instance.SaveManager;

        if (saveManager.TotalSavePoints > 0)
        {
            saveManager.SaveGame(saveKey);
        }
    }

    public virtual void Load()
    {
        //used for pressing load button
        PlayClickSound();

        var saveManager = LogaManager.Instance.SaveManager;

        if (saveManager.HasSaveData(saveKey))
        {
            saveManager.Load(saveKey);
        }
    }

    public virtual void Rewind()
    {
        //used for pressing rewind button
        PlayClickSound();

        var saveManager = LogaManager.Instance.SaveManager;

        if (saveManager.TotalSavePoints > 0)
        {
            saveManager.RewindSavePoint();
        }
    }

    public virtual void FastForward()
    {
        //used for pressing forward button
        PlayClickSound();

        var saveManager = LogaManager.Instance.SaveManager;

        if (saveManager.TotalRewoundSavePoints > 0)
        {
            saveManager.FastForwardSavePoint();
        }
    }

    public virtual void Restart()
    {
        //used for pressing restart button
        PlayClickSound();

        var saveManager = LogaManager.Instance.SaveManager;
        if (string.IsNullOrEmpty(saveManager.StartScene))
        {
            Debug.LogError("Start scene is not set in SaveManager. Please set the start scene in the inspector.");
            return;
        }

        saveManager.ClearHistory();

        if (deleteOnRestart)
        {
            SaveManager.DeleteSave(saveKey);
        }
        SaveManagerSignals.DoSaveHistoryReset();
        SceneManager.LoadScene(saveManager.StartScene);
    }
}
