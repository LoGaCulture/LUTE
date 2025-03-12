using UnityEngine;

/// Base class for all condition orders
[AddComponentMenu("")]
public abstract class Condition : Order
{
    protected LogicEnd end;

    public override void OnEnter()
    {
        if (ParentNode == null)
        {
            return;
        }

        //if we are looping and have no end statement, stop the node
        if (StatementLooping && !LogicEndFound())
        {
            Debug.LogError(GetLocationIdentifier() + "is looping but has no end found statement");
            Continue();
            return;
        }

        if (!HasRequiredProperties())
        {
            Debug.LogError(GetLocationIdentifier() + "is missing required properties");
            Continue();
            return;
        }

        //if we are an else if, make sure we have a parent if and that it is not looping
        if (IsElseIf && !PassesElseIfSanityCheck())
        {
            Debug.LogError(GetLocationIdentifier() + "is an else if but does not have a parent if");
            GoToEnd();
            return;
        }

        EvaluateThenContinue();
    }

    public override bool OpenNode()
    {
        return true;
    }

    public virtual bool StatementLooping
    {
        get { return false; }
    }

    //Moves execution to the end of the statement if there is an end statement otherwises stops the node
    public virtual void GoToEnd()
    {
        if (end == null)
        {
            end = FindStatementEnd();
        }

        if (end != null)
        {
            //end found so stop looping and continue
            end.Loop = false;
            Continue(end.OrderIndex + 1);
        }
        else
        {
            //no end found so stop the node
            Debug.LogWarning(GetLocationIdentifier() + "has no end found statement");
            StopParentNode();
        }
    }
    protected LogicEnd FindStatementEnd()
    {
        return FindMatchingEnd(this);
    }

    //Checks if the end statement has been found - returns null if not
    public static LogicEnd FindMatchingEnd(Order order)
    {
        if (order.ParentNode == null)
            return null;

        int indent = order.IndentLevel;
        for (int i = order.OrderIndex + 1; i < order.ParentNode.OrderList.Count; i++)
        {
            Order nextOrder = order.ParentNode.OrderList[i];
            if (nextOrder.IndentLevel == indent)
            {
                if (nextOrder is LogicEnd)
                {
                    return nextOrder as LogicEnd;
                }
            }
            else if (order.IndentLevel < indent)
            {
                return null;
            }
        }
        return null;
    }

    //Finds the end for statements that require an end statement (e.g. if, while, for)
    protected virtual bool LogicEndFound()
    {
        if (end == null)
        {
            end = FindStatementEnd();

            if (end == null)
            {
                Debug.LogError(GetLocationIdentifier() + "has no end found statement so looping is illogical");
                return false;
            }
        }

        if (StatementLooping)
        {
            end.Loop = true;
            end.LoopBackIndex = OrderIndex;
        }
        return true;
    }

    //When entering, the condition will evaluate and continue to execution
    protected virtual void EvaluateThenContinue()
    {
        PreEvaluate();

        if (EvaluateConditions())
        {
            OnTrue();
        }
        else
        {
            OnFalse();
        }
    }

    //When condition returns true, this executes
    public virtual void OnTrue()
    {
        Continue();
    }

    //When condition returns false, this executes
    protected virtual void OnFalse()
    {
        if (StatementLooping)
        {
            GoToEnd();
            return;
        }
        //find next else if or else or end at the same indent level
        for (int i = OrderIndex + 1; i < ParentNode.OrderList.Count; i++)
        {
            Order nextOrder = ParentNode.OrderList[i];
            if (nextOrder == null)
            {
                continue;
            }
            //skip disabled or orders that are not related to gameplay
            if (!((Order)nextOrder).enabled || nextOrder.GetType() == typeof(Comment) || nextOrder.IndentLevel != indentLevel)
            {
                continue;
            }

            System.Type type = nextOrder.GetType();
            if (type == typeof(Else) || type == typeof(LogicEnd))
            {
                if (i >= ParentNode.OrderList.Count - 1)
                {
                    //final order in node will now stop
                    StopParentNode();
                }
                else
                {
                    Continue(nextOrder.OrderIndex + 1);
                    return;
                }
            }
            else if (type.IsSubclassOf(typeof(Condition)) && (nextOrder as Condition).IsElseIf)
            {
                //else if found so evaluate and continue
                Continue(i);
                return;
            }
        }
        //no else if or else found so stop the node
        StopParentNode();
    }

    public abstract bool EvaluateConditions();

    protected virtual bool HasRequiredProperties()
    {
        return true;
    }
    protected virtual bool IsElseIf
    {
        get { return false; }
    }
    // Called before EvaluateCondition, allowing for child classes to gather required data
    protected virtual void PreEvaluate() { }
    // Ensure that this condition didn't come from a non matching if/elif.
    protected virtual bool PassesElseIfSanityCheck()
    {
        System.Type prevOrderType = ParentNode.GetPreviousActiveOrderType();
        var prevOrderIndent = ParentNode.GetPreviousActiveOrderIndent();
        var prevOrder = ParentNode.GetPreviousActiveOrder();

        //handle our matching if or else if in the chain failing and moving to us,
        //  need to make sure it is the same indent level
        if (prevOrder == null ||
            prevOrderIndent != IndentLevel ||
            !prevOrderType.IsSubclassOf(typeof(Condition)) ||
            (prevOrder as Condition).StatementLooping)
        {
            return false;
        }

        return true;
    }

    public override Color GetButtonColour()
    {
        return Color.cyan;
    }
}