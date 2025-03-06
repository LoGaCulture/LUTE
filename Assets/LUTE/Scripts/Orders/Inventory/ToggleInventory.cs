using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory",
                  "Toggle Inventory",
                  "Toggles the inventory panel on or off depending on its current state")]
    public class ToggleInventory : Order
    {
        [Tooltip("The inventory to toggle")]
        [SerializeField] protected BogInventoryBase inventory;
        public override void OnEnter()
        {
            if (inventory != null)
            {
                inventory.DrawInventory();
                inventory.BogInventoryCanvas.FadeInventoryCanvas();
            }
        }

        public override string GetSummary()
        {
            return inventory != null ? "    Toggles the inventory panel on or off depending on its current state" : "Error: Inventory not supplied";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}
