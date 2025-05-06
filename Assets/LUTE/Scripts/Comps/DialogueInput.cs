using UnityEngine;
using UnityEngine.EventSystems;

namespace LoGaCulture.LUTE
{
    public enum ClickMode
    {
        Disabled,
        ClickAnywhere,
        ClickOnDialogue,
        ClickOnButton
    }
    /// <summary>
    /// Input handler for Dialogue Boxes.
    /// </summary>
    public class DialogueInput : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("The mode of clicking that will advance the dialogue.")]
        [SerializeField] protected ClickMode clickMode;

        [Tooltip("Delat between clicks to advance the dialogue. Useful to prevent spamming.")]
        [SerializeField] protected float clickDelay = 0f;

        [Tooltip("Allow holding the cancel button to advance the dialogue.")]
        [SerializeField] protected bool cancelEnabled = false;

        [Tooltip("Ignore input if a menu dialogue (choice menu) is open.")]
        [SerializeField] protected bool ignoreMenuClicks = true;

        protected bool dialogueClickedFlag;
        protected bool nextLineInputFlag;
        protected float ignoreClickTimer;

        protected StandaloneInputModule currentInputModule;

        protected TextWriter writer;

        protected virtual void Awake()
        {
            writer = GetComponent<TextWriter>();

            CheckEventSystem();
        }

        // There must be an Event System in the scene for Say and Menu input to work.
        // This method will automatically instantiate one if none exists.
        protected virtual void CheckEventSystem()
        {
            EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                // Auto spawn an Event System from the prefab
                GameObject prefab = Resources.Load<GameObject>("Prefabs/EventSystem");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab) as GameObject;
                    go.name = "EventSystem";
                }
            }
        }

        protected virtual void Update()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            if (currentInputModule == null)
            {
                currentInputModule = EventSystem.current.GetComponent<StandaloneInputModule>();
            }

            if (writer != null)
            {
                if (Input.GetButtonDown(currentInputModule.submitButton) || cancelEnabled && Input.GetButton(currentInputModule.cancelButton))
                {
                    SetNextLineFlag();
                }
            }

            switch (clickMode)
            {
                case ClickMode.Disabled:
                    break;
                case ClickMode.ClickAnywhere:
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyUp(KeyCode.Space)) // Will work on android not sure on iOS
                    {
                        SetClickAnywhereClickedFlag();
                    }
                    break;
                case ClickMode.ClickOnDialogue:
                    if (dialogueClickedFlag)
                    {
                        SetNextLineFlag();
                        dialogueClickedFlag = false;
                    }
                    break;
                case ClickMode.ClickOnButton:
                    // input handled on the button itself but implement accordingly
                    break;
            }

            if (ignoreClickTimer > 0f)
            {
                ignoreClickTimer = Mathf.Max(ignoreClickTimer - Time.deltaTime, 0f);
            }

            if (ignoreMenuClicks)
            {
                // To implement when menu dialogue updated
            }

            // Tell any listeners to move to the next line
            if (nextLineInputFlag)
            {
                var inputListeners = gameObject.GetComponentsInChildren<IDialoguenputListener>();
                for (int i = 0; i < inputListeners.Length; i++)
                {
                    var inputListener = inputListeners[i];
                    inputListener.OnNextLineEvent();
                }
                nextLineInputFlag = false;
            }
        }
        public virtual void SetNextLineFlag()
        {
            if (writer.IsWaitingForInput || writer.IsTyping)
            {
                nextLineInputFlag = true;
            }
        }

        public virtual void SetClickAnywhereClickedFlag()
        {
            if (ignoreClickTimer > 0f)
            {
                return;
            }
            ignoreClickTimer = clickDelay;

            if (clickMode == ClickMode.ClickAnywhere)
            {
                SetNextLineFlag();
            }
        }

        public virtual void SetDialogueClickedFlag()
        {
            if (ignoreClickTimer > 0f)
            {
                return;
            }
            ignoreClickTimer = clickDelay;

            // Only applies in Click On Dialog mode
            if (clickMode == ClickMode.ClickOnDialogue)
            {
                dialogueClickedFlag = true;
            }
        }

        public virtual void SetButtonClickedFlag()
        {
            if (clickMode == ClickMode.ClickOnButton)
            {
                SetNextLineFlag();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetDialogueClickedFlag();
        }
    }
}
