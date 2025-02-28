using LoGaCulture.LUTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class GraphWindow : EventWindow
{
    private static List<Node> priorNodes = new List<Node>();
    private static List<Node> newNodes = new List<Node>();

    private int x, y;
    //A class to store a copy of an object
    protected class ClipboardObject
    {
        internal SerializedObject serializedObject;
        internal Type type;

        internal ClipboardObject(Object obj)
        {
            serializedObject = new SerializedObject(obj);
            type = obj.GetType();
        }
    }

    //A class to store a copy of a node
    public class NodeCopy
    {
        private SerializedObject node = null;
        private List<ClipboardObject> orders = new List<ClipboardObject>();
        private ClipboardObject eventHandler = null;
        private Node curNode = null;

        //initialise the node copy
        internal NodeCopy(Node node)
        {
            this.node = new SerializedObject(node);
            this.curNode = node;
            var orderList = node.OrderList;
            foreach (var order in orderList)
            {
                orders.Add(new ClipboardObject(order));
            }
            if (node._EventHandler != null)
                eventHandler = new ClipboardObject(node._EventHandler);
        }

        //method to copy properties from the node copy to a node
        private void CopyProperties(SerializedObject source, Object dest, params SerializedPropertyType[] excludeTypes)
        {
            var newSerializedObject = new SerializedObject(dest);
            var prop = source.GetIterator();
            //go through all the properties of the node and apply it our node on clipboard (except for the ones we want to exclude)
            while (prop.NextVisible(true))
            {
                if (!excludeTypes.Contains(prop.propertyType))
                {
                    newSerializedObject.CopyFromSerializedProperty(prop);
                }
            }
            newSerializedObject.ApplyModifiedProperties();
        }

        //now we can paste the properties
        internal Node PasteNode(GraphWindow pastingWindow, BasicFlowEngine engine, bool bluePrint = false)
        {
            var newNode = engine.CreateNode(Vector2.zero);

            //Copy all orders behaviours on the node 
            foreach (var order in orders)
            {
                var newOrder = Undo.AddComponent(engine.gameObject, order.type) as Order;
                CopyProperties(order.serializedObject, newOrder);
                newOrder.ItemId = engine.NextItemId();
                newNode.OrderList.Add(newOrder);

                //If we are copying from a blueprint then we must set the new node condition variables to new references in our new engine
                if (bluePrint)
                {
                    var orderObj = order.serializedObject.targetObject as Order;
                    var orderLocation = orderObj.GetOrderLocation();
                    if (orderLocation != null)
                    {
                        var newLocVar = storyEngine.GetVariable(orderLocation.Key);
                        if (newLocVar != null)
                        {
                            var newVar = engine.AddVariable(newLocVar.GetType(), newLocVar.Key);
                            newVar.Apply(SetOperator.Assign, newLocVar);
                            newVar.Scope = VariableScope.Global;

                            newOrder.SetLocationVariable(newVar as LocationVariable);
                        }
                    }
                    if (order.type == typeof(If))
                    {
                        var ifOrder = order.serializedObject.targetObject as If;
                        if (ifOrder != null)
                        {
                            foreach (var condition in ifOrder.conditions)
                            {
                                if (condition.AnyVariable.variable != null)
                                {
                                    //Variable on the original order now found on new engine
                                    var variable = engine.GetVariable(condition.AnyVariable.variable.Key);
                                    //Now set this variable to the new order
                                    if (variable != null)
                                    {
                                        if (newOrder.GetType() == typeof(If))
                                        {
                                            var newIfOrder = newOrder as If;
                                            newIfOrder.conditions[ifOrder.conditions.IndexOf(condition)].AnyVariable.variable = variable;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //if order uses location (set this up to be overriden by orders that use location to set as true) - do similar to above
                }
            }

            //if event handler on node, copy it
            if (eventHandler != null)
            {
                var newEventHandler = Undo.AddComponent(engine.gameObject, eventHandler.type) as EventHandler;
                CopyProperties(eventHandler.serializedObject, newEventHandler);
                newEventHandler.ParentNode = newNode;
                newNode._EventHandler = newEventHandler;
            }

            //Finally copy the node properties but not the referneces to the orders and event handler (they have just been assigned)
            CopyProperties(node, newNode, SerializedPropertyType.ObjectReference,
                SerializedPropertyType.Generic,
                SerializedPropertyType.ArraySize);

            string copyText = " (Copy)";
            string name = storyEngine.GetUniqueNodeKey(curNode._NodeName + copyText);
            if (bluePrint)
            {
                copyText = !newNode._NodeName.Contains("Blueprint") ? " (Blueprint)" : string.Empty;
                name = newNode._NodeName + copyText;
            }
            newNode._NodeName = name;

            //If we are copying from a blueprint then we must set the new node unlock node to the new reference in our new engine
            if (bluePrint)
            {
                var _node = node.targetObject as Node;


                //If node uses a location then find the new reference location and set it to this
                if (_node.NodeLocation != null)
                {
                    var newLocVar = engine.GetVariable(_node.NodeLocation.Key);
                    if (newLocVar != null)
                    {
                        newNode.NodeLocation = (LocationVariable)newLocVar as LocationVariable;
                    }
                }

                GraphWindow.priorNodes.Add(_node);
                GraphWindow.newNodes.Add(newNode);
            }

            return newNode;
        }
    }

    protected struct NodeGraphics
    {
        internal Color tint;
        internal Texture2D onTexture;
        internal Texture2D offTexture;
    }

    /// <summary>
    /// Helper class to maintain list of blocks that are currently executing when the game is running in editor
    /// </summary>
    protected class ExecutingBlocks
    {
        internal List<Node> areExecuting = new List<Node>(),
                             wereExecuting = new List<Node>(),
                             workspace = new List<Node>();

        internal bool isChangeDetected { get; set; }

        private float lastFade;

        internal void ProcessAllNodes(Node[] nodes)
        {
            isChangeDetected = false;
            workspace.Clear();
            //cache these once as they can end up being called thousands of times per frame otherwise
            var curRealTime = Time.realtimeSinceStartup;
            var fadeTimer = curRealTime + LogaConstants.ExecutingIconFadeTime;
            for (int i = 0; i < nodes.Length; ++i)
            {
                var b = nodes[i];
                var bIsExec = b.IsExecuting();
                if (bIsExec)
                {
                    b.ExecutingIconTimer = fadeTimer;
                    b.ActiveOrder.ExecutingIconTimer = fadeTimer;
                    workspace.Add(b);
                }
            }

            if (areExecuting.Count != workspace.Count || !WorkspaceMatchesExeucting())
            {
                wereExecuting.Clear();
                wereExecuting.AddRange(areExecuting);
                areExecuting.Clear();
                areExecuting.AddRange(workspace);
                isChangeDetected = true;
                lastFade = fadeTimer;
            }
        }

        internal bool WorkspaceMatchesExeucting()
        {
            for (int i = 0; i < areExecuting.Count; i++)
            {
                if (areExecuting[i] != workspace[i])
                    return false;
            }
            return true;
        }

        internal bool IsAnimFadeoutNeed()
        {
            return (lastFade - Time.realtimeSinceStartup) >= 0;
        }

        internal void ClearAll()
        {
            areExecuting.Clear();
            wereExecuting.Clear();
            workspace.Clear();
            isChangeDetected = true;
            lastFade = 0;
        }
    }

    public const float gridLineSpacingSize = 120;
    public const float minZoomValue = 0.25f;
    public const float maxZoomValue = 1f;
    public const float RightClickTolerance = 5f;
    public const int HorizontalPad = 20;
    public const int VerticalPad = 5;
    public const float DefaultNodeHeight = 40;
    public const float NodeMinWidth = 60;
    public const float NodeMaxWidth = 240;

    protected Vector2 startDragPosition = Vector2.zero;
    protected List<NodeCopy> copyList = new List<NodeCopy>();
    protected List<Node> pasteList = new List<Node>();

    public static List<Node> deleteList = new List<Node>();
    protected int forceRepaintCount;
    protected Vector2 rightClickDown = -Vector2.one;
    protected Vector2 selectionBoxStartPos = -Vector2.one;
    protected List<Node> mouseDownSelectionState = new List<Node>();
    protected Texture2D connectionPointTexture;
    protected Rect selectionBox;
    protected Vector2 windowPos;
    protected static BasicFlowEngine storyEngine;
    protected static NodeInspectorWindow nodeInspector;
    protected Node[] nodes;
    protected Node dragNode;
    protected bool hasDraggedSelected;
    protected static LogaStates logaStates;
    protected Label[] labels;
    protected Label dragLabel;
    protected AnnotationLine[] annotationLines;
    protected AnnotationLine dragLine;
    protected AnnotationBox[] annotationBoxes;
    protected AnnotationBox dragBox;
    protected GUIContent addButtonContent;
    protected GUIContent removeButtonContent;
    protected GUIContent minimiseButtonContent;
    protected GUIContent maximiseButtonContent;
    protected Texture2D addTexture;
    protected Texture2D removeTexture;
    protected Texture2D minimiseTexture;
    protected Texture2D maximiseTexture;
    protected List<Node> mouseDownSelectState = new List<Node>();
    protected Group dragGroup;
    protected static GroupInspector groupInspector;
    protected BasicFlowEngine prevEngine;
    protected static VariableListAdaptor variableListAdaptor;
    protected bool isDrawingAnnotationLine = false;
    protected AnnotationLine activeAnnotationLine;
    protected bool isDrawingAnnotationBox = false;
    protected int prevVarCount;

    private GUIStyle currentStyle, groupingBoxStyle, handlerStyle, nodeStyle, descriptionStyle;
    private bool didDoubleClick;
    private ExecutingBlocks executingBlocks = new ExecutingBlocks();

    private bool showBlueprintWindow = false;
    private Rect blueprintWindowRect;
    private string blueprintName = "Blueprint";

    [MenuItem("LUTE/Show Engine Window")]
    public static GraphWindow ShowWindow()
    {
        return GetWindow(typeof(GraphWindow), false, "Flow Engine") as GraphWindow;
    }

    private static GraphWindow instance;

    // Private constructor to prevent instantiation from outside the class
    private GraphWindow() { }

    // Public static method to get the instance of the class
    public static GraphWindow GetInstance()
    {
        if (instance == null)
        {
            instance = ShowWindow();
        }
        return instance;
    }

    private void OnEnable()
    {
        this.wantsMouseMove = true;

        addTexture = LogaEditorResources.Add;
        removeTexture = LogaEditorResources.Cross;
        minimiseTexture = LogaEditorResources.Minimise;
        maximiseTexture = LogaEditorResources.Maximise;
        addButtonContent = new GUIContent(addTexture, "Add a new node");
        removeButtonContent = new GUIContent(removeTexture, "Disband Group");
        minimiseButtonContent = new GUIContent(minimiseTexture, "Minimise Group");
        maximiseButtonContent = new GUIContent(maximiseTexture, "Maximise Group");

        connectionPointTexture = LogaEditorResources.ConnectionPoint;

        copyList.Clear();

        UpdateNodes();

        //List<Label> gameLabels = storyEngine.gameObject.GetComponents<Label>().ToList();
        //labels = gameLabels.ToArray();

        //List<AnnotationLine> gameAnnotationLines = storyEngine.gameObject.GetComponents<AnnotationLine>().ToList();
        //annotationLines = gameAnnotationLines.ToArray();

        //List<AnnotationBox> gameAnnotationBoxes = storyEngine.gameObject.GetComponents<AnnotationBox>().ToList();
        //annotationBoxes = gameAnnotationBoxes.ToArray();

        if (variableListAdaptor == null || variableListAdaptor.TargetEngine != storyEngine)
        {
            var fsSO = new SerializedObject(storyEngine);
            variableListAdaptor = new VariableListAdaptor(fsSO.FindProperty("variables"), storyEngine);
        }

        MapboxControls.engine = storyEngine;

        EditorApplication.update += OnEditorUpdate;
        Undo.undoRedoPerformed += Undo_ForceRepaint;
#if UNITY_2017_4_OR_NEWER
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif    
    }

    private void InitStyles()
    {
        if (nodeStyle == null)
        {
            nodeStyle = new GUIStyle();
        }

        // All block nodes use the same GUIStyle, but with a different background
        nodeStyle.border = new RectOffset(HorizontalPad, HorizontalPad, VerticalPad, VerticalPad);
        nodeStyle.padding = nodeStyle.border;
        nodeStyle.contentOffset = Vector2.zero;
        nodeStyle.alignment = TextAnchor.MiddleCenter;
        nodeStyle.wordWrap = true;

        var borderSize = -2; // Border size in pixels

        if (EditorStyles.helpBox != null && descriptionStyle == null)
        {
            descriptionStyle = new GUIStyle(EditorStyles.helpBox);
        }
        descriptionStyle.wordWrap = true;
        descriptionStyle.normal.background = MakeTex(2, 2, Color.white);
        descriptionStyle.normal.textColor = Color.black;

        if (currentStyle == null)
        {
            currentStyle = new GUIStyle();
            currentStyle.normal.background = MakeTex(2, 2, new Color32(255, 255, 255, 85));
            currentStyle.border = new RectOffset(borderSize, borderSize, borderSize, borderSize);
        }
        if (groupingBoxStyle == null)
        {
            groupingBoxStyle = new GUIStyle(GUI.skin.box);
            groupingBoxStyle.normal.background = MakeTex(2, 2, new Color32(255, 255, 200, 85));
            groupingBoxStyle.border = new RectOffset(borderSize, borderSize, borderSize, borderSize);
        }

        if (EditorStyles.whiteLabel != null && handlerStyle == null)
        {
            handlerStyle = new GUIStyle(EditorStyles.label);
        }
        handlerStyle.wordWrap = true;
        handlerStyle.margin.top = 0;
        handlerStyle.margin.bottom = 0;
        handlerStyle.alignment = TextAnchor.MiddleCenter;
        handlerStyle.normal.textColor = Color.cyan;
    }

    internal bool HandleEngineSelectionChange()
    {
        storyEngine = GetEngine();
        //target has changed, so clear the blockinspector
        if (storyEngine != prevEngine)
        {
            nodeInspector = null;
            prevEngine = storyEngine;
            executingBlocks.ClearAll();

            UpdateNodes();

            if (storyEngine != null)
                storyEngine.ReverseUpdateSelectedCache();//becomes reverse restore selected cache

            Repaint();
            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        if (HandleEngineSelectionChange()) return;

        if (storyEngine == null)
        {
            GUILayout.Label("No Engine scene object selected");
            return;
        }

        InitStyles();

        DeleteNodes();

        if (Event.current.type == EventType.Repaint)
        {
            UnityEditor.Graphs.Styles.graphBackground.Draw(
                new Rect(0, 17, position.width, position.height - 17), false, false, false, false
            );
        }

        Rect scriptViewRect = CalcFlowchartWindowViewRect();
        EditorZoomArea.Begin(storyEngine.Zoom, scriptViewRect);

        if (Event.current.type == EventType.Repaint)
        {
            DrawGrid(Event.current);

            //draw all non selected
            for (int i = 0; i < nodes.Length; ++i)
            {
                var node = nodes[i];
                if (!node.IsSelected && !node.IsControlSelected && node is not Group)
                    DrawNodeWindow(i);
            }

            //draw all held
            for (int i = 0; i < nodes.Length; ++i)
            {
                var node = nodes[i];
                if (node.IsControlSelected && node is not Group)
                    DrawNodeWindow(i);
            }

            //draw all selected
            for (int i = 0; i < nodes.Length; ++i)
            {
                var node = nodes[i];
                if (node.IsSelected && node is not Group)
                    DrawNodeWindow(i);
            }
        }

        if (storyEngine.ShowAnnotations)
        {
            if (labels != null && labels.Length > 0)
                for (int i = 0; i < labels.Length; i++)
                {
                    DrawLabel(i);
                }

            if (annotationLines != null && annotationLines.Length > 0)
                for (int i = 0; i < annotationLines.Length; i++)
                {
                    if (annotationLines[i].Start.position != Vector2.zero && annotationLines[i].End.position != Vector2.zero)
                        DrawLine(i);
                }

            if (annotationBoxes != null && annotationBoxes.Length > 0)
                for (int i = 0; i < annotationBoxes.Length; i++)
                {
                    DrawAnnotationBox(i);
                }
        }

        for (int i = 0; i < storyEngine.Groups.Count; i++)
        {
            if (storyEngine.Groups[i] == null)
                continue;
            if (storyEngine.Groups[i].GroupedNodes == null)
                continue;

            var groupedNode = storyEngine.Groups[i].GroupedNodes;
            try
            {
                DrawDottedBoxAroundNodes(groupedNode, i);
            }
            catch (Exception)
            {
                //Debug.Log("Failed to draw box in some way");
            }
        }

        BeginWindows();
        if (showBlueprintWindow)
        {
            DrawBlueprintWindow();
        }
        EndWindows();

        if (Application.isPlaying)
        {
            var emptyStyle = new GUIStyle();

            //cache these once as they can end up being called thousands of times per frame otherwise
            var curRealTime = Time.realtimeSinceStartup;

            for (int i = 0; i < nodes.Length; i++)
            {
                var n = nodes[i];
                DrawExecutingNodeIcon(n, scriptViewRect, (n.ExecutingIconTimer - curRealTime) / LogaConstants.ExecutingIconFadeTime, emptyStyle);
            }
            GUI.color = Color.white;
        }



        EditorZoomArea.End();

        // Draw selection box
        if (Event.current.type == EventType.Repaint)
        {
            if (selectionBoxStartPos.x >= 0 && selectionBoxStartPos.y >= 0)
            {
                GUI.Box(selectionBox, "", GUI.skin.FindStyle("SelectionRect"));
            }
        }

        // Draw toolbar, search popup, and variables window
        //  need try catch here as we are now invalidating the drawer if the target engine
        //      has changed which makes unity GUILayouts upset and this function appears to 
        //      actually get called partially outside our control
        try
        {
            DrawOverlay(Event.current);
        }
        catch (Exception)
        {
            //Debug.Log("Failed to draw overlay in some way");
        }

        base.HandleEvents(Event.current);

        if (forceRepaintCount > 0)
            Repaint();

#if UNITY_2020_1_OR_NEWER
        //Force exit gui once repainted
        GUIUtility.ExitGUI();
#endif
    }
    protected void DrawBlueprintWindow()
    {
        Rect tempRect = blueprintWindowRect;
        tempRect.width = Mathf.Clamp(200, NodeMinWidth, NodeMaxWidth);
        //Ensuring that the rect of our label is always expanded by the text content so we can drag/click it
        tempRect.height = DefaultNodeHeight + EditorStyles.toolbar.CalcHeight(new GUIContent(blueprintName), tempRect.width);

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.background = MakeTex(2, 2, Color.white);
        labelStyle.wordWrap = true;

        blueprintWindowRect = tempRect;

        Rect windowRelativeRect = new Rect(blueprintWindowRect);
        float labelHeight = labelStyle.CalcHeight(new GUIContent(blueprintName), tempRect.width);
        windowRelativeRect.height = tempRect.height;
        windowRelativeRect.position += storyEngine.ScrollPos;

        labelStyle.normal.textColor = Color.black;

        // Draw the label "Blueprint Name:"
        Rect labelRect = new Rect(windowRelativeRect.x, windowRelativeRect.y, windowRelativeRect.width, labelHeight);
        GUI.Label(labelRect, "Blueprint Name:", labelStyle);

        // Adjust the text field position and height
        Rect textFieldRect = new Rect(windowRelativeRect.x, windowRelativeRect.y + labelHeight, windowRelativeRect.width, windowRelativeRect.height - labelHeight);

        GUI.Box(windowRelativeRect, ""); // Empty box to draw behind label and text field
        EditorGUIUtility.AddCursorRect(textFieldRect, MouseCursor.Text);
        blueprintName = GUI.TextField(textFieldRect, blueprintName, labelStyle);

        // Add an exit button to the blueprint window
        if (GUI.Button(new Rect(windowRelativeRect.x + windowRelativeRect.width - 20, windowRelativeRect.y, 20, 20), "X"))
        {
            showBlueprintWindow = false;
        }
        // Adjust the position of the "Create Blueprint" button
        Rect buttonRect = new Rect(windowRelativeRect.x, windowRelativeRect.y + windowRelativeRect.height, windowRelativeRect.width, 20);

        // Add a button to the blueprint window to create a blueprint with the given name
        if (GUI.Button(buttonRect, "Create Blueprint"))
        {
            if (!string.IsNullOrEmpty(blueprintName))
            {
                // Replace any empty spaces in the blueprint name with underscores
                blueprintName = blueprintName.Replace(" ", "_");
                CreateBlueprint(blueprintName);
            }
        }
    }


    protected virtual void DrawOverlay(Event e)
    {

        //Tool bar group
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        {
            GUILayout.Space(2);

            // Draw add node button
            if (GUILayout.Button(addButtonContent, EditorStyles.toolbarButton))
            {
                DeselectAllNodes();
                Vector2 newNodePosition = new Vector2(
                    50 / storyEngine.Zoom - storyEngine.ScrollPos.x, 50 / storyEngine.Zoom - storyEngine.ScrollPos.y
                );
                AddNode(newNodePosition);
                UpdateNodes();
            }

            GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(8)); // Separator

            // Draw scale bar and labels
            GUILayout.Label("Scale", EditorStyles.miniLabel);
            var newZoom = GUILayout.HorizontalSlider(
                storyEngine.Zoom, minZoomValue, maxZoomValue, GUILayout.MinWidth(40), GUILayout.MaxWidth(100)
            );
            GUILayout.Label(storyEngine.Zoom.ToString("0.0#x"), EditorStyles.miniLabel, GUILayout.Width(30));

            if (newZoom != storyEngine.Zoom)
            {
                DoZoom(newZoom - storyEngine.Zoom, Vector2.one * 0.5f);
            }

            // Draw centre button
            if (GUILayout.Button("Centre Window", EditorStyles.toolbarButton))
            {
                CentreWindow();
            }
            if (GUILayout.Button("Show Map", EditorStyles.toolbarButton))
            {
                MapboxControls.engine = storyEngine;
                MapboxControls.ShowWindow();
                Selection.activeObject = storyEngine.GetAbstractMap();
            }

            //string annotationButton = storyEngine.ShowAnnotations ? "Hide Annotations" : "Show Annotations";

            //if (GUILayout.Button(annotationButton, EditorStyles.toolbarButton))
            //{
            //    storyEngine.ShowAnnotations = !storyEngine.ShowAnnotations;
            //}

            //string annotationBoxString = isDrawingAnnotationBox ? "Stop Drawing Annotation Box" : "Draw Annotation Box";
            //if (GUILayout.Button(annotationBoxString, EditorStyles.toolbarButton))
            //{
            //    isDrawingAnnotationBox = !isDrawingAnnotationBox;
            //}

            //if (GUILayout.Button("Clear Annotations", EditorStyles.toolbarButton))
            //{
            //    storyEngine.ClearAnnotations();
            //    labels.ToList().Clear();
            //    annotationLines.ToList().Clear();
            //    annotationBoxes.ToList().Clear();
            //    UpdateAnnotations();
            //}

            GUILayout.FlexibleSpace();

        }
        GUILayout.EndHorizontal();

        // Name and description group
        GUILayout.BeginHorizontal();
        {

            GUILayout.BeginVertical();
            {
                if (storyEngine != null)
                    GUILayout.Label(storyEngine.name, EditorStyles.boldLabel);

                GUILayout.Space(2);

                if (storyEngine != null && storyEngine.Description != null && storyEngine.Description.Length > 0)
                {
                    GUILayout.Label(storyEngine.Description, EditorStyles.label);
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        DrawVariablesBlock(Event.current);
    }

    private void DrawExecutingNodeIcon(Node n, Rect scriptViewRect, float alpha, GUIStyle style)
    {
        if (alpha <= 0)
            return;
        if (n is Group)
            return;

        Rect rect = new Rect(n._NodeRect);

        rect.x += storyEngine.ScrollPos.x - 37;
        rect.y += storyEngine.ScrollPos.y + 3;
        rect.width = 34;
        rect.height = 34;

        if (scriptViewRect.Overlaps(rect))
        {
            GUI.color = new Color(1f, 1f, 1f, alpha);

            if (GUI.Button(rect, LogaEditorResources.PlayBig, style))
            {
                SelectNode(n);
            }

            GUI.color = Color.white;
        }
    }

    protected void SelectNode(Node node)
    {
        DeselectAllNodes();
        storyEngine.SelectedNode = node;
        SetNodeForInspector(node);
    }

    //function to draw a dotted box around every node
    //it goes through all nodes and calculates a bounding box that contains all nodes
    //then it draws a box around that bounding box
    //this is a bit of a hacky way to do it but it works
    protected void DrawDottedBoxAroundNodes(List<Node> _groupedNodes, int id)
    {
        //if there are no nodes then return 
        if (_groupedNodes.Count == 0 || _groupedNodes.Count == 1)
            return;

        if (storyEngine.groupnames.Count <= id)
            storyEngine.groupnames.Add("");

        //get the first node
        Node firstNode = _groupedNodes[0];

        //get the position of the first node
        Vector2 topLeft = firstNode._NodeRect.position;
        Vector2 bottomRight = firstNode._NodeRect.position + firstNode._NodeRect.size;

        //go through all nodes and find the bounding box
        for (int i = 0; i < _groupedNodes.Count; i++)
        {
            Node node = _groupedNodes[i];
            if (node.GroupIndex < 0 || !storyEngine.Groups[node.GroupIndex].IsMinimised)
            {
                //get the position of the node
                Vector2 nodePos = node._NodeRect.position;
                Vector2 nodeSize = node._NodeRect.size;

                //get the top left and bottom right corners of the node
                Vector2 nodeTopLeft = nodePos;
                Vector2 nodeBottomRight = nodePos + nodeSize;

                //if the node is to the left of the bounding box
                if (nodeTopLeft.x < topLeft.x)
                {
                    topLeft.x = nodeTopLeft.x;
                }

                //if the node is to the right of the bounding box
                if (nodeBottomRight.x > bottomRight.x)
                {
                    bottomRight.x = nodeBottomRight.x;
                }

                //if the node is above the bounding box
                if (nodeTopLeft.y < topLeft.y)
                {
                    topLeft.y = nodeTopLeft.y;
                }

                //if the node is below the bounding box
                if (nodeBottomRight.y > bottomRight.y)
                {
                    bottomRight.y = nodeBottomRight.y;
                }
            }

            node.IsGrouped = true;
            node.GroupIndex = id;
        }

        //don't forget to add some padding to the bounding box - hard coded values should be replaced
        topLeft -= new Vector2(50, 50);
        bottomRight += new Vector2(50, 50);

        //calculate the size of the bounding box
        Vector2 boundingBoxSize = bottomRight - topLeft;
        //should be determined by the group itself
        string groupName = GetGroupName(storyEngine.Groups[id]);

        if (storyEngine.Groups[id].IsMinimised)
        {
            boundingBoxSize.y = EditorGUIUtility.singleLineHeight + groupingBoxStyle.fixedHeight;
            Vector2 groupNameSize = groupingBoxStyle.CalcSize(new GUIContent(groupName));
            groupNameSize += new Vector2(minimiseButtonContent.image.width, 0);
            boundingBoxSize.x = groupNameSize.x + 25;
        }
        //draw the bounding box and toolbar
        var borderSize = -2; // Border size in pixels
        groupingBoxStyle = new GUIStyle(GUI.skin.box);
        Color32 backgroundCol = new Color32(161, 255, 216, 50);
        if (storyEngine.Groups[id].UseCustomTint)
            backgroundCol = storyEngine.Groups[id].Tint;
        groupingBoxStyle.normal.background = MakeTex(2, 2, backgroundCol);
        groupingBoxStyle.border = new RectOffset(borderSize, borderSize, borderSize, borderSize);

        GUILayout.BeginArea(new Rect(topLeft + storyEngine.ScrollPos, boundingBoxSize));
        if (!storyEngine.Groups[id].IsMinimised)
            GUI.Box(new Rect(0, 0, boundingBoxSize.x, boundingBoxSize.y), GUIContent.none, groupingBoxStyle);

        storyEngine.Groups[id]._NodeRect = new Rect(topLeft, boundingBoxSize);

        if (!storyEngine.Groups[id].IsMinimised)
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, boundingBoxSize.x, boundingBoxSize.y), MouseCursor.MoveArrow);
        else
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, boundingBoxSize.x, EditorGUIUtility.singleLineHeight), MouseCursor.MoveArrow);
        GUIStyle toolBarStyle = new GUIStyle(EditorStyles.toolbar);
        toolBarStyle.normal.background = MakeTex(2, 2, Color.white);
        toolBarStyle.normal.textColor = Color.black;
        toolBarStyle.fontStyle = FontStyle.Bold;
        toolBarStyle.alignment = TextAnchor.MiddleLeft;
        toolBarStyle.fontSize = 12;
        GUILayout.BeginHorizontal(toolBarStyle);
        GUILayout.Label(groupName, toolBarStyle);

        //find the corresponding group variable and update the gameobject name
        var collNames = storyEngine.Variables.OfType<NodeCollectionVariable>().ToList().Find(x => _groupedNodes.All(node => x.Value.Contains(node)));
        if (collNames != null && collNames.Scope == VariableScope.Global)
        {
            collNames.Key = groupName;
        }
        var existingGroupObjsNames = storyEngine.GetComponentsInChildren<NodeCollection>();
        foreach (NodeCollection group in existingGroupObjsNames)
        {
            //if all nodes in the group obj are equal to the nodes provided here and have the same count size, then the group obj already exists
            if (storyEngine.Groups[id].GroupedNodes.All(x => group.Contains(x)) && storyEngine.Groups[id].GroupedNodes.Count == group.Count)
            {
                group.gameObject.name = groupName;
                break;
            }
        }

        GUILayout.Space(20);

        //add delete button for disbanding the group
        // if (GUILayout.Button(removeButtonContent, EditorStyles.toolbarButton))
        // {
        //     DisbandGroup(_groupedNodes, id);
        // }

        GUIContent icon = storyEngine.Groups[id].IsMinimised ? maximiseButtonContent : minimiseButtonContent;

        //for minimising/maximising the group
        if (GUILayout.Button(icon, toolBarStyle))
        {
            bool isMinimised = storyEngine.Groups[id].IsMinimised;
            storyEngine.Groups[id].IsMinimised = !isMinimised;
        }

        // Check if the object already exists
        var existingGroupObjs = storyEngine.GetComponentsInChildren<NodeCollection>();
        NodeCollection groupObj = null;
        foreach (NodeCollection group in existingGroupObjs)
        {
            //if all nodes in the group obj are equal to the nodes provided here then the group obj already exists
            if (storyEngine.Groups[id].GroupedNodes.All(x => group.Contains(x)) && storyEngine.Groups[id].GroupedNodes.Count == group.Count)
            {
                groupObj = group;
                break;
            }
        }

        if (groupObj)
        {
            //finally create the actual variable
            //first check if the variable already exists in the engine - if it does then we don't need to create a new one and simply update it
            bool varExists = false;
            var colls = storyEngine.GetComponents<NodeCollectionVariable>().ToList().Find(x => _groupedNodes.All(node => x.Value.Contains(node)));
            if (colls != null)
            {
                colls.Apply(SetOperator.Assign, groupObj);
                colls.Scope = VariableScope.Global;
                varExists = true;
            }
            if (!varExists)
            {
                NodeCollectionVariable newGroupVar = new NodeCollectionVariable();
                VariableSelectPopupWindowContent.AddVariable(newGroupVar.GetType(), groupName, groupObj.GetComponent<NodeCollection>());
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        Node targetNode = storyEngine.Groups[id].GetUnlockNode();
        if (targetNode != null)
        {
            //Draw a key icon opposite position of map icon
            Rect rect = new Rect(storyEngine.Groups[id]._NodeRect)
            {
                height = 28,
                width = 28
            };
            rect.x += storyEngine.ScrollPos.x - 30;
            rect.y += storyEngine.ScrollPos.y - rect.height + 25;

            if (LogaEditorResources.KeyIcon != null)
            {
                GUI.DrawTexture(rect, LogaEditorResources.KeyIcon);
            }

            // Draw a line to the target node
            List<Node> connectedNodes = new List<Node>()
            {
                storyEngine.Groups[id],
                targetNode
            };
            int groupCount = storyEngine.Groups[id].SetNodesToComplete ? storyEngine.Groups[id].NodesToComplete.Count : storyEngine.Groups[id].TotalToComplete;
            DrawConnectionsUnlock(storyEngine.Groups[id], connectedNodes, true, groupCount);
        }

        var groupNode = storyEngine.Groups[id];
        //Determine if this node requires another node to be completed before it can be unlocked
        Node unlockNode = groupNode.GetKeyNode();
        if (unlockNode != null)
        {
            Rect rect = new Rect(groupNode._NodeRect);
            rect.height = 28;
            rect.width = 28;
            rect.y += storyEngine.ScrollPos.y - rect.height + 25;
            if (targetNode != null)
            {
                rect.x += storyEngine.ScrollPos.x + groupNode._NodeRect.width - rect.width + 8;
            }
            else
            {
                rect.x += storyEngine.ScrollPos.x - 30;
            }
            if (LogaEditorResources.LockIcon != null)
            {
                GUI.DrawTexture(rect, LogaEditorResources.LockIcon);
            }
        }

        List<Order> locationOrders = new List<Order>();
        groupNode.GetLocationOrders(ref locationOrders);

        bool locationFound = false;

        if (groupNode._EventHandler != null && groupNode._EventHandler.GetType() == typeof(LocationClickEventHandler))
        {
            var handler = groupNode._EventHandler as LocationClickEventHandler;
            locationFound = handler.Location.locationRef != null;
        }

        if (groupNode.NodeLocation != null || locationOrders.Count > 0 || locationFound)
        {
            // Draw a small icon to show that the node has a location in the top right corner of the node
            Rect rect = new Rect(groupNode._NodeRect);
            rect.height = 28;
            rect.width = 28;
            rect.x += storyEngine.ScrollPos.x - 30;
            rect.y += storyEngine.ScrollPos.y - rect.height + 55;

            if (LogaEditorResources.MapIcon != null)
            {
                GUI.DrawTexture(rect, LogaEditorResources.MapIcon);
            }
        }

        List<Order> conditions = new List<Order>();
        groupNode.GetConditionOrders(ref conditions);
        if (conditions.Count > 0)
        {
            Rect rect = new Rect(groupNode._NodeRect);
            rect.height = 28;
            rect.width = 28;
            rect.x += storyEngine.ScrollPos.x - 30;
            rect.y += storyEngine.ScrollPos.y - rect.height + 85;

            if (LogaEditorResources.Diamond != null)
            {
                GUI.DrawTexture(rect, LogaEditorResources.Diamond);
            }
        }

        Rect windowRelativeRect = new Rect(groupNode._NodeRect);
        windowRelativeRect.position += storyEngine.ScrollPos;
        // Draw the node's description if the mouse is over the node
        if (groupNode.ShowDesc && !groupNode.IsSelected && !groupNode.IsControlSelected)
        {
            if (groupNode._Description.Length > 0)
            {
                var content = new GUIContent(groupNode._GroupDescription);
                windowRelativeRect.y += windowRelativeRect.height;
                windowRelativeRect.height = descriptionStyle.CalcHeight(content, windowRelativeRect.width);
                GUI.Label(windowRelativeRect, content, descriptionStyle);
            }
        }
    }

    protected void DisbandGroup(Group group, int id)
    {
        //remove the group from the list of storyEngine.Groups as well as any orders and event handlers
        //var _groupedNodes = group.GroupedNodes;

        if (group.IsMinimised)
        {
            group.IsMinimised = false;
        }

        group.DisbandGroup(storyEngine, id);

        //Group groupComp = storyEngine.GetComponents<Group>().ToList().Find(x => x.GroupedNodes.All(node => _groupedNodes.Any(gn => gn._NodeRect.position == node._NodeRect.position)));
        //var colls = storyEngine.Variables.OfType<NodeCollectionVariable>().ToList().Find(x => _groupedNodes.All(node => x.Value.Contains(node)));

        //if (colls != null)
        //{
        //    //remove the variable from the list of variables
        //    storyEngine.Variables.Remove(colls);
        //    //destroy the variable component
        //    Undo.DestroyObjectImmediate(colls);
        //}

        //if (groupComp != null)
        //{
        //    //remove the event handler from the group
        //    if (groupComp._EventHandler != null)
        //    {
        //        Undo.DestroyObjectImmediate(groupComp._EventHandler);
        //    }
        //    //remove all orders from the group
        //    foreach (var order in groupComp.OrderList)
        //    {
        //        Undo.DestroyObjectImmediate(order);
        //    }
        //    //remove the group component from the groupu
        //    Undo.DestroyObjectImmediate(groupComp);
        //}

        ////find and remove the gameobject in the scene
        //var existingGroupObjs = storyEngine.GetComponentsInChildren<NodeCollection>();
        //foreach (NodeCollection groupColl in existingGroupObjs)
        //{
        //    //if all nodes in the group obj are equal to the nodes provided here then the group obj already exists
        //    if (group.GroupedNodes.All(x => groupColl.Contains(x)))
        //    {
        //        Undo.DestroyObjectImmediate(groupColl.gameObject);
        //        break;
        //    }
        //}
        //storyEngine.groupnames.RemoveAt(id);
        //_groupedNodes.Clear();
        ////remove the group from the list of storyEngine.Groups
        //storyEngine.Groups.Remove(group);
        storyEngine.SelectedNodes.Clear();
        //GameObject.DestroyImmediate(group);
        DeselectAllNodes();
    }

    //ideally done on the group component rather than here
    private string GetGroupName(Group group)
    {
        return group._NodeName;
    }

    private bool CheckDefaultName(string name)
    {
        if (name == "Linked_Group" || name == "Unlinked_Group")
        {
            return true;
        }
        return false;
    }

    // Go through all nodes: if the node has a connection to the provided node or the provided node is creating a conncetion, return true
    //the above method should only return calligraphic if all nodes in the group have a connection or create a connection
    public virtual bool HasConnection(Node _node, List<Node> _nodes = null)
    {
        var nodesWithConnections = new List<Node>();
        foreach (Node node in nodes)
        {
            var orderList = node.OrderList;
            if (orderList.Count > 0)
            {
                foreach (Order order in orderList)
                {
                    order.GetConnectedNodes(ref nodesWithConnections);
                    if (nodesWithConnections.Count > 0 && order.GetType() != typeof(If)) // Check if there are any connected nodes
                    {
                        foreach (var connectedNode in nodesWithConnections)
                        {
                            if (connectedNode == _node && _nodes.Contains(_node)) // Check if the connected node has a connection to the provided node
                            {
                                return true;
                            }
                        }
                        if (node == _node && _nodes.Contains(_node)) // Check if the provided node is creating a connection
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    protected virtual void DrawVariablesBlock(Event e)
    {
        // Variables group
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical(GUILayout.Width(440));
            {
                GUILayout.FlexibleSpace();

                storyEngine.VariablesScrollPos = GUILayout.BeginScrollView(storyEngine.VariablesScrollPos);
                {
                    GUILayout.Space(8);

                    EditorGUI.BeginChangeCheck();

                    if (variableListAdaptor != null)
                    {
                        if (variableListAdaptor.TargetEngine != null)
                        {
                            //440 - space for scrollbar
                            variableListAdaptor.DrawVarList(440);
                        }
                        else
                        {
                            variableListAdaptor = null;
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(storyEngine);
                    }
                }
                GUILayout.EndScrollView();


                // Eat mouse events
                if (e.type == EventType.MouseDown)
                {
                    Rect variableWindowRect = GUILayoutUtility.GetLastRect();
                    if (storyEngine.VariablesExpanded && storyEngine.Variables.Count > 0)
                    {
                        variableWindowRect.y -= 20;
                        variableWindowRect.height += 20;
                    }

                    if (variableWindowRect.Contains(e.mousePosition))
                    {
                        e.Use();
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }

    public Node AddNodeFromLocation(LocationVariable locationVariable)
    {
        DeselectAllNodes();
        Vector2 newNodePosition = new Vector2(
            50 / storyEngine.Zoom - storyEngine.ScrollPos.x, 50 / storyEngine.Zoom - storyEngine.ScrollPos.y
        );
        var node = AddNode(newNodePosition);
        UpdateNodes();
        node.NodeLocation = locationVariable;
        return node;
    }
    protected Node AddNode(Vector2 position)
    {
        Node newNode = storyEngine.CreateNode(position);
        UpdateNodes();
        storyEngine.AddNode(newNode);
        SetNodeForInspector(newNode);
        return newNode;
    }

    protected Node AddNodeWithHandler(Vector2 position, Type handlerType)
    {
        Node newNode = storyEngine.CreateNodeWithHandler(position, handlerType);
        newNode._EventHandler = Undo.AddComponent(storyEngine.gameObject, handlerType) as EventHandler;
        newNode._EventHandler.ParentNode = newNode;
        UpdateNodes();
        storyEngine.AddNode(newNode);
        SetNodeForInspector(newNode);
        return newNode;
    }

    protected Group AddGroup(List<Node> groupNodes, string name, Group existingGroup, BasicFlowEngine engine = null)
    {
        var currentEngine = engine != null ? engine : storyEngine;

        Group newGroup = currentEngine.CreateGroup(groupNodes, existingGroup);
        UpdateNodes();
        currentEngine.AddGroup(newGroup);
        SetGroupForInspector(groupNodes, name, newGroup);
        return newGroup;
    }

    protected Label AddLabel(Vector2 position)
    {
        Label newLabel = storyEngine.CreateLabel(position);
        UpdateAnnotations();
        storyEngine.AddLabel(newLabel);
        return newLabel;
    }

    protected AnnotationBox CreateAnnotateBox(Rect rect)
    {
        AnnotationBox newBox = storyEngine.CreateAnnotationBox(rect);
        UpdateAnnotations();
        return newBox;
    }

    protected void UpdateNodes()
    {
        storyEngine = GetEngine();

        //if (storyEngine == null)
        //    storyEngine = GameObject.FindObjectOfType<BasicFlowEngine>();

        if (storyEngine == null)
        {
            nodes = new Node[0];
        }
        else
        {
            nodes = storyEngine.gameObject.GetComponents<Node>();
        }
    }
    protected void UpdateAnnotations()
    {
        if (storyEngine == null)
            storyEngine = GameObject.FindObjectOfType<BasicFlowEngine>();

        if (storyEngine == null)
        {
            labels = new Label[0];
            annotationLines = new AnnotationLine[0];
            annotationBoxes = new AnnotationBox[0];
        }
        else
        {
            labels = storyEngine.gameObject.GetComponents<Label>();
            annotationLines = storyEngine.gameObject.GetComponents<AnnotationLine>();
            annotationBoxes = storyEngine.gameObject.GetComponents<AnnotationBox>();
        }
    }

    private void EndSelection()
    {
        if (GetAppendModifierDown())
        {
            for (int i = mouseDownSelectState.Count - 1; i >= 0; i--)
            {
                var item = mouseDownSelectState[i];
                if (item.IsSelected)
                {
                    storyEngine.DeselectNodeNoCheck(item);
                    RemoveMouseDownSelectState(item);
                }
                else
                {
                    storyEngine.AddSelectedNode(item);
                }
            }
        }
        else
        {
            for (int i = mouseDownSelectState.Count - 1; i >= 0; i--)
            {
                var item = mouseDownSelectState[i];
                storyEngine.AddSelectedNode(item);
                RemoveMouseDownSelectState(item);
            }
        }
    }

    private void RemoveMouseDownSelectState(Node item)
    {
        mouseDownSelectState.Remove(item);
        item.IsControlSelected = false;
    }

    private void AddMouseDownSelectState(Node item)
    {
        mouseDownSelectState.Add(item);
        item.IsControlSelected = true;
    }

    private void StartControlSelection()
    {
        mouseDownSelectState.AddRange(storyEngine.SelectedNodes);
        storyEngine.ClearSelectedNodes();
        for (int i = 0; i < mouseDownSelectState.Count; i++)
        {
            if (mouseDownSelectState[i] != null)
            {
                mouseDownSelectState[i].IsControlSelected = true;
            }
            else
            {
                Debug.LogWarning("Null node found in mouseDownSelectionState. May be a symptom of an underlying issue :(");
            }
        }
    }

    private void EndControlSelection()
    {
        //we can be called either by mouse up with control still held or because ctrl was released
        if (GetAppendModifierDown())
        {
            //remove items selected from the mouse down and then move the mouse down to the selection
            for (int i = mouseDownSelectState.Count - 1; i >= 0; i--)
            {
                var item = mouseDownSelectState[i];

                if (item.IsSelected)
                {
                    storyEngine.DeselectNodeNoCheck(item);
                    RemoveMouseDownSelectState(item);
                }
                else
                {
                    storyEngine.AddSelectedNode(item);
                }
            }
        }
        else
        {
            //ctrl released moves all back to selection
            for (int i = mouseDownSelectState.Count - 1; i >= 0; i--)
            {
                var item = mouseDownSelectState[i];
                storyEngine.AddSelectedNode(item);
                RemoveMouseDownSelectState(item);
            }
        }
    }

    protected virtual bool GetAppendModifierDown()
    {
        //could be shift or control or perhaps set a custom key in the editor preferences
        return (Event.current != null && Event.current.shift) || EditorGUI.actionKey;
    }

    static protected List<Node> nodeGraphicsUniqueListWorkSpace = new List<Node>();
    static protected List<Node> nodeGraphicsConnectedWorkSpace = new List<Node>();
    protected virtual NodeGraphics GetNodeGraphics(Node node)
    {
        var graphics = new NodeGraphics();

        nodeGraphicsUniqueListWorkSpace.Clear();
        nodeGraphicsConnectedWorkSpace.Clear();
        Color defaultTint;
        if (node._EventHandler != null)
        {
            graphics.offTexture = LogaEditorResources.EventNodeOff;
            graphics.onTexture = LogaEditorResources.EventNodeOn;
            defaultTint = LogaConstants.DefaultEventNodeTint;
        }
        else
        {
            // Count the number of unique connections (excluding self references)
            node.GetConnectedNodes(ref nodeGraphicsConnectedWorkSpace);
            foreach (var connectedNode in nodeGraphicsConnectedWorkSpace)
            {
                if (connectedNode == node ||
                    nodeGraphicsUniqueListWorkSpace.Contains(connectedNode))
                {
                    continue;
                }
                nodeGraphicsUniqueListWorkSpace.Add(connectedNode);
            }

            if (nodeGraphicsUniqueListWorkSpace.Count > 1)
            {
                graphics.offTexture = LogaEditorResources.ChoiceNodeOff;
                graphics.onTexture = LogaEditorResources.ChoiceNodeOn;
                defaultTint = LogaConstants.DefaultChoiceNodeTint;
            }
            else
            {
                graphics.offTexture = LogaEditorResources.ProcessNodeOff;
                graphics.onTexture = LogaEditorResources.ProcessNodeOn;
                defaultTint = LogaConstants.DefaultProcessNodeTint;
            }
        }

        graphics.tint = node.UseCustomTint ? node.Tint : defaultTint;

        return graphics;
    }

    protected Node DrawNodeWindow(int id)
    {
        var graphics = GetNodeGraphics(nodes[id]);

        var node = nodes[id];

        float nodeWidthA = nodeStyle.CalcSize(new GUIContent(node._NodeName)).x + 10;

        Rect tempRect = node._NodeRect;
        tempRect.width = Mathf.Clamp(nodeWidthA, NodeMinWidth, NodeMaxWidth);
        tempRect.height = DefaultNodeHeight;

        node._NodeRect = tempRect;

        Rect windowRelativeRect = new Rect(node._NodeRect);
        windowRelativeRect.position += storyEngine.ScrollPos;

        var tmpNormBg = nodeStyle.normal.background;

        // Draw untinted highlight
        if (node.IsSelected && !node.IsControlSelected)
        {
            GUI.backgroundColor = Color.white;
            nodeStyle.normal.background = graphics.onTexture;
            GUI.Box(windowRelativeRect, "", nodeStyle);
            nodeStyle.normal.background = tmpNormBg;
        }

        if (node.IsControlSelected && !node.IsSelected)
        {
            GUI.backgroundColor = Color.white;
            nodeStyle.normal.background = graphics.onTexture;
            var c = GUI.backgroundColor;
            c.a = 0.5f;
            GUI.backgroundColor = c;
            GUI.Box(windowRelativeRect, "", nodeStyle);
            nodeStyle.normal.background = tmpNormBg;
        }

        // Draw tinted node; ensure text is readable
        var brightness = graphics.tint.r * 0.3 + graphics.tint.g * 0.59 + graphics.tint.b * 0.11;
        var tmpNormTxtCol = nodeStyle.normal.textColor;
        nodeStyle.normal.textColor = brightness >= 0.5 ? Color.black : Color.white;

        int groupIndex = node.GroupIndex;

        //if (node.IsGrouped && groupIndex >= 0 && groupIndex < storyEngine.Groups.Count && !storyEngine.Groups[node.GroupIndex].IsMinimised || !node.IsGrouped)
        //{
        nodeStyle.normal.background = graphics.offTexture;
        GUI.backgroundColor = graphics.tint;
        GUI.Box(windowRelativeRect, node._NodeName, nodeStyle);
        //}

        GUI.backgroundColor = Color.white;

        nodeStyle.normal.textColor = tmpNormTxtCol;
        nodeStyle.normal.background = tmpNormBg;
        if (node._EventHandler != null && storyEngine.ShowHandlerInfoOnGraph)
        {
            string handlerLabel = "";
            EventHandlerInfoAttribute info = EventHandlerEditor.GetEventHandlerInfo(node._EventHandler.GetType());
            if (info != null)
            {
                handlerLabel = "<" + info.EventHandlerName + "> ";
            }

            Rect rect = new Rect(node._NodeRect);
            rect.height = handlerStyle.CalcHeight(new GUIContent(handlerLabel), node._NodeRect.width);
            rect.x += storyEngine.ScrollPos.x;
            rect.y += storyEngine.ScrollPos.y + windowRelativeRect.height;

            GUI.Label(rect, handlerLabel, handlerStyle);
        }
        Node targetNode = node.GetUnlockNode();
        if (targetNode != null)
        {
            //Draw a key icon opposite position of map icon
            Rect rect = new Rect(node._NodeRect)
            {
                height = 28,
                width = 28
            };
            rect.x += storyEngine.ScrollPos.x;
            rect.y += storyEngine.ScrollPos.y - rect.height + 25;

            if (LogaEditorResources.KeyIcon != null)
            {
                GUI.DrawTexture(rect, LogaEditorResources.KeyIcon);
            }

            // Draw a line to the target node
            List<Node> connectedNodes = new List<Node>
            {
                node,
                targetNode
            };
            DrawConnectionsUnlock(node, connectedNodes);
        }

        //Determine if this node requires another node to be completed before it can be unlocked
        Node unlockNode = node.GetKeyNode();
        if (unlockNode != null)
        {
            Rect rect = new Rect(node._NodeRect);
            rect.height = 28;
            rect.width = 28;
            rect.x += storyEngine.ScrollPos.x;
            if (targetNode != null)
            {
                rect.y += storyEngine.ScrollPos.y - rect.height;
            }
            else
            {
                rect.y += storyEngine.ScrollPos.y - rect.height + 25;
            }
            if (LogaEditorResources.LockIcon != null)
            {
                GUI.DrawTexture(rect, LogaEditorResources.LockIcon);
            }
        }

        bool locationFound = false;

        // Determine location on node or any order
        if (node.NodeLocation != null)
        {
            locationFound = true;
        }
        else if (node._EventHandler != null && node._EventHandler.GetType() == typeof(LocationClickEventHandler))
        {
            var handler = node._EventHandler as LocationClickEventHandler;
            locationFound = handler.Location.locationRef != null;
        }

        if (!locationFound)
        {
            List<Order> locationOrders = new List<Order>();
            node.GetLocationOrders(ref locationOrders);
            locationFound = locationOrders.Count > 0;
        }

        if (locationFound)
        {
            if (LogaEditorResources.MapIcon != null)
            {
                // Draw a small icon to show that the node has a location in the top right corner of the node
                Rect rect = new Rect(node._NodeRect);
                rect.height = 28;
                rect.width = 28;
                rect.x += storyEngine.ScrollPos.x + node._NodeRect.width - rect.width + 8;
                rect.y += storyEngine.ScrollPos.y - rect.height + 25;
                GUI.DrawTexture(rect, LogaEditorResources.MapIcon);
            }
        }

        // Draw the logic symbol if any node has IF conditions
        List<Order> conditions = new List<Order>();
        node.GetConditionOrders(ref conditions);
        if (conditions.Count > 0)
        {
            Rect rect = new Rect(node._NodeRect);
            rect.height = 28;
            rect.width = 28;
            rect.x += storyEngine.ScrollPos.x + node._NodeRect.width / 2 - rect.width / 2;
            rect.y += storyEngine.ScrollPos.y - node._NodeRect.height / 2 + 5;

            if (LogaEditorResources.Diamond != null)
            {
                GUI.DrawTexture(rect, LogaEditorResources.Diamond);
            }
        }

        // Draw the node's description if the mouse is over the node
        if (node.ShowDesc)
        {
            if (node._Description.Length > 0)
            {
                var content = new GUIContent(node._Description);
                windowRelativeRect.y += windowRelativeRect.height;
                windowRelativeRect.height = descriptionStyle.CalcHeight(content, windowRelativeRect.width);
                GUI.Label(windowRelativeRect, content, descriptionStyle);
            }
        }

        DrawConnections(node);
        return node;
    }

    protected Label DrawLabel(int id)
    {
        var label = labels[id];

        Rect tempRect = label.LabelRect;
        tempRect.width = Mathf.Clamp(label.LabelRect.width, NodeMinWidth, NodeMaxWidth);
        //Ensuring that the rect of our label is always expanded by the text content so we can drag/click it
        tempRect.height = DefaultNodeHeight + label.LabelStyle.CalcHeight(new GUIContent(label.LabelText), tempRect.width);

        GUIStyle labelStyle = label.LabelStyle;
        label.Tint = storyEngine.LabelTint;
        labelStyle.normal.background = MakeTex(2, 2, label.Tint);
        labelStyle.wordWrap = true;

        label.LabelRect = tempRect;

        Rect windowRelativeRect = new Rect(label.LabelRect);
        float labelHeight = labelStyle.CalcHeight(new GUIContent(label.LabelText), tempRect.width);
        windowRelativeRect.height = labelHeight;
        windowRelativeRect.position += storyEngine.ScrollPos;

        // Ensure text is readable
        var brightness = label.Tint.r * 0.3 + label.Tint.g * 0.59 + label.Tint.b * 0.11;
        var tmpNormTxtCol = nodeStyle.normal.textColor;
        labelStyle.normal.textColor = brightness >= 0.5 ? Color.black : Color.white;

        GUI.Box(windowRelativeRect, label.LabelText, labelStyle);
        EditorGUIUtility.AddCursorRect(windowRelativeRect, MouseCursor.Text);
        label.LabelText = EditorGUI.TextField(windowRelativeRect, label.LabelText, labelStyle);

        return label;
    }


    protected AnnotationLine StartLine(Vector2 start)
    {
        var line = storyEngine.CreateAnnotationLine(start);
        UpdateAnnotations();
        isDrawingAnnotationLine = true;
        activeAnnotationLine = line;
        return line;
    }

    protected AnnotationLine DrawLine(int id)
    {
        var line = annotationLines[id];

        // Draw a line between start and end rects
        Handles.BeginGUI();
        Rect start = line.Start;
        Rect end = line.End;
        start.position += storyEngine.ScrollPos;
        end.position += storyEngine.ScrollPos;
        DrawRectConnection(start, end, false, false, false, null, true, storyEngine.LabelTint);
        Handles.EndGUI();
        return line;
    }

    protected AnnotationBox DrawAnnotationBox(int id)
    {
        var box = annotationBoxes[id];

        // Create a GUIStyle for the box with a dotted border
        Texture2D border = LogaEditorResources.DottedBox;
        GUIStyle boxStyle = new GUIStyle();
        boxStyle.normal.background = border; // Assign the background texture to the GUIStyle
        boxStyle.border = new RectOffset(border.width / 2, border.width / 2, border.height / 2, border.height / 2); // Set the border of the GUIStyle to match the texture

        // Draw the box
        var rect = box.Box;
        rect.position += storyEngine.ScrollPos;
        GUI.Box(rect, "", boxStyle);

        return box;
    }

    // Helper method to create a texture of solid color
    private Texture2D _MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    /// Displays a temporary text alert in the center of the window (uses ShowNotification from EditorWindow)
    public static void ShowNotification(string text)
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(GraphWindow), false, "Flow Engine");
        if (window != null)
        {
            window.ShowNotification(new GUIContent(text));
        }
    }

    protected static void ShowNodeInspector(Node node)
    {
        if (nodeInspector == null)
        {
            // Create a Scriptable Object with a custom editor which we can use to inspect the selected node.
            // Editors for Scriptable Objects display using the full height of the inspector window.
            nodeInspector =
                ScriptableObject.CreateInstance<NodeInspectorWindow>() as NodeInspectorWindow;
            nodeInspector.hideFlags = HideFlags.DontSave;
        }

        if (node != null)
            nodeInspector.node = node;


        Selection.activeObject = nodeInspector;

        EditorUtility.SetDirty(nodeInspector);
    }

    public static void SetNodeForInspector(Node node)
    {
        ShowNodeInspector(node);
        storyEngine.ClearSelectedOrders();
        if (node.ActiveOrder != null)
            storyEngine.AddSelectedOrder(node.ActiveOrder);
    }

    protected void ShowGroupInspector(List<Node> groupedNodes, string name, Group group)
    {
        if (groupInspector == null)
        {
            groupInspector = ScriptableObject.CreateInstance<GroupInspector>() as GroupInspector;
            groupInspector.hideFlags = HideFlags.DontSave;
        }

        if (groupedNodes != null && groupedNodes.Count > 0)
        {
            groupInspector.groupedNodes = groupedNodes;
        }

        if (name != null)
        {
            groupInspector.groupName = name;
        }

        groupInspector.graphWindow = this;

        if (group != null)
            groupInspector.group = group;

        Selection.activeObject = groupInspector;
        EditorUtility.SetDirty(groupInspector);
    }

    protected void SetGroupForInspector(List<Node> groupNodes, string name, Group group)
    {
        ShowGroupInspector(groupNodes, name, group);
    }

    protected virtual void CopyNode()
    {
        copyList.Clear();

        foreach (var node in storyEngine.SelectedNodes.Union(mouseDownSelectState))
        {
            copyList.Add(new NodeCopy(node));
        }
    }

    protected virtual void CutNode()
    {
        CopyNode();
        Undo.RecordObject(storyEngine, "Cut");

        AddToDeleteList(storyEngine.SelectedNodes);
    }

    protected virtual void DuplicateNode()
    {
        var tempCopyList = new List<NodeCopy>(copyList);
        CopyNode();
        PasteNode(new Vector2(20, 0), true);
        copyList = tempCopyList;
    }

    //Centre is position in unscaled window space (for pasting nodes)
    protected virtual void PasteNode(Vector2 centre, bool relative = false, bool bluePrint = false, BasicFlowEngine engine = null)
    {
        Undo.RecordObject(storyEngine, "Deselect");
        //deslect all nodes when implemented with rect selection
        DeselectAllNodes();

        var pasteList = new List<Node>();

        foreach (var nodeCopy in copyList)
        {
            pasteList.Add(nodeCopy.PasteNode(this, storyEngine, bluePrint));
        }

        var copiedCentre = GetNodeCentre(pasteList.ToArray()) + storyEngine.ScrollPos;
        var delta = relative ? centre : (centre / storyEngine.Zoom - copiedCentre);

        foreach (var node in pasteList)
        {
            var tempRect = node._NodeRect;
            tempRect.position += delta;
            node._NodeRect = tempRect;
        }

        UpdateNodes();
    }

    protected override void OnValidateOrder(Event e)
    {
        if (e.type == EventType.ValidateCommand)
        {
            var c = e.commandName;
            if (c == "Copy" || c == "Cut" || c == "SoftDelete" || c == "Delete" || c == "Duplicate")
            {
                if (storyEngine.SelectedNodes.Count > 0 || mouseDownSelectionState.Count > 0)
                {
                    e.Use();
                }
            }
            else if (c == "Paste")
            {
                if (copyList.Count > 0)
                {
                    e.Use();
                }
            }
            else if (c == "SelectAll" || c == "Find")
            {
                e.Use();
            }
        }
    }

    protected override void OnExecuteOrder(Event e)
    {
        switch (e.commandName)
        {
            case "Copy":
                CopyNode();
                e.Use();
                break;

            case "Cut":
                CutNode();
                e.Use();
                break;

            case "Paste":
                PasteNode(position.center - position.position);
                e.Use();
                break;

            case "Delete":
                AddToDeleteList(storyEngine.SelectedNodes);
                e.Use();
                break;

            case "SoftDelete":
                AddToDeleteList(storyEngine.SelectedNodes);
                e.Use();
                break;

            case "Duplicate":
                DuplicateNode();
                e.Use();
                break;

            case "SelectAll":
                Undo.RecordObject(storyEngine, "Selection");
                storyEngine.ClearSelectedNodes();
                for (int i = 0; i < nodes.Length; ++i)
                {
                    storyEngine.AddSelectedNode(nodes[i]);
                }
                e.Use();
                break;

                //case "Find":
                //    selectionBoxStartPos = 0;
                //    popupScroll = Vector2.zero;
                //    EditorGUI.FocusTextInControl(SearchFieldName);
                //    e.Use();
                //    break;
        }
    }

    public Vector2 GetNodeCentre(Node[] nodes)
    {
        if (nodes.Length == 0)
            return Vector2.zero;

        Vector2 min = nodes[0]._NodeRect.min;
        Vector2 max = nodes[0]._NodeRect.max;

        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            min.x = Mathf.Min(min.x, node._NodeRect.center.x);
            min.y = Mathf.Min(min.y, node._NodeRect.center.y);
            max.x = Mathf.Max(max.x, node._NodeRect.center.x);
            max.y = Mathf.Max(max.y, node._NodeRect.center.y);
        }

        return (min + max) * 0.5f;
    }


    protected virtual void CenterNode(Node node)
    {
        if (storyEngine.Zoom < 1)
        {
            DoZoom(1 - storyEngine.Zoom, Vector2.one * 0.5f);
        }

        storyEngine.ScrollPos = -node._NodeRect.center + position.size * 0.5f / storyEngine.Zoom;
    }

    protected void CentreWindow()
    {
        UpdateNodes();

        if (nodes.Length > 0)
        {
            var centre = -GetNodeCentre(nodes);
            // var centre = Vector2.zero;
            // Vector2 pos = new Vector2(-50000 / 2, -50000 / 2);

            // centre.x = (position.width * 0.5f / zoom);
            // centre.y = (position.height * 0.5f / zoom);

            centre.x += position.width * 0.5f / storyEngine.Zoom;
            centre.y += position.height * 0.5f / storyEngine.Zoom;

            // DoZoom(1 - zoom, Vector2.one * 0.5f);
            // scrollPos = pos;

            storyEngine.CenterPosition = centre;
            storyEngine.ScrollPos = storyEngine.CenterPosition;
        }
    }

    public void AddToDeleteList(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            GraphWindow.deleteList.Add(nodes[i]);
        }
    }

    internal void StopThisNode(Node node)
    {
        node.Stop();
    }

    internal void StopAllNodes()
    {
        storyEngine.StopAllNodes();
    }

    internal void ExecuteThisNode(Node node, bool stopRunningNodes)
    {
        if (stopRunningNodes)
            StopAllNodes();

        node.StartExecution();
    }

    public void DeleteNodes()
    {
        //delete any nodes that have been scheduled to be deleted
        for (int i = 0; i < deleteList.Count; i++)
        {
            var deleteNode = deleteList[i];
            var orderList = deleteNode.OrderList;
            //remove all orders from given node
            for (int j = 0; j < orderList.Count; j++)
            {
                Undo.DestroyObjectImmediate(orderList[j]);
            }

            //remove event handler from given node
            if (deleteNode._EventHandler != null)
            {
                if (deleteNode._EventHandler.GetType() == typeof(ConditionalEventHandler))
                {
                    var conditionalHandler = deleteNode._EventHandler as ConditionalEventHandler;
                    if (conditionalHandler != null)
                    {
                        foreach (var condition in conditionalHandler.Conditions)
                        {
                            if (condition != null)
                            {
                                Undo.DestroyObjectImmediate(condition);
                            }
                        }
                    }
                }
                Undo.DestroyObjectImmediate(deleteNode._EventHandler);
            }

            if (deleteNode.IsSelected)
            {
                //deselect the deleted node
                storyEngine.DeselectNodeNoCheck(deleteNode);
            }

            //finally destroy the node
            if (deleteNode.IsGrouped)
            {
                storyEngine.Groups[deleteNode.GroupIndex].GroupedNodes.Remove(deleteNode);
                deleteNode.IsGrouped = false;
                deleteNode.GroupIndex = -1;
            }
            Undo.DestroyObjectImmediate(deleteNode);
        }

        if (deleteList.Count > 0)
        {
            UpdateNodes();
            //show the properties of the engine in the inspector
            Selection.activeGameObject = storyEngine.gameObject;
            storyEngine.ClearSelectedOrders();
            Repaint();
        }

        //go through all storyEngine.Groups to determine if the group should be disbanded
        for (int i = storyEngine.Groups.Count - 1; i >= 0; i--)
        {
            var group = storyEngine.Groups[i];
            if (group != null)
            {
                if (group.GroupedNodes.Count <= 1)
                {
                    storyEngine.Groups.Remove(group);
                    if (i < storyEngine.Groups.Count && storyEngine.Groups[i] != null)
                    {
                        storyEngine.Groups.RemoveAt(i);
                    }
                }
            }
        }

        deleteList.Clear();
    }

    public void DeleteLabel(Label label)
    {
        Undo.DestroyObjectImmediate(label);
        labels.ToList().Remove(label);
        UpdateAnnotations();
    }
    public void DeleteAnnotationLine(AnnotationLine line)
    {
        Undo.DestroyObjectImmediate(line);
        annotationLines.ToList().Remove(line);
        UpdateAnnotations();
    }

    public void DeleteAnnotationBox(AnnotationBox box)
    {
        Undo.DestroyObjectImmediate(box);
        annotationBoxes.ToList().Remove(box);
        UpdateAnnotations();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    protected virtual void DrawGrid(Event e)
    {
        float width = this.position.width / storyEngine.Zoom;
        float height = this.position.height / storyEngine.Zoom;

        Handles.color = Color.black;

        float x = storyEngine.ScrollPos.x % gridLineSpacingSize;
        while (x < width)
        {
            Handles.DrawLine(new Vector2(x, 0), new Vector2(x, height));
            x += gridLineSpacingSize;
        }

        float y = (storyEngine.ScrollPos.y % gridLineSpacingSize);
        while (y < height)
        {
            if (y >= 0)
            {
                Handles.DrawLine(new Vector2(0, y), new Vector2(width, y));
            }
            y += gridLineSpacingSize;
        }
        Handles.color = Color.white;
    }

    protected List<Node> connectedNodes = new List<Node>();

    protected virtual void DrawConnectionsUnlock(Node node, List<Node> unlockNodes = null, bool isGroup = false, int? groupCount = null)
    {
        if (node == null)
        {
            return;
        }

        Rect scriptViewRect = CalcFlowchartWindowViewRect();

        foreach (var _node in unlockNodes)
        {
            if (_node == null || node == _node || !_node.GetEngine().Equals(storyEngine))
            {
                continue;
            }

            Rect startRect = new Rect(node._NodeRect);
            startRect.x += storyEngine.ScrollPos.x;
            startRect.y += storyEngine.ScrollPos.y;

            Rect endRect = new Rect(_node._NodeRect);

            endRect.x += storyEngine.ScrollPos.x;
            endRect.y += storyEngine.ScrollPos.y;

            Rect boundRect = new Rect();
            boundRect.xMin = Mathf.Min(startRect.xMin, endRect.xMin);
            boundRect.xMax = Mathf.Max(startRect.xMax, endRect.xMax);
            boundRect.yMin = Mathf.Min(startRect.yMin, endRect.yMin);
            boundRect.yMax = Mathf.Max(startRect.yMax, endRect.yMax);

            if (boundRect.Overlaps(scriptViewRect))
                DrawRectConnection(startRect, endRect, false, true, isGroup, groupCount);
        }
    }

    protected virtual void DrawConnections(Node node, bool drawDottedLine = false, List<Node> unlockNodes = null)
    {
        if (node == null)
        {
            return;
        }

        bool nodeIsSelected = storyEngine.SelectedNode == node;
        bool highlight = false;

        Rect scriptViewRect = CalcFlowchartWindowViewRect();

        var orderList = node.OrderList;
        foreach (var order in orderList)
        {
            if (order == null)
            {
                continue;
            }

            bool orderIsSelected = false;
            var selectedOrders = storyEngine.SelectedOrders;
            foreach (var selectedOrder in selectedOrders)
            {
                if (selectedOrder == order)
                {
                    orderIsSelected = true;
                    break;
                }
            }

            highlight = order.IsExecuting || (nodeIsSelected && orderIsSelected);

            connectedNodes.Clear();
            if (!drawDottedLine)
                order.GetConnectedNodes(ref connectedNodes);
            else if (unlockNodes != null && unlockNodes.Count > 1)
                connectedNodes.AddRange(unlockNodes);

            foreach (var _node in connectedNodes)
            {
                if (_node == null || node == _node || !_node.GetEngine().Equals(storyEngine))
                {
                    continue;
                }

                Rect startRect = new Rect(node._NodeRect);
                startRect.x += storyEngine.ScrollPos.x;
                startRect.y += storyEngine.ScrollPos.y;

                Rect endRect = new Rect(_node._NodeRect);

                endRect.x += storyEngine.ScrollPos.x;
                endRect.y += storyEngine.ScrollPos.y;

                Rect boundRect = new Rect();
                boundRect.xMin = Mathf.Min(startRect.xMin, endRect.xMin);
                boundRect.xMax = Mathf.Max(startRect.xMax, endRect.xMax);
                boundRect.yMin = Mathf.Min(startRect.yMin, endRect.yMin);
                boundRect.yMax = Mathf.Max(startRect.yMax, endRect.yMax);

                if (boundRect.Overlaps(scriptViewRect))
                    DrawRectConnection(startRect, endRect, highlight, drawDottedLine);
            }
        }
    }

    static readonly Vector2[] pointsA = new Vector2[4];
    static readonly Vector2[] pointsB = new Vector2[4];

    //we only connect mids on sides to matching opposing middle side on other node
    private struct IndexPair { public int a, b; public IndexPair(int a, int b) { this.a = a; this.b = b; } }
    static readonly IndexPair[] closestCornerIndexPairs = new IndexPair[]
    {
            new IndexPair(){a=0,b=3 },
            new IndexPair(){a=3,b=0 },
            new IndexPair(){a=1,b=2 },
            new IndexPair(){a=2,b=1 },
    };

    //prevent alloc in DrawAAConvexPolygon
    static readonly Vector3[] beizerWorkSpace = new Vector3[3];

    protected readonly Color connectionColour = new Color(0.1f, 0.85f, 0.85f, 1.0f);

    protected virtual void DrawRectConnection(Rect rectA, Rect rectB, bool highlight, bool drawDottedLine = false, bool isGroup = false, int? groupCount = null, bool customColour = false, Color? colour = null)
    {
        pointsA[0] = new Vector2(rectA.xMin, rectA.center.y);
        pointsA[1] = new Vector2(rectA.xMin + rectA.width / 2, rectA.yMin);
        pointsA[2] = new Vector2(rectA.xMin + rectA.width / 2, rectA.yMax);
        pointsA[3] = new Vector2(rectA.xMax, rectA.center.y);

        pointsB[0] = new Vector2(rectB.xMin, rectB.center.y);
        pointsB[1] = new Vector2(rectB.xMin + rectB.width / 2, rectB.yMin);
        pointsB[2] = new Vector2(rectB.xMin + rectB.width / 2, rectB.yMax);
        pointsB[3] = new Vector2(rectB.xMax, rectB.center.y);


        Vector2 pointA = Vector2.zero;
        Vector2 pointB = Vector2.zero;
        float minDist = float.MaxValue;

        //  we only check mathcing opposing mids
        for (int i = 0; i < closestCornerIndexPairs.Length; i++)
        {
            var a = pointsA[closestCornerIndexPairs[i].a];
            var b = pointsB[closestCornerIndexPairs[i].b];
            float d = Vector2.Distance(a, b);
            if (d < minDist)
            {
                pointA = a;
                pointB = b;
                minDist = d;
            }
        }

        Color color = connectionColour;

        if (highlight)
        {
            color = Color.green;
        }

        if (!customColour && colour != null)
            Handles.color = color;
        else
        {
            Handles.color = colour ?? connectionColour;
            color = colour ?? connectionColour;
        }

        // Place control based on distance between points
        // Weight the min component more so things don't get overly curvy
        var diff = pointA - pointB;
        diff.x = Mathf.Abs(diff.x);
        diff.y = Mathf.Abs(diff.y);
        var min = Mathf.Min(diff.x, diff.y);
        var max = Mathf.Max(diff.x, diff.y);
        var mod = min * 0.75f + max * 0.25f;

        // Draw bezier curve connecting blocks
        var directionA = (rectA.center - pointA).normalized;
        var directionB = (rectB.center - pointB).normalized;
        var controlA = pointA - directionA * mod * 0.67f;
        var controlB = pointB - directionB * mod * 0.67f;

        if (!drawDottedLine)
            Handles.DrawBezier(pointA, pointB, controlA, controlB, color, null, 3f);
        else
            DrawDottedCurve(pointA, pointB, controlA, controlB, Color.white, 3f, 12f, 10f);

        // Draw arrow on curve
        var point = GetPointOnCurve(pointA, controlA, pointB, controlB, 0.7f);
        var direction = (GetPointOnCurve(pointA, controlA, pointB, controlB, 0.6f) - point).normalized;
        var perp = new Vector2(direction.y, -direction.x);
        //reuse same array to avoid the auto alloced one in DrawAAConvexPolygon
        beizerWorkSpace[0] = point;
        beizerWorkSpace[1] = point + direction * 10 + perp * 7;
        beizerWorkSpace[2] = point + direction * 10 - perp * 7;
        Vector3 boxPosition = beizerWorkSpace[0] - new Vector3(50f, 0f, 0f);
        float boxSize = 50f;

        if (isGroup && groupCount != null)
        {
            Handles.color = Color.white;
            Vector3[] diamondPoints = new Vector3[4];
            diamondPoints[0] = boxPosition + new Vector3(boxSize / 2f, 0f, 0f);
            diamondPoints[1] = boxPosition + new Vector3(0f, boxSize / 2f, 0f);
            diamondPoints[2] = boxPosition + new Vector3(-boxSize / 2f, 0f, 0f);
            diamondPoints[3] = boxPosition + new Vector3(0f, -boxSize / 2f, 0f);

            Handles.DrawAAConvexPolygon(diamondPoints);
            var textBoxSize = new Vector2(100f, 20f);
            var textBoxRect = new Rect(boxPosition - (Vector3)(textBoxSize / 2f), textBoxSize);
            var textSize = GUI.skin.textField.CalcSize(new GUIContent("Text"));
            textBoxRect.width = Mathf.Max(textBoxRect.width, textSize.x);
            textBoxRect.height = Mathf.Max(textBoxRect.height, textSize.y);

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.black;

            GUI.Label(textBoxRect, groupCount.ToString(), style);
        }

        if (drawDottedLine)
        {
            Handles.color = Color.green;
        }
        Handles.DrawAAConvexPolygon(beizerWorkSpace);

        var connectionPointA = pointA + directionA * 4f;
        var connectionRectA = new Rect(connectionPointA.x - 4f, connectionPointA.y - 4f, 8f, 8f);
        var connectionPointB = pointB + directionB * 4f;
        var connectionRectB = new Rect(connectionPointB.x - 4f, connectionPointB.y - 4f, 8f, 8f);

        GUI.DrawTexture(connectionRectA, connectionPointTexture, ScaleMode.ScaleToFit);
        GUI.DrawTexture(connectionRectB, connectionPointTexture, ScaleMode.ScaleToFit);

        Handles.color = Color.white;
    }

    void DrawDottedCurve(Vector3 pointA, Vector3 pointB, Vector3 controlA, Vector3 controlB, Color color, float thickness, float segmentLength, float gapLength)
    {
        float t = 0f;
        for (t = 0f; t < 1f; t += segmentLength / Vector3.Distance(pointA, pointB))
        {
            float nextT = Mathf.Min(t + segmentLength / Vector3.Distance(pointA, pointB), 1f);
            Vector3 nextPoint = BezierCurve(pointA, pointB, controlA, controlB, nextT);
            DrawDottedLine(pointA, nextPoint, color, thickness, gapLength, segmentLength);
            pointA = nextPoint;
        }
    }

    Vector3 BezierCurve(Vector3 pointA, Vector3 pointB, Vector3 controlA, Vector3 controlB, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * pointA;
        p += 3 * uu * t * controlA;
        p += 3 * u * tt * controlB;
        p += ttt * pointB;

        return p;
    }

    void DrawDottedLine(Vector3 start, Vector3 end, Color color, float thickness, float gapLength, float segmentLength)
    {
        Handles.color = color;
        float totalDistance = Vector3.Distance(start, end);
        Vector3 direction = (end - start).normalized;
        float drawnLength = 0f;

        for (drawnLength = 0f; drawnLength <= totalDistance; drawnLength += gapLength + segmentLength)
        {
            Vector3 segmentStart = start + direction * drawnLength;
            float segmentEndLength = Mathf.Min(gapLength, totalDistance - drawnLength);
            Vector3 segmentEnd = segmentStart + direction * segmentEndLength;
            Handles.DrawAAPolyLine(thickness, segmentStart, segmentEnd);
        }
    }

    private static Vector2 GetPointOnCurve(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
    {
        float rt = 1 - t;
        float rtt = rt * t;
        return rt * rt * rt * s + 3 * rt * rtt * st + 3 * rtt * t * et + t * t * t * e;
    }

    private Rect CalcFlowchartWindowViewRect()
    {
        return new Rect(0, 0, this.position.width / storyEngine.Zoom, this.position.height / storyEngine.Zoom);
    }

    protected virtual void OnInspectorUpdate()
    {
        if (HandleEngineSelectionChange()) return;

        var engine = GetEngine();
        if (engine == null || AnyNullNodes())
        {
            UpdateNodes();
            Repaint();
            return;
        }
        if (Selection.activeGameObject == null && engine.SelectedNode != null)
        {
            if (nodeInspector == null)
            {
                ShowNodeInspector(null);
            }
            nodeInspector.node = (Node)engine.SelectedNode;
        }
        if (forceRepaintCount != 0)
        {
            forceRepaintCount--;
            forceRepaintCount = Math.Max(0, forceRepaintCount);

            Repaint();
        }
    }

    private bool AnyNullNodes()
    {
        if (nodes == null)
            return true;

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == null)
                return true;
        }

        return false;
    }

    protected virtual void OnBecameVisible()
    {
        // Ensure that toolbar looks correct in both docked and undocked windows
        // The docked value doesn't always report correctly without the delayCall
        EditorApplication.delayCall += () =>
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var isDockedMethod = typeof(EditorWindow).GetProperty("docked", flags).GetGetMethod(true);
            if ((bool)isDockedMethod.Invoke(this, null))
            {
                EditorZoomArea.Offset = new Vector2(2.0f, 19.0f);
            }
            else
            {
                EditorZoomArea.Offset = new Vector2(0.0f, 22.0f);
            }
        };
    }

    public static BasicFlowEngine GetEngine()
    {
        // Using a temp hidden object to track the active Flowchart across 
        // serialization / deserialization when playing the game in the editor
        if (logaStates == null)
        {

            logaStates = GameObject.FindObjectOfType<LogaStates>();
            if (logaStates == null)
            {
                GameObject go = new GameObject("_LogaStates");
                go.hideFlags = HideFlags.HideInHierarchy;
                logaStates = go.AddComponent<LogaStates>();
            }
        }

        if (Selection.activeGameObject != null)
        {
            var ls = Selection.activeGameObject.GetComponent<BasicFlowEngine>();
            if (ls != null)
                logaStates.SelectedEngine = ls;
        }

        if (logaStates.SelectedEngine == null)
        {
            variableListAdaptor = null;
        }
        else if (variableListAdaptor == null || variableListAdaptor.TargetEngine != logaStates.SelectedEngine)
        {
            var lsSO = new SerializedObject(logaStates.SelectedEngine);
            variableListAdaptor = new VariableListAdaptor(lsSO.FindProperty("variables"), logaStates.SelectedEngine);
        }

        return logaStates.SelectedEngine;
    }

    protected Node GetNodeAtPoint(Vector2 point)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            var rect = node._NodeRect;
            rect.position += storyEngine.ScrollPos;

            if (rect.Contains(point / storyEngine.Zoom) && node is not Group)
            {
                int groupIndex = node.GroupIndex;
                if (node.IsGrouped && groupIndex >= 0 && groupIndex < storyEngine.Groups.Count && !storyEngine.Groups[node.GroupIndex].IsMinimised || !node.IsGrouped)
                    return node;
            }
        }

        return null;
    }

    protected Group GetGroupAtPoint(Vector2 point)
    {
        var groups = new List<Group>();
        for (int i = 0; i < storyEngine.Groups.Count; i++)
        {
            if (storyEngine.Groups[i] == null)
                continue;
            if (storyEngine.Groups[i].GroupedNodes == null)
                continue;

            var group = storyEngine.Groups[i];
            var rect = group._NodeRect;
            rect.position += storyEngine.ScrollPos;

            if (rect.Contains(point / storyEngine.Zoom) && group != null)
            {
                groups.Add(group);
            }
        }

        if (groups.Count > 1)
            return groups[groups.Count - 1];
        else if (groups.Count > 0)
            return groups[0];
        else
            return null;
    }

    protected Label GetLabelAtPoint(Vector2 point)
    {
        for (int i = 0; i < labels.Length; i++)
        {
            var label = labels[i];
            var rect = label.LabelRect;
            rect.position += storyEngine.ScrollPos;

            if (rect.Contains(point / storyEngine.Zoom))
            {
                return label;
            }
        }

        return null;
    }

    protected AnnotationLine GetLineAtPoint(Vector2 point)
    {
        for (int i = 0; i < annotationLines.Length; i++)
        {
            var line = annotationLines[i];
            var start = line.Start;
            var end = line.End;

            start.position += storyEngine.ScrollPos;
            end.position += storyEngine.ScrollPos;

            if (start.Contains(point / storyEngine.Zoom))
            {
                line.StartSelected = true;
                return line;
            }
            else if (end.Contains(point / storyEngine.Zoom))
            {
                line.StartSelected = false;
                return line;
            }
        }

        return null;
    }

    protected AnnotationBox GetAnnotatedBoxAtPoint(Vector2 point)
    {
        for (int i = 0; i < annotationBoxes.Length; i++)
        {
            var box = annotationBoxes[i];
            var rect = box.Box;
            rect.position += storyEngine.ScrollPos;

            if (rect.Contains(point / storyEngine.Zoom))
            {
                return box;
            }
        }

        return null;
    }

    protected override void OnMouseDown(Event e)
    {
        var hitNode = GetNodeAtPoint(e.mousePosition);
        var hitGroup = GetGroupAtPoint(e.mousePosition);
        //var hitLabel = GetLabelAtPoint(e.mousePosition);
        //var hitLine = GetLineAtPoint(e.mousePosition);
        //var hitBox = GetAnnotatedBoxAtPoint(e.mousePosition);

        // Convert Ctrl+Left click to a right click on macOS
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            if (e.button == MouseButton.Left && e.control)
            {
                e.button = MouseButton.Right;
            }
        }

        switch (e.button)
        {
            case MouseButton.Left:
                if (!e.alt)
                {
                    if (hitNode != null)
                    {
                        if (e.clickCount == 2)
                        {
                            CenterNode(hitNode);
                            e.Use();
                            didDoubleClick = true;
                            return;
                        }
                        startDragPosition = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos;

                        if (GetAppendModifierDown())
                        {
                            //ctrl clicking blocks toggles between
                            if (mouseDownSelectState.Contains(hitNode))
                            {
                                RemoveMouseDownSelectState(hitNode);
                            }
                            else
                            {
                                AddMouseDownSelectState(hitNode);
                            }
                        }
                        else
                        {
                            if (storyEngine.SelectedNodes.Contains(hitNode))
                            {
                                SetNodeForInspector(hitNode);
                            }
                            else
                            {
                                SelectNode(hitNode);
                            }
                            dragNode = hitNode;
                            hasDraggedSelected = false;
                        }

                        e.Use();
                        GUIUtility.keyboardControl = 0; // Fix for textarea not refeshing (change focus)
                    }
                    else if (hitGroup != null)
                    {
                        //for now we will only be able to drag single storyEngine.Groups but implement multiple later
                        string groupName = string.Empty;
                        int groupIndex = storyEngine.Groups.IndexOf(hitGroup);
                        if (groupIndex >= 0 && groupIndex < storyEngine.Groups.Count && storyEngine.Groups[groupIndex].GroupedNodes.Count > 0)
                        {
                            groupName = GetGroupName(storyEngine.Groups[groupIndex]);
                            var group = storyEngine.Groups[groupIndex];
                            Group groupComp = storyEngine.GetComponents<Group>().ToList().Find(x => x.GroupedNodes.All(node => group.GroupedNodes.Any(gn => gn._NodeRect.position == node._NodeRect.position)));
                            SetGroupForInspector(group.GroupedNodes, groupName, groupComp);
                        }

                        startDragPosition = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos;
                        dragGroup = hitGroup;
                        hasDraggedSelected = false;
                    }
                    //else if (hitLabel != null)
                    //{
                    //    startDragPosition = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos;
                    //    dragLabel = hitLabel;
                    //    hasDraggedSelected = false;
                    //}
                    //else if (hitLine != null)
                    //{
                    //    startDragPosition = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos;
                    //    dragLine = hitLine;
                    //    hasDraggedSelected = false;
                    //}
                    //else if (hitBox != null)
                    //{
                    //    startDragPosition = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos;
                    //    dragBox = hitBox;
                    //    hasDraggedSelected = false;
                    //}
                    else if (
                        !(
                            UnityEditor.Tools.current == Tool.View
                            && UnityEditor.Tools.viewTool == ViewTool.Zoom
                        )
                    )
                    {
                        selectionBoxStartPos = e.mousePosition;
                        selectionBox = Rect.MinMaxRect(
                            selectionBox.x,
                            selectionBox.y,
                            selectionBox.x,
                            selectionBox.y
                        );
                        e.Use();
                    }
                }
                break;
            case MouseButton.Right:
                {
                    rightClickDown = e.mousePosition;
                    e.Use();
                    break;
                }
        }
    }

    protected override void OnMouseDrag(Event e)
    {
        var draggingWindow = false;
        var hitGroup = GetGroupAtPoint(e.mousePosition);
        switch (e.button)
        {
            case MouseButton.Left:
                // node dragging rather than using built in drag
                if (dragNode != null)
                {
                    for (int i = 0; i < storyEngine.SelectedNodes.Count; i++)
                    {
                        var node = storyEngine.SelectedNodes[i];
                        var tempRect = node._NodeRect;
                        tempRect.position += e.delta / storyEngine.Zoom;
                        node._NodeRect = tempRect;
                    }

                    hasDraggedSelected = true;
                    e.Use();
                }
                else if (dragGroup != null)
                {
                    int index = storyEngine.Groups.IndexOf(hitGroup);
                    if (hitGroup != null && index >= 0 && index < storyEngine.Groups.Count)
                    {
                        var group = storyEngine.Groups[index];
                        foreach (var node in group.GroupedNodes)
                        {
                            var tempRect = node._NodeRect;
                            tempRect.position += e.delta / storyEngine.Zoom;
                            node._NodeRect = tempRect;
                        }

                        hasDraggedSelected = true;
                        e.Use();
                    }
                }
                else if (dragLabel != null)
                {
                    var tempRect = dragLabel.LabelRect;
                    tempRect.position += e.delta / storyEngine.Zoom;
                    dragLabel.LabelRect = tempRect;
                    e.Use();
                }
                else if (dragLine != null)
                {
                    var tempPosition = dragLine.StartSelected ? dragLine.Start : dragLine.End;
                    tempPosition.position += e.delta / storyEngine.Zoom;
                    if (dragLine.StartSelected)
                        dragLine.Start = tempPosition;
                    else
                        dragLine.End = tempPosition;
                    e.Use();
                }
                else if (dragBox != null)
                {
                    var tempPosition = dragBox.Box;
                    tempPosition.position += e.delta / storyEngine.Zoom;
                    dragBox.Box = tempPosition;
                    e.Use();
                }
                //pan tool 
                else if (
                    UnityEditor.Tools.current == Tool.View
                        && UnityEditor.Tools.viewTool == ViewTool.Pan || e.alt
                )
                {
                    draggingWindow = true;
                }
                else if (
                    UnityEditor.Tools.current == Tool.View
                    && UnityEditor.Tools.viewTool == ViewTool.Zoom
                )
                {
                    DoZoom(-e.delta.y * 0.01f, Vector2.one * 0.5f);
                    e.Use();
                }
                //selection box
                else if (selectionBoxStartPos.x >= 0 && selectionBoxStartPos.y >= 0)
                {
                    if (Mathf.Approximately(e.delta.magnitude, 0))
                        break;

                    var topLeft = Vector2.Min(selectionBoxStartPos, e.mousePosition);
                    var bottomRight = Vector2.Max(selectionBoxStartPos, e.mousePosition);
                    selectionBox = Rect.MinMaxRect(
                        topLeft.x,
                        topLeft.y,
                        bottomRight.x,
                        bottomRight.y
                    );

                    //accomodate for zooming
                    Rect zoomSelectionBox = selectionBox;
                    zoomSelectionBox.position -= storyEngine.ScrollPos * storyEngine.Zoom;
                    zoomSelectionBox.position /= storyEngine.Zoom;
                    zoomSelectionBox.size /= storyEngine.Zoom;

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = nodes[i];
                        if (node != null && !isDrawingAnnotationBox)
                        {
                            int groupIndex = node.GroupIndex;
                            if (node.IsGrouped && groupIndex >= 0 && groupIndex < storyEngine.Groups.Count && storyEngine.Groups[groupIndex] != null && !storyEngine.Groups[node.GroupIndex].IsMinimised || !node.IsGrouped)
                            {
                                bool doesOverlap = zoomSelectionBox.Overlaps(node._NodeRect);
                                if (doesOverlap)
                                {
                                    storyEngine.AddSelectedNode(node);
                                }
                                else
                                    storyEngine.DeselectNodeNoCheck(node);
                            }
                        }
                    }

                    e.Use();
                }
                break;
            case MouseButton.Right:
                if (Vector2.Distance(rightClickDown, e.mousePosition) > RightClickTolerance)
                {
                    rightClickDown = -Vector2.one;
                }
                draggingWindow = true;
                break;

            case MouseButton.Middle:
                draggingWindow = true;
                break;
        }

        if (draggingWindow)
        {
            storyEngine.ScrollPos += e.delta / storyEngine.Zoom;
            e.Use();
        }
    }

    protected override void OnRawMouseMove(Event e)
    {
        if (isDrawingAnnotationLine)
        {
            Rect tempPosition = activeAnnotationLine.End;
            tempPosition.position = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos;
            tempPosition.position -= new Vector2(0, 50);
            activeAnnotationLine.End = tempPosition;

            e.Use();
            return;
        }

        var hoverNode = GetNodeAtPoint(e.mousePosition);
        if (hoverNode == null)
        {
            hoverNode = GetGroupAtPoint(e.mousePosition);
        }
        if (hoverNode != null)
        {
            if (Time.realtimeSinceStartup - hoverNode.HoverStartTime >= LogaConstants.NodeHoverTime)
            {
                hoverNode.ShowDesc = true;
            }
        }
        else
        {
            foreach (Node node in nodes)
            {
                node.ShowDesc = false;
            }
        }
    }

    protected override void OnRawMouseUp(Event e)
    {
        var hitNode = GetNodeAtPoint(e.mousePosition);
        var hitGroup = GetGroupAtPoint(e.mousePosition);
        //var hitLabel = GetLabelAtPoint(e.mousePosition);
        //var hitLine = GetLineAtPoint(e.mousePosition);
        //var hitBox = GetAnnotatedBoxAtPoint(e.mousePosition);

        // Convert Ctrl+Left click to a right click on mac
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            if (e.button == MouseButton.Left && e.control)
            {
                e.button = MouseButton.Right;
            }
        }

        switch (e.button)
        {
            case MouseButton.Left:
                if (didDoubleClick)
                {
                    didDoubleClick = false;
                    return;
                }

                if (isDrawingAnnotationLine)
                {
                    isDrawingAnnotationLine = false;
                    Rect tempPosition = activeAnnotationLine.End;
                    tempPosition.position = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos;
                    tempPosition.position -= new Vector2(0, 50);
                    activeAnnotationLine.End = tempPosition;
                    Repaint();
                    return;
                }

                if (dragNode != null)
                {
                    for (int i = 0; i < storyEngine.SelectedNodes.Count; i++)
                    {
                        var node = storyEngine.SelectedNodes[i];
                        var tempRect = node._NodeRect;
                        var distance = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos - startDragPosition;
                        tempRect.position -= distance;
                        node._NodeRect = tempRect;
                        tempRect.position += distance;
                        node._NodeRect = tempRect;
                        //do some fancy snapping in future
                        Repaint();
                    }

                    dragNode = null;
                    startDragPosition = Vector2.zero;
                }
                else if (dragGroup != null)
                {
                    int index = storyEngine.Groups.IndexOf(hitGroup);
                    if (hitGroup != null && index >= 0 && index < storyEngine.Groups.Count)
                    {
                        var group = storyEngine.Groups[index];
                        foreach (var node in group.GroupedNodes)
                        {
                            var tempRect = node._NodeRect;
                            var distance = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos - startDragPosition;
                            tempRect.position -= distance;
                            node._NodeRect = tempRect;
                            tempRect.position += distance;
                            node._NodeRect = tempRect;
                            //do some fancy snapping in future
                            Repaint();
                        }
                        dragGroup = null;
                    }
                }
                else if (dragLabel != null)
                {
                    var tempRect = dragLabel.LabelRect;
                    var distance = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos - startDragPosition;
                    tempRect.position -= distance;
                    dragLabel.LabelRect = tempRect;
                    tempRect.position += distance;
                    dragLabel.LabelRect = tempRect;
                    dragLabel = null;
                }
                else if (dragLine != null)
                {
                    var tempPosition = dragLine.StartSelected ? dragLine.Start : dragLine.End;
                    var distance = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos - startDragPosition;
                    tempPosition.position -= distance;
                    if (dragLine.StartSelected)
                    {
                        dragLine.Start = tempPosition;
                        tempPosition.position += distance;
                        dragLine.Start = tempPosition;
                    }
                    else
                    {
                        dragLine.End = tempPosition;
                        tempPosition.position += distance;
                        dragLine.End = tempPosition;
                    }
                    dragLine = null;
                }
                else if (dragBox != null)
                {
                    var tempPosition = dragBox.Box;
                    var distance = e.mousePosition / storyEngine.Zoom - storyEngine.ScrollPos - startDragPosition;
                    tempPosition.position -= distance;
                    dragBox.Box = tempPosition;
                    tempPosition.position += distance;
                    dragBox.Box = tempPosition;
                    dragBox = null;
                }

                //Check selection box changed
                if (selectionBox.size.x > 0 && selectionBox.size.y > 0)
                {
                    Undo.RecordObject(storyEngine, "Select");

                    storyEngine.UpdateSelectedNodeCache();
                    EndSelection();
                    if (GetAppendModifierDown())
                        StartControlSelection();

                    Repaint();

                    if (storyEngine.SelectedNode != null)
                        SetNodeForInspector(storyEngine.SelectedNode);

                    Repaint();
                }
                else
                {
                    if (!GetAppendModifierDown() && !hasDraggedSelected)
                    {
                        DeselectAllNodes();

                        if (hitNode != null)
                        {
                            SelectNode(hitNode);
                        }

                        else if (hitGroup != null)
                        {
                            string name = GetGroupName(storyEngine.Groups[storyEngine.Groups.IndexOf(hitGroup)]);
                            var group = storyEngine.Groups[storyEngine.Groups.IndexOf(hitGroup)];
                            Group groupComp = storyEngine.GetComponents<Group>().ToList().Find(x => x.GroupedNodes.All(node => group.GroupedNodes.Any(gn => gn._NodeRect.position == node._NodeRect.position)));
                            SetGroupForInspector(group.GroupedNodes, name, groupComp);
                        }
                    }
                }

                hasDraggedSelected = false;
                break;

            case MouseButton.Right:
                if (rightClickDown != -Vector2.one)
                {
                    var menu = new GenericMenu();
                    var mousePosition = rightClickDown;

                    if (hitNode != null)
                    {
                        storyEngine.AddSelectedNode(hitNode);

                        //get a copy of the node list as the selected nodes list gets modified
                        var selectedNodes = new List<Node>(storyEngine.SelectedNodes);

                        //make a menu for the node that was clicked on
                        menu.AddItem(new GUIContent("Copy"), false, () => CopyNode());
                        menu.AddItem(new GUIContent("Cut"), false, () => CutNode());
                        menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateNode());
                        menu.AddItem(new GUIContent("Delete"), false, () => AddToDeleteList(selectedNodes));
                        menu.AddItem(new GUIContent("Save as Blueprint"), false, () => ShowBlueprintSubMenu(mousePosition / storyEngine.Zoom - storyEngine.ScrollPos));

                        if (Application.isPlaying)
                        {
                            menu.AddItem(new GUIContent("StopAll"), false, () => StopAllNodes());
                            menu.AddItem(new GUIContent("Stop"), false, () => StopThisNode(hitNode));
                            menu.AddItem(new GUIContent("Execute"), false, () => ExecuteThisNode(hitNode, false));
                            menu.AddItem(new GUIContent("Execute (Stop All First)"), false, () => ExecuteThisNode(hitNode, true));
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("StopAll"));//, false), () => StopAllBlocks());
                            menu.AddDisabledItem(new GUIContent("Stop"));//, false);, () => StopThisBlock(hitBlock));
                            menu.AddDisabledItem(new GUIContent("Execute"));//, false);, () => ExecuteThisBlock(hitBlock, false));
                            menu.AddDisabledItem(new GUIContent("Execute (Stop All First)"));//, false);, () => ExecuteThisBlock(hitBlock, true));
                        }

                        //check to see if the node is already grouped - if it is then we can ungroup it
                        foreach (var group in storyEngine.Groups)
                        {
                            if (group == null)
                                continue;
                            if (group == null)
                                continue;
                            if (group.GroupedNodes.Contains(hitNode))
                            {
                                menu.AddItem(new GUIContent("Ungroup"), false, () => UngroupNode(hitNode));
                            }
                        }


                        //remove any group types from the selected nodes
                        for (int i = 0; i < storyEngine.SelectedNodes.Count; i++)
                        {
                            if (storyEngine.SelectedNodes[i] is Group)
                            {
                                storyEngine.SelectedNodes.Remove(storyEngine.SelectedNodes[i]);
                            }
                        }

                        //if we have multiple nodes selected then we can group them
                        if (storyEngine.SelectedNodes.Count > 1)
                        {
                            bool allUngrouped = storyEngine.SelectedNodes.All(node => !node.IsGrouped && node is not Group);

                            if (allUngrouped)
                            {
                                menu.AddItem(new GUIContent("Group Nodes"), false, () => SetGroupedNodes());
                            }
                            else
                            {
                                //if all nodes have the same group index as all the selected nodes
                                bool allSameGroup = storyEngine.SelectedNodes.All(node => node.IsGrouped && node.GroupIndex == storyEngine.SelectedNodes[0].GroupIndex);
                                if (allSameGroup)
                                {
                                    //we can add the selected nodes to an inner group of an exisiting group - if we have not selected all the nodes in the group 
                                    //otherwise we cannot do anything!
                                    if (storyEngine.Groups[storyEngine.SelectedNodes[0].GroupIndex].GroupedNodes.Count != storyEngine.SelectedNodes.Count)
                                    {
                                        menu.AddItem(new GUIContent("Add to Inner Group"), false, () => CreateInnerGroup(true));
                                        menu.AddItem(new GUIContent("Create New Group"), false, () => CreateInnerGroup(false));
                                    }
                                }
                                else
                                {
                                    var firstGroupedNode = storyEngine.SelectedNodes.FirstOrDefault(node => node.IsGrouped);
                                    int firstGroupIndex = firstGroupedNode != null ? firstGroupedNode.GroupIndex : -1;

                                    bool sameGroupOrUngrouped = storyEngine.SelectedNodes.All(
                                        node => (!node.IsGrouped) || (node.IsGrouped && node.GroupIndex == firstGroupIndex));

                                    //the selected nodes all share the same group index or are ungrouped
                                    if (sameGroupOrUngrouped)
                                    {
                                        List<Node> ungroupedNodes = new List<Node>();
                                        for (int i = 0; i < storyEngine.SelectedNodes.Count; i++)
                                        {
                                            if (!storyEngine.SelectedNodes[i].IsGrouped)
                                            {
                                                ungroupedNodes.Add(storyEngine.SelectedNodes[i]);
                                            }
                                        }

                                        menu.AddItem(new GUIContent("Add Nodes to Group"),
             storyEngine.SelectedNodes.Any(node => node.IsGrouped),
             () =>
             {
                 if (storyEngine.SelectedNodes.Any(node => node.IsGrouped))
                 {
                     int groupIndex = storyEngine.SelectedNodes.First(node => node.IsGrouped).GroupIndex;
                     AddNodesToGroup(storyEngine.Groups[groupIndex], ungroupedNodes);
                 }
             });

                                    }
                                }
                            }
                        }
                    }
                    else if (hitGroup != null)
                    {
                        //get the node within group that was clicked on
                        int groupID = storyEngine.Groups.IndexOf(hitGroup);
                        menu.AddItem(new GUIContent("Ungroup"), false, () => DisbandGroup(hitGroup, groupID));
                        //ensure that the new nodes are added to a new group
                        //menu.AddItem(new GUIContent("Duplicate Group"), false, () => DuplicateGroup(hitGroup.GroupedNodes));
                    }
                    //else if (hitLabel != null)
                    //{
                    //    menu.AddItem(new GUIContent("Delete Label"), false, () => DeleteLabel(hitLabel));
                    //}
                    //else if (hitLine != null)
                    //{
                    //    menu.AddItem(new GUIContent("Delete Line"), false, () => DeleteAnnotationLine(hitLine));
                    //}
                    //else if (hitBox != null)
                    //{
                    //    menu.AddItem(new GUIContent("Delete Box"), false, () => DeleteAnnotationBox(hitBox));
                    //}
                    else
                    {
                        DeselectAllNodes();

                        menu.AddItem(
                            new GUIContent("Add New Node"),
                            false,
                            () => AddNewNode(mousePosition / storyEngine.Zoom - storyEngine.ScrollPos)
                        );

                        menu.AddItem(new GUIContent("Custom Nodes/Starting Node"), false, () => AddNodeWithEventHandler(mousePosition / storyEngine.Zoom - storyEngine.ScrollPos, typeof(GameStarted)));
                        menu.AddItem(new GUIContent("Custom Nodes/Update Node"), false, () => AddNodeWithEventHandler(mousePosition / storyEngine.Zoom - storyEngine.ScrollPos, typeof(UpdateEventHandler)));

                        if (copyList.Count >= 1)
                        {
                            //only show this if the list of copied nodes is not empty (i.e. we have copied a node)
                            menu.AddItem(new GUIContent("Paste Node"), false, () => PasteNode(mousePosition));
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Paste Node"));
                        }
                        //menu.AddItem(
                        //    new GUIContent("Add Label"),
                        //    false,
                        //    () => AddLabel(mousePosition / storyEngine.Zoom - storyEngine.ScrollPos)
                        //);
                        //menu.AddItem(
                        //    new GUIContent("Start Line"),
                        //    false,
                        //    () => StartLine(rightClickDown / storyEngine.Zoom - storyEngine.ScrollPos)
                        //);
                        menu.AddSeparator("");
                        if (!Application.isPlaying)
                        {
                            menu.AddItem(new GUIContent("Play"), false, () => PlayGame());
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Stop"), false, () => PlayGame());
                        }
                    }

                    var menuRect = new Rect();
                    menuRect.position = new Vector2(mousePosition.x, mousePosition.y);
                    menu.DropDown(menuRect);
                    e.Use();
                }
                break;
        }

        //selection box
        if (isDrawingAnnotationBox)
        {
            selectionBox.position -= storyEngine.ScrollPos;
            isDrawingAnnotationBox = false;
            CreateAnnotateBox(selectionBox);
        }

        selectionBox.size = Vector2.zero;
        selectionBox.position = -Vector2.one;
        selectionBoxStartPos = selectionBox.position;
    }

    protected void DuplicateGroup(List<Node> group)
    {
        storyEngine.SelectedNodes.AddRange(group);
        DuplicateNode();
        foreach (var node in pasteList)
        {
            UngroupNode(node);
            node.IsGrouped = true;
            node.GroupIndex = storyEngine.Groups.Count;
            SetGroupedNodes();
        }
        storyEngine.SelectedNodes.Clear();
        DeselectAllNodes();
    }

    private void SetGroupedNodes()
    {
        List<Node> nodesToGroup = new List<Node>();
        nodesToGroup.AddRange(storyEngine.SelectedNodes);
        for (int i = 0; i < nodesToGroup.Count; i++)
        {
            if (nodesToGroup[i] is Group)
            {
                nodesToGroup.Remove(nodesToGroup[i]);
            }
        }

        AddGroup(nodesToGroup, "Group " + storyEngine.Groups.Count, null);
    }

    private void CreateInnerGroup(bool createInner)
    {
        var selectedNodesCopy = new List<Node>();
        selectedNodesCopy.AddRange(storyEngine.SelectedNodes);
        for (int i = 0; i < selectedNodesCopy.Count; i++)
        {
            if (selectedNodesCopy[i] is Group)
            {
                selectedNodesCopy.Remove(selectedNodesCopy[i]);
            }
        }
        //if we want to create a group from nodes that are already grouped but not have them inner we must ungroup them first
        if (!createInner)
        {
            foreach (var node in selectedNodesCopy) // Iterate over the copied list
            {
                UngroupNode(node);
            }
        }
        foreach (var node in selectedNodesCopy) // Iterate over the copied list
        {
            node.IsGrouped = false;
            node.GroupIndex = -1;
        }
        //otherwise we can leave them in the parent group and create a new inner group
        AddGroup(selectedNodesCopy, "Group " + storyEngine.Groups.Count, null);
    }

    private void AddNodesToGroup(Group group, List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is Group)
            {
                nodes.Remove(nodes[i]);
            }
        }
        //find the group object and also update this
        var existingGroupObjs = storyEngine.GetComponentsInChildren<NodeCollection>();
        foreach (NodeCollection groupColl in existingGroupObjs)
        {
            //find the matching group obj to the group provided
            if (group.GroupedNodes.All(x => groupColl.Contains(x)) && group.GroupedNodes.Count == groupColl.Count)
            {
                //add the new nodes to the group obj
                foreach (var node in nodes)
                {
                    groupColl.Add(node);
                }
                break;
            }
        }
        foreach (var node in nodes)
        {
            group.GroupedNodes.Add(node);
        }
    }

    public void UngroupNode(Node node)
    {
        storyEngine.SelectedNodes.Clear();

        for (int i = 0; i < storyEngine.Groups.Count; i++)
        {
            var group = storyEngine.Groups[i];
            foreach (var _node in group.GroupedNodes)
            {
                if (_node == node)
                {
                    //find the relevant game object with collection attached
                    var existingGroupObjs = storyEngine.GetComponentsInChildren<NodeCollection>();
                    NodeCollection groupCollObj = null;
                    foreach (NodeCollection groupColl in existingGroupObjs)
                    {
                        //find the matching group obj to the group provided
                        if (group.GroupedNodes.All(x => groupColl.Contains(x)))
                        {
                            //remove the node from the group obj
                            groupCollObj = groupColl;
                            groupColl.Remove(node);
                            break;
                        }
                    }
                    group.GroupedNodes.Remove(node);
                    if (group.GroupedNodes.Count <= 1)
                    {
                        DisbandGroup(group, node.GroupIndex);
                    }
                    node.IsGrouped = false;
                    node.GroupIndex = -1;

                    break;
                }
            }
        }
        DeselectAllNodes();
    }

    protected virtual void DeselectAllNodes()
    {
        Undo.RecordObject(storyEngine, "Deselect");
        storyEngine.ClearSelectedOrders();
        EndControlSelection();
        storyEngine.ClearSelectedNodes();
        Selection.activeGameObject = storyEngine.gameObject;
    }

    protected void AddNewNode(Vector2 nodePos)
    {
        windowPos = nodePos;
        AddNode(nodePos);
    }

    protected void AddNodeWithEventHandler(Vector2 nodePos, Type type)
    {
        windowPos = nodePos;
        AddNodeWithHandler(nodePos, type);
    }

    protected void PlayGame()
    {
        if (!Application.isPlaying)
            EditorApplication.EnterPlaymode();
        else
            EditorApplication.ExitPlaymode();
    }

    protected void FakeEvent()
    {
        //do nothing - this is a fake event to allow the context menu to show
    }

    protected override void OnScrollWheel(Event e)
    {
        if (selectionBox.size == Vector2.zero)
        {
            Vector2 zoomCenter;
            zoomCenter.x = e.mousePosition.x / storyEngine.Zoom / position.width;
            zoomCenter.y = e.mousePosition.y / storyEngine.Zoom / position.height;
            zoomCenter *= storyEngine.Zoom;

            DoZoom(-e.delta.y * 0.01f, zoomCenter);
            e.Use();
        }
    }

    protected virtual void DoZoom(float delta, Vector2 centre)
    {
        var prevZoom = storyEngine.Zoom;
        storyEngine.Zoom += delta;
        storyEngine.Zoom = Mathf.Clamp(storyEngine.Zoom, minZoomValue, maxZoomValue);
        var deltaSize = position.size / prevZoom - position.size / storyEngine.Zoom;
        var offset = -Vector2.Scale(deltaSize, centre);
        storyEngine.ScrollPos += offset;
        forceRepaintCount = 1;
    }

    protected virtual void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        Undo.undoRedoPerformed -= Undo_ForceRepaint;
#if UNITY_2017_4_OR_NEWER
        EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
#endif
        //close the map window
        if (EditorWindow.HasOpenInstances<MapboxControls>())
        {
            var mapWindow = MapboxControls.GetWindow<MapboxControls>();
            mapWindow.Close();
        }
    }

#if UNITY_2017_4_OR_NEWER
    private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
    {
        //force null so it can refresh context on the other side of the context
        storyEngine = null;
        prevEngine = null;
        nodeInspector = null;
    }
#endif

    protected void Undo_ForceRepaint()
    {
        //an undo redo may have added or removed nodes so
        UpdateNodes();
        storyEngine.UpdateSelectedNodeCache();
        Repaint();
    }

    protected void OnEditorUpdate()
    {
        HandleEngineSelectionChange();

        if (storyEngine != null)
        {
            var varcount = storyEngine.VariableCount;
            if (varcount != prevVarCount)
            {
                prevVarCount = varcount;
                Repaint();
            }

            if (storyEngine.SelectedOrdersStale)
            {
                storyEngine.SelectedOrdersStale = false;
                Repaint();
            }

            if (OrderEditor.SelectedOrderDataStale)
            {
                OrderEditor.SelectedOrderDataStale = false;
                Repaint();
            }

            if (NodeEditor.SelectedNodeDataStale)
            {
                NodeEditor.SelectedNodeDataStale = false;
                Repaint();
            }
        }
        else
            prevVarCount = 0;

        if (Application.isPlaying)
        {
            executingBlocks.ProcessAllNodes(nodes);
            if (executingBlocks.isChangeDetected || executingBlocks.IsAnimFadeoutNeed())
                Repaint();
        }
    }

    protected void ShowBlueprintSubMenu(Vector2 pos)
    {
        showBlueprintWindow = true;
        blueprintWindowRect.position = pos;
    }

    public void AddBlueprint(List<Node> BPNodes, List<Group> BPGroups, BasicFlowEngine engine, BasicFlowEngine originalEngine = null)
    {
        float xOffset = 0;
        float yOffset = 0;

        bool firstNode = false;

        //copy any relevant variables and set the values to the new engine
        foreach (var node in BPNodes)
        {
            if (firstNode == false)
            {
                xOffset = node._NodeRect.x + 150; //add a small offset to avoid covering existing nodes
                yOffset = node._NodeRect.y;
                firstNode = true;
            }

            //subtract the offset from the node position
            Rect nodeRect = node._NodeRect;
            nodeRect.x -= xOffset;
            nodeRect.y -= yOffset;
            node._NodeRect = nodeRect;

            if (node.NodeLocation != null)
            {
                var newLocVar = engine.AddVariable(node.NodeLocation.GetType(), node.NodeLocation.Key);
                newLocVar.Apply(SetOperator.Assign, node.NodeLocation);
                newLocVar.Scope = VariableScope.Global;
            }
            var nodeOrders = new List<Order>();
            node.GetConditionOrders(ref nodeOrders);
            if (nodeOrders.Count > 0)
            {
                foreach (Order order in nodeOrders)
                {
                    var ifNode = order as If;
                    if (ifNode != null)
                    {
                        foreach (var condition in ifNode.conditions)
                        {
                            if (condition.AnyVariable.variable != null)
                            {
                                var newVar = engine.AddVariable(condition.AnyVariable.variable.GetType(), condition.AnyVariable.variable.Key);
                                //ensure we set the value of the new var to the original
                                newVar.Apply(SetOperator.Assign, condition.AnyVariable.variable);
                            }
                        }
                    }
                }
            }
        }

        GraphWindow.priorNodes.Clear();
        GraphWindow.newNodes.Clear();

        copyList.Clear();
        foreach (var node in BPNodes)
        {
            if (node is not Group)
                copyList.Add(new NodeCopy(node));
        }
        PasteNode(new Vector2(0, 0), true, true, engine);

        if (BPGroups != null && BPGroups.Count > 0)
        {
            for (int i = 0; i < BPGroups.Count; i++)
            {
                var group = BPGroups[i];
                group.GroupedNodes.Clear();
                foreach (var node in pasteList)
                {
                    if (node.IsGrouped && node.GroupIndex == i)
                    {
                        group.GroupedNodes.Add(node);
                    }
                }

                var g = AddGroup(group.GroupedNodes, group._NodeName, group, engine);
                g.SetGroup(group);

                if (originalEngine != null)
                {
                    var originalGroups = originalEngine.Groups;
                    Group originalGroup = null;
                    List<Node> originalNodesToComplete = new List<Node>();
                    foreach (Group _group in originalGroups)
                    {
                        if (_group._NodeRect == g._NodeRect)
                        {
                            originalGroup = _group;
                        }
                    }
                    if (originalGroup.NodeLocation != null)
                    {
                        //Find the original node location in the new engine
                        var originalLoc = engine.GetVariable(originalGroup.NodeLocation.Key);
                        Variable newVar = originalLoc;
                        if (originalLoc == null)
                        {
                            newVar = engine.AddVariable(originalGroup.NodeLocation.GetType(), originalGroup.NodeLocation.Key);
                            //ensure we set the value of the new var to the original
                            newVar.Apply(SetOperator.Assign, originalGroup.NodeLocation);
                            newVar.Scope = VariableScope.Global;
                        }
                        g.NodeLocation = (LocationVariable)newVar as LocationVariable;
                    }
                    //Find the original target node in the new engine
                    if (originalGroup.TargetUnlockNode != null)
                    {
                        var originalTargetNode = engine.GetComponents<Node>().FirstOrDefault(x => x._NodeRect == originalGroup.TargetUnlockNode._NodeRect);
                        if (originalTargetNode != null)
                        {
                            g.TargetUnlockNode = originalTargetNode;
                        }
                    }
                    //Find the original nodes to complete in the new engine
                    for (int j = 0; j < originalGroup.NodesToComplete.Count; j++)
                    {
                        var originalNode = engine.GetComponents<Node>().FirstOrDefault(x => x._NodeRect == originalGroup.NodesToComplete[j]._NodeRect);
                        originalNodesToComplete.Add(originalNode);
                    }
                    if (originalNodesToComplete.Count > 0)
                    {
                        g.NodesToComplete.AddRange(originalNodesToComplete);
                    }
                }
            }
        }
        SetNewNodes(engine);

        CentreWindow();
    }

    public void AddBlueprintContextMenu(string blueprintName)
    {

        // Define the path where the new script will be created
        string folderPath = "Assets/LUTE/Editor";
        string scriptName = "CustomBlueprintMenu.cs";
        string fullPath = Path.Combine(folderPath, scriptName);


        // Ensure the folder exists, create if not
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        FileInfo fileInfo = new FileInfo(fullPath);

        if (!fileInfo.Exists || fileInfo.Length == 0)
        {
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using UnityEditor;");
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine();
                writer.WriteLine("public class " + Path.GetFileNameWithoutExtension(scriptName));
                writer.WriteLine("{");
                writer.WriteLine("}");
            }
        }

        // Refresh the asset database to show the new script in the Unity Editor
        AssetDatabase.Refresh();

        try
        {
            // Read the content of the script file
            string scriptContent = File.ReadAllText(fullPath);

            // Find the position of the last curly brace
            int lastCurlyBraceIndex = scriptContent.LastIndexOf('}');

            if (lastCurlyBraceIndex == -1)
            {
                Debug.LogError("No closing curly brace found in the script file.");
                return;
            }

            string textToInsert = "[MenuItem(\"LUTE/Create/Blueprints/" + blueprintName + "\", false, 100)]";
            textToInsert += "\n" + "static void AddBlueprint_" + blueprintName + "()";
            textToInsert += "\n" + "{";
            textToInsert += "\n" + "    EngineMenuItems.AddBlueprint(\"" + blueprintName + "\");";
            textToInsert += "\n" + "}";

            // Insert the desired text just before the last curly brace
            scriptContent = scriptContent.Insert(lastCurlyBraceIndex, textToInsert + "\n");

            // Write the modified content back to the script file
            File.WriteAllText(fullPath, scriptContent);

        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to modify script file: " + ex.Message);
        }
        //save the script file
        AssetDatabase.Refresh();
    }

    protected void CreateBlueprint(string BPName)
    {
        //// Store a reference to the original storyEngine
        var originalStoryEngine = storyEngine;
        var savedNodes = storyEngine.SelectedNodes;

        // Create a new engine object for nodes and groups to be saved to
        var engine = EngineMenuItems.SpawnPrefab("EmptyEngine").GetComponent<BasicFlowEngine>();
        engine.Version = LogaConstants.CurrentVersion;

        // Create a list to store copies of the groups
        List<Group> copiedGroups = new List<Group>();

        foreach (Group group in storyEngine.Groups)
        {
            // Create a new instance of the Group class and copy the values of all properties and fields from the original group to the copied group
            Group copiedGroup = new Group();
            copiedGroup.SetGroup(group);
            copiedGroup.GroupedNodes = new List<Node>(group.GroupedNodes); // Make a copy of the list of nodes

            copiedGroups.Add(copiedGroup);
        }

        for (int i = 0; i < copiedGroups.Count; i++)
        {
            Group group = copiedGroups[i];
            //If the group contains none of the selected nodes then we don't need to copy it
            if (!savedNodes.Any(node => group.GroupedNodes.Contains(node)))
            {
                copiedGroups.Remove(group);
            }
        }
        for (int i = 0; i < copiedGroups.Count; i++)
        {
            engine.groupnames.Add(copiedGroups[i]._NodeName + " (Blueprint)");
        }

        // Set the current version for the blueprint to be copied to
        AddBlueprint(savedNodes, copiedGroups, engine, storyEngine);

        // Save the object as a prefab within the resources folder
        string resourcePath = "Assets/LUTE/Resources/Prefabs/" + BPName + ".prefab";
        //Check to see if the path exists (is there a blueprint with same name)
        if (AssetDatabase.LoadAssetAtPath(resourcePath, typeof(GameObject)))
        {
            //If the blueprint already exists, ask the user if they want to overwrite it
            if (EditorUtility.DisplayDialog("Overwrite Blueprint?", "A blueprint with the name " + BPName + " already exists. Do you want to overwrite it?", "Yes", "No"))
            {
                //If the user selects yes, save the blueprint
                PrefabUtility.SaveAsPrefabAsset(engine.gameObject, resourcePath);
            }
        }
        else
        {
            //If the blueprint does not exist, save the blueprint
            PrefabUtility.SaveAsPrefabAsset(engine.gameObject, resourcePath);
            //Finally we can add a context menu using another script
            AddBlueprintContextMenu(BPName);
        }

        // Destroy the object
        DestroyImmediate(engine.gameObject);

        // Reset the storyEngine back to the original
        storyEngine = originalStoryEngine;
        storyEngine.Version = LogaConstants.CurrentVersion;
        Selection.activeGameObject = storyEngine.gameObject;

        // Update any nodes missing or added during this process on the original engine
        UpdateNodes();

        blueprintName = "Blueprint";
        showBlueprintWindow = false;
    }

    private void SetNewNodes(BasicFlowEngine newEngine)
    {
        var priorNodes = GraphWindow.priorNodes;
        var newNodes = GraphWindow.newNodes;
        for (int i = 0; i < priorNodes.Count; i++)
        {
            var _node = priorNodes[i];
            var unlockNode = _node.GetUnlockNode();
            if (unlockNode != null)
            {
                Node newUnlockNode = null;
                var engineNodes = newEngine.GetComponents<Node>();
                foreach (var node in engineNodes)
                {
                    //If the unlock node is found on the new engine then set it (finding by rect seems bad)
                    if (node._NodeRect == unlockNode._NodeRect)
                    {
                        newUnlockNode = node;
                        break;
                    }
                }
                newNodes[i].TargetUnlockNode = newUnlockNode;
            }

            var orders = _node.OrderList;
            foreach (Order order in orders)
            {
                if (order is Choice || order is MenuChoice || order is NextNode)
                {
                    Node targetNodeNew = null;
                    var engineNodes = newEngine.GetComponents<Node>();
                    Node targetNode = null;

                    if (order is NextNode)
                    {
                        targetNode = (order as NextNode).targetNode;
                    }
                    else if (order is Choice)
                    {
                        targetNode = (order as Choice).targetNode;
                    }
                    else if (order is MenuChoice)
                    {
                        targetNode = (order as MenuChoice).targetNode;
                    }

                    foreach (var node in engineNodes)
                    {
                        if (node._NodeRect == targetNode._NodeRect)
                        {
                            targetNodeNew = node;
                            break;
                        }
                    }

                    if (order is NextNode)
                    {
                        (newNodes[i].OrderList[_node.OrderList.IndexOf(order)] as NextNode).targetNode = targetNodeNew;
                    }
                    else if (order is Choice)
                    {
                        (newNodes[i].OrderList[_node.OrderList.IndexOf(order)] as Choice).targetNode = targetNodeNew;
                    }
                    else if (order is MenuChoice)
                    {
                        (newNodes[i].OrderList[_node.OrderList.IndexOf(order)] as MenuChoice).targetNode = targetNodeNew;
                    }
                }
            }
        }
    }
}