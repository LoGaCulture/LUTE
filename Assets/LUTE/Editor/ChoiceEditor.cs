using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Choice))]
public class ChoiceEditor : OrderEditor
{
    protected SerializedProperty textProp;
    protected SerializedProperty descriptionProp;
    protected SerializedProperty targetNodeProp;
    protected SerializedProperty hideIfVisitedProp;
    protected SerializedProperty interactableProp;
    protected SerializedProperty setMenuDialogProp;
    protected SerializedProperty hideThisOptionProp;
    protected SerializedProperty closeMenuOnSelectProp;
    protected SerializedProperty justContinueProp;
    protected SerializedProperty showNextChoiceProp;
    protected SerializedProperty buttonSoundProp;

    public override void OnEnable()
    {
        base.OnEnable();

        textProp = serializedObject.FindProperty("text");
        descriptionProp = serializedObject.FindProperty("description");
        targetNodeProp = serializedObject.FindProperty("targetNode");
        hideIfVisitedProp = serializedObject.FindProperty("hideIfVisited");
        interactableProp = serializedObject.FindProperty("interactable");
        setMenuDialogProp = serializedObject.FindProperty("setMenuDialogue");
        hideThisOptionProp = serializedObject.FindProperty("hideThisOption");
        closeMenuOnSelectProp = serializedObject.FindProperty("closeMenuOnSelect");
        justContinueProp = serializedObject.FindProperty("justContinue");
        showNextChoiceProp = serializedObject.FindProperty("showNextChoice");
        buttonSoundProp = serializedObject.FindProperty("buttonSound");
    }

    public override void DrawOrderGUI()
    {
        Choice t = target as Choice;
        var engine = (BasicFlowEngine)t.GetEngine();
        if (engine == null)
        {
            return;
        }

        serializedObject.Update();

        EditorGUILayout.PropertyField(textProp);

        EditorGUILayout.PropertyField(descriptionProp);

        EditorGUILayout.BeginHorizontal();
        NodeEditor.NodeField(targetNodeProp,
                               new GUIContent("Target Node", "Node to call when option is selected"),
                               new GUIContent("<None>"),
                               engine);
        const int popupWidth = 17;
        if (targetNodeProp.objectReferenceValue == null && GUILayout.Button("+", GUILayout.MaxWidth(popupWidth)))
        {
            engine.SelectedNode = t.ParentNode;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(hideIfVisitedProp);
        EditorGUILayout.PropertyField(interactableProp);
        EditorGUILayout.PropertyField(setMenuDialogProp);
        EditorGUILayout.PropertyField(hideThisOptionProp);
        EditorGUILayout.PropertyField(closeMenuOnSelectProp);
        EditorGUILayout.PropertyField(buttonSoundProp);
        EditorGUILayout.PropertyField(justContinueProp);
        EditorGUILayout.PropertyField(showNextChoiceProp);

        serializedObject.ApplyModifiedProperties();
    }
}
