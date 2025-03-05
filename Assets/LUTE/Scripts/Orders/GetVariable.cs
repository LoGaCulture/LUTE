using LoGaCulture.LUTE;
using UnityEngine;

[OrderInfo("Server",
              "Get Variable From Server",
              "Get shared variable from server")]
[AddComponentMenu("")]
public class GetVariable : Order
{
    [SerializeField]
    protected AnyVariableAndDataPair variable = new AnyVariableAndDataPair();

    public override void OnEnter()
    {
        // Get the variable's name and type
        string variableName = variable.variable.Key;
        string variableType = variable.variable.GetType().Name;

        // Fetch the variable from the server
        LogaManager.Instance.ConnectionManager.FetchSharedVariables(variableName, callback: (variables) =>
        {
            foreach (var serverVariable in variables)
            {
                // Update the local variable with the fetched value
                //SetVariableValue(variable.variable, serverVariable.data);
            }

            // Continue execution after updating the variable
            Continue();
        });
    }

    private void SetVariableValue(Variable localVariable, string value)
    {
        var valueProperty = localVariable.GetType().GetProperty("Value");
        if (valueProperty != null)
        {
            // Convert the string value to the appropriate type
            object convertedValue = ConvertValue(valueProperty.PropertyType, value);
            if (convertedValue != null)
            {
                valueProperty.SetValue(localVariable, convertedValue);
            }
        }
    }

    private object ConvertValue(System.Type targetType, string value)
    {
        try
        {
            if (targetType == typeof(int))
                return int.Parse(value);
            else if (targetType == typeof(float))
                return float.Parse(value);
            else if (targetType == typeof(bool))
                return bool.Parse(value);
            else if (targetType == typeof(string))
                return value;
            else if (targetType.IsEnum)
                return System.Enum.Parse(targetType, value);
            else
                return null;
        }
        catch
        {
            Debug.LogWarning($"Failed to convert '{value}' to type {targetType.Name}");
            return null;
        }
    }

    public override string GetSummary()
    {
        return "";
    }
}
