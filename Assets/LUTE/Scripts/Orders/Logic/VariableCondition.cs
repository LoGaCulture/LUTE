using LoGaCulture.LUTE;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BooleanVariable;
using static FloatVariable;

//This class handles single comparison conditions - an implemented list is used for multiple conditions
[System.Serializable]
public class ConditionExpression
{
    [SerializeField] protected ComparisonOperator compareOperator;
    [SerializeField] protected AnyVariableAndDataPair anyVariable;

    public virtual AnyVariableAndDataPair AnyVariable
    {
        get { return anyVariable; }
    }
    public virtual ComparisonOperator CompareOperator
    {
        get { return compareOperator; }
    }

    public ConditionExpression() { }

    public ConditionExpression(AnyVariableAndDataPair anyVariable, ComparisonOperator compareOperator)
    {
        this.anyVariable = anyVariable;
        this.compareOperator = compareOperator;
    }
}

public abstract class VariableCondition : Condition, ISerializationCallbackReceiver
{
    public enum AnyOrAll
    {
        AnyOf_OR,
        AllOf_AND
    }

    [Tooltip("Choosing 'AnyOf' will give a true outcome if at least one condition is true. Opting for 'AllOf' will give a true outcome only when all conditions are true.")]
    [SerializeField] protected AnyOrAll anyOrAllCondition;
    [SerializeField] public List<ConditionExpression> conditions = new List<ConditionExpression>();

#if UNITY_EDITOR
    // // Called when the script is loaded or a value is changed in the inspector (Called in the editor only).
    public override void OnValidate()
    {
        base.OnValidate();

        if (conditions == null)
        {
            conditions = new List<ConditionExpression>();
        }

        if (conditions.Count == 0)
        {
            conditions.Add(new ConditionExpression());
        }
    }
#endif

    public override bool EvaluateConditions()
    {
        if (conditions == null || conditions.Count == 0)
        {
            return false;
        }
        bool resultAny = false, resultAll = true;
        foreach (ConditionExpression condition in conditions)
        {
            bool curResult = false;
            if (condition.AnyVariable == null)
            {
                resultAll &= curResult;
                resultAny |= curResult;
                continue;
            }
            condition.AnyVariable.Compare(condition.CompareOperator, ref curResult);
            resultAll &= curResult;
            resultAny |= curResult;
        }

        if (anyOrAllCondition == AnyOrAll.AnyOf_OR) return resultAny;


        return resultAll;
    }

    protected override bool HasRequiredProperties()
    {
        if (conditions == null || conditions.Count == 0)
        {
            return false;
        }

        foreach (ConditionExpression condition in conditions)
        {
            if (condition.AnyVariable == null || condition.AnyVariable.variable == null)
            {
                return false;
            }
        }
        return true;
    }

    public override string GetSummary()
    {
        if (!this.HasRequiredProperties())
        {
            return "Error: No variable selected";
        }

        string connector = "";
        if (anyOrAllCondition == AnyOrAll.AnyOf_OR)
        {
            connector = " <OR> ";
        }
        else
        {
            connector = " <AND> ";
        }

        StringBuilder summary = new StringBuilder("");
        for (int i = 0; i < conditions.Count; i++)
        {
            string dataDesc = conditions[i].AnyVariable.GetDataDescription();
            Variable var = conditions[i].AnyVariable.variable;
            if (var.GetType() == typeof(LocationVariable))
            {
                if (GetEngine().DemoMapMode)
                {
                    dataDesc = "Tracker location";
                }
                else
                {
                    dataDesc = "Device location";
                }
            }
            if (var.GetType() == typeof(SaveKeyVariable))
            {
                string key = conditions[i].AnyVariable.data.saveKeyData.Value;
                if (string.IsNullOrEmpty(key))
                {
                    dataDesc = "Save Manager Default Key (checks if saves exists)";
                }
            }
            summary.Append(conditions[i].AnyVariable.variable.Key + " " +
                       VariableUtil.GetCompareOperatorDescription(conditions[i].CompareOperator) + " " +
                       dataDesc);

            if (i < conditions.Count - 1)
            {
                summary.Append(connector);
            }
        }
        return summary.ToString();
    }

    public override bool HasReference(Variable variable)
    {
        bool hasReference = false;

        foreach (var condition in conditions)
        {
            hasReference |= condition.AnyVariable.HasReference(variable);
        }

        return hasReference;
    }

    public override LocationVariable ReferencesLocation()
    {
        foreach (ConditionExpression condition in conditions)
        {
            if (condition.AnyVariable.variable != null && condition.AnyVariable.variable.GetType() == typeof(LocationVariable))
            {
                return condition.AnyVariable.variable as LocationVariable;
            }
        }
        return null;
    }

    public override void GetConditions(ref List<ConditionExpression> conditions)
    {
        if (conditions.Count > 0)
        {
            conditions.AddRange(this.conditions);
        }
    }

#if UNITY_EDITOR
    protected override void RefreshVariableCache()
    {
        base.RefreshVariableCache();

        if (conditions != null)
        {
            foreach (var item in conditions)
            {
                item.AnyVariable.RefreshVariableCacheHelper(GetEngine(), ref referencedVariables);
            }
        }
    }
#endif

    #region backwards compat

    [HideInInspector]
    [SerializeField] protected ComparisonOperator compareOperator;

    [HideInInspector]
    [SerializeField] protected AnyVariableAndDataPair anyVariable;

