using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
/// <summary>
/// Attribute class for orders.
/// </summary>
/// 
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class OrderInfoAttribute : Attribute
{
    /// <summary>
    /// Metadata atribute for the Order class. 
    /// </summary>
    /// <param name="category">The category to place this order in.</param>
    /// <param name="commandName">The display name of the order.</param>
    /// <param name="helpText">Help information to display in the inspector.</param>
    /// <param name="priority">If two order classes have the same name, the one with highest priority is listed. Negative priority removess the order from the list.</param>///

    public OrderInfoAttribute(string category, string orderName, string helpText, int priority = 0)
    {
        Category = category;
        OrderName = orderName;
        HelpText = helpText;
        Priority = priority;
    }

    public string Category { get; set; }
    public string OrderName { get; set; }
    public string HelpText { get; set; }
    public int Priority { get; set; }
}
public abstract class Order : MonoBehaviour
{
    protected int itemId = -1; // Invalid engine item id
    protected int indentLevel;

    /// Unique identifier for this order.
    /// Unique for this Engine.
    public virtual int ItemId { get { return itemId; } set { itemId = value; } }
    /// Indent depth of the current orders.
    /// Orders are indented inside If, While, etc. sections
    public virtual int IndentLevel { get { return indentLevel; } set { indentLevel = value; } }

    /// Index of the order in the parent node's order list
    public virtual int OrderIndex { get; set; } //sets to true when parent node is executing
    public virtual bool IsExecuting { get; set; }
    public virtual float ExecutingIconTimer { get; set; }
    public virtual Node ParentNode { get; set; }

    /// Called when this order starts execution.
    public virtual void OnEnter()
    { }

    /// Populates a list with the nodes that this order references (mostly used in next node and choice order)
    public virtual void GetConnectedNodes(ref List<Node> connectedNodes)
    { }

    public virtual void GetLocationVariables(ref List<LocationVariable> locationVariables)
    { }

    public virtual void GetConditions(ref List<ConditionExpression> conditions)
    { }

    public virtual LocationVariable GetOrderLocation()
    {
        //For copying and pasting orders between engines
        FieldInfo fieldInfo = null;
        foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (field.FieldType == typeof(LocationVariable))
            {
                fieldInfo = field;
                return (LocationVariable)field.GetValue(this);
            }
            if (field.FieldType == typeof(List<LocationVariable>) || field.FieldType == typeof(LocationVariable[]))
            {
                var collection = field.GetValue(this) as IEnumerable<LocationVariable>;
                return collection != null ? collection.FirstOrDefault() : null;
            }
        }
        // Find relevant if statements as they are not referencing location variables explicitly
        if (this.GetType() == typeof(If))
        {
            var ifNode = this as If;
            List<ConditionExpression> conditions = new List<ConditionExpression>();
            ifNode.GetConditions(ref conditions);
            foreach (var condition in ifNode.conditions)
            {
                if (condition.AnyVariable.variable != null && condition.AnyVariable.variable.GetType() == typeof(LocationVariable))
                {
                    return (LocationVariable)condition.AnyVariable.variable;
                }
            }
        }
        return null;
    }

    public virtual void SetLocationVariable(LocationVariable locationVariable)
    {
        var locVar = GetOrderLocation();
        if (locVar != null)
        {
            locVar = locationVariable;
        }
    }

    public virtual void StopParentNode()
    {
        OnExit();
        if (ParentNode != null)
        {
            ParentNode.Stop();
        }
    }

    /// Called when the parent node has been requested to stop executing, and
    /// this order is currently executing.
    /// Use this callback to terminate any asynchronous operations and 
    /// cleanup state so that the order is ready to execute again later on.
    public virtual void OnStopExecuting()
    { }

    public virtual BasicFlowEngine GetEngine()
    {
        var engine = GetComponent<BasicFlowEngine>();
        if (engine == null &&
            transform.parent != null)
        {
            engine = transform.parent.GetComponent<BasicFlowEngine>();
        }
        return engine;
    }

    public virtual void Execute()
    {
        OnEnter();
    }

    /// End execution of this order and continue execution at the next order (not specific).
    public virtual void Continue()
    {
        if (IsExecuting)
        {
            Continue(OrderIndex + 1);
        }
    }

    /// End execution of this order and continue execution at a specific order index.
    public virtual void Continue(int nextOrderIndex)
    {
        OnExit();
        if (ParentNode != null)
        {
            ParentNode.JumpToOrderIndex = nextOrderIndex;
        }
    }

    public virtual string GetLocationIdentifier()
    {
        return ParentNode.GetEngine().gameObject.name + ":" + ParentNode._NodeName + "." + this.GetType().Name + "#" + OrderIndex.ToString();
    }

    /// Returns the summary text to display in the order inspector.
    public virtual string GetSummary()
    {
        return "";
    }

    public virtual bool OpenNode()
    {
        return false;
    }

    public virtual bool CloseNode()
    {
        return false;
    }

    public virtual Color GetButtonColour()
    {
        return Color.white;
    }

    public virtual void OnOrderAdded(Node parentNode)
    { }
    public virtual void OnOrderRemoved(Node parentNode)
    { }

    /// Returns true if the specified property should be displayed in the inspector. 
    /// This is useful for hiding certain properties based on the value of another property.
    public virtual bool IsPropertyVisible(string propertyName)
    {
        return true;
    }

    /// Returns true if the specified property should be displayed as a reorderable list in the inspector.
    /// This only applies for array properties and has no effect for non-array properties.
    public virtual bool IsReorderableArray(string propertyName)
    {
        return false;
    }

    public virtual void OnExit()
    { }

    public virtual void OnReset()
    { }

    /// Returns the localization id for the Flowchart that contains this command.
    public virtual string GetEngineLocalizationId()
    {
        // If no localization id has been set then use the Flowchart name
        var engine = GetEngine();
        if (engine == null)
        {
            return "";
        }

        string localizationId = GetEngine().LocalizationId;
        if (localizationId.Length == 0)
        {
            localizationId = engine.gameObject.name;
        }

        return localizationId;
    }
    /// <summary>
    /// Returns true if this command references the variable.
    /// Used to highlight variables in the variable list when a command is selected.
    /// </summary>
    public virtual bool HasReference(Variable variable)
    {
        return false;
    }

    public virtual LocationVariable ReferencesLocation()
    {
        return null;
    }

    public virtual void OnValidate()
    {
#if UNITY_EDITOR

        RefreshVariableCache();
#endif
    }

#if UNITY_EDITOR
    protected List<Variable> referencedVariables = new List<Variable>();
    //used by var list adapter to highlight variables 
    public bool IsVariableReferenced(Variable variable)
    {
        return referencedVariables.Contains(variable) || HasReference(variable);
    }
    protected virtual void RefreshVariableCache()
    {
        referencedVariables.Clear();
    }
#endif
}
