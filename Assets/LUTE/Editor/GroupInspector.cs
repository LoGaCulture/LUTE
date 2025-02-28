using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GroupInspector : ScriptableObject
{
    public List<Node> groupedNodes = new List<Node>();
    public string groupName;
    public GraphWindow graphWindow;
    public Group group;
}

[CustomEditor(typeof(GroupInspector), true)]
public class GroupInspectorEditor : Editor
{
    protected Vector2 nodeScrollPos;
    protected NodeEditor nodeEditor;
    protected OrderEditor orderEditor;
    protected Order activeOrder;
    protected static List<OrderEditor> cachedEditors = new List<OrderEditor>();
    protected float windowHeight = 0;
    protected float topPanelHeight = 2;
    protected bool resize = false;
    protected bool clamp = false;
    protected Vector2 orderScrollPos;

    protected void OnDestroy()
    {
        ClearEditors();
    }

    protected void OnEnable()
    {
        ClearEditors();
    }

    protected void OnDisable()
    {
        ClearEditors();
    }

    protected void ClearEditors()
    {
        //should destroy all cached editors here then clear that list
        foreach (OrderEditor editor in cachedEditors)
        {
            DestroyImmediate(editor);
        }
        cachedEditors.Clear();
        orderEditor = null;
    }

    public override void OnInspectorGUI()
    {
        GroupInspector inspectorWindow = target as GroupInspector;
        serializedObject.Update();

        //create an input field for the group name
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Group Name: ", EditorStyles.boldLabel);
        inspectorWindow.groupName = EditorGUILayout.TextField(inspectorWindow.groupName);
        inspectorWindow.group._NodeName = inspectorWindow.groupName;
        //ensure we upadte the group name in the graph window
        GUILayout.EndHorizontal();

        if (inspectorWindow.groupedNodes == null)
        {
            return;
        }

        var group = inspectorWindow.group;
        if (group == null)
        {
            return;
        }

        if (nodeEditor == null || !group.Equals(nodeEditor.target))
        {
            DestroyImmediate(nodeEditor);
            nodeEditor = Editor.CreateEditor(group, typeof(NodeEditor)) as NodeEditor;
        }

        var engine = (BasicFlowEngine)group.GetEngine();

        UpdateWindowHeight();
        float width = EditorGUIUtility.currentViewWidth;

        nodeScrollPos = GUILayout.BeginScrollView(nodeScrollPos, GUILayout.Height(engine.NodeViewHeight));
        //draw group editor GUI stuff
        nodeEditor.DrawNodeGUI(engine);
        GUILayout.EndScrollView();

        Order inspectOrder = null;
        if (engine.SelectedOrders.Count == 1)
        {
            inspectOrder = engine.SelectedOrders[0];
        }

        if (Application.isPlaying && inspectOrder != null && !inspectOrder.ParentNode.Equals(group))
        {
            Repaint();
            return;
        }

        if (Event.current.type == EventType.Layout)
        {
            activeOrder = inspectOrder;
        }

        DrawOrderUI(engine, inspectOrder);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Nodes within this " + inspectorWindow.groupName + " group", EditorStyles.toolbar);
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Update nodes in group", EditorStyles.boldLabel);
        inspectorWindow.group.UpdateGroupNodes = EditorGUILayout.Toggle(inspectorWindow.group.UpdateGroupNodes);
        GUILayout.EndHorizontal();
        GUILayout.Space(20);

        for (int i = 0; i < inspectorWindow.groupedNodes.Count; i++)
        {
            Node node = inspectorWindow.groupedNodes[i];

            if (node == null)
            {
                continue;
            }

            if (engine == null)
            {
                continue;
            }

            if (inspectorWindow.group.UpdateGroupNodes)
            {
                if (inspectorWindow.group._EventHandler != null)
                {
                    if (node._EventHandler != null)
                        Undo.DestroyObjectImmediate(node._EventHandler);
                    Type selectedType = inspectorWindow.group._EventHandler.GetType();
                    EventHandler newHandler = Undo.AddComponent(node.gameObject, selectedType) as EventHandler;
                    newHandler.ParentNode = node;
                    node._EventHandler = newHandler;
                }

                //go through the orders on the group and add each one to the node ensuring the order parent node is the node rather than the group
                var groupOrders = inspectorWindow.group.OrderList;

                foreach (Order order in groupOrders)
                {
                    if (order == null)
                    {
                        continue;
                    }
                    if (!node.OrderList.Contains(order))
                    {
                        node.OrderList.Add(order);
                        node.OrderList.Find(o => o == order).ParentNode = node;
                        node.OrderList.Insert(0, node.OrderList[node.OrderList.Count - 1]);
                        node.OrderList.RemoveAt(node.OrderList.Count - 1);
                    }
                }

                if (inspectorWindow.group.TargetKeyNode != null)
                {
                    node.TargetKeyNode = inspectorWindow.group.TargetKeyNode;
                }
                else
                {
                    node.TargetKeyNode = null;
                }

                //if (inspectorWindow.group.NodeLocation != null)
                //{
                //    node.NodeLocation = inspectorWindow.group.NodeLocation;
                //}
                //else
                //{
                //    node.NodeLocation = null;
                //}

                node.CanExecuteAgain = inspectorWindow.group.CanExecuteAgain;
            }

            if (nodeEditor == null || !node.Equals(nodeEditor.target))
            {
                DestroyImmediate(nodeEditor);
                nodeEditor = Editor.CreateEditor(node, typeof(NodeEditor)) as NodeEditor;
            }
            nodeEditor.DrawNodeName(engine);
            // nodeEditor.DrawGroupUI(engine);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Remove from group"))
            {
                inspectorWindow.graphWindow.UngroupNode(node);
            }
            if (GUILayout.Button("Delete node"))
            {
                //create a new list to hold the node
                List<Node> nodesToDelete = new List<Node>
                {
                    node
                };
                inspectorWindow.graphWindow.AddToDeleteList(nodesToDelete);
            }
            if (GUILayout.Button("Select node"))
            {
                GraphWindow.SetNodeForInspector(node);
            }

            GUILayout.EndHorizontal();

            //draw a line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        serializedObject.ApplyModifiedProperties();
    }

    public void DrawOrderUI(BasicFlowEngine engine, Order inspectOrder)
    {
        ResizeScrollView(engine);

        EditorGUILayout.Space();

        nodeEditor.DrawNodeToolBar();

        orderScrollPos = GUILayout.BeginScrollView(orderScrollPos);

        if (inspectOrder != null)
        {
            if (orderEditor == null || !inspectOrder.Equals(orderEditor.target))
            {
                var editors = from e in cachedEditors where e != null && e.target.Equals(inspectOrder) select e;
                if (editors.Count() > 0)
                {
                    orderEditor = editors.First();
                }
                else
                {
                    orderEditor = Editor.CreateEditor((Order)inspectOrder) as OrderEditor;
                    cachedEditors.Add(orderEditor);
                }
            }
            if (orderEditor != null)
            {
                orderEditor.DrawOrderInpsectorGUI();
            }
        }

        GUILayout.EndScrollView();

        // Draw the resize bar after everything else has finished drawing
        // This is mainly to avoid incorrect indenting.
        Rect resizeRect = new Rect(0, engine.NodeViewHeight + topPanelHeight, EditorGUIUtility.currentViewWidth, 4f);
        GUI.color = new Color(0.64f, 0.64f, 0.64f);
        GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
        resizeRect.height = 1;
        GUI.color = new Color32(132, 132, 132, 255);
        GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
        resizeRect.y += 3;
        GUI.DrawTexture(resizeRect, EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;

        Repaint();
    }

    private void ResizeScrollView(BasicFlowEngine engine)
    {
        Rect cursorChangeRect = new Rect(0, engine.NodeViewHeight + 1 + topPanelHeight, EditorGUIUtility.currentViewWidth, 4f);

        EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeVertical);

        if (cursorChangeRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                resize = true;
            }
        }

        if (resize && Event.current.type == EventType.Repaint)
        {
            //add a undo event here if you like
            engine.NodeViewHeight = Event.current.mousePosition.y - topPanelHeight;
        }

        ClampNodeViewHeight(engine);

        if (resize && Event.current.type == EventType.MouseDrag)
        {
            Rect windowRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, windowHeight);
            if (!windowRect.Contains(Event.current.mousePosition))
            {
                resize = false;
            }
        }

        if (Event.current.type == EventType.MouseUp)
        {
            resize = false;
        }
    }

    private void ClampNodeViewHeight(BasicFlowEngine engine)
    {
        // Screen.height seems to temporarily reset to 480 for a single frame whenever a command like 
        // Copy, Paste, etc. happens. Only clamp the block view height when one of these operations is not occuring.
        if (Event.current.commandName != "")
            clamp = false;

        if (clamp)
        {
            //make sure node view is clamped to visible area
            float height = engine.NodeViewHeight;
            height = Mathf.Max(200, height);
            height = Mathf.Min(windowHeight - 200, height);
            engine.NodeViewHeight = height;
        }

        if (Event.current.type == EventType.Repaint)
            clamp = true;
    }


    /// <summary>
    /// In Unity 5.4, Screen.height returns the pixel height instead of the point height
    /// of the inspector window. We can use EditorGUIUtility.currentViewWidth to get the window width
    /// but we have to use this horrible hack to find the window height.
    /// For one frame the windowheight will be 0, but it doesn't seem to be noticeable.
    /// </summary>
    protected void UpdateWindowHeight()
    {
        windowHeight = Screen.height * EditorGUIUtility.pixelsPerPoint;
    }
}
