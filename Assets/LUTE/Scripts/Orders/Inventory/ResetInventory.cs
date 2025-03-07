using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory",
        "Reset Inventory",
        "Reset the inventory so it is entirely empty.")]
    public class ResetInventory : Order
    {
        [Tooltip("The inventory to reset")]
        [SerializeField] protected BogInventoryBase inventory;

        public override void OnEnter()
        {
            if (inventory == null)
            {
                Continue();
                return;
            }

            inventory.ResetInventory();
            Continue();
        }

        public override string GetSummary()
        {
            return inventory != null ? "     Resetting inventory " + "@ " + inventory.InventoryID : "Error: No inventory set";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
