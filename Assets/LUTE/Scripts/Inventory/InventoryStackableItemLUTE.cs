using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CreateAssetMenu(fileName = "New LUTE Stackable Inventory Item", menuName = "LUTE/Inventory/LUTE Stackable Inventory Item")]
    public class InventoryStackableItemLUTE : InventoryItemLUTE
    {
        public override void Use()
        {
            base.Use();
        }

        public override void Drop()
        {
            base.Drop();
        }
    }
}
