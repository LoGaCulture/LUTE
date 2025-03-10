using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// An inventory item that revolves around characters in the scene.
    /// </summary>
    [CreateAssetMenu(fileName = "New LUTE Character Inventory Item", menuName = "LUTE/Inventory/LUTE Character Inventory Item")]
    /// 
    public class CharacterLUTEInventoryItem : InventoryItemLUTE
    {
        [Tooltip("The hint text to display when the item is locked.")]
        [TextArea(3, 10)]
        [SerializeField] protected string hint = "This item is locked. You need to find a key to unlock it.";
        [Tooltip("The description of the character panel (used when unlocked).")]
        [TextArea(3, 10)]
        [SerializeField] protected string characterDescription = "A character in the scene.";

        public string Hint => hint;
        public string CharacterDescription => characterDescription;
    }
}
