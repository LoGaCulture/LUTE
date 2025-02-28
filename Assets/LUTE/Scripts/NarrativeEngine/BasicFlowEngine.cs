using LoGaCulture.LUTE;
using Mapbox.Examples;
using Mapbox.Unity.Map;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public static class ComponentExtensions
{
    public static void AddBlueprint(GameObject go, BasicFlowEngine engineBP)
    {
        if (go != null && engineBP != null)
        {
            var components = engineBP.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component.GetType() != typeof(BasicFlowEngine) && component.GetType() != typeof(Transform))
                {
                    go.AddComponent(component.GetType()).GetCopyOf(component);
                }
            }
        }
    }

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
}

public class BasicFlowEngine : MonoBehaviour, ISubstitutionHandler
{
    public const string SubstituteVariableRegexString = "{\\$.*?}";

    public TextMeshProUGUI storyText;
    protected List<Node> nodes = new List<Node>();
    [SerializeField] protected List<Group> groups = new List<Group>();
    protected List<Label> labels = new List<Label>();
    protected List<AnnotationLine> annotationLines = new List<AnnotationLine>();

    protected string defaultBlockName = "New Node";
    protected static List<BasicFlowEngine> cachedEngines = new List<BasicFlowEngine>();
    [SerializeField] protected string description = "";
    [SerializeField] protected bool hideComponents = true;
    [Tooltip("Unique identifier for this engine in localised string keys. If no id is specified then the name of the engine object will be used.")]
    [SerializeField] protected string localizationId = "";
    [SerializeField] protected List<Order> selectedOrders = new List<Order>();
    [SerializeField] protected List<Node> selectedNodes = new List<Node>();
    [SerializeField] protected float nodeViewHeight = 400;
    [Tooltip("List of orders to hide in the Add Order menu. Use this to restrict the set of orders available when editing an Engine.")]
    [SerializeField] protected List<string> hideOrders = new List<string>();
    [HideInInspector]
    [SerializeField] protected List<Variable> variables = new List<Variable>();
    [HideInInspector]
    [SerializeField] protected bool variablesExpanded = true;
    [HideInInspector]
    [SerializeField] protected Vector2 variablesScrollPos;
    [SerializeField] protected List<Vector2> mapLocations = new List<Vector2>();
    [Tooltip("If true, the engine will be in demo map mode. This means that any location conditions are evaluated on the centre position rather than the device location")]
    [SerializeField] protected bool demoMapMode = false;
    [SerializeField] protected List<Sprite> mapSprites = new List<Sprite>();
    [SerializeField] protected bool colourOrders = true;
    [SerializeField] protected bool showLineNumbers = false;
    [Tooltip("If true, the handler info will be shown on the graph view for each node")]
    [SerializeField] protected bool showHandlerInfoOnGraph = true;
    [Tooltip("If true, annotations will be shown on the graph view")]
    [SerializeField] protected bool showAnnotations = true;
    [Tooltip("The colour tint to apply to the labels - default is white")]
    [SerializeField] protected UnityEngine.Color labelTint = UnityEngine.Color.white;
    [HideInInspector]
    [SerializeField] protected float zoom = 1f;
    [HideInInspector]
    [SerializeField] protected Vector2 scrollPos;
    [HideInInspector]
    [SerializeField] protected int version = 0; // Default to 0 to always trigger an update for older versions.
    [SerializeField] protected int sidesOfDie = 6;
    [SerializeField] protected List<Postcard> postcards = new List<Postcard>();
    [SerializeField] protected List<OptionSetting.OptionType> optionSettings = new List<OptionSetting.OptionType>();

