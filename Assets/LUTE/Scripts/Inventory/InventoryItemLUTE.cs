using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CreateAssetMenu(fileName = "New LUTE Inventory Item", menuName = "LUTE/Inventory/LUTE Inventory Item")]
    public class InventoryItemLUTE : BogInventoryItem
    {
        public override void Use()
        {
            base.Use();
        }

        public override void Drop()
        {
            base.Drop();
        }

        public override void UnlockItem()
        {
            base.UnlockItem();
        }

        public override void LockItem()
        {
            base.LockItem();
        }
    }
}