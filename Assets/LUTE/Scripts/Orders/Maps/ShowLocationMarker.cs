using UnityEngine;

[OrderInfo("Map",
             "Reveal Location",
             "Reveals a location marker based on the location provided - can be hidden again using the 'Hide Location Marker' Order")]
[AddComponentMenu("")]
public class ShowLocationMarker : Order
{
    [Tooltip("The location of the marker to show.")]
    [SerializeField] protected LocationData location;
    public override void OnEnter()
    {
        if (location.locationRef == null)
        {
            Continue();
            return;
        }

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

        mapManager.ShowLocationMarker(location.locationRef);
        Continue();
    }

    public override string GetSummary()
    {
        if (location.locationRef != null)
            return "Shows location marker at: " + location.locationRef?.Key;

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