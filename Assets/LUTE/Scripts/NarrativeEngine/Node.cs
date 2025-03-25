using LoGaCulture.LUTE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attribute class for Node properties - used to display Nodes in the inspector without the need for a custom editor
public sealed class NodePropertyAttribute : PropertyAttribute
{
    public NodePropertyAttribute(string defaultText)
    {
        this.defaultText = defaultText;
    }

    public string defaultText = "<None>";
}

public enum ExecutionState
{
    /// <summary> No order executing </summary>
    Idle,
    /// <summary> Executing an order </summary>
    Executing,
    /// <summary> All orders executed </summary>
    Complete
}

/// <summary>
/// Base class for all nodes.
/// </summary>
/// 
[ExecuteInEditMode]
[RequireComponent(typeof(BasicFlowEngine))]
[AddComponentMenu("")]
public class Node : MonoBehaviour
{
    [SerializeField] protected Rect nodeRect = new Rect(0, 0, 125, 40);
    [SerializeField] protected string nodeName = "Node";
    [SerializeField] protected string nodeDescription = "Basic building block of a narrative engine";
    [SerializeField] protected string groupDescription = "A series of nodes that can be executed as a group";
    [SerializeField] protected int itemId = 0;
    [SerializeField] protected List<Order> orderList = new List<Order>();
    [SerializeField] protected EventHandler eventHandler;
    [SerializeField] protected bool isGrouped;
    [SerializeField] protected int groupIndex = -1;
    [Tooltip("If true, allows this node to be repeated otherwise any return calls to this node will not execute it")]
    [SerializeField] protected bool repeatable = false;
    //[SerializeField] protected LocationVariable nodeLocation;
    [SerializeField] protected Color tint = Color.white;
    [SerializeField] protected bool useCustomTint = false;
    [SerializeField] protected float hoverStartTime = 0.0f;
    [Tooltip("The node that will be unlocked when this node is complete")]
    [SerializeField] protected Node targetUnlockNode;
    [Tooltip("The node that requires to be unlocked to execute this node")]
    [SerializeField] protected Node targetKeyNode;
    [Tooltip("Will show the description of the node if true")]
    [SerializeField] protected bool showDesc;
    [Tooltip("If true, the node will be saved when completed (persistent). Useful for ensuring location nodes are save upon completition")]
    [SerializeField] protected bool saveable = false;
    [Tooltip("Whether the location (whether this is in an EventHandler or condition on this Node) will update to complete when this Node completes")]
    [SerializeField] protected bool updateLocationStatusToComplete = true;

    protected int jumpToOrderIndex = -1;
    protected ExecutionState executionState;
    protected Action lastOnCompleteAction;
    protected bool executionInfoSet = false;
    protected int executionCount;
    protected Order activeOrder;
    public virtual float ExecutingIconTimer { get; set; }
    protected int prevActiveOrderIndex = -1;

    //for editor state only
    public bool IsControlSelected { get; set; } //local cache of being part of the control exclusion group


    #region Public members

    public virtual Rect _NodeRect { get { return nodeRect; } set { nodeRect = value; } }
    public virtual string _NodeName { get { return nodeName; } set { nodeName = value; } }
    public virtual int _ItemId { get { return itemId; } set { itemId = value; } }
    public virtual string _Description { get { return nodeDescription; } set { nodeDescription = value; } }
    public virtual string _GroupDescription { get { return groupDescription; } set { groupDescription = value; } }
    public virtual List<Order> OrderList { get { return orderList; } }
    /// Controls the next order to execute in the node execution coroutine
    public virtual int JumpToOrderIndex { set { jumpToOrderIndex = value; } }
    public virtual ExecutionState State { get { return executionState; } set { executionState = value; } }
    public virtual Order ActiveOrder { get { return activeOrder; } }
    public int PrevActiveOrderIndex { get { return prevActiveOrderIndex; } }
    /// An optional Event Handler which can execute the node when an event occurs.
    public virtual EventHandler _EventHandler { get { return eventHandler; } set { eventHandler = value; } }
    //public virtual LocationVariable NodeLocation { get { return nodeLocation; } set { nodeLocation = value; } }
    public virtual Node TargetUnlockNode { get { return targetUnlockNode; } set { targetUnlockNode = value; } }
    public virtual Node TargetKeyNode { get { return targetKeyNode; } set { targetKeyNode = value; } }
    public bool IsSelected { get; set; }    //local cache of selectedness
    public bool IsGrouped { get { return isGrouped; } set { isGrouped = value; } } //determines is a node is grouped
    public int GroupIndex { get { return groupIndex; } set { groupIndex = value; } } //determines is a node is grouped
    public Vector2 DefaultPos { get; set; } //used for grouping
    public bool ChangePos { get; set; } //used for grouping
    //If this is true then if one returns to this node it can be repeated otherwise it will never be called once it has already been called
    public bool CanExecuteAgain { get { return repeatable; } set { repeatable = value; } }
    public bool NodeComplete;
    public virtual Color Tint { get { return tint; } set { tint = value; } }
    public virtual bool UseCustomTint { get { return useCustomTint; } set { useCustomTint = value; } }
    public virtual float HoverStartTime { get { return hoverStartTime; } set { hoverStartTime = value; } }
    public virtual Node CurrentUnlockNode { get; set; }
    public virtual bool ShowDesc { get { return showDesc; } set { showDesc = value; } }
    public virtual bool Saveable { get { return saveable; } set { saveable = value; } }
    public bool ShouldCancel { get; set; }
    public virtual bool UpdateLocationStatusToComplete { get { return updateLocationStatusToComplete; } }

