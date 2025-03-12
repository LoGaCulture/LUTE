using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Inherited class that will use an inventory item to reference a scene object for a specific action.
    /// In this case, draw various panels based on the item state and reference specific character or objects/methods in the scene.
    /// </summary>
    public class CustomLUTEInventoryItemMonoRef : InventoryItemLUTEMonoRef
    {
        [Tooltip("Character that is used on the information panel.")]
        [HideInInspector]
        [SerializeField] protected Character character;
        [Tooltip("Portrait that represents the character.")]
        [HideInInspector]
        [SerializeField] protected Sprite characterPortrait;

        public Character Character { get { return character; } }
        public Sprite Portrait { get { return characterPortrait; } set { characterPortrait = value; } }

        protected override void OnItemUse(BogInventoryItem item)
        {
            if (item != null)
            {
                if (item.ItemID != inventoryItem.ItemID)
                {
                    return;
                }

                if (item.IsLocked)
                {
                    // If the item is locked but is allowed to be used then we should show a hint to how to unlock this item.
                    HintPanel hintPanel = HintPanel.GetPanel("HintPanel") as HintPanel;
                    if (hintPanel == null)
                    {
                        return;
                    }

                    // As long as the item is a character item then we can show the hint.
                    // Provided the hint text is not empty.
                    var charItem = item as CharacterLUTEInventoryItem;
                    if (charItem == null)
                    {
                        return;
                    }

                    string hintText = "This item is locked. You need to find a key to unlock it.";

                    if (!string.IsNullOrEmpty(charItem.Hint))
                    {
                        hintText = charItem.Hint;
                    }

                    hintPanel.SetInformation(hintText);
                    hintPanel.TogglePanel();
                }
                else
                {
                    // If the item is unlocked then we show the character panel (the unlocked state).
                    CharacterPanel characterPanel = CharacterPanel.GetPanel("CharacterPanel") as CharacterPanel;
                    if (characterPanel == null)
                    {
                        return;
                    }

                    var charItem = item as CharacterLUTEInventoryItem;
                    if (charItem == null)
                    {
                        return;
                    }

                    string description = "This is a character item that can be used to interact with the scene.";
                    if (!string.IsNullOrEmpty(charItem.CharacterDescription))
                    {
                        description = charItem.CharacterDescription;
                    }

                    characterPanel.SetInformation(characterPortrait, character.CharacterName, description, unityAction);
                    characterPanel.TogglePanel();
                }
            }
        }
    }
}