    public virtual string Description { get { return description; } }
    public virtual List<Order> SelectedOrders { get { return selectedOrders; } }
    public virtual float NodeViewHeight { get { return nodeViewHeight; } set { nodeViewHeight = value; } }
    public virtual Node SelectedNode
    {
        get
        {
            if (selectedNodes == null || selectedNodes.Count == 0)
                return null;

            return selectedNodes[0];
        }
        set
        {
            ClearSelectedOrders();
            AddSelectedNode(value);
        }
    }
    public virtual List<Node> SelectedNodes { get { return selectedNodes; } set { selectedNodes = value; } }
    public virtual List<Variable> Variables { get { return variables; } }
    public virtual int VariableCount { get { return variables.Count; } }
    public List<string> groupnames = new List<string>();
    public virtual string LocalizationId { get { return localizationId; } }
    public static List<BasicFlowEngine> CachedEngines { get { return cachedEngines; } }
    public virtual bool VariablesExpanded { get { return variablesExpanded; } set { variablesExpanded = value; } }
    public virtual Vector2 VariablesScrollPos { get { return variablesScrollPos; } set { variablesScrollPos = value; } }
    public virtual List<Vector2> MapLocations { get { return mapLocations; } }
    public virtual Vector2 MapMousePosition { get; set; }
    public virtual List<Group> Groups { get { return groups; } set { groups = value; } }
    public virtual bool DemoMapMode { get { return demoMapMode; } set { demoMapMode = value; } }
    public virtual List<Sprite> MapSprites { get { return mapSprites; } }
    public virtual bool ColourOrders { get { return colourOrders; } }
    public virtual bool ShowLineNumbers { get { return showLineNumbers; } }
    public virtual bool ShowHandlerInfoOnGraph { get { return showHandlerInfoOnGraph; } }
    public virtual bool ShowAnnotations { get { return showAnnotations; } set { showAnnotations = value; } }
    public virtual UnityEngine.Color LabelTint { get { return labelTint; } set { labelTint = value; } }
    public virtual float Zoom { get { return zoom; } set { zoom = value; } }
    public virtual Vector2 ScrollPos { get { return scrollPos; } set { scrollPos = value; } }
    public virtual Vector2 CenterPosition { set; get; }
    public int Version { set { version = value; } }
    public int SidesOfDie { get { return sidesOfDie; } set { sidesOfDie = value; } }
    public List<Postcard> Postcards { get { return postcards; } }
    public List<OptionSetting.OptionType> OptionSettings { get { return optionSettings; } }

    protected static bool eventSystemPresent;
    protected StringSubstituter stringSubstituer;


#if UNITY_EDITOR
    public bool SelectedOrdersStale { get; set; }
#endif

    protected virtual void LevelWasLoaded()
    {
        // Reset the flag for checking for an event system as there may not be one in the newly loaded scene.
        eventSystemPresent = false;
    }

    protected virtual void Start()
    {
        CheckEventSystem();
        MMGameEvent.Trigger("Load");


        if (!this.name.Contains("GlobalVariablesEngine"))
            LogaManager.Instance.LogManager.Log(LoGaCulture.LUTE.Logs.LogLevel.Info, "Engine started", "Engine: " + description);
    }

