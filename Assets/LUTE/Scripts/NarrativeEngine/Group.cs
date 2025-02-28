using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Group : Node
{
    [Tooltip("The nodes that are part of this group.")]
    [SerializeField] protected List<Node> groupedNodes = new List<Node>();
    [Tooltip("If true, the group will be complete after specific nodes have been completed.")]
    [SerializeField]
    protected bool setNodesToComplete;
    [Tooltip("The nodes that need to be completed to finish this group.")]
    [SerializeField] protected List<Node> nodesToComplete = new List<Node>();
    [Tooltip("The total number of nodes that need to be completed to finish this group.")]
    [SerializeField] protected int totalNodesToComplete = 1;
    [SerializeField] protected bool updatedGroupNodes = false;

    public virtual List<Node> GroupedNodes { get { return groupedNodes; } set { groupedNodes = value; } }
    public virtual bool SetNodesToComplete { get { return setNodesToComplete; } set { setNodesToComplete = value; } }
    public virtual List<Node> NodesToComplete { get { return nodesToComplete; } set { nodesToComplete = value; } }
    public virtual int TotalToComplete { get { return totalNodesToComplete; } set { totalNodesToComplete = value; } }
    public virtual bool IsMinimised { get; set; }
    public virtual bool UpdateGroupNodes { get { return updatedGroupNodes; } set { updatedGroupNodes = value; } }

    public Group()
    {
        // Default constructor
    }

    public void SetGroup(Group existingGroup)
    {
        // Assign values from the existing group
        _NodeRect = existingGroup.nodeRect;
        groupedNodes = existingGroup.GroupedNodes;
        setNodesToComplete = existingGroup.SetNodesToComplete;
        nodesToComplete = existingGroup.NodesToComplete;
        totalNodesToComplete = existingGroup.TotalToComplete;
        updatedGroupNodes = existingGroup.UpdateGroupNodes;
        IsMinimised = existingGroup.IsMinimised;
        UseCustomTint = existingGroup.UseCustomTint;
        tint = existingGroup.tint;
        nodeName = existingGroup.nodeName;
        nodeDescription = existingGroup.nodeDescription;
        groupDescription = existingGroup.groupDescription;
        repeatable = existingGroup.repeatable;
        targetKeyNode = existingGroup.targetKeyNode;
        targetUnlockNode = existingGroup.targetUnlockNode;
        showDesc = existingGroup.showDesc;

        //need to set this up
        //orderList = existingGroup.orderList;
        //eventHandler = existingGroup.eventHandler;
    }

    public virtual bool GroupComplete
    {
        get
        {
            if (!setNodesToComplete)
            {
                int completeCount = 0;
                for (int i = 0; i < groupedNodes.Count; i++)
                {
                    if (groupedNodes[i].NodeComplete)
                    {
                        completeCount++;
                    }
                }
                return completeCount >= totalNodesToComplete;
            }
            else if (nodesToComplete.Count > 0)
            {
                int completeCount = 0;
                for (int i = 0; i < nodesToComplete.Count; i++)
                {
                    if (nodesToComplete[i].NodeComplete)
                    {
                        completeCount++;
                    }
                }
                return completeCount >= nodesToComplete.Count;
            }
            else
                return true;
        }
    }
#if UNITY_EDITOR
    //Remove any relation the group had to the nodes that were inside of it
    public virtual void DisbandGroup(BasicFlowEngine engine, int id)
    {
        for (int i = 0; i < groupedNodes.Count; i++)
        {
            var node = groupedNodes[i];
            node.IsControlSelected = false;
            node.IsGrouped = false;
            node.GroupIndex = -1;

            //remove any locations that are shared with this group and the node
            //if (NodeLocation != null && NodeLocation == node.NodeLocation)
            //{
            //    node.NodeLocation = null;
            //}

            //remove any unlocking or locking nodes that are shared
            if (TargetUnlockNode != null && node.TargetUnlockNode == TargetUnlockNode)
            {
                node.TargetUnlockNode = null;
            }
            if (TargetKeyNode != null && node.TargetKeyNode == TargetKeyNode)
            {
                node.TargetKeyNode = null;
            }

            //reset repeatable to false if this is true
            if (CanExecuteAgain && node.CanExecuteAgain)
            {
                node.CanExecuteAgain = false;
            }

            if (node._EventHandler != null)
            {
                node._EventHandler.ParentNode = null;

                Undo.DestroyObjectImmediate(node._EventHandler);
            }
        }

        // remove the name from engine group names
        engine.groupnames.RemoveAt(id);

        // delete the related variable - better way to do this?
        var groupVar = engine.Variables.OfType<NodeCollectionVariable>().ToList().Find(x => groupedNodes.All(node => x.Value.Contains(node)));
        if (groupVar != null)
        {
            //remove the variable from the list of variables
            engine.Variables.Remove(groupVar);
            //destroy the variable component
            Undo.DestroyObjectImmediate(groupVar);
        }

        // destroy event handler
        if (eventHandler != null)
        {
            Undo.DestroyObjectImmediate(eventHandler);
        }
        // destroy all orders
        foreach (var order in orderList)
        {
            Undo.DestroyObjectImmediate(order);
        }

        // destroy related game object - better way to do this?
        var existingGroupObjs = engine.GetComponentsInChildren<NodeCollection>();
        foreach (NodeCollection groupColl in existingGroupObjs)
        {
            //if all nodes in the group obj are equal to the nodes provided here then the group obj already exists
            if (groupedNodes.All(x => groupColl.Contains(x)))
            {
                Undo.DestroyObjectImmediate(groupColl.gameObject);
                break;
            }
        }
        // remove the group from the story engine groups
        engine.Groups.Remove(this);

        foreach (var group in engine.Groups)
        {
            if (group != null)
            {
                foreach (var node in group.groupedNodes)
                {
                    // Decrement the other grouped nodes so they match when drawing boxes etc.
                    node.GroupIndex--;
                }
            }
        }

        // destroy this group
        Undo.DestroyObjectImmediate(this);
    }
#endif
}
