using LoGaCulture.LUTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(NodePropertyAttribute))]
public class NodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        NodePropertyAttribute nodeProperty = attribute as NodePropertyAttribute;
        if (nodeProperty == null) return;

        EditorGUI.BeginProperty(position, label, property);

        // Call your NodeField
        NodeEditor.NodeField(property, label, new GUIContent(nodeProperty.defaultText), GraphWindow.GetEngine(), null);

        // Apply changes and handle ScriptableObjects specially
        property.serializedObject.ApplyModifiedProperties();
        if (property.serializedObject.targetObject is ScriptableObject)
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(Node))]
public class NodeEditor : Editor
{
    public static List<Action> actionList = new List<Action>();

    public static bool SelectedNodeDataStale { get; set; }

    protected Texture2D addIcon;
    protected Texture2D duplicateIcon;
    protected Texture2D removeIcon;


    private OrderListAdapter orderListAdapter;
    private SerializedProperty orderListProp;
    private Rect lastEventPopupPos, lastCMDpopupPos;
    private int nodeIndex = 0;

    public EventHandlerEditor eventHandlerEditor;


    protected virtual void OnEnable()
    {
        //this appears to happen when leaving playmode
        try
        {
            if (serializedObject == null)
                return;
        }
        catch (Exception)
        {
            return;
        }

        addIcon = LogaEditorResources.Add;
        duplicateIcon = LogaEditorResources.Duplicate;
        removeIcon = LogaEditorResources.Remove;

        orderListProp = serializedObject.FindProperty("orderList");
        orderListAdapter = new OrderListAdapter(target as Node, orderListProp, null);
    }

