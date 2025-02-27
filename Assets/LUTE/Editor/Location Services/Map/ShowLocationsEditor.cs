using System.Linq;
using UnityEditor;

//[CustomEditor(typeof(ShowLocationMarkers))]
public class ShowLocationsEditor : OrderEditor
{
    protected SerializedProperty locationProps;

    protected int locationVarIndex = 0;

    public override void OnEnable()
    {
        base.OnEnable();
        locationProps = serializedObject.FindProperty("locations");
    }

    public override void OnInspectorGUI()
    {
        DrawOrderGUI();
    }

    public override void DrawOrderGUI()
    {
        serializedObject.Update();

        ShowLocationMarkers t = target as ShowLocationMarkers;
        var engine = (BasicFlowEngine)t.GetEngine();

        locationProps.arraySize = EditorGUILayout.IntField("Size", locationProps.arraySize);

        var locationVars = engine.GetComponents<LocationVariable>();

        for (int i = 0; i < locationProps.arraySize; i++)
        {
            for (int j = 0; j < locationVars.Length; j++)
            {
                if (locationVars[j] == locationProps.GetArrayElementAtIndex(i).objectReferenceValue as LocationVariable)
                {
                    locationVarIndex = j;
                }
            }

            locationVarIndex = EditorGUILayout.Popup("Location", locationVarIndex, locationVars.Select(x => x.Key).ToArray());
            if (locationVars.Length > 0)
                locationProps.GetArrayElementAtIndex(i).objectReferenceValue = locationVars[locationVarIndex];
        }

        serializedObject.ApplyModifiedProperties();
    }
}
