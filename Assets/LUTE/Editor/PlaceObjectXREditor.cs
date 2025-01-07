using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

[CustomEditor(typeof(PlaceObjectXR))]
public class PlaceObjectXREditor : OrderEditor
{

    protected SerializedProperty prefabToPlace;
    protected SerializedProperty objectName;
    protected SerializedProperty raycastHitProp;
    protected SerializedProperty autoPlace;
    protected SerializedProperty rotateable;
    protected SerializedProperty scaleable;
    protected SerializedProperty moveable;

    protected SerializedProperty planeAlignment;


    protected int planeAlignmentIndex = 0;

    public override void OnEnable()
    {
        base.OnEnable();

        prefabToPlace = serializedObject.FindProperty("m_PrefabToPlace");
        objectName = serializedObject.FindProperty("m_ObjectName");
        raycastHitProp = serializedObject.FindProperty("raycastHitEvent");
        autoPlace = serializedObject.FindProperty("automaticallyPlaceObject");

        rotateable = serializedObject.FindProperty("rotateable");
        scaleable = serializedObject.FindProperty("scaleable");
        moveable = serializedObject.FindProperty("moveable");

        planeAlignment = serializedObject.FindProperty("planeAlignment");

       

    }

    public override void OnInspectorGUI()
    {
        DrawOrderGUI();
    }

    public override void DrawOrderGUI()
    {
        PlaceObjectXR t = target as PlaceObjectXR;
        var engine = (BasicFlowEngine)t.GetEngine();


        //var locationVars = engine.GetComponents<LocationVariable>();
        //for (int i = 0; i < locationVars.Length; i++)
        //{
        //    if (locationVars[i] == objectLocProp.objectReferenceValue as LocationVariable)
        //    {
        //        locationVarIndex = i;
        //    }
        //}

        //locationVarIndex = EditorGUILayout.Popup("Location", locationVarIndex, locationVars.Select(x => x.Key).ToArray());
        //if (locationVars.Length > 0)
        //    objectLocProp.objectReferenceValue = locationVars[locationVarIndex];


        //EditorGUILayout.PropertyField(objectProp);

        ////name property
        EditorGUILayout.PropertyField(prefabToPlace);
        EditorGUILayout.PropertyField(objectName);
        EditorGUILayout.PropertyField(raycastHitProp);
        EditorGUILayout.PropertyField(planeAlignment);
        EditorGUILayout.PropertyField(autoPlace);

        //if (!autoPlace.boolValue)

        //draw separating line
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

        {
            EditorGUILayout.PropertyField(rotateable);
            EditorGUILayout.PropertyField(scaleable);
            EditorGUILayout.PropertyField(moveable);

           

        }

        serializedObject.ApplyModifiedProperties();
    }
}
