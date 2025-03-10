using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Inventory", "Unlock Random Item", "Unlock a random item in the inventory.")]
    public class UnlockRandomItem : Order
    {
        [Tooltip("The inventory to choose an item from")]
        [SerializeField] protected BogInventoryBase inventory;

        public override void OnEnter()
        {
            if (inventory == null)
            {
                Continue();
                return;
            }

            inventory.UnlockRandomItem();
            Continue();
        }

        public override string GetSummary()
        {
            return inventory != null ? " Unlocking a random item" : " Error: No inventory set";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}