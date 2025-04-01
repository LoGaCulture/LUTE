using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupIcon : MonoBehaviour
{
    public static PopupIcon ActivePopupIcon { get; set; }

    private Popup popupWindow;
    private Button[] cachedButtons;
    private int nextOptionIndex;
    private readonly string BUTTON_NAME = "PopupIconButton";

    public virtual Button[] CachedButtons { get { return cachedButtons; } }

    private void Awake()
    {
        Button[] optionButtons = GetComponentsInChildren<Button>();
        cachedButtons = optionButtons;

        foreach (Button button in cachedButtons)
        {
            button.gameObject.SetActive(false);
        }

        if (cachedButtons.Length <= 0)
        {
            Debug.LogError("PopupIcon requires a Button component on a child object");
            return;
        }

        CheckEventSystem();
    }

    protected virtual void CheckEventSystem()
    {
        EventSystem eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            // Auto spawn an Event System from the prefab - ensure you have one in a Resources folder
            GameObject prefab = Resources.Load<GameObject>("Prefabs/EventSystem");
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab) as GameObject;
                go.name = "EventSystem";
            }
        }
    }

    public static PopupIcon GetPopupIcon()
    {
        if (ActivePopupIcon == null)
        {
            var pi = GameObject.FindFirstObjectByType<PopupIcon>();
            if (pi != null)
            {
                ActivePopupIcon = pi;
            }

            if (ActivePopupIcon == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/HamburgerMenuButton");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab) as GameObject;
                    go.SetActive(false);
                    go.name = "PopupIcon";
                    ActivePopupIcon = go.GetComponent<PopupIcon>();
                }
            }
        }
        return ActivePopupIcon;
    }

    public bool SetIcon(Sprite icon)
    {
        if (nextOptionIndex >= CachedButtons.Length)
        {
            Debug.LogWarning("Unable to add popup option, not enough buttons!");
            return false;
        }
        ActivePopupIcon.CachedButtons[nextOptionIndex].image.sprite = icon;
        return true;
    }

    public void SetPopupWindow(Popup popupWindow)
    {
        this.popupWindow = popupWindow;
    }

    private void OnClick()
    {
        if (popupWindow != null)
        {
            popupWindow.OpenClose();
        }
    }

    public bool SetAction(UnityAction onClick, string buttonName = "")
    {
        if (nextOptionIndex >= CachedButtons.Length)
        {
            Debug.LogWarning("Unable to add popup option, not enough buttons!");
            return false;
        }

        // Just in case a user passes in an empty string then reset to default
        if (string.IsNullOrEmpty(buttonName))
        {
            buttonName = BUTTON_NAME;
        }

        if (buttonName != BUTTON_NAME)
        {
            // Go through all cached buttons and check if a button has the same name
            // If it does then simply reset on click event, set it to the unityaction provided, switch it on and return true
            foreach (Button button in CachedButtons)
            {
                if (button.gameObject.name == buttonName)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => { onClick.Invoke(); });
                    button.gameObject.SetActive(true);
                    return true;
                }
            }
        }

        ActivePopupIcon.CachedButtons[nextOptionIndex].gameObject.name = buttonName;
        ActivePopupIcon.CachedButtons[nextOptionIndex].onClick.AddListener(() => { onClick.Invoke(); });
        ActivePopupIcon.CachedButtons[nextOptionIndex].gameObject.SetActive(true);
        return true;
    }

    public void MoveToNextOption()
    {
        nextOptionIndex++;
    }

    public virtual void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }

    public virtual void RemoveButton(string buttonName)
    {
        if (string.IsNullOrEmpty(buttonName))
        {
            return;
        }

        foreach (Button button in CachedButtons)
        {
            if (button.gameObject.name == buttonName)
            {
                button.name = BUTTON_NAME;
                button.onClick.RemoveAllListeners();
                button.gameObject.SetActive(false);

                // The next option will be the first button found which is disabled
                for (int i = 0; i < CachedButtons.Length; i++)
                {
                    if (!CachedButtons[i].gameObject.activeSelf)
                    {
                        nextOptionIndex = i;
                        break;
                    }
                }

                return;
            }
        }
    }
}