    [Tooltip("Variable to use in expression")]
    [VariableProperty(AllVariableTypes.VariableAny.Any)]
    [SerializeField] protected Variable variable;

    [SerializeField] protected IntegerData integerData;
    [SerializeField] protected LocationData locationData;
    [SerializeField] protected CollectionData collectionData;
    [SerializeField] protected NodeCollectionData nodeCollectionData;
    [SerializeField] protected NodeData nodeData;
    [SerializeField] protected DiceData diceData;
    [SerializeField] protected BooleanData booleanData;
    [SerializeField] protected FloatData floatData;
    [SerializeField] protected StringData stringData;
    [SerializeField] protected SpriteData spriteData;
    [SerializeField] protected TimeOfDayData timeOfDayData;
    [SerializeField] protected SeasonData seasonData;
    [SerializeField] protected UDateTimeData uDateTimeData;
    [SerializeField] protected UDateData uDateData;
    [SerializeField] protected UTimeData uTimeData;
    [SerializeField] protected SaveKeyData saveKeyData;
    [SerializeField] protected InventoryData inventoryData;

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (variable != null)
        {
            anyVariable.variable = variable;

            if (variable.GetType() == typeof(IntegerVariable) && !integerData.Equals(new IntegerData()))
            {
                anyVariable.data.integerData = integerData;
                integerData = new IntegerData();
            }
            else if (variable.GetType() == typeof(LocationVariable) && !locationData.Equals(new LocationData()))
            {
                anyVariable.data.locationData = locationData;
                locationData = new LocationData();
            }
            else if (variable.GetType() == typeof(CollectionVariable) && !collectionData.Equals(new CollectionData()))
            {
                anyVariable.data.collectionData = collectionData;
                collectionData = new CollectionData();
            }
            else if (variable.GetType() == typeof(NodeCollectionVariable) && !nodeCollectionData.Equals(new NodeCollectionData()))
            {
                anyVariable.data.nodeCollectionData = nodeCollectionData;
                nodeCollectionData = new NodeCollectionData();
            }
            else if (variable.GetType() == typeof(NodeVariable) && !nodeData.Equals(new NodeData()))
            {
                anyVariable.data.nodeData = nodeData;
                nodeData = new NodeData();
            }
            else if (variable.GetType() == typeof(DiceVariable) && !diceData.Equals(new DiceData()))
            {
                anyVariable.data.diceData = diceData;
                diceData = new DiceData();
            }
            else if (variable.GetType() == typeof(BooleanVariable) && !booleanData.Equals(new BooleanData()))
            {
                anyVariable.data.booleanData = booleanData;
                booleanData = new BooleanData();
            }
            else if (variable.GetType() == typeof(FloatVariable) && !floatData.Equals(new FloatData()))
            {
                anyVariable.data.floatData = floatData;
                floatData = new FloatData();
            }
            else if (variable.GetType() == typeof(StringVariable) && !stringData.Equals(new StringData()))
            {
                anyVariable.data.stringData.stringRef = stringData.stringRef;
                anyVariable.data.stringData.stringVal = stringData.stringVal;
                stringData = new StringData();
            }
            else if (variable.GetType() == typeof(SpriteVariable) && !spriteData.Equals(new SpriteData()))
            {
                anyVariable.data.spriteData = spriteData;
                spriteData = new SpriteData();
            }
            else if (variable.GetType() == typeof(TimeOfDayVariable) && !timeOfDayData.Equals(new TimeOfDayData()))
            {
                anyVariable.data.timeOfDayData = timeOfDayData;
                timeOfDayData = new TimeOfDayData();
            }
            else if (variable.GetType() == typeof(SeasonVariable) && !seasonData.Equals(new SeasonData()))
            {
                anyVariable.data.seasonData = seasonData;
                seasonData = new SeasonData();
            }
            else if (variable.GetType() == typeof(UDateTimeVariable) && !uDateTimeData.Equals(new UDateTimeData()))
            {
                anyVariable.data.uDateTimeData = uDateTimeData;
                uDateTimeData = new UDateTimeData();
            }
            else if (variable.GetType() == typeof(UDateVariable) && !uDateData.Equals(new UDateTimeData()))
            {
                anyVariable.data.uDateData = uDateData;
                uDateTimeData = new UDateTimeData();
            }
            else if (variable.GetType() == typeof(UTimeVariable) && !uTimeData.Equals(new UTimeData()))
            {
                anyVariable.data.uTimeData = uTimeData;
                uTimeData = new UTimeData();
            }
            else if (variable.GetType() == typeof(SaveKeyVariable) && !saveKeyData.Equals(new SaveKeyData()))
            {
                anyVariable.data.saveKeyData = saveKeyData;
                saveKeyData = new SaveKeyData();
            }
            else if (variable.GetType() == typeof(InventoryVariable) && !inventoryData.Equals(new InventoryData()))
            {
                anyVariable.data.inventoryData = inventoryData;
                inventoryData = new InventoryData();
            }
            //moved to new anyvar storage, clear legacy.
            variable = null;
        }

        // just checking for anyVar != null fails here. is any var being reintilaized somewhere?

        if (anyVariable != null && anyVariable.variable != null)
        {
            ConditionExpression c = new ConditionExpression(anyVariable, compareOperator);
            if (!conditions.Contains(c))
            {
                conditions.Add(c);
            }

            anyVariable = null;
            variable = null;
        }
    }
    #endregion backwards compat
}