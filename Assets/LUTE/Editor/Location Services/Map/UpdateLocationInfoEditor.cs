using UnityEditor;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(UpdateLocationInfo))]

    public class UpdateLocationInfoEditor : OrderEditor
    {
        protected SerializedProperty locationProp;
        protected SerializedProperty statusProp;
        protected SerializedProperty labelProp;
        protected SerializedProperty forcePermanentChangeProp;

        private int locationVarIndex = 0;

        public override void OnEnable()
        {
            base.OnEnable();
            locationProp = serializedObject.FindProperty("location");
            statusProp = serializedObject.FindProperty("status");
            labelProp = serializedObject.FindProperty("customLabel");
            forcePermanentChangeProp = serializedObject.FindProperty("forcePermanentChange");
        }

        public override void OnInspectorGUI()
        {
            DrawOrderGUI();
        }

        public override void DrawOrderGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(locationProp);
            EditorGUILayout.PropertyField(statusProp);

            LocationStatus locationStatus = (LocationStatus)statusProp.enumValueIndex;
            if (locationStatus == LocationStatus.Custom)
            {
                EditorGUILayout.PropertyField(labelProp);
                EditorGUILayout.PropertyField(forcePermanentChangeProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}