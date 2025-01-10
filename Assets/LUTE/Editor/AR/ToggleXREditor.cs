using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

[CustomEditor(typeof(ToggleXR))]
public class ToggleXREditor : OrderEditor
{
    private SerializedProperty _toggle;
    private SerializedProperty _planeVisualizer;
    private SerializedProperty _planeDetectionMode;
    private SerializedProperty _pointCloudVisualizer;

    public override void OnEnable()
    {
        base.OnEnable();

        // Find serialized properties
        _toggle = serializedObject.FindProperty("_toggle");
        _planeVisualizer = serializedObject.FindProperty("_planeVisualizer");
        _planeDetectionMode = serializedObject.FindProperty("_planeDetectionMode");
        _pointCloudVisualizer = serializedObject.FindProperty("_pointCloudVisualizer");

        // Update serialized object before accessing properties
        serializedObject.Update();

        // Set default plane visualizer if it's null
        if (_planeVisualizer.objectReferenceValue == null)
        {
            _planeVisualizer.objectReferenceValue = Resources.Load<GameObject>("Prefabs/AR Feathered Plane");
            if (_planeVisualizer.objectReferenceValue == null)
            {
                Debug.LogWarning("Default plane visualizer 'AR Feathered Plane' not found in Resources/Prefabs.");
            }
        }


       // Debug.Log("Plane detection mode: " + _planeDetectionMode.enumValueIndex);

        // Set default plane detection mode to Horizontal if not set
        if (_planeDetectionMode.enumValueIndex == -1 && _planeDetectionMode.hasMultipleDifferentValues)
        {
            _planeDetectionMode.enumValueIndex = (int)PlaneDetectionMode.Horizontal;
        }
        

        // Set default point cloud visualizer if it's null
        if (_pointCloudVisualizer.objectReferenceValue == null)
        {
            _pointCloudVisualizer.objectReferenceValue = Resources.Load<GameObject>("Prefabs/AR Point Cloud Debug Visualizer");
            if (_pointCloudVisualizer.objectReferenceValue == null)
            {
                Debug.LogWarning("Default point cloud visualizer 'AR Point Cloud Debug Visualizer' not found in Resources/Prefabs.");
            }
        }

        // Apply modified properties
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        DrawOrderGUI();
    }

    public override void DrawOrderGUI()
    {
        // Update serialized object before making changes
        serializedObject.Update();

        // Draw the toggle property
        EditorGUILayout.PropertyField(_toggle);

        // If toggle is true, display additional properties
        if (_toggle.boolValue)
        {
            EditorGUILayout.PropertyField(_planeVisualizer);
            EditorGUILayout.PropertyField(_planeDetectionMode);
            EditorGUILayout.PropertyField(_pointCloudVisualizer);
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}