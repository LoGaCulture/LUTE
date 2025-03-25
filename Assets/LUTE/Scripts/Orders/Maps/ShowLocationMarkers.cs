using LoGaCulture.LUTE;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[OrderInfo("Map",
             "Reveal Locations",
             "Reveals a series of location markers based on the locations provided - can be hidden again using the 'Hide Location Marker(s)' Order")]
[AddComponentMenu("")]
public class ShowLocationMarkers : Order
{
    [Tooltip("The locations of the markers to reveal.")]
    [VariableProperty(typeof(LocationVariable))]
    [SerializeField] protected LocationVariable[] locations;

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
        foreach (LocationVariable location in locations)
        {
            if (location.Value != null)
                mapManager.ShowLocationMarker(location);
        }
    }

    public override string GetSummary()
    {
        if (locations != null)
            return "Shows location markers at: " + locations.Length + " locations";

        return "Error: No locations provided.";
    }

    public override void GetLocationVariables(ref List<LocationVariable> locationVariables)
    {
        if (locations != null && locations.Count() >= 1)
        {
            foreach (LocationVariable location in locations)
            {
                if (location != null && location.Value != null)
                    locationVariables.Add(location);
            }
        }
    }

    public override bool HasReference(Variable variable)
    {
        bool hasReference = false;

        foreach (LocationVariable location in locations)
        {
            hasReference = location == variable || hasReference;
        }
        return hasReference;
    }

#if UNITY_EDITOR
    protected override void RefreshVariableCache()
    {
        base.RefreshVariableCache();

        if (locations != null)
        {
            foreach (LocationVariable location in locations)
            {
                if (location != null && location.Value != null)
                    GetEngine().DetermineSubstituteVariables(location.Key, referencedVariables);
            }
        }
    }
#endif
}
