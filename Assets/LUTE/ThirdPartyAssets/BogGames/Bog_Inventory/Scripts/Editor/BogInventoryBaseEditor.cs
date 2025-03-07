using UnityEditor;
using UnityEngine;

namespace BogGames.Tools.Inventory
{
    /// <summary>
    /// Editor class to allow a button to be added to the inspector to reset the inventory.
    /// </summary>
    [CustomEditor(typeof(BogInventoryBase))]
    public class BogInventoryBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BogInventoryBase inventory = (BogInventoryBase)target;

            if (GUILayout.Button("Reset Inventory"))
            {
                inventory.ResetInventory();
            }
        }
    }
}
