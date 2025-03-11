using LoGaCulture.LUTE;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static BooleanVariable;
using static FloatVariable;

[CustomEditor(typeof(Variable), true)]
public class VariableEditor : OrderEditor
{
    public override void OnEnable()
    {
        base.OnEnable();

        Variable t = target as Variable;
        t.hideFlags = HideFlags.HideInInspector;
    }

    public static VariableInfoAttribute GetVariableInfo(System.Type variableType)
    {
        object[] attributes = variableType.GetCustomAttributes(typeof(VariableInfoAttribute), false);
        foreach (object obj in attributes)
        {
            VariableInfoAttribute variableInfoAttr = obj as VariableInfoAttribute;
            if (variableInfoAttr != null)
            {
                return variableInfoAttr;
            }
        }

        return null;
    }

    public static void VariableField(SerializedProperty property, GUIContent label, BasicFlowEngine engine, string defaultText,
                                    Func<Variable, bool> filter, Func<string, int, string[], int> drawer = null)
    {
        List<string> variableKeys = new List<string>();
        List<Variable> variableObjs = new List<Variable>();

        variableKeys.Add(defaultText);
        variableObjs.Add(null);

        List<Variable> variables = engine.Variables;
        int index = 0;
        int selectedIndex = 0;

        Variable selecedVariable = property.objectReferenceValue as Variable;

        // when there are multiple engines, switching between them can cause incorrect variable properties (specifically for private ones)
        // when this occurs we just skip displaying the property for this frame
        if (selecedVariable != null && selecedVariable.gameObject != engine.gameObject && selecedVariable.Scope == VariableScope.Private)
        {
            property.objectReferenceValue = null;
            return;
        }

        foreach (Variable v in variables)
        {
            if (filter != null)
            {
                if (!filter(v))
                {
                    continue;
                }
            }

            variableKeys.Add(v.Key);
            variableObjs.Add(v);

            index++;

            if (v == selecedVariable)
            {
                selectedIndex = index;
            }
        }

        List<BasicFlowEngine> esList = BasicFlowEngine.CachedEngines;
        foreach (BasicFlowEngine es in esList)
        {
            if (es == engine)
            {
                continue;
            }

            List<Variable> publicVars = es.GetPublicVariables();
            foreach (Variable v in publicVars)
            {
                if (filter != null)
                {
                    if (!filter(v))
                    {
                        continue;
                    }
                }

                variableKeys.Add(es.name + "/" + v.Key);
                variableObjs.Add(v);

                index++;

                if (v == selecedVariable)
                {
                    selectedIndex = index;
                }
            }
        }

        if (drawer == null)
        {
            selectedIndex = EditorGUILayout.Popup(label.text, selectedIndex, variableKeys.ToArray());
        }
        else
        {
            selectedIndex = drawer(label.text, selectedIndex, variableKeys.ToArray());
        }

        property.objectReferenceValue = variableObjs[selectedIndex];
    }
}

[CustomPropertyDrawer(typeof(VariablePropertyAttribute))]
public class VariableDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        VariablePropertyAttribute variableProperty = attribute as VariablePropertyAttribute;
        if (variableProperty == null)
        {
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Filter the variables by the types listed in the VariableProperty attribute
        Func<Variable, bool> compare = v =>
        {
            if (v == null)
            {
                return false;
            }

            if (variableProperty.VariableTypes.Length == 0)
            {
                var compatCheck = property.serializedObject.targetObject as ICollectionCompatible;
                if (compatCheck != null)
                {
                    return compatCheck.IsVarCompatibleWithCollection(v, variableProperty.compatibleVariableName);
                }
                else
                    return true;
            }

            return variableProperty.VariableTypes.Contains<System.Type>(v.GetType());
        };

        VariableEditor.VariableField(property, label, GraphWindow.GetEngine(), variableProperty.defaultText,
                                    compare, (s, t, u) => EditorGUI.Popup(position, s, t, u));

        EditorGUI.EndProperty();
    }
}

