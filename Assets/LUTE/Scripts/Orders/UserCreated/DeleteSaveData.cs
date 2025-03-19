using BogGames.Tools.Inventory;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Saving", "Delete Save Data", "Deletes the save data for the current user.")]
    public class DeleteSaveData : Order
    {
        [Tooltip("The save menu associated with this Order. This will be found automatically if not provided.")]
        [SerializeField] protected SaveMenu saveMenu;
        [Tooltip("The save key to delete. If not provided, the current save key will be used from the found save menu.")]
        [SerializeField] protected string saveKey;
        [Tooltip("Once the save has been deleted, should we reset the scene?")]
        [SerializeField] protected bool resetScene = true;
        [Tooltip("Whether to reset the locations back to their default states.")]
        [SerializeField] protected bool resetLocations = true;
        [Tooltip("The inventory to reset. If not provided, the game will require a reset to reset the items.")]
        [SerializeField] protected BogInventoryBase inventory;
        [Tooltip("Image to display while loading the scene.")]
        [SerializeField]
        protected Texture2D loadingImage;

        public override void OnEnter()
        {
            if (saveMenu == null)
            {
                saveMenu = FindFirstObjectByType<SaveMenu>();
                if (saveMenu == null || string.IsNullOrEmpty(saveKey))
                {
                    Debug.LogError("No SaveMenu found in the scene nor any save key provided.");
                    return;
                }
            }

            if (string.IsNullOrEmpty(saveKey))
            {
                saveKey = saveMenu.SaveKey;
            }

            SaveManager.DeleteSave(saveMenu.SaveKey);

            if (resetLocations)
            {
                var engine = GetEngine();
                engine.ResetLocationsToDefault();
            }

            if (inventory != null)
            {
                inventory.ResetInventory();
            }

            if (resetScene)
            {
                LevelSelector.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, loadingImage);
            }
            else
                Continue(); // If the scene is not reloaded then continue on to next order in the list, otherwise this is a redundant call.
        }

        public override string GetSummary()
        {
            return "Delete Save Data and optionally reset the scene.";
        }

        public override Color GetButtonColour()
        {
            return new Color32(235, 191, 217, 255);
        }
    }
}