    protected virtual void Awake()
    {
        SetExecutionInfo();
        ChangePos = true;
    }

    protected virtual void SetExecutionInfo()
    {
        // Give each child order a reference back to its parent block and tell each order its index in the list
        int index = 0;
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order == null)
            {
                continue;
            }
            order.ParentNode = this;
            order.OrderIndex = index++;
        }

        UpdateIndentLevels();

        executionInfoSet = true;
    }

#if UNITY_EDITOR
    // The user can modify the order list order while playing in the editor,
    // so we keep the order indices updated every frame
    //There's no need to do this in player builds so we compile this bit out for those builds
    protected virtual void Update()
    {
        int index = 0;
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order == null)// Null entry will be deleted automatically later
            {
                continue;
            }
            order.OrderIndex = index++;
        }
    }

#endif

    public virtual BasicFlowEngine GetEngine()
    {
        return GetComponent<BasicFlowEngine>();
    }

    public virtual bool IsExecuting()
    {
        return executionState == ExecutionState.Executing;
    }

    /// <summary>
    /// Returns the number of times this node has executed.
    /// </summary>
    public virtual int GetExecutionCount()
    {
        return executionCount;
    }

    public virtual void StartExecution()
    {
        StartCoroutine(Execute());
    }
    public virtual IEnumerator Execute(int orderIndex = 0, Action onComplete = null)
    {
        ShouldCancel = false;

        // Wait until the node location is true (if it is set)
        //while (NodeLocation != null && !NodeLocation.Evaluate(ComparisonOperator.Equals, null))
        //{
        //    //set bool here to prevent node from executing
        //    yield return null;
        //}

        // Wait until the target unlock node is complete (if it is set)
        while (targetKeyNode != null && targetKeyNode.NodeComplete == false && targetKeyNode is not Group)
        {
            //set bool here to prevent node from executing
            yield return null;
        }

        // Wait until the target unlock group is complete (if it is set)
        while (targetKeyNode != null && targetKeyNode is Group && (targetKeyNode as Group).GroupComplete == false)
        {
            //set bool here to prevent node from executing
            yield return null;
        }

        // Do not execute if the node is not repeatable and has already been executed
        while (NodeComplete && !CanExecuteAgain)
        {
            yield return null;
        }

        if (executionState != ExecutionState.Idle)
        {
            Debug.LogWarning(_NodeName + " cannot be executed, it is already running.");
            yield break;
        }

        LogaManager.Instance.LogManager.Log(LoGaCulture.LUTE.Logs.LogLevel.Info, "Executing node: " + _NodeName);

        {
            lastOnCompleteAction = onComplete;

            if (!executionInfoSet)
            {
                SetExecutionInfo();
            }
            executionCount++;
            var executionCountAtStart = executionCount;

            var engine = GetEngine();
            executionState = ExecutionState.Executing;

#if UNITY_EDITOR

            // Select the executing block & the first command
            engine.SelectedNode = this;
            if (orderList.Count > 0)
            {
                engine.ClearSelectedOrders();
                engine.AddSelectedOrder(orderList[0]);
            }
#endif

            jumpToOrderIndex = orderIndex;

            int i = 0;
            while (true)
            {
                if (ShouldCancel)
                {
                    ReturnToIdle();
                    yield break;
                }

                if (jumpToOrderIndex > -1)
                {
                    i = jumpToOrderIndex;
                    jumpToOrderIndex = -1;
                }

                while (i < orderList.Count && (!orderList[i].enabled || orderList[i].GetType() == typeof(Comment)))
                {
                    i = orderList[i].OrderIndex + 1;
                }
                if (i >= orderList.Count)
                {
                    bool allOrdersChecked = true;

                    foreach (var _order in orderList)
                    {
                        if (_order.GetType() == typeof(If))
                        {
                            var ifOrder = _order as If;
                            if (ifOrder.EvaluateConditions() != true)
                            {
                                NodeComplete = false;
                                allOrdersChecked = false;
                                break;
                            }
                        }
                    }

                    if (allOrdersChecked)
                    {
                        NodeComplete = true;
                        SetComplete();
                    }
                    else
                    {
                        NodeComplete = false;
                    }
                    break;
                }

                if (activeOrder == null)
                    prevActiveOrderIndex = -1;
                else
                    prevActiveOrderIndex = activeOrder.OrderIndex;

                var order = orderList[i];
                activeOrder = order;

                if (engine.gameObject.activeInHierarchy)
                {
                    // Auto select a order in some situations
                    if ((engine.SelectedOrders.Count == 0 && i == 0) ||
                        (engine.SelectedOrders.Count == 1 && engine.SelectedOrders[0].OrderIndex == prevActiveOrderIndex))
                    {
                        engine.ClearSelectedOrders();
                        engine.AddSelectedOrder(orderList[i]);
                    }
                }

                order.IsExecuting = true;

#if UNITY_EDITOR
                try
                {
                    order.Execute();
                }
                catch (Exception)
                {
                    Debug.LogError("Rethrowing Exception thrown by:" + order.GetLocationIdentifier());
                    throw;
                }
#else
                order.Execute();
#endif

                // Wait until the executing order sets another order to jump to via Order.Continue()
                while (jumpToOrderIndex == -1)
                {
                    yield return null;
                }

                order.IsExecuting = false;
            }

            //Only allow idle state once the node completes if we are allowing it to be repeated
            if (State == ExecutionState.Executing && executionCountAtStart == executionCount && CanExecuteAgain || State == ExecutionState.Complete && executionCountAtStart == executionCount && CanExecuteAgain)
            {
                // This node has been restarted while it was executing, so we need to stop
                // executing the current order and start again from the beginning and prevent stopping future runs
                ReturnToIdle();
                yield break;
            }
        }
        //If any locations or nodes unlocked become false then we need to reset the node to idle
        ReturnToIdle();
        yield break;
    }

    //if you wish to force a node to complete, call this method as this will ensure the node will break out of the execution loop
    public virtual void ForceComplete()
    {
        executionState = ExecutionState.Complete;
    }

    private void SetComplete()
    {
        executionState = ExecutionState.Complete;

        // If saving is allowed then we save the node as complete
        if (saveable)
        {
            SaveNode();
        }
    }

    private void SaveNode()
    {
        string saveName = nodeName;
        string saveDesc = System.DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");

        var saveManager = LogaManager.Instance.SaveManager;
        saveManager.AddSavePoint(saveName, saveDesc, false);
    }

    //when we need to know if a node has been completed we can use a simple if to determine the state based on the node
    public virtual ExecutionState CheckExecutionState()
    {
        return executionState;
    }

    private void ReturnToIdle()
    {
        executionState = ExecutionState.Idle;
        activeOrder = null;
        NodeSignals.NodeEnd(this);

        if (lastOnCompleteAction != null)
        {
            lastOnCompleteAction();
        }
        lastOnCompleteAction = null;
    }

    public virtual void Stop()
    {
        // Tell the executing order to stop immediately
        if (activeOrder != null)
        {
            activeOrder.IsExecuting = false;
            activeOrder.OnStopExecuting();
        }

        // This will cause the execution loop to break on the next iteration
        jumpToOrderIndex = int.MaxValue;

        //force idle here so other orders that rely on node not executing are informed this frame rather than next
        ReturnToIdle();
        ShouldCancel = true;
    }


    public virtual void GetConnectedNodes(ref List<Node> connectedNodes)
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order != null)
            {
                order.GetConnectedNodes(ref connectedNodes);
            }
        }
    }

    public virtual void GetConditionOrders(ref List<Order> conditionOrders)
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order != null)
            {
                if (order.GetType() == typeof(If))
                {
                    var ifNode = order as If;
                    List<ConditionExpression> conditions = new List<ConditionExpression>();
                    ifNode.GetConditions(ref conditions);
                    foreach (var condition in ifNode.conditions)
                    {
                        //As long as the if we find does not use a node var (i.e. it is not related to unlocking)
                        if (condition.AnyVariable.variable != null && condition.AnyVariable.variable.GetType() != typeof(NodeVariable))
                        {
                            conditionOrders.Add(order);
                        }
                    }
                }
            }
        }
    }

    public virtual void GetLocationOrders(ref List<Order> locationOrders)
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order != null)
            {
                if (order.GetType() == typeof(If))
                {
                    var ifNode = order as If;
                    List<ConditionExpression> conditions = new List<ConditionExpression>();
                    ifNode.GetConditions(ref conditions);
                    foreach (var condition in ifNode.conditions)
                    {
                        if (condition.AnyVariable.variable != null && condition.AnyVariable.variable.GetType() == typeof(LocationVariable))
                        {
                            locationOrders.Add(order);
                        }
                    }
                }
            }
        }
        if (eventHandler != null && eventHandler.GetType() == typeof(ConditionalEventHandler))
        {
            var conditionalEventHandler = eventHandler as ConditionalEventHandler;
            foreach (var condition in conditionalEventHandler.Conditions)
            {
                var i = condition as If;
                foreach (var handlerExpression in i.conditions)
                {
                    if (handlerExpression.AnyVariable.variable != null && handlerExpression.AnyVariable.variable.GetType() == typeof(LocationVariable))
                    {
                        locationOrders.Add(condition);
                    }
                }
            }
        }

        // Finally, check to see if any of the orders explicity reference a location variable
        foreach (var order in orderList)
        {
            //order.GetLocationVariables(ref locationOrders);
        }
    }

    // Find all locations in any possible case on the Node.
    // You can override this method if you wish to find locations given a specific condition.
    public virtual void GetAllLocations(ref List<LocationVariable> nodeLocations)
    {
        if (eventHandler != null && eventHandler.GetType() == typeof(LocationClickEventHandler))
        {
            var locationClickEventHandler = eventHandler as LocationClickEventHandler;
            if (locationClickEventHandler.Location.Value != null)
            {
                if (!nodeLocations.Contains(locationClickEventHandler.Location.locationRef))
                    nodeLocations.Add(locationClickEventHandler.Location.locationRef);
            }
        }

        if (eventHandler != null && eventHandler.GetType() == typeof(ConditionalEventHandler))
        {
            var conditionalEventHandler = eventHandler as ConditionalEventHandler;
            foreach (var condition in conditionalEventHandler.Conditions)
            {
                var i = condition as If;
                foreach (var handlerExpression in i.conditions)
                {
                    if (handlerExpression.AnyVariable.variable != null)
                    {
                        var locationVar = handlerExpression.AnyVariable.variable as LocationVariable;
                        if (!nodeLocations.Contains(locationVar))
                            nodeLocations.Add(locationVar);
                    }
                }
            }
        }

        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order != null)
            {
                if (order.GetType() == typeof(If))
                {
                    var ifNode = order as If;
                    List<ConditionExpression> conditions = new List<ConditionExpression>();
                    ifNode.GetConditions(ref conditions);
                    foreach (var condition in ifNode.conditions)
                    {
                        if (condition.AnyVariable.variable != null)
                        {
                            var locationVar = condition.AnyVariable.variable as LocationVariable;
                            if (!nodeLocations.Contains(locationVar))
                                nodeLocations.Add(locationVar);
                        }
                    }
                }
                else
                {
                    order.GetLocationVariables(ref nodeLocations);
                }
            }
        }
    }

    // If we find a location variable that matches and we can update it to complete then we return true
    // Used to update visuals of a LocationMarker
    public virtual bool CanUpdateLocationToComplete(LocationVariable locationVariable)
    {
        // This node prevents the location from updating to complete
        if (!updateLocationStatusToComplete)
            return false;

        bool canUpdate = false;

        // We look for the location eventhandler first as it is most logical
        if (eventHandler != null && eventHandler.GetType() == typeof(LocationClickEventHandler))
        {
            var locationClickEventHandler = eventHandler as LocationClickEventHandler;
            if (locationClickEventHandler.Location.locationRef != null && locationClickEventHandler.Location.locationRef == locationVariable)
            {
                // If we find an event handler with a matching location variable and we are allowed to update this location info then we do so
                canUpdate = true;
            }
        }

        // If the location event handler has no matching location, check the conditional list event handler
        if (!canUpdate)
        {
            if (eventHandler != null && eventHandler.GetType() == typeof(ConditionalEventHandler))
            {
                var conditionalEventHandler = eventHandler as ConditionalEventHandler;
                foreach (var condition in conditionalEventHandler.Conditions)
                {
                    var i = condition as If;
                    foreach (var handlerExpression in i.conditions)
                    {
                        if (handlerExpression.AnyVariable.variable != null && handlerExpression.AnyVariable.variable.GetType() == typeof(LocationVariable))
                        {
                            var locationVar = handlerExpression.AnyVariable.variable as LocationVariable;
                            if (locationVar == locationVariable)
                            {
                                canUpdate = true;
                            }
                        }
                    }
                }
            }
        }

        // If we still cannot update (i.e., not matching event handlers, then check the orders)
        if (!canUpdate)
        {
            for (int i = 0; i < orderList.Count; i++)
            {
                var order = orderList[i];
                if (order != null)
                {
                    if (order.GetType() == typeof(If))
                    {
                        var ifNode = order as If;
                        List<ConditionExpression> conditions = new List<ConditionExpression>();
                        ifNode.GetConditions(ref conditions);
                        foreach (var condition in ifNode.conditions)
                        {
                            if (condition.AnyVariable.variable != null && condition.AnyVariable.variable.GetType() == typeof(LocationVariable))
                            {
                                var locationVar = condition.AnyVariable.variable as LocationVariable;
                                if (locationVar == locationVariable)
                                {
                                    canUpdate = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return canUpdate;
    }

    public virtual Node GetUnlockNode()
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order != null)
            {
                if (order.GetType() == typeof(UnlockNode))
                {
                    var unlockNode = order as UnlockNode;
                    var node = unlockNode.targetNode;
                    return node;
                }
            }
        }

        if (targetUnlockNode != null)
        {
            return targetUnlockNode;
        }

        return null;
    }

    public virtual Node GetKeyNode()
    {
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order != null)
            {
                if (order.GetType() == typeof(If))
                {
                    var ifNode = order as If;
                    List<ConditionExpression> conditions = new List<ConditionExpression>();
                    ifNode.GetConditions(ref conditions);
                    foreach (var condition in ifNode.conditions)
                    {
                        if (condition.AnyVariable.variable != null && condition.AnyVariable.variable.GetType() == typeof(NodeVariable))
                        {
                            var nodeVariable = condition.AnyVariable.variable as NodeVariable;
                            return nodeVariable.Value;
                        }
                    }
                }
            }
        }
        if (targetKeyNode != null)
        {
            return targetKeyNode;
        }
        return null;
    }

    //returns type of previously executing order
    public virtual Type GetPreviousActiveOrderType()
    {
        if (prevActiveOrderIndex >= 0 && prevActiveOrderIndex < orderList.Count)
        {
            return orderList[prevActiveOrderIndex].GetType();
        }

        return null;
    }

    public virtual int GetPreviousActiveOrderIndent()
    {
        if (prevActiveOrderIndex >= 0 && prevActiveOrderIndex < orderList.Count)
        {
            return orderList[prevActiveOrderIndex].IndentLevel;
        }

        return -1;
    }

    public virtual Order GetPreviousActiveOrder()
    {
        if (prevActiveOrderIndex >= 0 && prevActiveOrderIndex < orderList.Count)
        {
            return orderList[prevActiveOrderIndex];
        }

        return null;
    }

    public virtual void UpdateIndentLevels()
    {
        int indentLevel = 0;
        for (int i = 0; i < orderList.Count; i++)
        {
            var order = orderList[i];
            if (order == null)
            {
                continue;
            }
            if (order.CloseNode())
            {
                indentLevel--;
            }
            // Negative indent level is not permitted
            indentLevel = Math.Max(indentLevel, 0);
            order.IndentLevel = indentLevel;
            if (order.OpenNode())
            {
                indentLevel++;
            }
        }
    }
    #endregion
}
