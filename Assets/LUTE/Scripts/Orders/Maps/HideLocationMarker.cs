using System.Collections.Generic;
using UnityEngine;

[OrderInfo("Map",
             "Hide Location",
             "Hides a location marker based on the location provided - can be shown again using the 'Show Location Marker' Order")]
[AddComponentMenu("")]
public class HideLocationMarker : Order
{
    [Tooltip("The location of the marker to hide.")]
    [VariableProperty(typeof(LocationVariable))]
    [SerializeField] protected LocationVariable location;
    public override void OnEnter()
    {
        if (location.Value == null)
        {
            Continue();
            return;
        }

        HideLocation();

        Continue();
    }

    private void HideLocation()
    {
        var engine = GetEngine();

        if (engine == null)
        {
            Continue();
            return;
        }

        var mapManager = engine.GetMapManager();

        if (mapManager == null)
        {
            Continue();
            return;
        }
        mapManager.HideLocationMarker(location);
    }

    public override string GetSummary()
    {
        if (location != null)
            return "Hides location marker at: " + location?.Key;

        return "Error: No location provided.";
    }

    public override void GetLocationVariables(ref List<LocationVariable> locationVariables)
    {
        if (location != null && location.Value != null)
        {
            locationVariables.Add(location);
        }
    }

    public override bool HasReference(Variable variable)
    {
        return location == variable || base.HasReference(variable);
    }

#if UNITY_EDITOR
    protected override void RefreshVariableCache()
    {
        base.RefreshVariableCache();

        if (location != null)
        {
            GetEngine().DetermineSubstituteVariables(location.Key, referencedVariables);
        }
    }
#endif
}
