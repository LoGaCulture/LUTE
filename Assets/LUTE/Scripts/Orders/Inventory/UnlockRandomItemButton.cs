using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory",
        "Unlock Random Item Button",
        "Creates a button which will unlock a random item from the inventory")]
    public class UnlockRandomItemButton : GenericButton
    {
        [Tooltip("The inventory to choose an item from")]
        [SerializeField] protected BogInventoryBase inventory;

        public override void OnEnter()
        {
            if (inventory == null)
            {
                Debug.LogError("No inventory set in the UnlockRandomItemButton order");
                Continue();
                return;
            }

            var popupIcon = SetupButton();

            UnityEngine.Events.UnityAction action = () =>
            {
                inventory.UnlockRandomItem();
            };

            SetAction(popupIcon, action);

            Continue();
        }

        public override string GetSummary()
        {
            return inventory != null ? "    Creates a button which will unlock a random item from the inventory" : "    Error: No inventory set";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
