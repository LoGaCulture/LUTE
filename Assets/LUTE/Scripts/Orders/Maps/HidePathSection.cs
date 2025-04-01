using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Map",
    "Hide Path Section",
    "Hides part of a path between a pair of locations.")]
    public class HidePathSection : Order
    {
        [Tooltip("The directions factory in which the waypoints are stored.")]
        [SerializeField] protected LUTEDirectionsFactory directionsFactory;
        [Tooltip("The start location to start hiding the path from.")]
        [VariableProperty(typeof(LocationVariable))]
        [SerializeField] protected LocationVariable startLocation;
        [Tooltip("The end location to stop hiding the path at.")]
        [VariableProperty(typeof(LocationVariable))]
        [SerializeField] protected LocationVariable endLocation;

        public override void OnEnter()
        {
            if (directionsFactory == null || startLocation == null || endLocation == null)
            {
                Continue();
                return;
            }

            var mapManager = GetEngine().GetAbstractMap();
            if (mapManager == null)
            {
                Continue();
                return;
            }

            List<Vector3> sectionLocations = new List<Vector3>();
            Vector3 startLocationV3 = mapManager.GeoToWorldPosition(startLocation.Value.LatLongString());
            Vector3 endLocationV3 = mapManager.GeoToWorldPosition(endLocation.Value.LatLongString());
            sectionLocations.Add(startLocationV3);
            sectionLocations.Add(endLocationV3);

            directionsFactory.UpdatePathVisibility(sectionLocations, true);

            Continue();
        }

        public override string GetSummary()
        {
            string summary = "  Hiding path between ";
            if (startLocation == null || startLocation.Value == null)
            {
                return "    Error: No start location specified.";
            }

            if (endLocation == null || endLocation.Value == null)
            {
                return "    Error: No end location specified.";
            }

            summary += startLocation.Value.LocationName + " and " + endLocation.Value.LocationName;
            return summary;
        }
    }
}
