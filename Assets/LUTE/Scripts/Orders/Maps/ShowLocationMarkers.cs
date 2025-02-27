using LoGaCulture.LUTE;
using UnityEngine;

[OrderInfo("Map",
             "Reveal Locations",
             "Reveals a series of location markers based on the locations provided - can be hidden again using the 'Hide Location Marker(s)' Order")]
[AddComponentMenu("")]
public class ShowLocationMarkers : Order
{
    [Tooltip("The locations of the markers to reveal.")]
    [SerializeField] protected LocationData[] locations;

    private LUTEMapManager mapManager;
    public override void OnEnter()
    {
        var engine = GetEngine();

        if (engine == null)
        {
            Continue();
            return;
        }

        mapManager = engine.GetMapManager();

        if (mapManager == null)
        {
            Continue();
            return;
        }

        if (locations == null || locations.Length <= 0)
        {
            Continue();
            return;
        }

        //Wait a moment before hiding the locations to ensure the map has been loaded correctly
        Invoke("ShowLocations", 0.45f);

        Continue();
    }

    private void ShowLocations()
    {
        foreach (LocationData location in locations)
        {
            if (location.locationRef != null)
                mapManager.ShowLocationMarker(location.locationRef);
        }
    }

    public override string GetSummary()
    {
        if (locations != null)
            return "Shows location markers at: " + locations.Length + " locations";

        return "Error: No locations provided.";
    }

    public override bool HasReference(Variable variable)
    {
        bool hasReference = false;

        foreach (LocationData location in locations)
        {
            hasReference = location.locationRef == variable || hasReference;
        }
        return hasReference;
    }

#if UNITY_EDITOR
    protected override void RefreshVariableCache()
    {
        base.RefreshVariableCache();

        if (locations != null)
        {
            foreach (LocationData location in locations)
            {
                if (location.locationRef != null)
                    GetEngine().DetermineSubstituteVariables(location.locationRef.Key, referencedVariables);
            }
        }
    }
#endif
}