    public virtual void DrawGroupUI(BasicFlowEngine engine)
    {
        serializedObject.Update();
        SerializedProperty descriptionProp = serializedObject.FindProperty("nodeDescription");
        SerializedProperty nodeNameProperty = serializedObject.FindProperty("nodeName");

        EditorGUILayout.LabelField(nodeNameProperty.stringValue, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(descriptionProp.stringValue);
        SerializedProperty allowMultipleExecutes = serializedObject.FindProperty("CanExecuteAgain");
        EditorGUILayout.PropertyField(allowMultipleExecutes);

        var node = target as Node;
        if (node._EventHandler != null)
        {
            EditorGUILayout.LabelField(node._EventHandler.GetSummary());
        }
        if (node.OrderList.Count > 0)
        {
            EditorGUILayout.Space();
            orderListAdapter.DrawOrderList();
        }
        else
            EditorGUILayout.LabelField("No Orders in this Node");
        serializedObject.ApplyModifiedProperties();
    }

    public virtual void DrawNodeName(BasicFlowEngine engine)
    {
        serializedObject.Update();
        var node = target as Node;

        SerializedProperty nodeNameProperty = serializedObject.FindProperty("nodeName");
        EditorGUILayout.BeginHorizontal();
        string nameLabel = "Node Name";
        if (node is Group)
            nameLabel = "Group Name";
        EditorGUILayout.PrefixLabel(new GUIContent(nameLabel), EditorStyles.largeLabel);
        EditorGUI.BeginChangeCheck();
        nodeNameProperty.stringValue = EditorGUILayout.TextField(nodeNameProperty.stringValue);
        if (EditorGUI.EndChangeCheck())
        {
            string uniqueName = engine.GetUniqueNodeKey(nodeNameProperty.stringValue, node);

            //ensure that if a variable of this node exists the name is updated
            var variables = engine.Variables; // Assuming engine is an instance of BasicFlowEngine
            foreach (var variable in variables)
            {
                if (variable.GetType() == typeof(NodeVariable) && variable.Scope == VariableScope.Global && variable.Key == node._NodeName)
                {
                    variable.Key = uniqueName;
                }
            }

            // Ensure node name is unique for this Flowchart
            if (uniqueName != node._NodeName)
            {
                nodeNameProperty.stringValue = uniqueName;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }

    public virtual void DrawNodeGUI(BasicFlowEngine engine)
    {
        serializedObject.Update();

        var node = target as Node;

        // Execute any queued cut, copy, paste, etc. operations from the prevous GUI update
        // We need to defer applying these operations until the following update because
        // the ReorderableList control emits GUI errors if you clear the list in the same frame
        // as drawing the control (e.g. select all and then delete)
        if (Event.current.type == EventType.Layout)
        {
            foreach (Action action in actionList)
            {
                if (action != null)
                {
                    action();
                }
            }
            actionList.Clear();
        }

        // Custom tinting
        SerializedProperty useCustomTintProp = serializedObject.FindProperty("useCustomTint");
        SerializedProperty tintProp = serializedObject.FindProperty("tint");

        EditorGUILayout.BeginHorizontal();

        useCustomTintProp.boolValue = GUILayout.Toggle(useCustomTintProp.boolValue, " Custom Tint");
        if (useCustomTintProp.boolValue)
        {
            EditorGUILayout.PropertyField(tintProp, GUIContent.none);
        }

        EditorGUILayout.EndHorizontal();

        SerializedProperty updateLocationStatusProperty = serializedObject.FindProperty("updateLocationStatusToComplete");
        EditorGUILayout.PropertyField(updateLocationStatusProperty);

        EditorGUILayout.BeginHorizontal();
        SerializedProperty descProp = serializedObject.FindProperty("nodeDescription");
        SerializedProperty groupDescProp = serializedObject.FindProperty("groupDescription");
        SerializedProperty nodeLocationProp = serializedObject.FindProperty("nodeLocation");
        SerializedProperty targetKeyNodeProp = serializedObject.FindProperty("targetKeyNode");
        SerializedProperty targetUnlockNodeProp = serializedObject.FindProperty("targetUnlockNode");

        string descLabel = "Node Description";
        if (node is Group)
        {
            descLabel = "Group Description";
        }
        EditorGUILayout.PrefixLabel(new GUIContent(descLabel), EditorStyles.largeLabel);
        EditorGUI.BeginChangeCheck();
        if (node is not Group)
            descProp.stringValue = EditorGUILayout.TextField(descProp.stringValue);
        else
            groupDescProp.stringValue = EditorGUILayout.TextField(groupDescProp.stringValue);
        if (EditorGUI.EndChangeCheck())
        {
            descProp.stringValue = descProp.stringValue;
            groupDescProp.stringValue = groupDescProp.stringValue;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (targetKeyNodeProp.objectReferenceValue != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Target Key Node"), EditorStyles.largeLabel);
            Node targetKeyNode = targetKeyNodeProp.objectReferenceValue as Node;
            EditorGUILayout.LabelField(new GUIContent(targetKeyNode._NodeName), EditorStyles.largeLabel);
            EditorGUILayout.EndHorizontal();

            if (targetKeyNode is Group)
            {
                Group targetGroup = targetKeyNode as Group;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Target Key Node Count"), EditorStyles.largeLabel);
                if (targetGroup.SetNodesToComplete)
                    EditorGUILayout.LabelField(new GUIContent(targetGroup.NodesToComplete.Count.ToString()), EditorStyles.largeLabel);
                else
                    EditorGUILayout.LabelField(new GUIContent(targetGroup.TotalToComplete.ToString()), EditorStyles.largeLabel);
                EditorGUILayout.EndHorizontal();
            }
        }

        NodeField(targetUnlockNodeProp,
                         new GUIContent("Target Unlock Node", "Node to unlock when this one has completed"),
                         new GUIContent("<None>"),
                         engine, node);

        if (targetUnlockNodeProp.objectReferenceValue != null)
        {
            Node targetUnlockNode = targetUnlockNodeProp.objectReferenceValue as Node;
            if (targetUnlockNode != null)
            {
                //ensure that the target unlock node has the current node as its target key node
                if (node.CurrentUnlockNode != null && node.CurrentUnlockNode != targetUnlockNode)
                {
                    node.CurrentUnlockNode.TargetKeyNode = null;
                }
                targetUnlockNode.TargetKeyNode = node;
                node.CurrentUnlockNode = targetUnlockNode;
            }

            if (node is Group)
            {
                //Show the boolean to determine if the group is completed by specific nodes or by a total number of nodes
                SerializedProperty setNodesToCompleteProp = serializedObject.FindProperty("setNodesToComplete");
                EditorGUILayout.PropertyField(setNodesToCompleteProp);
                if (!setNodesToCompleteProp.boolValue)
                {
                    //Create a field for number of nodes to complete if the group is not completed by specific nodes
                    SerializedProperty totalNodesToCompleteProp = serializedObject.FindProperty("totalNodesToComplete");
                    EditorGUILayout.PropertyField(totalNodesToCompleteProp);
                    if (totalNodesToCompleteProp.intValue < 1)
                    {
                        totalNodesToCompleteProp.intValue = 1;
                    }
                    if (totalNodesToCompleteProp.intValue > (node as Group).GroupedNodes.Count)
                    {
                        totalNodesToCompleteProp.intValue = (node as Group).GroupedNodes.Count;
                    }
                }
                else
                {
                    //Create a list of dropdowns that shows each node in the group
                    SerializedProperty nodesToCompleteProp = serializedObject.FindProperty("nodesToComplete");
                    SerializedProperty groupedNodesProp = serializedObject.FindProperty("groupedNodes");
                    //If there are nodes in the group
                    if (groupedNodesProp.arraySize > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Nodes to complete group", EditorStyles.boldLabel);
                        if (nodesToCompleteProp.arraySize <= 0)
                            nodesToCompleteProp.arraySize = 1;
                        if (nodesToCompleteProp.arraySize > groupedNodesProp.arraySize)
                            nodesToCompleteProp.arraySize = groupedNodesProp.arraySize;
                        nodesToCompleteProp.arraySize = EditorGUILayout.IntField(nodesToCompleteProp.arraySize);
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < nodesToCompleteProp.arraySize; i++)
                        {
                            EditorGUILayout.BeginVertical(GUI.skin.box); // Add a GUI box for readability
                            var nodes = (node as Group).GroupedNodes;
                            if (nodesToCompleteProp.arraySize <= nodes.Count)
                            {
                                for (int j = 0; j < nodes.Count; j++)
                                {
                                    if (nodes[j] == nodesToCompleteProp.GetArrayElementAtIndex(i).objectReferenceValue as Node)
                                    {
                                        nodeIndex = j;
                                    }
                                }
                                //Create a drop down list based on the items in the project
                                nodeIndex = EditorGUILayout.Popup("Node", nodeIndex, nodes.Select(x => x._NodeName).ToArray());
                                nodesToCompleteProp.GetArrayElementAtIndex(i).objectReferenceValue = nodes[nodeIndex];
                            }

                            EditorGUILayout.EndVertical(); // End of GUI box
                        }
                    }
                }
            }
        }
        else
        {
            if (node.CurrentUnlockNode != null)
            {
                node.CurrentUnlockNode.TargetKeyNode = null;
                node.CurrentUnlockNode = null;
            }
        }

        var locationVars = engine.GetComponents<LocationVariable>();

        OrderEditor.ObjectField<LocationVariable>(nodeLocationProp,
                new GUIContent("Node Location", "If set, this node will only be active when the player is at this location"),
                new GUIContent("<None>"),
                locationVars.ToList());

        SerializedProperty allowMultipleExecutes = serializedObject.FindProperty("repeatable");
        EditorGUILayout.PropertyField(allowMultipleExecutes);
        SerializedProperty saveableProp = serializedObject.FindProperty("saveable");
        EditorGUILayout.PropertyField(saveableProp);

        //first thing to do is ensure that each order has a reference to its parent node
        foreach (var order in node.OrderList)
        {
            if (order == null) // Will be deleted from the list later on
            {
                continue;
            }
            order.ParentNode = node;
        }

        EditorGUILayout.Space();

        // Draw the event handler popup
        DrawEventHandlerGUI(engine);

        node.UpdateIndentLevels();

        EditorGUILayout.Space();

        //add a button to add this node as a variable for use in other nodes (i.e. is this node complete)
        if (node is not Group)
            if (GUILayout.Button("Add as Variable"))
            {
                //add the node as a variable
                NodeVariable nodeVar = new NodeVariable();
                VariableSelectPopupWindowContent.AddVariable(nodeVar.GetType(), node._NodeName, node);
            }

        EditorGUILayout.Space(15);

        //then we draw the order list using a custom script
        orderListAdapter.DrawOrderList();

        // EventType.contextClick doesn't register since we moved the Block Editor to be inside
        // a GUI Area, no idea why. As a workaround we just check for right click instead.
        if (Event.current.type == EventType.MouseUp &&
            Event.current.button == 1)
        {
            ShowContextMenu();
            Event.current.Use();
        }

        if (GUIUtility.keyboardControl == 0) //Only call keyboard shortcuts when not typing in a text field
        {
            Event e = Event.current;

            // Copy keyboard shortcut
            if (e.type == EventType.ValidateCommand && e.commandName == "Copy")
            {
                if (engine.SelectedOrders.Count > 0)
                {
                    e.Use();
                }
            }

            if (e.type == EventType.ExecuteCommand && e.commandName == "Copy")
            {
                actionList.Add(CopyOrder);
                e.Use();
            }

            // Cut keyboard shortcut
            if (e.type == EventType.ValidateCommand && e.commandName == "Cut")
            {
                if (engine.SelectedOrders.Count > 0)
                {
                    e.Use();
                }
            }

            if (e.type == EventType.ExecuteCommand && e.commandName == "Cut")
            {
                actionList.Add(CutOrder);
                e.Use();
            }

            // Paste keyboard shortcut
            if (e.type == EventType.ValidateCommand && e.commandName == "Paste")
            {
                OrderCopyBuffer orderCopyBuffer = OrderCopyBuffer.GetInstance();
                if (orderCopyBuffer.HasOrders())
                {
                    e.Use();
                }
            }

            if (e.type == EventType.ExecuteCommand && e.commandName == "Paste")
            {
                actionList.Add(PasteOrder);
                e.Use();
            }

            // Duplicate keyboard shortcut
            if (e.type == EventType.ValidateCommand && e.commandName == "Duplicate")
            {
                if (engine.SelectedOrders.Count > 0)
                {
                    e.Use();
                }
            }

            if (e.type == EventType.ExecuteCommand && e.commandName == "Duplicate")
            {
                actionList.Add(CopyOrder);
                actionList.Add(PasteOrder);
                e.Use();
            }

            // Delete keyboard shortcut
            if (e.type == EventType.ValidateCommand && e.commandName == "Delete")
            {
                if (engine.SelectedOrders.Count > 0)
                {
                    e.Use();
                }
            }

            if (e.type == EventType.ExecuteCommand && e.commandName == "Delete")
            {
                actionList.Add(DeleteOrder);
                e.Use();
            }

            // SelectAll keyboard shortcut
            if (e.type == EventType.ValidateCommand && e.commandName == "SelectAll")
            {
                e.Use();
            }

            if (e.type == EventType.ExecuteCommand && e.commandName == "SelectAll")
            {
                actionList.Add(SelectAll);
                e.Use();
            }
        }

        //last thing to do is delete any null orders from the list (if they have been deleted or renamed)
        for (int i = orderListProp.arraySize - 1; i >= 0; --i)
        {
            SerializedProperty orderProperty = orderListProp.GetArrayElementAtIndex(i);
            if (orderProperty.objectReferenceValue == null)
            {
                orderListProp.DeleteArrayElementAtIndex(i);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            SelectedNodeDataStale = true;
        }

        serializedObject.ApplyModifiedProperties();
    }

    public virtual void DrawNodeToolBar()
    {
        var node = target as Node;

        //Should handle drawing the add button, duplicate, remove and navigation buttons
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        //using false to prevent forcing a longer row than will fit on smallest inspector -- for now we only want add button
        var pos = EditorGUILayout.GetControlRect(false, 0, EditorStyles.objectField);
        if (pos.x != 0)
        {
            lastCMDpopupPos = pos;
            lastCMDpopupPos.x += EditorGUIUtility.labelWidth;
            lastCMDpopupPos.y += EditorGUIUtility.singleLineHeight * 2;
        }
        // Add Button
        if (GUILayout.Button(addIcon))
        {
            //this may be less reliable for HDPI scaling but previous method using editor window height is now returning 
            //  null in 2019.2 suspect ongoing ui changes, so default to screen.height and then attempt to get the better result
            int h = Screen.height;
            if (EditorWindow.focusedWindow != null) h = (int)EditorWindow.focusedWindow.position.height;
            else if (EditorWindow.mouseOverWindow != null) h = (int)EditorWindow.mouseOverWindow.position.height;

            OrderSelectorPopupWindowContent.ShowOrderMenu(lastCMDpopupPos, "", target as Node,
                (int)(EditorGUIUtility.currentViewWidth),
                (int)(h - lastCMDpopupPos.y));
        }
        if (GUILayout.Button(duplicateIcon))
        {
            CopyOrder();
            PasteOrder();
        }
        if (GUILayout.Button(removeIcon))
        {
            DeleteOrder();
        }

        GUILayout.EndHorizontal();
    }

    protected virtual void DrawEventHandlerGUI(BasicFlowEngine engine)
    {
        //show all available event handlers in a popup
        Node node = target as Node;
        Type currentType = null;
        if (node._EventHandler != null)
        {
            currentType = node._EventHandler.GetType();
        }

        string currentName = "<None>";
        if (currentType != null)
        {
            EventHandlerInfoAttribute info = EventHandlerEditor.GetEventHandlerInfo(currentType);
            if (info != null)
            {
                currentName = info.EventHandlerName;
            }
        }

        var pos = EditorGUILayout.GetControlRect(true, 0, EditorStyles.objectField);
        if (pos.x != 0)
        {
            lastEventPopupPos = pos;
            lastEventPopupPos.x += EditorGUIUtility.labelWidth;
            lastEventPopupPos.y += EditorGUIUtility.singleLineHeight;
        }

        EditorGUILayout.BeginHorizontal();
        string eventHandlerLabel = "Activate By: ";
        if (node is Group)
        {
            eventHandlerLabel = "Activate By: ";
        }
        EditorGUILayout.PrefixLabel(new GUIContent(eventHandlerLabel), EditorStyles.largeLabel);
        if (EditorGUILayout.DropdownButton(new GUIContent(currentName), FocusType.Passive))
        {
            EventSelectorPopupWindowContent.DoEventHandlerPopup(lastEventPopupPos, currentName, node, (int)(EditorGUIUtility.currentViewWidth - lastEventPopupPos.x), 200);
        }
        EditorGUILayout.EndHorizontal();

        if (node._EventHandler != null)
        {
            if (eventHandlerEditor == null || !node._EventHandler.Equals(eventHandlerEditor.target))
            {
                DestroyImmediate(eventHandlerEditor);
                if (currentType != typeof(ConditionalEventHandler))
                    eventHandlerEditor = Editor.CreateEditor(node._EventHandler, typeof(EventHandlerEditor)) as EventHandlerEditor;
                else
                    eventHandlerEditor = Editor.CreateEditor(node._EventHandler, typeof(ConditionalEventHandlerEditor)) as ConditionalEventHandlerEditor;
            }
            if (eventHandlerEditor != null)
                eventHandlerEditor.DrawInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                SelectedNodeDataStale = true;
            }
        }

    }

    public static void NodeField(SerializedProperty property, GUIContent label, GUIContent nullLabel, BasicFlowEngine engine, Node currentNode = null)
    {
        if (engine == null)
        {
            return;
        }

        var node = property.objectReferenceValue as Node;

        // Build dictionary of child nodes
        List<GUIContent> nodeNames = new List<GUIContent>();

        int selectedIndex = 0;
        nodeNames.Add(nullLabel);
        var nodes = engine.GetComponents<Node>();
        nodes = nodes.OrderBy(x => x._NodeName).ToArray();
        if (currentNode != null)
        {
            nodes = nodes.Where(x => x != currentNode).ToArray();
        }

        for (int i = 0; i < nodes.Length; ++i)
        {
            nodeNames.Add(new GUIContent(nodes[i]._NodeName));

            if (node == nodes[i])
            {
                selectedIndex = i + 1;
            }
        }

        selectedIndex = EditorGUILayout.Popup(label, selectedIndex, nodeNames.ToArray());
        if (selectedIndex == 0)
        {
            node = null; // Option 'None'
        }
        else
        {
            node = nodes[selectedIndex - 1];
        }

        property.objectReferenceValue = node;
    }

    public static void NodeField(Rect position, SerializedProperty property, GUIContent label, GUIContent nullLabel, BasicFlowEngine engine, Node currentNode = null)
    {
        if (engine == null)
        {
            return;
        }
        var node = property.objectReferenceValue as Node;
        // Build dictionary of child nodes
        List<GUIContent> nodeNames = new List<GUIContent>();
        int selectedIndex = 0;
        nodeNames.Add(nullLabel);
        var nodes = engine.GetComponents<Node>();
        nodes = nodes.OrderBy(x => x._NodeName).ToArray();
        if (currentNode != null)
        {
            nodes = nodes.Where(x => x != currentNode).ToArray();
        }
        for (int i = 0; i < nodes.Length; ++i)
        {
            nodeNames.Add(new GUIContent(nodes[i]._NodeName));
            if (node == nodes[i])
            {
                selectedIndex = i + 1;
            }
        }
        selectedIndex = EditorGUI.Popup(position, selectedIndex, nodeNames.ToArray());
        if (selectedIndex == 0)
        {
            node = null; // Option 'None'
        }
        else
        {
            node = nodes[selectedIndex - 1];
        }
        property.objectReferenceValue = node;
    }

    public virtual void ShowContextMenu()
    {
        var node = target as Node;
        var engine = node.GetEngine();

        if (engine == null)
        {
            return;
        }

        bool showCut = false;
        bool showCopy = false;
        bool showDelete = false;
        bool showPaste = false;
        bool showPlay = false;

        if (engine.SelectedOrders.Count > 0)
        {
            showCut = true;
            showCopy = true;
            showDelete = true;
            if (engine.SelectedOrders.Count == 1 && Application.isPlaying)
            {
                showPlay = true;
            }
        }

        OrderCopyBuffer orderCopyBuffer = OrderCopyBuffer.GetInstance();

        if (orderCopyBuffer.HasOrders())
        {
            showPaste = true;
        }

        GenericMenu orderMenu = new GenericMenu();

        if (showCut)
        {
            orderMenu.AddItem(new GUIContent("Cut"), false, CutOrder);
        }
        else
        {
            orderMenu.AddDisabledItem(new GUIContent("Cut"));
        }

        if (showCopy)
        {
            orderMenu.AddItem(new GUIContent("Copy"), false, CopyOrder);
        }
        else
        {
            orderMenu.AddDisabledItem(new GUIContent("Copy"));
        }

        if (showPaste)
        {
            orderMenu.AddItem(new GUIContent("Paste"), false, PasteOrder);
        }
        else
        {
            orderMenu.AddDisabledItem(new GUIContent("Paste"));
        }

        if (showDelete)
        {
            orderMenu.AddItem(new GUIContent("Delete"), false, DeleteOrder);
        }
        else
        {
            orderMenu.AddDisabledItem(new GUIContent("Delete"));
        }

        if (showPlay)
        {
            orderMenu.AddItem(new GUIContent("Play from selected"), false, PlayOrder);
            orderMenu.AddItem(new GUIContent("Stop all and play"), false, StopAllPlayOrder);
        }
        orderMenu.AddSeparator("");

        orderMenu.AddItem(new GUIContent("Select All"), false, SelectAll);
        orderMenu.AddItem(new GUIContent("Select None"), false, SelectNone);

        orderMenu.ShowAsContext();
    }

    protected void SelectAll()
    {
        var node = target as Node;
        var engine = node.GetEngine();

        if (engine == null ||
            engine.SelectedNode == null)
        {
            return;
        }

        engine.ClearSelectedOrders();
        Undo.RecordObject(engine, "Select All");
        foreach (Order order in engine.SelectedNode.OrderList)
        {
            engine.AddSelectedOrder(order);
        }

        Repaint();
    }

    protected void SelectNone()
    {
        var node = target as Node;
        var engine = node.GetEngine();

        if (engine == null ||
            engine.SelectedNode == null)
        {
            return;
        }

        Undo.RecordObject(engine, "Select None");
        engine.ClearSelectedOrders();

        Repaint();
    }

    protected void CutOrder()
    {
        CopyOrder();
        DeleteOrder();
    }

    protected void CopyOrder()
    {
        var node = target as Node;
        var engine = (BasicFlowEngine)node.GetEngine();

        if (engine == null || engine.SelectedNode == null)
        {
            return;
        }

        OrderCopyBuffer orderCopyBuffer = OrderCopyBuffer.GetInstance();
        orderCopyBuffer.Clear();

        //go through all orders to deterimine which ones need copying
        foreach (Order order in engine.SelectedNode.OrderList)
        {
            if (engine.SelectedOrders.Contains(order))
            {
                var type = order.GetType();
                Order newOrder = Undo.AddComponent(orderCopyBuffer.gameObject, type) as Order;
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                foreach (var field in fields)
                {
                    //copy all public fields
                    bool copy = field.IsPublic;
                    //copy non public fields that have the SerializeField attribute
                    var attributes = field.GetCustomAttributes(typeof(SerializeField), true);
                    if (attributes.Length > 0)
                    {
                        copy = true;
                    }

                    if (copy)
                    {
                        field.SetValue(newOrder, field.GetValue(order));
                    }
                }
            }
        }
    }

    protected void PasteOrder()
    {
        var node = target as Node;
        var engine = node.GetEngine();

        if (engine == null || engine.SelectedNode == null)
        {
            return;
        }

        //get the index to paste the orders at
        OrderCopyBuffer orderCopyBuffer = OrderCopyBuffer.GetInstance();

        //if there is no selected order, paste at the end of the list
        int pasteIndex = engine.SelectedNode.OrderList.Count;
        //if there is a selected order, paste after the last selected order
        if (engine.SelectedOrders.Count > 0)
        {
            for (int i = 0; i < engine.SelectedNode.OrderList.Count; i++)
            {
                Order order = engine.SelectedNode.OrderList[i];

                foreach (Order selectedOrder in engine.SelectedOrders)
                {
                    if (order == selectedOrder)
                    {
                        pasteIndex = i + 1;
                    }
                }
            }
        }

        //go through all orders in the copy buffer and paste them
        foreach (Order order in orderCopyBuffer.GetOrders())
        {
            // Using the Editor copy / paste functionality instead of reflection
            // because this does a deep copy
            if (ComponentUtility.CopyComponent(order))
            {
                // Paste the component as a new component
                if (ComponentUtility.PasteComponentAsNew(engine.gameObject))
                {
                    // Get the newly pasted component
                    Order[] orders = engine.GetComponents<Order>();
                    // Get the last pasted component
                    Order pastedOrder = orders.Last<Order>();
                    // Set the item id to a new unique value
                    if (pastedOrder != null)
                    {
                        pastedOrder.ItemId = engine.NextItemId();
                        engine.SelectedNode.OrderList.Insert(pasteIndex++, pastedOrder);
                    }
                }

                // This stops the user pasting the order manually into another game object
                ComponentUtility.CopyComponent(engine.transform);
            }
        }

        // Because this is an async call, we need to force prefab instances to record changes
        PrefabUtility.RecordPrefabInstancePropertyModifications(node);

        Repaint();
    }

    protected void DeleteOrder()
    {
        var node = target as Node;
        var engine = node.GetEngine();

        if (engine == null || engine.SelectedNode == null)
        {
            return;
        }
        //go through all orders to determine which ones need deleting
        int lastSelectedIndex = 0;
        for (int i = engine.SelectedNode.OrderList.Count - 1; i >= 0; i--)
        {
            Order order = engine.SelectedNode.OrderList[i];
            foreach (Order selectedOrder in engine.SelectedOrders)
            {
                if (order == selectedOrder)
                {
                    order.OnOrderRemoved(node);

                    //remove the order from the list - important to do this to ensure undo works
                    Undo.DestroyObjectImmediate(order);

                    Undo.RecordObject(engine.SelectedNode, "DeleteOrder");
                    engine.SelectedNode.OrderList.RemoveAt(i);

                    lastSelectedIndex = i;

                    break;
                }
            }
        }
        //clear the selected orders
        Undo.RecordObject(engine, "DeleteOrder");
        engine.ClearSelectedOrders();

        //select the next order in the list
        if (lastSelectedIndex < engine.SelectedNode.OrderList.Count)
        {
            var nextOrder = engine.SelectedNode.OrderList[lastSelectedIndex];
            node.GetEngine().AddSelectedOrder(nextOrder);
        }

        Repaint();
    }

    protected void PlayOrder()
    {
        var targetNode = target as Node;
        var engine = targetNode.GetEngine();
        Order order = engine.SelectedOrders[0];
        if (targetNode.IsExecuting())
        {
            // Stop the node, wait a while so the executing order has a chance to stop and then execute again from the new order
            targetNode.Stop();
            engine.StartCoroutine(RunNode(engine, targetNode, order.OrderIndex, 0.2f));
        }
        else
        {
            // We can just start right away
            engine.ExecuteNode(targetNode, order.OrderIndex);
        }
    }

    protected void StopAllPlayOrder()
    {
        var targetNode = target as Node;
        var engine = targetNode.GetEngine();
        Order order = engine.SelectedOrders[0];

        // Stop all active nodes then run this selected node
        engine.StopAllNodes();
        engine.StartCoroutine(RunNode(engine, targetNode, order.OrderIndex, 0.2f));
    }

    protected IEnumerator RunNode(BasicFlowEngine engine, Node targetNode, int orderIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        engine.ExecuteNode(targetNode, orderIndex);
    }

    public static List<KeyValuePair<System.Type, OrderInfoAttribute>> GetFilteredOrderInfoAttribute(List<System.Type> menuTypes)
    {
        Dictionary<string, KeyValuePair<System.Type, OrderInfoAttribute>> filteredAttributes = new Dictionary<string, KeyValuePair<System.Type, OrderInfoAttribute>>();

        foreach (System.Type type in menuTypes)
        {
            object[] attributes = type.GetCustomAttributes(false);
            foreach (object obj in attributes)
            {
                OrderInfoAttribute infoAttr = obj as OrderInfoAttribute;
                if (infoAttr != null)
                {
                    string dictionaryName = string.Format("{0}/{1}", infoAttr.Category, infoAttr.OrderName);

                    int existingItemPriority = -1;
                    if (filteredAttributes.ContainsKey(dictionaryName))
                    {
                        existingItemPriority = filteredAttributes[dictionaryName].Value.Priority;
                    }

                    if (infoAttr.Priority > existingItemPriority)
                    {
                        KeyValuePair<System.Type, OrderInfoAttribute> keyValuePair = new KeyValuePair<System.Type, OrderInfoAttribute>(type, infoAttr);
                        filteredAttributes[dictionaryName] = keyValuePair;
                    }
                }
            }
        }
        return filteredAttributes.Values.ToList<KeyValuePair<System.Type, OrderInfoAttribute>>();
    }

    // Compare delegate for sorting the list of command attributes
    public static int CompareOrderAttributes(KeyValuePair<System.Type, OrderInfoAttribute> x, KeyValuePair<System.Type, OrderInfoAttribute> y)
    {
        int compare = (x.Value.Category.CompareTo(y.Value.Category));
        if (compare == 0)
        {
            compare = (x.Value.OrderName.CompareTo(y.Value.OrderName));
        }
        return compare;
    }
}
