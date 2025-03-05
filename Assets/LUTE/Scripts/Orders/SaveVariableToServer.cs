using LoGaCulture.LUTE;
using UnityEngine;

[OrderInfo("Server",
              "Save Variable To Server",
              "Saves var to server")]
[AddComponentMenu("")]
public class SaveVariableToServer : Order
{


    [SerializeField] public AnyVariableAndDataPair variable = new AnyVariableAndDataPair();

    public override void OnEnter()
    {
        // Get the variable's name, type, and value
        string variableName = variable.variable.Key;
        string variableType = variable.variable.GetType().Name;
        object variableValue = GetVariableValue(variable.variable);

        // Send the variable data to the server
        LogaManager.Instance.ConnectionManager.SaveSharedVariable(variableName, variableType, variableValue.ToString());

        Debug.Log("Saved variable " + variableName + " with value " + variableValue + " and type " + variableType);

        Continue();
    }

    private object GetVariableValue(Variable variable)
    {
        var valueProperty = variable.GetType().GetProperty("Value");
        if (valueProperty != null)
        {
            return valueProperty.GetValue(variable);
        }
        return null;
    }

    public override string GetSummary()
    {
        return "";
    }
}