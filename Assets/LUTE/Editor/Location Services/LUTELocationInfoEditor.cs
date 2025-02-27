using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(LUTELocationInfo))]
    public class LUTELocationInfoEditor : Editor
    {
        SerializedProperty executeNodeProp;

        private void OnEnable()
        {
            executeNodeProp = serializedObject.FindProperty("executeNode");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draws all other fields

            serializedObject.Update(); // Sync with the actual object
            EditorGUI.BeginChangeCheck();

            NodeEditor.NodeField(executeNodeProp,
                     new GUIContent("Execute Node", "Node to execute when location is clicked."),
                     new GUIContent("<None>"),
                     GraphWindow.GetEngine());


            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}