using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Map",
        "Add Direction Waypoints",
        "Adds waypoints to the directions factory using LUTE location variables.")]
    public class AddDirectionWaypoints : Order
    {
        [Tooltip("The directions factory in which the waypoints are stored.")]
        [SerializeField] protected LUTEDirectionsFactory directionsFactory;
        [Tooltip("Whether to draw the directions from the player's location.")]
        [SerializeField] protected bool drawFromPlayer = true;
        [Tooltip("The locations to draw the directions for.")]
        [VariableProperty(typeof(LocationVariable))]
        [SerializeField] protected List<LocationVariable> locationVariables = new List<LocationVariable>();

        public override void OnEnter()
        {
            if (directionsFactory == null || locationVariables.Count <= 0)
            {
                Continue();
                return;
            }

            directionsFactory.ClearWaypoints();

            if (drawFromPlayer)
            {
                directionsFactory.AddWaypoint(GetEngine().GetMapManager().TrackerTransform());
            }

            foreach (var locationVariable in locationVariables)
            {
                if (locationVariable == null || locationVariable.Value == null)
                {
                    continue;
                }

                var mapManager = GetEngine().GetMapManager();

                var relatedMarkers = mapManager.GetMarkerTransforms(locationVariable);

                if (relatedMarkers != null && relatedMarkers.Count >= 1)
                {
                    foreach (var marker in relatedMarkers)
                    {
                        directionsFactory.AddWaypoint(marker);
                    }
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (directionsFactory == null || locationVariables.Count == 0)
                return $"    Error: No directions factory provided or location variables list is empty.";

            return $"    Adds waypoints to directions factory. Total variables: {locationVariables.Count}.";
        }
    }
}
