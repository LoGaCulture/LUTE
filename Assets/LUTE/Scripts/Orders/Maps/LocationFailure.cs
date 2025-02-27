using Mapbox.Unity.Location;
using Mapbox.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Map",
                 "Location Failure",
                 "Determines location automatically or provided by designer/player and updates the inaccesible location with an accesible one")]
    [AddComponentMenu("")]
    public class LocationFailure : Order
    {
        [Tooltip("The location that is inaccesible - if left empty the nearest location to the player is chosen.")]
        [SerializeField] protected LocationVariable location;

        ILocationProvider _locationProvider;
        ILocationProvider LocationProvider
        {
            get
            {
                if (_locationProvider == null)
                {
                    _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
                }

                return _locationProvider;
            }
        }

        public override void OnEnter()
        {
            var engine = GetEngine();
            if (engine == null)
            {
                Continue();
                return;
            }
            var locationFailureHandler = engine.GetComponent<LocationFailureHandler>();
            if (locationFailureHandler == null)
            {
                Continue();
                return;
            }
            if (location != null)
            {
                // If location is provided by designer/player
                // Do the location failure handling based on this location
                var loc2d = location.Value.LatLongString();
                if (loc2d != Vector2d.zero)
                {
                    locationFailureHandler.HandleFailure(loc2d);
                    //LocationFailureHandler.HandleFailure(loc2d);
                }
                Continue();
                return;
            }
            var mapManager = engine.GetMapManager();
            if (mapManager == null)
            {
                Continue();
                return;
            }
            var tracker = mapManager.TrackerPos();
            if (tracker == null)
            {
                Continue();
                return;
            }
            if (LocationProvider == null)
            {
                Continue();
                return;
            }

            var trackerPos = tracker;
            var deviceLoc = LocationProvider.CurrentLocation.LatitudeLongitude;

            // Find the closest location variable to either tracker pos or device loc

            Vector2d closestLocation = Vector2d.zero;

            if (engine.DemoMapMode)
            {
                // Using demo map mode so use dummy
                if (trackerPos != null)
                {
                    closestLocation = FindClosestLocation(trackerPos, engine);
                }
            }
            else
            {
                // Use the actual location of the device
                if (deviceLoc != null)
                {
                    closestLocation = FindClosestLocation(deviceLoc, engine);
                }
            }
            // Do the location failure handling based on the closest location
            if (closestLocation != Vector2d.zero)
            {
                locationFailureHandler.HandleFailure(closestLocation);

                //LocationFailureHandler.HandleFailure(closestLocation);
            }
            Continue();
        }

        private Vector2d FindClosestLocation(Vector2d location, BasicFlowEngine engine)
        {
            // Needs to take into account the failed locations and if they have been turned off
            if (engine == null)
            {
                Debug.LogWarning("Engine is null");
                return Vector2d.zero;
            }
            var locations = engine.GetComponents<LocationVariable>();
            if (locations == null || locations.Length == 0)
            {
                Debug.LogWarning("No locations found");
                return Vector2d.zero;
            }

            var failureHandler = engine.GetComponent<LocationFailureHandler>();
            if (failureHandler == null)
            {
                Debug.LogWarning("No Failure Handler in place!");
                return Vector2d.zero;
            }

            Vector2d closestPoint = Vector2d.zero;
            double closestDistance = double.MaxValue;

            foreach (var loc in locations)
            {
                Vector2d locPosition = loc.Value.LatLongString();
                if (locPosition == Vector2d.zero || locPosition == null)
                {
                    continue;
                }

                // Check if this location has already been handled by the failure handler
                bool isHandled = failureHandler.FailureMethods.Any(method =>
                    method.QueriedLocation != null && Equals(method.QueriedLocation.Value, loc.Value) && method.IsHandled);

                if (isHandled)
                {
                    continue; // Skip this location if it has been handled
                }

                double distance = CalculateDistance(location, locPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = locPosition;
                }

            }

            return closestPoint;
        }

        private double CalculateDistance(Vector2d point1, Vector2d point2)
        {
            const double EarthRadiusKm = 6371.0;
            double lat1 = DegreesToRadians(point1.y);
            double lon1 = DegreesToRadians(point1.x);
            double lat2 = DegreesToRadians(point2.y);
            double lon2 = DegreesToRadians(point2.x);

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public override string GetSummary()
        {
            if (location != null)
                return "Location Failure at: " + location?.Key;

            return "    Location Failure at automatic location provided by device.";
        }
    }
}
