using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[VariableInfo("Other", "NodeCollection")]
[AddComponentMenu("")]
[System.Serializable]
public class NodeCollectionVariable : BaseVariable<NodeCollection>
{
    public override bool SupportsComparison()
    {
        return true;
    }
    public override bool Evaluate(ComparisonOperator comparisonOperator, object value)
    {
        bool condition = false;
        int completeCount = 0;

        switch (comparisonOperator)
        {
            case ComparisonOperator.Equals:
                {
                    for (int i = 0; i < Value.Count; i++)
                    {
                        var node = Value.Get(i) as Node;
                        if (node.NodeComplete == true)
                        {
                            completeCount++;
                        }
                    }
                }
                condition = completeCount == (int)value;
                break;
            case ComparisonOperator.NotEquals:
                {
                    for (int i = 0; i < Value.Count; i++)
                    {
                        var node = Value.Get(i) as Node;
                        if (node.NodeComplete == true)
                        {
                            completeCount++;
                        }
                    }
                }
                condition = completeCount != (int)value;
                break;
            case ComparisonOperator.GreaterThan:
                {
                    for (int i = 0; i < Value.Count; i++)
                    {
                        var node = Value.Get(i) as Node;
                        if (node.NodeComplete == true)
                        {
                            completeCount++;
                        }
                    }
                    condition = completeCount > (int)value;
                }
                break;
            case ComparisonOperator.GreaterThanOrEquals:
                {
                    for (int i = 0; i < Value.Count; i++)
                    {
                        var node = Value.Get(i) as Node;
                        if (node.NodeComplete == true)
                        {
                            completeCount++;
                        }
                    }
                    condition = completeCount >= (int)value;
                }
                break;
            case ComparisonOperator.LessThan:
                {
                    for (int i = 0; i < Value.Count; i++)
                    {
                        var node = Value.Get(i) as Node;
                        if (node.NodeComplete == true)
                        {
                            completeCount++;
                        }
                    }
                    condition = completeCount < (int)value;
                }
                break;
            case ComparisonOperator.LessThanOrEquals:
                {
                    for (int i = 0; i < Value.Count; i++)
                    {
                        var node = Value.Get(i) as Node;
                        if (node.NodeComplete == true)
                        {
                            completeCount++;
                        }
                    }
                    condition = completeCount <= (int)value;
                }
                break;
            default:
                Debug.LogError("The " + comparisonOperator.ToString() + " comparison operator is not valid.");
                break;
        }
        return condition;
    }
}

/// <summary>
/// Container for a Node Collection variable reference or constant value.
/// </summary>
[System.Serializable]
public struct NodeCollectionData
{
    [SerializeField]
    [VariableProperty("<Value>", typeof(NodeCollectionVariable))]
    public NodeCollectionVariable nodeCollectionRef;

    [SerializeField]
    public NodeCollection nodeCollectionVal;
    [SerializeField]
    public int total;

    public NodeCollectionData(NodeCollection v, int _total)
    {
        nodeCollectionVal = v;
        nodeCollectionRef = null;
        total = _total;
    }

    public static implicit operator NodeCollection(NodeCollectionData NodeCollectionData)
    {
        return NodeCollectionData.Value;
    }

    public NodeCollection Value
    {
        get { return (nodeCollectionRef == null) ? nodeCollectionVal : nodeCollectionRef.Value; }
        set { if (nodeCollectionRef == null) { nodeCollectionVal = value; } else { nodeCollectionRef.Value = value; } }
    }

    public string GetDescription()
    {
        if (nodeCollectionRef == null)
        {
            return total.ToString();
        }
        else
        {
            return nodeCollectionRef.Key;
        }
    }
}