public class VariableDataDrawer<T> : PropertyDrawer where T : Variable
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // The variable reference and data properties must follow the naming convention 'typeRef', 'typeVal'

        VariableInfoAttribute typeInfo = VariableEditor.GetVariableInfo(typeof(T));
        if (typeInfo == null)
        {
            return;
        }

        string propNameBase = typeInfo.VariableType;
        propNameBase = Char.ToLowerInvariant(propNameBase[0]) + propNameBase.Substring(1);

        SerializedProperty referenceProp = property.FindPropertyRelative(propNameBase + "Ref");
        SerializedProperty valueProp = property.FindPropertyRelative(propNameBase + "Val");

        if (typeInfo.VariableType == "Inventory")
        {
            valueProp = property.FindPropertyRelative("item");
        }

        if (referenceProp == null || valueProp == null)
        {
            return;
        }

        Order order = property.serializedObject.targetObject as Order;
        if (order == null)
        {
            return;
        }

        var engine = order.GetEngine() as BasicFlowEngine;
        if (engine == null)
        {
            return;
        }

        var origLabel = new GUIContent(label);

        var itemH = EditorGUI.GetPropertyHeight(valueProp, label);

        if (propNameBase != "location")
        {
            if (itemH <= EditorGUIUtility.singleLineHeight * 2)
            {
                DrawSingleLineProperty(position, origLabel, referenceProp, valueProp, engine, typeInfo);
            }
            else
            {
                DrawMultiLineProperty(position, origLabel, referenceProp, valueProp, engine, typeInfo);
            }
        }

        EditorGUI.EndProperty();
    }

    protected virtual void DrawSingleLineProperty(Rect rect, GUIContent label, SerializedProperty referenceProp, SerializedProperty valueProp, BasicFlowEngine engine,
        VariableInfoAttribute typeInfo)
    {
        int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
        const int popupGap = 5;

        //get out starting rect with intent honoured
        Rect controlRect = EditorGUI.PrefixLabel(rect, label);
        Rect valueRect = controlRect;
        valueRect.width = controlRect.width - popupWidth - popupGap;
        Rect popupRect = controlRect;

        //we are overriding much of the auto layout to cram this all on 1 line so zero the intend and restore it later
        var prevIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        if (referenceProp.objectReferenceValue == null)
        {
            CustomVariableDrawerLookup.DrawCustomOrPropertyField(typeof(T), valueRect, valueProp, GUIContent.none);
            popupRect.x += valueRect.width + popupGap;
            popupRect.width = popupWidth;
        }

        EditorGUI.PropertyField(popupRect, referenceProp, GUIContent.none);
        EditorGUI.indentLevel = prevIndent;
    }

    protected virtual void DrawMultiLineProperty(Rect rect, GUIContent label, SerializedProperty referenceProp, SerializedProperty valueProp, BasicFlowEngine engine,
        VariableInfoAttribute typeInfo)
    {
        const int popupWidth = 100;

        Rect controlRect = rect;
        Rect valueRect = controlRect;
        //valueRect.width = controlRect.width - 5;
        Rect popupRect = controlRect;
        popupRect.height = EditorGUIUtility.singleLineHeight;

        if (referenceProp.objectReferenceValue == null)
        {
            //EditorGUI.PropertyField(valueRect, valueProp, label);
            CustomVariableDrawerLookup.DrawCustomOrPropertyField(typeof(T), valueRect, valueProp, label);
            popupRect.x = rect.width - popupWidth + 5;
            popupRect.width = popupWidth;
        }
        else
        {
            popupRect = EditorGUI.PrefixLabel(rect, label);
        }

        EditorGUI.PropertyField(popupRect, referenceProp, GUIContent.none);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        VariableInfoAttribute typeInfo = VariableEditor.GetVariableInfo(typeof(T));
        if (typeInfo == null)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        string propNameBase = typeInfo.VariableType;
        propNameBase = Char.ToLowerInvariant(propNameBase[0]) + propNameBase.Substring(1);

        SerializedProperty referenceProp = property.FindPropertyRelative(propNameBase + "Ref");

        if (referenceProp.objectReferenceValue != null)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        SerializedProperty valueProp = property.FindPropertyRelative(propNameBase + "Val");
        return EditorGUI.GetPropertyHeight(valueProp, label);
    }
}

[CustomPropertyDrawer(typeof(IntegerData))]
public class IntegerDataDrawer : VariableDataDrawer<IntegerVariable>
{ }
[CustomPropertyDrawer(typeof(BooleanData))]
public class BooleanDataDrawer : VariableDataDrawer<BooleanVariable>
{ }
[CustomPropertyDrawer(typeof(FloatData))]
public class FloatDataDrawer : VariableDataDrawer<FloatVariable>
{ }
[CustomPropertyDrawer(typeof(StringData))]
public class StringDataDrawer : VariableDataDrawer<StringVariable>
{ }
[CustomPropertyDrawer(typeof(SaveKeyData))]
public class SaveKeyDataDrawer : VariableDataDrawer<SaveKeyVariable>
{ }
[CustomPropertyDrawer(typeof(InventoryData))]
public class InventoryDataDrawer : VariableDataDrawer<InventoryVariable>
{ }