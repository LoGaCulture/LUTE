using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Editor class for the custom item mono reference class.
    /// Used to draw the character selection drop downs as intended.
    /// </summary>
    [CustomEditor(typeof(CustomLUTEInventoryItemMonoRef))]
    public class CustomLUTEInventoryItemMonoRefEditor : Editor
    {
        protected SerializedProperty characterProp;
        protected SerializedProperty portraitProp;

        public void OnEnable()
        {
            characterProp = serializedObject.FindProperty("character");
            portraitProp = serializedObject.FindProperty("characterPortrait");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            CustomLUTEInventoryItemMonoRef t = target as CustomLUTEInventoryItemMonoRef;

            bool showPortraits = false;
            OrderEditor.ObjectField<Character>(characterProp,
                                                new GUIContent("Character", "Character that is used on the information panel."),
                                                new GUIContent("<None>"),
                                                Character.ActiveCharacters);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            characterProp.objectReferenceValue = (Character)EditorGUILayout.ObjectField(characterProp.objectReferenceValue, typeof(Character), true);
            EditorGUILayout.EndHorizontal();

            // Only show portrait selection if...
            if (t.Character != null &&              // Character is selected
                t.Character.Portraits != null &&    // Character has a portraits field
                t.Character.Portraits.Count > 0)   // Selected Character has at least 1 portrait
            {
                showPortraits = true;
            }

            if (showPortraits)
            {
                OrderEditor.ObjectField<Sprite>(portraitProp,
                                                  new GUIContent("Portrait", "Portrait that represents the character."),
                                                  new GUIContent("<None>"),
                                                  t.Character.Portraits);
            }
            else
            {
                t.Portrait = null;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}