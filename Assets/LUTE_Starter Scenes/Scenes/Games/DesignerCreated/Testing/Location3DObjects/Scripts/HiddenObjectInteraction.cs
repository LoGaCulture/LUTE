//using UnityEngine;
//using UnityEngine.EventSystems;

//namespace LoGaCulture.LUTE
//{
//    public class HiddenObjectInteraction : MonoBehaviour, IPointerClickHandler
//    {
//        [Tooltip("The node to be called when the button is pressed")]
//        [SerializeField] protected Node callNode;
//        [Tooltip("The info related to this hidden object")]
//        [SerializeField] protected ObjectInfo objectInfo; // Will fill out certain canvas objects
//        [SerializeField] protected bool createNewPanel; // Will create a new panel or simply reveal the description

//        public ObjectInfo ObjectInfo
//        {
//            get { return objectInfo; }
//            set { objectInfo = value; }
//        }

//        private bool isVisible = false;

//        // need to setup using nodes

//        protected virtual void Start()
//        {
//            isVisible = true;
//        }

//        public void OnPointerClick(PointerEventData eventData)
//        {
//            if (isVisible)
//            {
//                SetActive(false); // Can you fade this instead for smoother transition? Also ensure you fade the whole object
//                ObjectInfoPanel newPanel = ObjectInfoPanel.GetInfoPanel();
//                if (newPanel != null && objectInfo != null)
//                {
//                    if (!objectInfo.Unlocked)
//                    {
//                        newPanel.UnlockInfo();
//                        objectInfo.Unlocked = true;
//                        var saveManager = LogaManager.Instance.SaveManager;
//                        saveManager.AddSavePoint("ObjectInfo" + objectInfo.ObjectName, "A list of historical info to be stored " + System.DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy"), false);
//                    }
//                    if (createNewPanel)
//                    {
//                        // Show a brand new panel and turn it on
//                        newPanel.SetInfo(objectInfo);
//                        newPanel.ToggleMenu();
//                    }
//                    else
//                    {
//                        // Or simply reveal more details about the panel
//                        newPanel.RevealInfo();
//                    }
//                }
//            }
//            else
//            {
//                SetActive(true);
//            }
//        }

//        public virtual void SetActive(bool state) => gameObject.SetActive(state);
//    }
//}