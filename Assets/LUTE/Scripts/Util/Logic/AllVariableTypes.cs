//static cache of all variable types which are used by orders designed to work with variables
//new types created need to be added here and also anyvariable data and variable data pair
using LoGaCulture.LUTE;
using System.Collections.Generic;
using static BooleanVariable;
using static FloatVariable;

public static class AllVariableTypes
{
    public enum VariableAny
    {
        Any
    }

    public static readonly System.Type[] AllLogaVarTypes = new System.Type[]
    {
        typeof(IntegerVariable),
        typeof(LocationVariable),
        typeof(CollectionVariable),
        typeof(NodeCollectionVariable),
        typeof(NodeVariable),
        typeof(DiceVariable),
        typeof(BooleanVariable),
        typeof(FloatVariable),
        typeof(StringVariable),
        typeof(SpriteVariable),
        typeof(TimeOfDayVariable),
        typeof(SeasonVariable),
        typeof(UDateTimeVariable),
        typeof(UDateVariable),
        typeof(UTimeVariable),
        typeof(SaveKeyVariable),
    };
}

// Collection of every VariableData type, used in orders that are designed to
// support any and all types. Those command just have a AnyVariableData anyVar or
// an AnyVaraibleAndDataPair anyVarDataPair to encapsulate the more unpleasant parts.
// New types created need to be added to the list below and also to AllVariableTypes and AnyVaraibleAndDataPair
// Note; when using this in a command ensure that RefreshVariableCache is also handled for
// string var substitution

[System.Serializable]
public partial struct AnyVariableData
{
    public IntegerData integerData;
    public LocationData locationData;
    public CollectionData collectionData;
    public NodeCollectionData nodeCollectionData;
    public NodeData nodeData;
    public DiceData diceData;
    public BooleanData booleanData;
    public FloatData floatData;
    public StringData stringData;
    public SpriteData spriteData;
    public TimeOfDayData timeOfDayData;
    public SeasonData seasonData;
    public UDateTimeData uDateTimeData;
    public UDateData uDateData;
    public UTimeData uTimeData;
    public SaveKeyData saveKeyData;

    public bool HasReference(Variable variable)
    {
        return integerData.integerRef == variable ||
                locationData.locationRef == variable ||
                booleanData.booleanRef == variable ||
                   floatData.floatRef == variable ||
                   stringData.stringRef == variable;
    }
}

// Pairing of an AnyVariableData and an variable reference. Internal lookup for
// making the right kind of variable with the correct data in the AnyVariableData.
// This is the primary mechanism for hiding the ugly need to match variable to
// correct data type so we can perform comparisons and operations.

// New types created need to be added to the list below and also to AllVariableTypes and AnyVariableData
/// Note: to ensure use of RefreshVariableCacheHelper in commands, see SetVariable for example
[System.Serializable]
public class AnyVariableAndDataPair
{
    public class TypeActions
    {
        public TypeActions(string dataPropName,
                           System.Func<AnyVariableAndDataPair, ComparisonOperator, bool> comparer,
                           System.Func<AnyVariableAndDataPair, string> description,
                           System.Action<AnyVariableAndDataPair, SetOperator> set)
        {
            DataPropName = dataPropName;
            CompareFunc = comparer;
            DescriptionFunc = description;
            SetFunc = set;
        }

        public string DataPropName { get; set; }

        public System.Func<AnyVariableAndDataPair, ComparisonOperator, bool> CompareFunc;
        public System.Func<AnyVariableAndDataPair, string> DescriptionFunc;
        public System.Action<AnyVariableAndDataPair, SetOperator> SetFunc;
    }

    [VariableProperty(AllVariableTypes.VariableAny.Any)]
    [UnityEngine.SerializeField] public Variable variable;

    [UnityEngine.SerializeField] public AnyVariableData data;

    //need a static lookup function table for getting a function or string based on the type of variable
    //all new var types will need to be added below to be supported

