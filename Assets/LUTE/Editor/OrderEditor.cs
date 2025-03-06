using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Order), true)]
public class OrderEditor : Editor
{
    #region  statics
    public static Order selectedOrder;

    public static bool SelectedOrderDataStale { get; set; }

    //Get the order info here for the list on the node inspector (using static method)
    public static OrderInfoAttribute GetOrderInfo(System.Type orderType)
    {
        OrderInfoAttribute retval = null;
        object[] attributes = orderType.GetCustomAttributes(typeof(OrderInfoAttribute), false);
        foreach (object obj in attributes)
        {
            OrderInfoAttribute orderInfoAttribute = obj as OrderInfoAttribute;
            if (orderInfoAttribute != null)
            {
                if (retval == null)
                {
                    retval = orderInfoAttribute;
                }
                else if (retval.Priority < orderInfoAttribute.Priority)
                    retval = orderInfoAttribute;
            }
        }
        return retval;
    }

    #endregion statics

    private Dictionary<string, ReorderableList> reoderableLists;

    public virtual void OnEnable()
    {
        if (NullTargetCheck()) // Check for an orphaned editor instance
            return;

        reoderableLists = new Dictionary<string, ReorderableList>();
    }

    public virtual void DrawOrderInpsectorGUI()
    {
        Order t = target as Order;
        if (t == null)
        {
            return;
        }

        var engine = (BasicFlowEngine)t.GetEngine();
        if (engine == null)
        {
            return;
        }

        OrderInfoAttribute orderInfo = OrderEditor.GetOrderInfo(t.GetType());
        if (orderInfo == null)
        {
            return;
        }

        GUILayout.BeginVertical(GUI.skin.box);

        if (t.enabled)
            if (engine.ColourOrders)
            {
                GUI.backgroundColor = t.GetButtonColour();
            }
            else
                GUI.backgroundColor = Color.white;
        else
            GUI.backgroundColor = Color.grey;

        GUILayout.BeginHorizontal(GUI.skin.button);

        string orderName = orderInfo.OrderName;
        GUILayout.Label(orderName, GUILayout.MinWidth(80), GUILayout.ExpandWidth(true));

        GUILayout.FlexibleSpace();

        GUILayout.Label(new GUIContent("(" + t.ItemId + ")"));

        GUILayout.Space(10);

        GUI.backgroundColor = Color.white;
        bool enabled = t.enabled;
        enabled = GUILayout.Toggle(enabled, new GUIContent());

        if (t.enabled != enabled)
        {
            t.enabled = enabled;
        }

        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Separator();

        //Display help text
        OrderInfoAttribute oderInfo = OrderEditor.GetOrderInfo(t.GetType());
        if (oderInfo != null)
        {
            EditorGUILayout.HelpBox(oderInfo.HelpText, MessageType.Info, true);
        }

        EditorGUILayout.Separator();

        EditorGUI.BeginChangeCheck();
        DrawOrderGUI();
        if (EditorGUI.EndChangeCheck())
        {
            SelectedOrderDataStale = true;
        }

        EditorGUILayout.Separator();

        //display error msgs here

        GUILayout.EndVertical();
    }

