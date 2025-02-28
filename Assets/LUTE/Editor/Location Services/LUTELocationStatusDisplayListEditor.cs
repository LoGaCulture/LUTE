using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(LUTELocationStatusDisplayList))]
    public class LUTELocationStatusDisplayListEditor : Editor
    {
        private SerializedProperty listProperty;

        private void OnEnable()
        {
            listProperty = serializedObject.FindProperty("list");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Location Status Display List", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
                SerializedProperty statusProperty = elementProperty.FindPropertyRelative("status");
                SerializedProperty displayOptionsProperty = elementProperty.FindPropertyRelative("locationDisplayOptions");
                SerializedProperty customLabelProperty = elementProperty.FindPropertyRelative("CustomStatusLabel");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                // Get the enum name to use as the label
                string statusName = statusProperty.enumDisplayNames[statusProperty.enumValueIndex];
                EditorGUILayout.LabelField(statusName, EditorStyles.boldLabel);

                // Remove button
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    listProperty.DeleteArrayElementAtIndex(i);
                    break; // Break to avoid index out of range after deletion
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(statusProperty);
                EditorGUILayout.PropertyField(displayOptionsProperty);

                // Only show CustomStatusLabel field if status is "Custom"
                if (statusProperty.enumValueIndex == (int)LocationStatus.Custom)
                {
                    EditorGUILayout.PropertyField(customLabelProperty);
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            // Add button
            if (GUILayout.Button("Add New Status Display"))
            {
                int index = listProperty.arraySize;
                listProperty.arraySize++;
                SerializedProperty newElement = listProperty.GetArrayElementAtIndex(index);

                // Clear values of the new element
                newElement.FindPropertyRelative("status").enumValueIndex = 0;
                newElement.FindPropertyRelative("locationDisplayOptions").objectReferenceValue = null;
                newElement.FindPropertyRelative("CustomStatusLabel").stringValue = "";
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}