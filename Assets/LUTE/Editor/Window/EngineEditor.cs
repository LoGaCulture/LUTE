using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BasicFlowEngine))]
public class EngineEditor : Editor
{
    protected SerializedProperty descriptionProp;
    protected SerializedProperty hideComponentsProp;
    protected SerializedProperty variablesProp;
    protected SerializedProperty locationsProp;
    protected VariableListAdaptor variableListAdaptor;
    protected SerializedProperty groupsProp;
    protected SerializedProperty demoModeProp;
    protected SerializedProperty showLineNumbersProp;
    protected SerializedProperty colorOrdersProp;
    protected SerializedProperty showHandlerInfoProp;
    protected SerializedProperty annotationBoolProp;
    protected SerializedProperty labelTintProp;
    protected SerializedProperty optionSettingsProp;
    protected SerializedProperty defaultLocationDisplayListProp;

    protected virtual void OnEnable()
    {
        if (NullTargetCheck()) // Check for an orphaned editor instance
            return;

        descriptionProp = serializedObject.FindProperty("description");
        hideComponentsProp = serializedObject.FindProperty("hideComponents");
        variablesProp = serializedObject.FindProperty("variables");
        locationsProp = serializedObject.FindProperty("mapLocations");
        groupsProp = serializedObject.FindProperty("groups");
        demoModeProp = serializedObject.FindProperty("demoMapMode");
        showLineNumbersProp = serializedObject.FindProperty("showLineNumbers");
        colorOrdersProp = serializedObject.FindProperty("colourOrders");
        showHandlerInfoProp = serializedObject.FindProperty("showHandlerInfoOnGraph");
        annotationBoolProp = serializedObject.FindProperty("showAnnotations");
        labelTintProp = serializedObject.FindProperty("labelTint");
        optionSettingsProp = serializedObject.FindProperty("optionSettings");

        variableListAdaptor = new VariableListAdaptor(variablesProp, target as BasicFlowEngine);

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var engine = target as BasicFlowEngine;

        engine.UpdateHideFlags();

        EditorGUILayout.PropertyField(descriptionProp);
        EditorGUILayout.PropertyField(showLineNumbersProp);
        EditorGUILayout.PropertyField(colorOrdersProp);
        EditorGUILayout.PropertyField(annotationBoolProp);
        if (annotationBoolProp.boolValue)
        {
            EditorGUILayout.PropertyField(labelTintProp);
        }
        EditorGUILayout.PropertyField(hideComponentsProp);
        EditorGUILayout.PropertyField(showHandlerInfoProp);
        EditorGUILayout.PropertyField(demoModeProp);
        EditorGUILayout.PropertyField(optionSettingsProp);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("Open Flow Engine Window", "Opens the Engine Window")))
        {
            EditorWindow.GetWindow(typeof(GraphWindow), false, "Flow Engine");
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //If needed you can draw the location list here as well showing locations in the general variable list
        // EditorGUILayout.PropertyField(locationsProp);

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(20);

        DrawVariablesGUI(true, Mathf.FloorToInt(EditorGUIUtility.currentViewWidth) - VariableListAdaptor.ReorderListSkirts);
    }

    public virtual void DrawVariablesGUI(bool showVariableToggleButton, int w)
    {
        var engine = target as BasicFlowEngine;
        if (engine == null)
            return;

        serializedObject.Update();

        if (engine.Variables.Count == 0)
        {
            engine.VariablesExpanded = true;
        }

        if (showVariableToggleButton && !engine.VariablesExpanded)
        {
            if (GUILayout.Button("Variables (" + engine.Variables.Count + ")", GUILayout.Height(24)))
            {
                engine.VariablesExpanded = true;
            }

            // Draw disclosure triangle
            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x += 5;
            lastRect.y += 5;
            EditorGUI.Foldout(lastRect, false, "");
        }
        else
        {
            // Remove any null variables from the list
            for (int i = engine.Variables.Count - 1; i >= 0; i--)
            {
                if (engine.Variables[i] == null)
                {
                    //If location variable then we should remove this from the map window also
                    engine.Variables.RemoveAt(i);
                }
            }

            variableListAdaptor.DrawVarList(w);
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// When modifying custom editor code you can occasionally end up with orphaned editor instances.
    /// When this happens, you'll get a null exception error every time the scene serializes / deserialized.
    /// Once this situation occurs, the only way to fix it is to restart the Unity editor.
    /// As a workaround, this function detects if this editor is an orphan and deletes it. 
    protected virtual bool NullTargetCheck()
    {
        try
        {
            // The serializedObject accessor creates a new SerializedObject if needed.
            // However, this will fail with a null exception if the target object no longer exists.
#pragma warning disable 0219
            SerializedObject so = serializedObject;
        }
        catch (System.NullReferenceException)
        {
            DestroyImmediate(this);
            return true;
        }

        return false;
    }
}