    public virtual void DrawOrderGUI()
    {
        Order t = target as Order;

        // Update the serialized object before making changes
        serializedObject.Update();
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Skip the MonoScript field, as usual
            if (iterator.name == "m_Script")
            {
                continue;
            }

            // Manually handle the "layers" array property
            if (iterator.name == "layers")
            {
                // Add a field to modify the array size
                int newArraySize = EditorGUILayout.IntField("Number of Layers", iterator.arraySize);
                if (newArraySize == 0)
                {
                    newArraySize = 1;
                }

                // Check if the array size has been modified
                if (newArraySize != iterator.arraySize)
                {
                    // Update the array size
                    iterator.arraySize = newArraySize;
                }

                // Loop through each element in the "layers" array
                for (int i = 0; i < iterator.arraySize; i++)
                {
                    // Get the specific element at index i
                    SerializedProperty layerElement = iterator.GetArrayElementAtIndex(i);

                    // Find the _layerProperty of the element and draw it
                    SerializedProperty layerProperty = layerElement.FindPropertyRelative("_layerProperty");

                    // Draw the _layerProperty field for this element
                    if (layerProperty != null)
                    {
                        EditorGUILayout.PropertyField(layerProperty, new GUIContent($"Layer {i + 1} Property"), true);
                    }
                }

                // Continue to the next property after handling "layers"
                continue;
            }

            // Check if the property should be visible based on custom conditions
            if (!t.IsPropertyVisible(iterator.name))
            {
                continue;
            }

            // Check if the property is an array or generic type, and handle it as a ReorderableList if necessary
            if ((iterator.isArray || iterator.propertyType == SerializedPropertyType.Generic) && t.IsReorderableArray(iterator.name))
            {
                ReorderableList reorderableList = null;

                // Try to find an existing ReorderableList for this property
                if (!reoderableLists.TryGetValue(iterator.displayName, out reorderableList))
                {
                    var locSerProp = iterator.Copy();

                    // Initialize the ReorderableList for the array or generic property
                    reorderableList = new ReorderableList(serializedObject, locSerProp, true, false, true, true)
                    {
                        drawHeaderCallback = (Rect rect) =>
                        {
                            EditorGUI.LabelField(rect, locSerProp.displayName);
                        },
                        drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                        {
                            SerializedProperty element = locSerProp.GetArrayElementAtIndex(index);
                            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
                        },
                        elementHeightCallback = (int index) =>
                        {
                            SerializedProperty element = locSerProp.GetArrayElementAtIndex(index);
                            return EditorGUI.GetPropertyHeight(element, null, true);
                        }
                    };

                    // Cache the ReorderableList for later reuse
                    reoderableLists.Add(iterator.displayName, reorderableList);
                }

                // Draw the ReorderableList
                reorderableList.DoLayoutList();
            }
            else
            {
                // For all other properties, draw them as usual
                EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
            }
        }