    public static readonly Dictionary<System.Type, TypeActions> typeActionLookup = new Dictionary<System.Type, TypeActions>()
    {
            { typeof(IntegerVariable),
                new TypeActions( "integerData",
                    (anyVar, compareOperator) => { return anyVar.variable.Evaluate(compareOperator, anyVar.data.integerData.Value); },
                    (anyVar) => anyVar.data.integerData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.integerData.Value)) },
            { typeof(LocationVariable),
                new TypeActions( "locationData",
                    (anyVar, compareOperator) => { return anyVar.variable.Evaluate(compareOperator, anyVar.data.locationData.Value);},
                    (anyVar) => anyVar.data.locationData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.locationData.Value)) },
            { typeof(CollectionVariable),
                new TypeActions( "collectionData",
                    (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.collectionData.Value); },
                    (anyVar) => anyVar.data.collectionData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.collectionData.Value)) },
            { typeof(NodeCollectionVariable),
                new TypeActions( "nodeCollectionData",
                    (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.nodeCollectionData.total); },
                    (anyVar) => anyVar.data.nodeCollectionData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.nodeCollectionData.Value)) },
            { typeof(NodeVariable),
                new TypeActions( "nodeData",
                    (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.nodeData.Value); },
                    (anyVar) => anyVar.data.nodeData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.nodeData.Value)) },
            { typeof(DiceVariable),
                new TypeActions( "diceData",
                    (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.diceData.Value); },
                    (anyVar) => anyVar.data.diceData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.diceData.Value)) },
            { typeof(BooleanVariable),
                new TypeActions( "booleanData",
                    (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.booleanData.Value); },
                    (anyVar) => anyVar.data.booleanData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.booleanData.Value)) },
            { typeof(FloatVariable),
                new TypeActions( "floatData",
                    (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.floatData.Value); },
                    (anyVar) => anyVar.data.floatData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.floatData.Value)) },
            { typeof(StringVariable),
                new TypeActions( "stringData",
                    (anyVar, compareOperator) =>
                    {
                        var subbedRHS = anyVar.variable.GetEngine().SubstituteVariables(anyVar.data.stringData.Value);
                        return anyVar.variable.Evaluate(compareOperator, subbedRHS);
                    },
                    (anyVar) => anyVar.data.stringData.GetDescription(),
                    (anyVar, setOperator) =>
                    {
                        var subbedRHS = anyVar.variable.GetEngine().SubstituteVariables(anyVar.data.stringData.Value);
                        anyVar.variable.Apply(setOperator, subbedRHS);
                    })},
                        { typeof(SpriteVariable),
                new TypeActions( "spriteData",
                    (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.spriteData.Value); },
                    (anyVar) => anyVar.data.spriteData.GetDescription(),
                    (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.spriteData.Value)) },
                        { typeof(TimeOfDayVariable),
            new TypeActions( "timeOfDayData",
                (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.timeOfDayData.Value); },
                (anyVar) => anyVar.data.timeOfDayData.GetDescription(),
                (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.timeOfDayData.Value)) },
                        { typeof(SeasonVariable),
            new TypeActions( "seasonData",
                (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.seasonData.Value); },
                (anyVar) => anyVar.data.seasonData.GetDescription(),
                (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.seasonData.Value)) },
                        { typeof(UDateTimeVariable),
            new TypeActions( "uDateTimeData",
                (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.uDateTimeData.Value); },
                (anyVar) => anyVar.data.uDateTimeData.GetDescription(),
                (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.uDateTimeData.Value)) },
                        { typeof(UDateVariable),
            new TypeActions( "uDateData",
                (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.uDateData.Value); },
                (anyVar) => anyVar.data.uDateData.GetDescription(),
                (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.uDateData.Value)) },
                        { typeof(UTimeVariable),
            new TypeActions( "uTimeData",
                (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.uTimeData.Value); },
                (anyVar) => anyVar.data.uTimeData.GetDescription(),
                (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.uTimeData.Value)) },
                        { typeof(SaveKeyVariable),
            new TypeActions( "saveKeyData",
                (anyVar, compareOperator) => {return anyVar.variable.Evaluate(compareOperator, anyVar.data.saveKeyData.Value); },
                (anyVar) => anyVar.data.saveKeyData.GetDescription(),
                (anyVar, setOperator) => anyVar.variable.Apply(setOperator, anyVar.data.saveKeyData.Value)) }

    };

    public bool HasReference(Variable variable)
    {
        return variable == this.variable || data.HasReference(variable);
    }

#if UNITY_EDITOR
    public void RefreshVariableCacheHelper(BasicFlowEngine f, ref List<Variable> referencedVariables)
    {
        if (variable is StringVariable asStringVar && asStringVar != null && !string.IsNullOrEmpty(asStringVar.Value))
            f.DetermineSubstituteVariables(asStringVar.Value, referencedVariables);

        if (!string.IsNullOrEmpty(data.stringData.Value))
            f.DetermineSubstituteVariables(data.stringData.Value, referencedVariables);
    }
#endif

    public string GetDataDescription()
    {
        TypeActions ta = null;
        if (typeActionLookup.TryGetValue(variable.GetType(), out ta))
        {
            return ta.DescriptionFunc(this);
        }
        return "Null";
    }

    public bool Compare(ComparisonOperator compareOperator, ref bool compareResult)
    {
        TypeActions ta = null;
        if (typeActionLookup.TryGetValue(variable.GetType(), out ta))
        {
            compareResult = ta.CompareFunc(this, compareOperator);
            return true;
        }
        return false;
    }

    public void SetOp(SetOperator setOperator)
    {
        TypeActions ta = null;
        if (typeActionLookup.TryGetValue(variable.GetType(), out ta))
        {
            ta.SetFunc(this, setOperator);
        }
    }
}