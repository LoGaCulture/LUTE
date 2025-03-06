using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory",
        "Inventory Button",
        "Creates a button which will toggle the inventory on/off (rather than using a nested button in popups)")]
    public class InventoryButton : GenericButton
    {
        [Tooltip("The inventory to toggle")]
        [SerializeField] protected BogInventoryBase inventory;

        public override void OnEnter()
        {
            if (inventory == null)
            {
                Debug.LogError("No inventory set in the InventoryButton order");
                Continue();
                return;
            }

            var popupIcon = SetupButton();

            UnityEngine.Events.UnityAction action = () =>
            {
                inventory.DrawInventory();
                inventory.BogInventoryCanvas.FadeInventoryCanvas();
            };

            SetAction(popupIcon, action);

            Continue();
        }

        public override string GetSummary()
        {
            return "    Creates a button which will toggle the inventory on/off (rather than using a nested button in popups)";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
