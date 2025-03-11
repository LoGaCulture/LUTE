//using BogGames.Tools.Inventory;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(Container))]
//public class ContainerCardEditor : OrderEditor
//{
//    protected SerializedProperty containerProp;
//    protected SerializedProperty requiresKeyProp;
//    protected SerializedProperty keyIDProp;
//    protected SerializedProperty keyActionProp;
//    protected SerializedProperty activableProp;
//    protected SerializedProperty delayProp;
//    protected SerializedProperty unlimitedUsesProp;
//    protected SerializedProperty activeFeedbackProp;
//    protected SerializedProperty inactiveFeedbackProp;
//    protected SerializedProperty closeOnUseProp;
//    protected SerializedProperty itemsProp;
//    protected SerializedProperty itemsQuantitiesProp;
//    protected SerializedProperty promptInfoProp;
//    protected SerializedProperty promptInfoOpenProp;
//    protected SerializedProperty promptFadeDurationProp;
//    protected SerializedProperty promptInfoColour;
//    protected SerializedProperty hideOnPlayerProp;
//    protected SerializedProperty locationVarProp;
//    protected SerializedProperty openAnimProp;
//    protected SerializedProperty closeAnimProp;
//    protected SerializedProperty showPromptProp;
//    protected int locationVarIndex = 0;
//    protected int itemIndex = 0;

//    public override void OnEnable()
//    {
//        base.OnEnable();
//        containerProp = serializedObject.FindProperty("setContainerCard");
//        requiresKeyProp = serializedObject.FindProperty("requiresKey");
//        keyIDProp = serializedObject.FindProperty("keyID");
//        keyActionProp = serializedObject.FindProperty("keyAction");
//        activableProp = serializedObject.FindProperty("activable");
//        delayProp = serializedObject.FindProperty("delayBetweenUses");
//        unlimitedUsesProp = serializedObject.FindProperty("unlimitedActivations");
//        activeFeedbackProp = serializedObject.FindProperty("activationSound");
//        inactiveFeedbackProp = serializedObject.FindProperty("deniedSound");
//        closeOnUseProp = serializedObject.FindProperty("closeOnUse");
//        itemsProp = serializedObject.FindProperty("itemsToPickup");
//        itemsQuantitiesProp = serializedObject.FindProperty("itemsQuantities");
//        showPromptProp = serializedObject.FindProperty("showPrompt");
//        promptInfoProp = serializedObject.FindProperty("promptInfoError");
//        promptInfoOpenProp = serializedObject.FindProperty("promptInfoOpened");
//        promptFadeDurationProp = serializedObject.FindProperty("promptFadeDuration");
//        promptInfoColour = serializedObject.FindProperty("promptColor");
//        hideOnPlayerProp = serializedObject.FindProperty("hideIfPlayerNotNearby");
//        locationVarProp = serializedObject.FindProperty("spawnLocation");
//        openAnimProp = serializedObject.FindProperty("openAnim");
//        closeAnimProp = serializedObject.FindProperty("closeAnim");
//    }
//    public override void OnInspectorGUI()
//    {
//        DrawOrderGUI();
//    }
//    public override void DrawOrderGUI()
//    {
//        Container t = target as Container;
//        var engine = (BasicFlowEngine)t.GetEngine();

//        EditorGUILayout.PropertyField(containerProp);
//        EditorGUILayout.PropertyField(requiresKeyProp);
//        EditorGUILayout.PropertyField(keyIDProp);
//        EditorGUILayout.PropertyField(keyActionProp);
//        EditorGUILayout.PropertyField(activableProp);
//        EditorGUILayout.PropertyField(delayProp);
//        EditorGUILayout.PropertyField(unlimitedUsesProp);
//        EditorGUILayout.PropertyField(closeOnUseProp);
//        EditorGUILayout.PropertyField(activeFeedbackProp);
//        EditorGUILayout.PropertyField(inactiveFeedbackProp);
//        EditorGUILayout.PropertyField(showPromptProp);
//        if (showPromptProp.boolValue == true)
//        {
//            EditorGUILayout.PropertyField(promptInfoProp);
//            EditorGUILayout.PropertyField(promptInfoOpenProp);
//        }
//        EditorGUILayout.PropertyField(promptFadeDurationProp);
//        EditorGUILayout.PropertyField(promptInfoColour);
//        EditorGUILayout.PropertyField(openAnimProp);
//        EditorGUILayout.PropertyField(closeAnimProp);
//        EditorGUILayout.PropertyField(hideOnPlayerProp);

//        if (hideOnPlayerProp.boolValue == true)
//        {
//            var locationVars = engine.GetComponents<LocationVariable>();
//            for (int i = 0; i < locationVars.Length; i++)
//            {
//                if (locationVars[i] == locationVarProp.objectReferenceValue as LocationVariable)
//                {
//                    locationVarIndex = i;
//                }
//            }
//            locationVarIndex = EditorGUILayout.Popup("Location", locationVarIndex, locationVars.Select(x => x.Key).ToArray());
//            if (locationVars.Length > 0)
//                locationVarProp.objectReferenceValue = locationVars[locationVarIndex];
//        }

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Items to Add", EditorStyles.boldLabel);
//        itemsProp.arraySize = EditorGUILayout.IntField(itemsProp.arraySize);
//        EditorGUILayout.EndHorizontal();
//        itemsQuantitiesProp.arraySize = itemsProp.arraySize;

//        for (int i = 0; i < itemsProp.arraySize; i++)
//        {
//            EditorGUILayout.BeginVertical(GUI.skin.box); // Add a GUI box for readability
//            var items = GetAllInstances<BogInventoryItem>();
//            for (int j = 0; j < items.Length; j++)
//            {
//                if (items[j] == itemsProp.GetArrayElementAtIndex(i).objectReferenceValue as BogInventoryItem)
//                {
//                    itemIndex = j;
//                }
//            }

//            //Create a drop down list based on the items in the project
//            itemIndex = EditorGUILayout.Popup("Item to Add", itemIndex, items.Select(x => x.name).ToArray());
//            itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = items[itemIndex];

//            itemsQuantitiesProp.GetArrayElementAtIndex(i).intValue = EditorGUILayout.IntField("Total to Add", itemsQuantitiesProp.GetArrayElementAtIndex(i).intValue);
//            EditorGUILayout.EndVertical(); // End of GUI box
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