    // There must be an Event System in the scene for Say and Menu input to work.
    // This method will automatically instantiate one if none exists.
    protected virtual void CheckEventSystem()
    {
        if (eventSystemPresent)
        {
            return;
        }

        EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            // Auto spawn an Event System from the prefab
            GameObject prefab = Resources.Load<GameObject>("Prefabs/EventSystem");
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab) as GameObject;
                go.name = "EventSystem";
            }
        }

        eventSystemPresent = true;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        LevelWasLoaded();
    }

    protected virtual void OnEnable()
    {
        if (!cachedEngines.Contains(this))
        {
            cachedEngines.Add(this);
#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
#endif
        }

        UpdateHideFlags();
        CleanupComponents();
        UpdateVersion();
    }

    protected virtual void OnDisable()
    {
        cachedEngines.Remove(this);
#if UNITY_5_4_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
#endif
    }

    protected virtual void OnApplicationQuit()
    {
        if (!this.name.Contains("GlobalVariablesEngine"))
            LogaManager.Instance.LogManager.Log(LoGaCulture.LUTE.Logs.LogLevel.Info, "Engine Ended", "Engine: " + description);
    }

    protected virtual void UpdateVersion()
    {
        if (version == LogaConstants.CurrentVersion)
        {
            // No need to update
            return;
        }

        // Tell all components that implement IUpdateable to update to the new version
        var components = GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            IUpdateable u = component as IUpdateable;
            if (u != null)
            {
                u.UpdateToVersion(version, LogaConstants.CurrentVersion);
            }
        }

        version = LogaConstants.CurrentVersion;
    }

    /// Returns the next id to assign to a new engine item.
    /// Item ids increase monotically so they are guaranteed to
    /// be unique within a Engine.
    public int NextItemId()
    {
        int maxId = -1;
        var nodes = GetComponents<Node>();
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            maxId = Math.Max(maxId, node._ItemId);
        }

        var orders = GetComponents<Order>();
        for (int i = 0; i < orders.Length; i++)
        {
            var order = orders[i];
            maxId = Math.Max(maxId, order.ItemId);
        }
        return maxId + 1;
    }

    protected virtual void CleanupComponents()
    {
        // Delete any unreferenced components which shouldn't exist any more
        // Unreferenced components don't have any effect on the engine behavior, but
        // they waste memory so should be cleared out periodically.

        // Remove any null entries in the variables list
        // It shouldn't happen but it seemed to occur for a user on the forum 
        variables.RemoveAll(item => item == null);

        if (selectedNodes == null) selectedNodes = new List<Node>();
        if (selectedOrders == null) selectedOrders = new List<Order>();

        selectedNodes.RemoveAll(item => item == null);
        selectedOrders.RemoveAll(item => item == null);

        var allVariables = GetComponents<Variable>();
        for (int i = 0; i < allVariables.Length; i++)
        {
            var variable = allVariables[i];
            if (!variables.Contains(variable))
            {
                DestroyImmediate(variable);
            }
        }

        var nodes = GetComponents<Node>();
        var orders = GetComponents<Order>();
        for (int i = 0; i < orders.Length; i++)
        {
            var order = orders[i];
            bool found = false;
            for (int j = 0; j < nodes.Length; j++)
            {
                var node = nodes[j];
                if (node.OrderList.Contains(order))
                {
                    found = true;
                    break;
                }
                else if (node._EventHandler != null && node._EventHandler.GetType() == typeof(ConditionalEventHandler))
                {
                    var handler = node._EventHandler as ConditionalEventHandler;
                    if (handler.Conditions.Contains(order))
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                DestroyImmediate(order);
            }
        }

        var eventHandlers = GetComponents<EventHandler>();
        for (int i = 0; i < eventHandlers.Length; i++)
        {
            var eventHandler = eventHandlers[i];
            bool found = false;
            for (int j = 0; j < nodes.Length; j++)
            {
                var node = nodes[j];
                if (node._EventHandler == eventHandler)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                DestroyImmediate(eventHandler);
            }
        }
    }

    public virtual Node CreateNode(Vector2 position)
    {
        Node node = CreateNodeComponent(gameObject);
        node._NodeRect = new Rect(position.x, position.y, node._NodeRect.width, node._NodeRect.height);
        node._NodeName = GetUniqueNodeKey(node._NodeName, node);
        node._ItemId = nodes.Count;
        return node;
    }

    public virtual Node CreateNodeWithHandler(Vector2 position, Type handlerType)
    {
        Node node = CreateNodeComponent(gameObject);
        node._NodeRect = new Rect(position.x, position.y, node._NodeRect.width, node._NodeRect.height);
        node._NodeName = GetUniqueNodeKey(node._NodeName, node);
        node._ItemId = nodes.Count;
        return node;
    }

    protected virtual Node CreateNodeComponent(GameObject parent)
    {
        Node node = parent.AddComponent<Node>();
        return node;
    }

    public virtual Label CreateLabel(Vector2 position)
    {
        Label label = CreateLabelComponent(gameObject);
        label.LabelRect = new Rect(position.x, position.y, label.LabelRect.width, label.LabelRect.height);
        label.ItemId = labels.Count;
        return label;
    }

    protected virtual Label CreateLabelComponent(GameObject parent)
    {
        Label label = parent.AddComponent<Label>();
        return label;
    }

    public virtual AnnotationLine CreateAnnotationLine(Vector2 start)
    {
        AnnotationLine line = CreateAnnotationLineComponent(gameObject);
        var tempRect = line.Start;
        tempRect.position = start;
        line.Start = tempRect;
        line.ItemId = annotationLines.Count;
        return line;
    }

    protected virtual AnnotationLine CreateAnnotationLineComponent(GameObject parent)
    {
        AnnotationLine line = parent.AddComponent<AnnotationLine>();
        return line;
    }

    public virtual AnnotationBox CreateAnnotationBox(Rect rect)
    {
        AnnotationBox annotationBox = CreateAnnotationBoxComponent(gameObject);
        annotationBox.Box = rect;
        return annotationBox;
    }

    protected virtual AnnotationBox CreateAnnotationBoxComponent(GameObject parent)
    {
        AnnotationBox box = parent.AddComponent<AnnotationBox>();
        return box;
    }

    public virtual void AddNode(Node node)
    {
        if (!nodes.Contains(node))
        {
            nodes.Add(node);
        }
    }

    public virtual void AddLabel(Label label)
    {
        if (!labels.Contains(label))
        {
            labels.Add(label);
        }
    }

    public virtual void AddAnnotationLine(AnnotationLine line)
    {
        if (!annotationLines.Contains(line))
        {
            annotationLines.Add(line);
        }
    }

    public virtual Group CreateGroup(List<Node> nodes, Group exisitingGroup)
    {
        Group group = CreateGroupComponent(gameObject, nodes, exisitingGroup);
        return group;
    }

    protected virtual Group CreateGroupComponent(GameObject parent, List<Node> nodes, Group exisitingGroup)
    {
        Group group;
        if (exisitingGroup != null)
        {
            group = parent.AddComponent(exisitingGroup);
        }
        else
        {
            group = parent.AddComponent<Group>();
            group.GroupedNodes.Clear();
            group.GroupedNodes.AddRange(nodes);
        }

        //we must also create a new object for our group
        var newGroupObj = new GameObject().AddComponent<NodeCollection>();
        newGroupObj.transform.SetParent(transform);
        foreach (Node node in nodes)
        {
            newGroupObj.Add(node);
        }
        return group;
    }

    public virtual void AddGroup(Group group)
    {
        if (!groups.Contains(group))
        {
            groups.Add(group);
        }
    }

    public virtual void InsertGroupAtFront(Group group)
    {
        if (!groups.Contains(group))
        {
            groups.Insert(0, group);
        }
    }

    // Returns a new node key that is guaranteed not to clash with any existing Node in the Engine.
    public virtual string GetUniqueNodeKey(string originalKey, Node ignoreNode = null)
    {
        int suffix = 0;
        string baseKey = originalKey.Trim();

        // No empty keys allowed
        if (baseKey.Length == 0)
        {
            baseKey = defaultBlockName;
        }

        var nodes = GetComponents<Node>();

        string key = baseKey;
        while (true)
        {
            bool collision = false;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == ignoreNode || node._NodeName == null)
                {
                    continue;
                }
                if (node._NodeName.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    collision = true;
                    suffix++;
                    key = baseKey + " " + suffix;
                }
            }

            if (!collision)
            {
                return key;
            }
        }
    }

    // Returns a new group key that is guaranteed not to clash with any existing Block in the Flowchart.
    public virtual string GetUniqueGroupKey(string originalKey, Node ignoreNode = null)
    {
        int suffix = 0;
        string baseKey = originalKey.Trim();

        // No empty keys allowed
        if (baseKey.Length == 0)
        {
            baseKey = defaultBlockName;
        }

        var nodes = GetComponentsInChildren<NodeCollection>();

        string key = baseKey;
        while (true)
        {
            bool collision = false;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == ignoreNode || node.gameObject.name == null)
                {
                    continue;
                }
                if (node.gameObject.name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    collision = true;
                    suffix++;
                    key = baseKey + " " + suffix;
                }
            }

            if (!collision)
            {
                return key;
            }
        }
    }

    // Returns the variable with the specified key, or null if the key is not found
    // You will need to cast the returned variable to the correct sub-type
    // You can then access the variable's value using the Value property. e.g.
    // BooleanVariable boolVar = flowchart.GetVariable("MyBool") as BooleanVariable;
    // boolVar.Value = false;
    public Variable GetVariable(string key)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];

            if (variable != null && variable.Key == key)
            {
                return variable;
            }
        }

        return null;
    }

    // Returns the variable with the specified key, or null if the key is not found
    // You can then access the variable's value using the Value property. e.g.
    // BooleanVariable boolVar = flowchart.GetVariable<BooleanVariable>("MyBool");
    // boolVar.Value = false;
    public T GetVariable<T>(string key) where T : Variable
    {
        for (int i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            if (variable != null && variable.Key == key)
            {
                return variable as T;
            }
        }

        Debug.LogWarning("Variable " + key + " not found.");
        return null;
    }

    // Returns a list of variables matching the specified type
    public virtual List<T> GetVariables<T>() where T : Variable
    {
        var varsFound = new List<T>();

        for (int i = 0; i < Variables.Count; i++)
        {
            var currentVar = Variables[i];
            if (currentVar is T)
                varsFound.Add(currentVar as T);
        }

        return varsFound;
    }

    public Variable GetLocationVariable(LocationData locationData)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].GetType() == typeof(LocationVariable))
            {
                var locVar = variables[i] as LocationVariable;
                if (locVar != null && locVar.Value.InfoID == locationData.Value.InfoID)
                {
                    return locVar;
                }
            }
        }
        return null;
    }

    // Register a new variable with the Engine at runtime
    // The variable should be added as a component on the Engine game object
    public void SetVariable<T>(string key, T newvariable) where T : Variable
    {
        for (int i = 0; i < variables.Count; i++)
        {
            var v = variables[i];
            if (v != null && v.Key == key)
            {
                T variable = v as T;
                if (variable != null)
                {
                    variable = newvariable;
                    return;
                }
            }
        }

        Debug.LogWarning("Variable " + key + " not found.");
    }

    // Checks if a given variable exists in the Engine
    public virtual bool HasVariable(string key)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            var v = variables[i];
            if (v != null && v.Key == key)
            {
                return true;
            }
        }
        return false;
    }

    // Returns the list of variable names in the Engine
    public virtual string[] GetVariableNames()
    {
        var vList = new string[variables.Count];

        for (int i = 0; i < variables.Count; i++)
        {
            var v = variables[i];
            if (v != null)
            {
                vList[i] = v.Key;
            }
        }
        return vList;
    }

    /// Gets a list of all variables with public scope in this Engine
    public virtual List<Variable> GetPublicVariables()
    {
        var publicVariables = new List<Variable>();
        for (int i = 0; i < variables.Count; i++)
        {
            var v = variables[i];
            if (v != null && v.Scope == VariableScope.Public)
            {
                publicVariables.Add(v);
            }
        }

        return publicVariables;
    }

    // Gets the value of an integer variable
    public virtual int GetIntegerVariable(string key)
    {
        var variable = GetVariable<IntegerVariable>(key);
        if (variable != null)
        {
            return GetVariable<IntegerVariable>(key).Value;
        }
        else
        {
            return 0;
        }
    }

    // Sets the value of an integer variable
    // The variable must already be added to the list of variables in the Engine
    public virtual void SetIntegerVariable(string key, int value)
    {
        var variable = GetVariable<IntegerVariable>(key);
        if (variable != null)
        {
            variable.Value = value;
        }
    }

    // Gets the value of a boolean variable
    public virtual bool GetBooleanVariable(string key)
    {
        var variable = GetVariable<BooleanVariable>(key);
        if (variable != null)
        {
            return GetVariable<BooleanVariable>(key).Value;
        }
        else
        {
            return false;
        }
    }

    // Sets the value of a boolean variable
    // The variable must already be added to the list of variables in the Engine
    public virtual void SetBooleanVariable(string key, bool value)
    {
        var variable = GetVariable<BooleanVariable>(key);
        if (variable != null)
        {
            variable.Value = value;
        }
    }

    // Gets the value of a float variable
    public virtual float GetFloatVariable(string key)
    {
        var variable = GetVariable<FloatVariable>(key);
        if (variable != null)
        {
            return GetVariable<FloatVariable>(key).Value;
        }
        else
        {
            return 0f;
        }
    }

    // Sets the value of a float variable
    // The variable must already be added to the list of variables in the Engine
    public virtual void SetFloatVariable(string key, float value)
    {
        var variable = GetVariable<FloatVariable>(key);
        if (variable != null)
        {
            variable.Value = value;
        }
    }

    // Gets the value of a string variable
    public virtual string GetStringVariable(string key)
    {
        var variable = GetVariable<StringVariable>(key);
        if (variable != null)
        {
            return GetVariable<StringVariable>(key).Value;
        }
        else
        {
            return "";
        }
    }

    // Sets the value of a string variable
    // The variable must already be added to the list of variables in the Engine
    public virtual void SetStringVariable(string key, string value)
    {
        var variable = GetVariable<StringVariable>(key);
        if (variable != null)
        {
            variable.Value = value;
        }
    }

    public virtual void SetNodeState(string nodeName, ExecutionState state, bool completed)
    {
        var node = FindNode(nodeName);
        if (node != null)
        {
            //node.State = state;
            node.NodeComplete = completed;
        }
    }

    public virtual Postcard SetPostcard(PostcardVar postcard)
    {
        // Try to find the one that is being referenced to be saved
        Postcard selectedPostcard = postcards.FirstOrDefault(x => x.PostcardName == postcard.Name);
        // If this cannot be found then create a new one
        if (selectedPostcard == null)
        {
            selectedPostcard = this.AddComponent<Postcard>();
            this.postcards.Add(selectedPostcard);
        }
        selectedPostcard.PostcardName = postcard.Name;
        selectedPostcard.PostcardDesc = postcard.Desc;
        selectedPostcard.PostcardCreator = postcard.Creator;
        selectedPostcard.TotalStickers = postcard.Total;
        selectedPostcard.StickerVars = new List<PostcardVar.StickerVar>(postcard.StickerVars);

        return selectedPostcard;
    }

    public virtual Postcard SetPostcard(Postcard postcard)
    {
        // Try to find the one that is being referenced to be saved
        Postcard selectedPostcard = postcards.FirstOrDefault(x => x.PostcardName == postcard.PostcardName);
        // If this cannot be found then create a new one
        if (selectedPostcard == null)
        {
            selectedPostcard = this.AddComponent<Postcard>();
            this.postcards.Add(selectedPostcard);
        }
        selectedPostcard.PostcardName = postcard.PostcardName;
        selectedPostcard.PostcardDesc = postcard.PostcardDesc;
        selectedPostcard.PostcardCreator = postcard.PostcardCreator;
        selectedPostcard.TotalStickers = postcard.TotalStickers;

        var originalStickers = postcard.stickers;
        selectedPostcard.StickerVars.Clear();

        foreach (var original in originalStickers)
        {
            if (original != null)
            {
                var newStickerVar = new PostcardVar.StickerVar();
                newStickerVar.Name = original.StickerName;
                newStickerVar.Desc = original.StickerDescription;
                newStickerVar.Type = original.StickerType;
                newStickerVar.Image = original.StickerImage;
                newStickerVar.Position = original.StickerPosition;
                newStickerVar.StickerScale = original.StickerScale;
                newStickerVar.StickerRot = original.StickerRotation;

                selectedPostcard.StickerVars.Add(newStickerVar);
            }
        }

        return selectedPostcard;
    }

    public virtual void SetObjectInfo(string objectName, bool objectUnlocked)
    {
        foreach (var item in Resources.FindObjectsOfTypeAll<BaseInfo>())
        {
            if (item.ObjectName == objectName)
            {
                item.Unlocked = objectUnlocked;
            }
        }
    }

    public virtual void SetLocationInfo(string infoID, LoGaCulture.LUTE.LocationStatus status)
    {
        foreach (var item in GetComponents<LocationVariable>())
        {
            if (item.Value.InfoID == infoID)
            {
                item.Value.LocationStatus = status;
            }
        }
    }

    public virtual DiceVariable GetRandomDice()
    {
        for (int i = 0; i < variables.Count; i++)
        {
            var v = variables[i];
            if (v != null && v.GetType() == typeof(DiceVariable))
            {
                return v as DiceVariable;
            }
        }
        return null;
    }

    public virtual void UpdateHideFlags()
    {
        if (hideComponents)
        {
            var nodes = GetComponents<Node>();
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                node.hideFlags = HideFlags.HideInInspector;
                if (node.gameObject != gameObject)
                {
                    node.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            var orders = GetComponents<Order>();
            for (int i = 0; i < orders.Length; i++)
            {
                var order = orders[i];
                order.hideFlags = HideFlags.HideInInspector;
                if (order.gameObject != gameObject)
                {
                    order.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            var comps = GetComponents<EventHandler>();
            for (int i = 0; i < comps.Length; i++)
            {
                var comp = comps[i];
                comp.hideFlags = HideFlags.HideInInspector;
                if (comp.gameObject != gameObject)
                {
                    comp.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            var annotations = GetComponents<Annotation>();
            for (int i = 0; i < annotations.Length; i++)
            {
                var annotation = annotations[i];
                annotation.hideFlags = HideFlags.HideInInspector;
                if (annotation.gameObject != gameObject)
                {
                    annotation.hideFlags = HideFlags.HideInHierarchy;
                }
            }
        }
        else
        {
            var monoBehaviours = GetComponents<MonoBehaviour>();
            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                var monoBehaviour = monoBehaviours[i];
                if (monoBehaviour == null)
                {
                    continue;
                }
                monoBehaviour.hideFlags = HideFlags.None;
                monoBehaviour.gameObject.hideFlags = HideFlags.None;
            }
        }
    }

    public virtual void ClearSelectedOrders()
    {
        selectedOrders.Clear();
#if UNITY_EDITOR
        SelectedOrdersStale = true;
#endif
    }

    public virtual void AddSelectedOrder(Order order)
    {
        if (!selectedOrders.Contains(order))
        {
            selectedOrders.Add(order);
#if UNITY_EDITOR
            SelectedOrdersStale = true;
#endif
        }
    }
#if UNITY_EDITOR
    public virtual void ClearAnnotations()
    {
        var annotations = GetComponents<Annotation>();
        for (int i = 0; i < annotations.Length; i++)
        {
            var annotation = annotations[i];
            if (annotation != null)
            {
                Undo.DestroyObjectImmediate(annotation);
            }
        }
    }
#endif

    public virtual void AddSelectedNode(Node node)
    {
        if (!selectedNodes.Contains(node))
        {
            node.IsSelected = true;
            selectedNodes.Add(node);
        }
    }

    public virtual bool DeselectNode(Node node)
    {
        if (selectedNodes.Contains(node))
        {
            DeselectNodeNoCheck(node);
            return true;
        }
        return false;
    }

    public virtual void DeselectNodeNoCheck(Node node)
    {
        node.IsSelected = false;
        selectedNodes.Remove(node);
    }

    public void UpdateSelectedNodeCache()
    {
        selectedNodes.Clear();
        var res = gameObject.GetComponents<Node>();
        selectedNodes = res.Where(x => x.IsSelected).ToList();
    }

    public void ReverseUpdateSelectedCache()
    {
        for (int i = 0; i < selectedNodes.Count; i++)
        {
            if (selectedNodes[i] != null)
            {
                selectedNodes[i].IsSelected = true;
            }
        }
    }

    /// Override this in the engine subclass to filter which orders are shown in the Add order list.
    public virtual bool IsOrderSupported(OrderInfoAttribute commandInfo)
    {
        for (int i = 0; i < hideOrders.Count; i++)
        {
            // Match on category or command name (case insensitive)
            var key = hideOrders[i];
            if (String.Compare(commandInfo.Category, key, StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(commandInfo.OrderName, key, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
        }

        return true;
    }

    public virtual Node FindNode(string name)
    {
        var nodes = GetComponents<Node>();
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node._NodeName == name)
            {
                return node;
            }
        }

        return null;
    }

    /// Execute a child node in the engine.
    /// You can use this method in a UI event. e.g. to handle a button click.
    public virtual void ExecuteNode(string name)
    {
        var node = FindNode(name);

        if (node == null)
        {
            Debug.LogError("Node " + name + " does not exist");
            return;
        }

        if (!ExecuteNode(node))
        {
            Debug.LogWarning("Node " + name + " failed to execute");
        }
    }

    public virtual void StopNode(string name)
    {
        var node = FindNode(name);

        if (node == null)
        {
            Debug.LogError("Node " + name + " does not exist");
            return;
        }

        if (node.IsExecuting())
        {
            node.Stop();
        }
    }

    /// Execute a child node in the engine
    /// This version provides extra options to control how the node is executed
    /// Returns true if the node started execution            
    public virtual bool ExecuteNode(Node node, int orderIndex = 0, Action onComplete = null)
    {
        if (node == null)
        {
            Debug.LogError("Node must not be null");
            return false;
        }

        if (((Node)node).gameObject != gameObject)
        {
            Debug.LogError("Node must belong to the same gameobject as this Engine");
            return false;
        }

        // Can't restart a running node so have to wait until it's idle again
        if (node.IsExecuting())
        {
            Debug.LogWarning(node._NodeName + " cannot be called/executed as it is already running.");
            return false;
        }

        StartCoroutine(node.Execute(orderIndex, onComplete));

        return true;
    }

    /// Stop all executing Blocks in this Engine
    public virtual void StopAllNodes()
    {
        var nodes = GetComponents<Node>();
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node.IsExecuting())
            {
                node.Stop();
            }
        }
    }

    // Returns a new variable key that is guaranteed not to clash with any existing variable in the list
    public virtual string GetUniqueVariableKey(string originalKey, Variable ignoreVariable = null)
    {
        int suffix = 0;
        string baseKey = originalKey;

        // Only letters and digits allowed
        char[] arr = baseKey.Where(c => (char.IsLetterOrDigit(c) || c == '_')).ToArray();
        baseKey = new string(arr);

        // No leading digits allowed
        baseKey = baseKey.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

        // No empty keys allowed
        if (baseKey.Length == 0)
        {
            baseKey = "Var";
        }

        string key = baseKey;
        while (true)
        {
            bool collision = false;
            for (int i = 0; i < variables.Count; i++)
            {
                var variable = variables[i];
                if (variable == null || variable == ignoreVariable || variable.Key == null)
                {
                    continue;
                }
                if (variable.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    collision = true;
                    suffix++;
                    key = baseKey + suffix;
                }
            }

            if (!collision)
            {
                return key;
            }
        }
    }

    /// Clear the list of selected nodes
    public virtual void ClearSelectedNodes()
    {
        if (selectedNodes == null)
        {
            selectedNodes = new List<Node>();
        }

        for (int i = 0; i < selectedNodes.Count; i++)
        {
            var item = selectedNodes[i];

            if (item != null)
            {
                item.IsSelected = false;
            }
        }
        selectedNodes.Clear();
    }

    public virtual SpawnOnMap GetMap() // should be deleted once all refs are removed
    {
        var map = GetComponentInChildren<SpawnOnMap>();
        if (map == null)
        {
            map = FindFirstObjectByType<SpawnOnMap>();
            if (map == null)
            {
                Debug.LogError("No map found in scene or in children");
                return null;
            }
        }
        return map;
    }

    public virtual LUTEMapManager GetMapManager()
    {
        var mapManager = GetComponentInChildren<LUTEMapManager>();
        if (mapManager == null)
        {
            mapManager = GetComponent<LUTEMapManager>();
        }
        if (mapManager == null)
        {
            mapManager = FindFirstObjectByType<LUTEMapManager>();
        }
        if (mapManager == null)
        {
            Debug.LogError("No map manager found in scene or related to engine object");
            return null;
        }
        return mapManager;
    }

    public virtual AbstractMap GetAbstractMap()
    {
        var map = GetComponentInChildren<AbstractMap>();
        if (map == null)
        {
            map = FindFirstObjectByType<AbstractMap>();
            if (map == null)
            {
                Debug.LogError("No map found in scene or in children");
                return null;
            }
        }
        return map;
    }

    /// <summary>
    /// Substitute variables in the input text with the format {$VarName}
    /// This will first match with private variables in this Engine, and then
    /// with public variables in all Engines in the scene (and any component
    /// in the scene that implements StringSubstituter.ISubstitutionHandler).
    /// </summary>
    public virtual string SubstituteVariables(string input)
    {
        if (stringSubstituer == null)
        {
            stringSubstituer = new StringSubstituter();
        }

        // Use the string builder from StringSubstituter for efficiency.
        StringBuilder sb = stringSubstituer._StringBuilder;
        sb.Length = 0;
        sb.Append(input);

        // Instantiate the regular expression object.
        Regex r = new Regex(SubstituteVariableRegexString);

        bool changed = false;

        // Match the regular expression pattern against a text string.
        var results = r.Matches(input);
        for (int i = 0; i < results.Count; i++)
        {
            Match match = results[i];
            string key = match.Value.Substring(2, match.Value.Length - 3);
            // Look for any matching private variables in this Flowchart first
            for (int j = 0; j < variables.Count; j++)
            {
                var variable = variables[j];
                if (variable == null)
                    continue;
                if (variable.Key == key)
                {
                    string value = variable.ToString();
                    sb.Replace(match.Value, value);
                    changed = true;
                }
            }
        }

        // Now do all other substitutions in the scene
        changed |= stringSubstituer.SubstituteStrings(sb);

        if (changed)
        {
            return sb.ToString();
        }
        else
        {
            return input;
        }
    }

    public virtual void DetermineSubstituteVariables(string str, List<Variable> vars)
    {
        Regex r = new Regex(BasicFlowEngine.SubstituteVariableRegexString);

        // Match the regular expression pattern against a text string.
        var results = r.Matches(str);
        for (int i = 0; i < results.Count; i++)
        {
            var match = results[i];
            var v = GetVariable(match.Value.Substring(2, match.Value.Length - 3));
            if (v != null)
            {
                vars.Add(v);
            }
        }
    }

    #region IStringSubstituter implementation

    /// <summary>
    /// Implementation of StringSubstituter.ISubstitutionHandler which matches any public variable in the Engine.
    /// To perform full variable substitution with all substitution handlers in the scene, you should
    /// use the SubstituteVariables() method instead.
    /// </summary>
    public virtual bool SubstituteStrings(StringBuilder input)
    {
        // Instantiate the regular expression object.
        Regex r = new Regex(SubstituteVariableRegexString);

        bool modified = false;

        // Match the regular expression pattern against a text string.
        var results = r.Matches(input.ToString());
        for (int i = 0; i < results.Count; i++)
        {
            Match match = results[i];
            string key = match.Value.Substring(2, match.Value.Length - 3);
            // Look for any matching public variables in this Flowchart
            for (int j = 0; j < variables.Count; j++)
            {
                var variable = variables[j];
                if (variable == null)
                {
                    continue;
                }
                if (variable.Scope == VariableScope.Public && variable.Key == key)
                {
                    string value = variable.ToString();
                    input.Replace(match.Value, value);
                    modified = true;
                }
            }
        }

        return modified;
    }

    #endregion

#if UNITY_EDITOR
    public Variable AddVariable(object obj, string suggestedName, Node node = null)
    {
        System.Type t = obj as System.Type;
        if (t == null)
        {
            return null;
        }
        char[] arr = suggestedName.Where(c => (char.IsLetterOrDigit(c) || c == '_')).ToArray();
        suggestedName = new string(arr);

        // No leading digits allowed
        suggestedName = suggestedName.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

        var existingVariable = this.GetVariable(suggestedName);
        if (existingVariable != null)
        {
            return existingVariable;
        }

        Undo.RecordObject(this, "Add Variable");
        Variable newVariable = this.gameObject.AddComponent(t) as Variable;
        newVariable.Key = this.GetUniqueVariableKey(suggestedName);

        if (newVariable.GetType() == typeof(NodeVariable) && node != null)
        {
            newVariable.Apply(SetOperator.Assign, node);
            newVariable.Scope = VariableScope.Global;
        }

        //if suggested exists, then insert, if not just add
        if (existingVariable != null)
        {
            this.Variables.Insert(this.Variables.IndexOf(existingVariable) + 1, newVariable);
        }
        else
        {
            this.Variables.Add(newVariable);
        }

        // Because this is an async call, we need to force prefab instances to record changes
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);

        return newVariable;
    }

    public void AddBlueprint(BasicFlowEngine engineBP)
    {
        ComponentExtensions.AddBlueprint(gameObject, engineBP);
    }
#endif
}