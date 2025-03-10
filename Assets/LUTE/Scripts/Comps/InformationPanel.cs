using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Simple class that defines the parameters for an information panel.
    /// The panel itself can be used to display various information to the player (often used in conjunction with the Inventory system).
    /// </summary>
    public class InformationPanel : MonoBehaviour
    {
        protected bool isPanelActive = false;

        protected static List<InformationPanel> ActiveInformationPanels = new List<InformationPanel>();

        public static InformationPanel ActiveInformationPanel { get; set; }

        protected virtual void Awake()
        {
            if (!ActiveInformationPanels.Contains(this))
            {
                ActiveInformationPanels.Add(this);
            }
        }

        protected virtual void OnDestroy()
        {
            ActiveInformationPanels.Remove(this);
        }

        /// <summary>
        /// Holder method for setting the information to be displayed on the panel.
        /// This requires the inheriting class to implement the method.
        /// </summary>
        protected virtual void SetInformation() { }

        /// <summary>
        /// Toggles the panel on and off based on the active state of the object.
        /// More complex panels may require additional logic to be added to this method.
        /// </summary>
        public virtual void TogglePanel()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            isPanelActive = !isPanelActive;
        }

        /// <summary>
        /// Attempts to get the requested panel based on a specific name.
        /// If no panel in the scene is found then one is created based on a prefab with the same name.
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        public static InformationPanel GetPanel(string panelName)
        {
            // If there is no active panel or the active panel is not the same as the requested panel
            if (ActiveInformationPanel == null || ActiveInformationPanel.name != panelName)
            {
                InformationPanel infoPanel = null;
                foreach (InformationPanel panel in ActiveInformationPanels)
                {
                    // If there active panels ensure that there is one with the requested name
                    if (panel.name == panelName)
                    {
                        infoPanel = panel;
                        break;
                    }
                }
                if (infoPanel != null)
                {
                    ActiveInformationPanel = infoPanel;
                }
                // If there is no active panel with the requested name then create one (as long as there is a prefab with the naem requested)
                if (ActiveInformationPanel == null || infoPanel == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/" + panelName);
                    if (prefab != null)
                    {
                        GameObject panel = Instantiate(prefab);
                        panel.name = panelName;
                        ActiveInformationPanel = panel.GetComponent<InformationPanel>();
                    }
                }
            }
            return ActiveInformationPanel;
        }
    }
}
