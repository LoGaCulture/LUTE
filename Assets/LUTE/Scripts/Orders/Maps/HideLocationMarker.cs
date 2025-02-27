using UnityEngine;

[OrderInfo("Map",
             "Hide Location",
             "Hides a location marker based on the location provided - can be shown again using the 'Show Location Marker' Order")]
[AddComponentMenu("")]
public class HideLocationMarker : Order
{
    [Tooltip("The location of the marker to hide.")]
    [SerializeField] protected LocationData location;
    public override void OnEnter()
    {
        if (location.locationRef == null)
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
        mapManager.HideLocationMarker(location.locationRef);
    }

    public override string GetSummary()
    {
        if (location.locationRef != null)
            return "Hides location marker at: " + location.locationRef?.Key;

        return "Error: No location provided.";
    }

    public override bool HasReference(Variable variable)
    {
        return location.locationRef == variable || base.HasReference(variable);
    }

#if UNITY_EDITOR
    protected override void RefreshVariableCache()
    {
        base.RefreshVariableCache();

        if (location.locationRef != null)
        {
            GetEngine().DetermineSubstituteVariables(location.locationRef.Key, referencedVariables);
        }
    }
#endif
}
