using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Menu",
                  "Remove Popup Button",
                  "Removes a popup button from the screen")]
    public class RemovePopupButton : Order
    {
        public enum ButtonType
        {
            Map,
            Inventory,
            Character,
            Quests,
            Options,
            Save,
            Load,
            Exit,
            Menu
        }

        [Tooltip("The type of button to remove")]
        [SerializeField] protected ButtonType buttonType;

        public override void OnEnter()
        {
            var popupIcon = PopupIcon.GetPopupIcon();
            RemoveButtonChoice(popupIcon);

            Continue();
        }

        public virtual void RemoveButtonChoice(PopupIcon pi)
        {
            switch (buttonType)
            {
                case ButtonType.Map:
                    pi.RemoveButton("MapButton");
                    break;
                case ButtonType.Inventory:
                    pi.RemoveButton("InventoryButton");
                    break;
                    break;
                case ButtonType.Options:
                    pi.RemoveButton("OptionsButton");
                    break;
                case ButtonType.Save:
                    pi.RemoveButton("SaveButton");
                    break;
                case ButtonType.Load:
                    pi.RemoveButton("LoadButton");
                    break;
                case ButtonType.Exit:
                    pi.RemoveButton("ExitButton");
                    break;
                case ButtonType.Menu:
                    pi.RemoveButton("MenuButton");
                    break;
            }
        }
    }
}
