using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(GenericNodeButton))]
    public class GenericNodeButtonEditor : OrderEditor
    {
        protected SerializedProperty nodeProp;

        public override void OnEnable()
        {
            nodeProp = serializedObject.FindProperty("node");
        }

        public override void DrawOrderGUI()
        {
            DrawDefaultInspector();

            GenericNodeButton t = target as GenericNodeButton;
            var engine = (BasicFlowEngine)t.GetEngine();
            if (engine == null)
            {
                return;
            }

            serializedObject.Update();

            NodeEditor.NodeField(nodeProp,
                                 new GUIContent("Node to Execute", "Node to execute once this button has been pressed."),
                                 new GUIContent("<None>"),
                                 engine);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
