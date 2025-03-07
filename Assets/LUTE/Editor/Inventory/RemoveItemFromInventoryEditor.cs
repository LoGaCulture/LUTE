using BogGames.Tools.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(RemoveItemFromInventory))]

    public class RemoveItemFromInventoryEditor : OrderEditor
    {
        protected SerializedProperty itemProp;

        private List<BogInventoryItem> scriptableItems = new List<BogInventoryItem>();
        private int itemIndex = 0;

        public override void OnEnable()
        {
            base.OnEnable();
            itemProp = serializedObject.FindProperty("item");

            scriptableItems = OrderEditor.GetAllInstances<BogInventoryItem>().ToList();
        }

        public override void OnInspectorGUI()
        {
            DrawOrderGUI();
        }

        public override void DrawOrderGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();

            if (scriptableItems == null || scriptableItems.Count <= 0)
            {
                EditorGUILayout.HelpBox("No BogInventoryItem scriptable objects found in the project. Please create one.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < scriptableItems.Count; i++)
            {
                if (scriptableItems[i] == itemProp.objectReferenceValue as BogInventoryItem)
                {
                    itemIndex = i;
                }
            }
            itemIndex = EditorGUILayout.Popup("Item to Remove", itemIndex, scriptableItems.Select(x => x.name).ToArray());
            itemProp.objectReferenceValue = scriptableItems[itemIndex];

            serializedObject.ApplyModifiedProperties();
        }
    }
}
