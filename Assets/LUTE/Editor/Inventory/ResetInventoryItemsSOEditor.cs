using BogGames.Tools.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(ResetInventoryItemsSO))]
    public class ResetInventoryItemsSOEditor : Editor
    {
        protected SerializedProperty inventoryProp;

        private List<BogInventoryItem> scriptableItems = new List<BogInventoryItem>();

        protected void OnEnable()
        {
            inventoryProp = serializedObject.FindProperty("inventory");
            scriptableItems = OrderEditor.GetAllInstances<BogInventoryItem>().ToList();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            var inventory = inventoryProp.objectReferenceValue as BogInventoryBase;
            if (inventory != null)
            {
                inventory.ResetInventory();
            }

            if (GUILayout.Button("Reset Inventory Scriptable Objects"))
            {
                foreach (var item in scriptableItems)
                {
                    // If an item is locked then unlock it
                    // If an item is unlocked then lock it
                    // Essentially resetting the item back to whatever it was before runtime began
                    item.IsLocked = !item.IsLocked;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif