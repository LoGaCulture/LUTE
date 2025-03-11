//using UnityEditor;

//[CustomEditor(typeof(LocationPickups))]
//public class ItemContainerEditor : OrderEditor
//{
//    protected SerializedProperty feedbackProp;
//    protected SerializedProperty showPromptProp;
//    protected SerializedProperty showCardProp;
//    protected SerializedProperty itemLocProp;
//    protected SerializedProperty itemProp;
//    protected SerializedProperty itemQuantProp;

//    protected int locationVarIndex = 0;
//    protected int itemIndex = 0;
//    public override void OnEnable()
//    {
//        base.OnEnable();
//        feedbackProp = serializedObject.FindProperty("pickupSound");
//        showPromptProp = serializedObject.FindProperty("showPrompt");
//        showCardProp = serializedObject.FindProperty("showPickupCard");
//        itemLocProp = serializedObject.FindProperty("itemLocation");
//        itemProp = serializedObject.FindProperty("item");
//        itemQuantProp = serializedObject.FindProperty("itemsQuantitiy");
//    }

//    public override void OnInspectorGUI()
//    {
//        DrawOrderGUI();
//    }

//    public override void DrawOrderGUI()
//    {
//        LocationPickups t = target as LocationPickups;
//        var engine = (BasicFlowEngine)t.GetEngine();

//        EditorGUILayout.PropertyField(feedbackProp);
//        EditorGUILayout.PropertyField(showPromptProp);
//        EditorGUILayout.PropertyField(showCardProp);
//        EditorGUILayout.PropertyField(itemLocProp);
//        EditorGUILayout.PropertyField(itemProp);

//        EditorGUILayout.PropertyField(itemQuantProp);

//        serializedObject.ApplyModifiedProperties();
//    }
//}