using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Holder class to draw the reset inventory button in the inspector.
    /// This is a workaround to allow the button to be drawn in the inspector using editor scripts.
    /// </summary>
    public class ResetInventoryItemsSO : MonoBehaviour
    {
        [Tooltip("The inventory to reset.")]
        [SerializeField] protected BogGames.Tools.Inventory.BogInventoryBase inventory;
    }
}