        // Apply any modified properties to the serialized object
        serializedObject.ApplyModifiedProperties();
    }


    //public virtual void DrawOrderGUI()
    //    {
    //        Order t = target as Order;

    //        // Code below was copied from here
    //        // http://answers.unity3d.com/questions/550829/how-to-add-a-script-field-in-custom-inspector.html

    //        // Users should not be able to change the MonoScript for the node using the usual Script field.
    //        // Doing so could cause node.orderList to contain null entries.
    //        // To avoid this we manually display all properties, except for m_Script.
    //        // This also allows us to control the order in which properties are displayed.

    //        serializedObject.Update();
    //        SerializedProperty iterator = serializedObject.GetIterator();
    //        bool enterChildren = true;
    //        while (iterator.NextVisible(enterChildren))
    //        {
    //            enterChildren = false;
    //            if (iterator.name == "  m_Script")
    //            {
    //                continue;
    //            }

    //            if (iterator.name == "layers")
    //            {
    //                // Add a field to modify the array size
    //                int newArraySize = EditorGUILayout.IntField("Number of Layers", iterator.arraySize);

    //                // Check if the array size has been modified
    //                if (newArraySize != iterator.arraySize)
    //                {
    //                    // Update the array size
    //                    iterator.arraySize = newArraySize;
    //                }

    //                // Loop through each element in the array
    //                for (int i = 0; i < iterator.arraySize; i++)
    //                {
    //                    // Get the specific element at index i
    //                    SerializedProperty layerElement = iterator.GetArrayElementAtIndex(i);

    //                    // Find the _layerProperty of the element and draw it
    //                    SerializedProperty layerProperty = layerElement.FindPropertyRelative("_layerProperty");

    //                    // Draw the _layerProperty field for this element
    //                    if (layerProperty != null)
    //                    {
    //                        EditorGUILayout.PropertyField(layerProperty, new GUIContent($"Layer {i + 1} Property"), true);
    //                    }
    //                }
    //            }

    //            if (!t.IsPropertyVisible(iterator.name))
    //            {
    //                continue;
    //            }

    //            if ((iterator.isArray || iterator.propertyType == SerializedPropertyType.Generic) && t.IsReorderableArray(iterator.name))
    //            {
    //                ReorderableList reorderableList = null;
    //                reoderableLists.TryGetValue(iterator.displayName, out reorderableList);
    //                if (reorderableList == null)
    //                {
    //                    var locSerProp = iterator.Copy();
    //                    reorderableList = new ReorderableList(serializedObject, locSerProp, true, false, true, true)
    //                    {
    //                        drawHeaderCallback = (Rect rect) =>
    //                        {
    //                            EditorGUI.LabelField(rect, locSerProp.displayName);
    //                        },
    //                        drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
    //                        {
    //                            SerializedProperty element = locSerProp.GetArrayElementAtIndex(index);
    //                            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
    //                        },
    //                        elementHeightCallback = (int index) =>
    //                        {
    //                            SerializedProperty element = locSerProp.GetArrayElementAtIndex(index);
    //                            return EditorGUI.GetPropertyHeight(element, null, true);
    //                        }
    //                    };

    //                    reoderableLists.Add(iterator.displayName, reorderableList);
    //                }
    //                reorderableList.DoLayoutList();
    //            }
    //            else
    //            {
    //                EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
    //            }

    //        }
    //        serializedObject.ApplyModifiedProperties();
    //    }

    public static void ObjectField<T>(SerializedProperty property, GUIContent label, GUIContent nullLabel, List<T> objectList) where T : Object
    {
        if (property == null)
        {
            return;
        }

        List<GUIContent> objectNames = new List<GUIContent>();

        T selectedObject = property.objectReferenceValue as T;

        int selectedIndex = -1; // Invalid index

        // First option in list is <None>
        objectNames.Add(nullLabel);
        if (selectedObject == null)
        {
            selectedIndex = 0;
        }

        for (int i = 0; i < objectList.Count; ++i)
        {
            if (objectList[i] == null) continue;
            if (objectList[i].GetType() == typeof(LocationVariable))
            {
                LocationVariable locVar = objectList[i] as LocationVariable;
                objectNames.Add(new GUIContent(locVar.Key));
            }
            else
                objectNames.Add(new GUIContent(objectList[i].name));

            if (selectedObject == objectList[i])
            {
                selectedIndex = i + 1;
            }
        }

        T result;

        selectedIndex = EditorGUILayout.Popup(label, selectedIndex, objectNames.ToArray());

        if (selectedIndex == -1)
        {
            // Currently selected object is not in list, but nothing else was selected so no change.
            return;
        }
        else if (selectedIndex == 0)
        {
            result = null; // Null option
        }
        else
        {
            result = objectList[selectedIndex - 1];
        }

        property.objectReferenceValue = result;
    }
    // When modifying custom editor code you can occasionally end up with orphaned editor instances.
    // When this happens, you'll get a null exception error every time the scene serializes / deserialized.
    // Once this situation occurs, the only way to fix it is to restart the Unity editor.
    // 
    // As a workaround, this function detects if this order editor is an orphan and deletes it. 
    // To use it, just call this function at the top of the OnEnable() method in your custom editor.
    protected virtual bool NullTargetCheck()
    {
        try
        {
            // The serializedObject accessor create a new SerializedObject if needed.
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

    /// <summary>
    /// Helper method to get all instances of a ScriptableObject in the project with a given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name); //FindAssets uses tags check documentation for more info
        T[] instances = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++) //probably could get optimized
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            instances[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return instances;
    }
}
