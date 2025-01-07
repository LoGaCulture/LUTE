using Mapbox.Unity.Location;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[OrderInfo("XR",
              "PlaceObjectAtLocation",
              "Place an object at the specified location")]
[AddComponentMenu("")]
public class XRObjectAtLocation : Order
{
    [Tooltip("Where this item will be placed on the map")]
    [SerializeField] private LocationVariable _objectLocation;

    [Tooltip("The object to place at the location")]
    [SerializeField] private GameObject _objectToPlace;

    [Tooltip("The name of the object to place at the location")]
    [SerializeField] private string _objectName;

    [SerializeField] private float _placementRadius = 3.0f;

    [SerializeField] private float _raycastHeightOffset = 2.0f;


    private Vector2 _sessionStartLatLon;
    private bool _sessionStartLocationKnown = false;


    private bool _objectPlaced = false;

    private GameObject _xrObject;
    private ARRaycastManager _arRaycastManager;
    private ARPlaneManager _planeManager;
    private List<ARRaycastHit> _arRaycastHits = new List<ARRaycastHit>();
    private bool _locationInitialized = false;

    private const float EarthRadius = 6378137f; // Earth's radius in meters

    public override void OnEnter()
    {



        _xrObject = XRManager.Instance.GetXRObject();


        Debug.Log("Placing object at location...");

        if (_xrObject == null)
        {
            Debug.LogError("XR Object not initialized.");
            Continue();
            return;
        }

        _planeManager = _xrObject.GetComponentInChildren<ARPlaneManager>();
        _arRaycastManager = _xrObject.GetComponentInChildren<ARRaycastManager>();

        if(_planeManager == null || _arRaycastManager == null)
        {
            Debug.LogError("ARPlaneManager or ARRaycastManager not found in XR Object.");
            Continue();
            return;
        }

        //save it here or probably at the start of the app
        if(Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running)
        {
            _sessionStartLatLon = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
            _sessionStartLocationKnown = true;
        }
        else
        {
            Debug.LogWarning("Location services are not enabled or running. The session start location will not be saved.");
        }

        StartCoroutine(InitializeLocationServices());
    }

    private IEnumerator InitializeLocationServices()
    {

        if(Input.location.status ==LocationServiceStatus.Running)
        {
            _locationInitialized = true;
            StartCoroutine(CheckProximityAndPlace());

            Debug.Log($"Location initialized: {Input.location.lastData.latitude}, {Input.location.lastData.longitude}");

            yield break;
        }

        // Check if location services are enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Location services are not enabled by the user.");
            Continue();
            yield break;
        }

        // Start location services
        Input.location.Start();

        // Wait for location services to initialize
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Check if initialization timed out
        if (maxWait < 1)
        {
            Debug.LogError("Location services initialization timed out.");
            Continue();
            yield break;
        }

        // Check if connection failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Failed to initialize location services.");
            Continue();
            yield break;
        }

        _locationInitialized = true;
        Debug.Log($"Location initialized: {Input.location.lastData.latitude}, {Input.location.lastData.longitude}");

        if(!_sessionStartLocationKnown)
        {
            _sessionStartLatLon = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
            _sessionStartLocationKnown = true;
        }

        // Proceed to check proximity to target location
        StartCoroutine(CheckProximityAndPlace());
    }

    private IEnumerator CheckProximityAndPlace()
    {

        Debug.Log("Checking proximity to target location...");

        // Keep checking until we place or user quits
        while (!_objectPlaced)
        {
            if (IsWithinTargetRadius())
            {
                // We are within the radius; let's do the combined approach
                Debug.Log("Within target radius; placing object using plane detection + GPS coordinate.");
                PlaceObjectUsingCombinedApproach();
                yield break; // stop checking
            }
            else
            {
                Debug.Log("Not in range. Checking again in 5s...");
                yield return new WaitForSeconds(5);
            }
        }
    }


    private IEnumerator CheckProximityToTargetLocation()
    {
        // Wait until the device is within the target radius
        while (true)
        {
            if (IsWithinTargetRadius())
            {
                Debug.Log("Device is within target radius.");
                InitializePlaneDetection();
                yield break;
            }
            else
            {
                Debug.Log("Device is not within target radius. Waiting...");
                yield return new WaitForSeconds(5); // Wait for 5 seconds before checking again
            }
        }
    }

    private bool IsWithinTargetRadius()
    {
        Vector2d targetLatLon = _objectLocation.Value.LatLongString();
        Vector2d deviceLatLon = new Vector2d(Input.location.lastData.latitude, Input.location.lastData.longitude);

        float distance = GetDistanceInMeters(targetLatLon, deviceLatLon);

        Debug.Log($"Distance to target: {distance} meters");

        return distance <= _placementRadius;
    }

    private float GetDistanceInMeters(Vector2d point1, Vector2d point2)
    {
        // Haversine formula
        double lat1Rad = point1.x * Mathf.Deg2Rad;
        double lat2Rad = point2.x * Mathf.Deg2Rad;
        double deltaLat = (point2.x - point1.x) * Mathf.Deg2Rad;
        double deltaLon = (point2.y - point1.y) * Mathf.Deg2Rad;

        double a = Mathf.Sin((float)(deltaLat / 2)) * Mathf.Sin((float)(deltaLat / 2)) +
                   Mathf.Cos((float)lat1Rad) * Mathf.Cos((float)lat2Rad) *
                   Mathf.Sin((float)(deltaLon / 2)) * Mathf.Sin((float)(deltaLon / 2));

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        double distance = EarthRadius * c;

        return (float)distance;
    }

    private void PlaceObjectUsingCombinedApproach()
    {
        if (!_sessionStartLocationKnown)
        {
            Debug.LogWarning("Session start location is unknown; can't compute offset. Just place on first plane?");
            // fallback: just place on first plane we detect
            // Or do something else
            _planeManager.planesChanged += PlaceOnFirstPlane;
            return;
        }

        // 1) Compute the local AR offset for the target lat/lon
        Vector2d targetLatLon = _objectLocation.Value.LatLongString();
        Vector3 localPos = ConvertLatLonToARPosition(targetLatLon);

        // 2) Raycast downward from (localPos.x, localPos.y+_raycastHeightOffset, localPos.z)
        Vector3 raycastOrigin = localPos + Vector3.up * _raycastHeightOffset;
        Ray downwardRay = new Ray(raycastOrigin, Vector3.down);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (_arRaycastManager.Raycast(downwardRay, hits, TrackableType.Planes))
        {
            // If we hit a plane, place the object at that plane’s pose
            var hit = hits[0];
            Pose hitPose = hit.pose;
            SpawnObject(hitPose.position, hitPose.rotation);
        }
        else
        {
            Debug.Log("No plane detected directly below that GPS position; will try planesChanged fallback.");

            _planeManager.planesChanged += OnPlanesChanged;
        }
    }

    private void PlaceOnFirstPlane(ARPlanesChangedEventArgs args)
    {
        if (_objectPlaced) return;

        if (args.added != null && args.added.Count > 0)
        {
            var plane = args.added[0];
            SpawnObject(plane.transform.position, plane.transform.rotation);
            _planeManager.planesChanged -= PlaceOnFirstPlane;
        }
    }

    private void InitializePlaneDetection()
    {
        var planeManager = _xrObject.GetComponentInChildren<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager not found in XR Object.");
            Continue();
            return;
        }

        _arRaycastManager = _xrObject.GetComponentInChildren<ARRaycastManager>();
        if (_arRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager not found in XR Object.");
            Continue();
            return;
        }

        // Subscribe to planesChanged event
        planeManager.planesChanged += OnPlanesChanged;
        Debug.Log("Plane detection initialized.");
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (args.added != null && args.added.Count > 0)
        {
            // Unsubscribe after first plane detection
            var planeManager = _xrObject.GetComponentInChildren<ARPlaneManager>();
            if (planeManager != null)
            {
                planeManager.planesChanged -= OnPlanesChanged;
            }

            // Perform raycast to place object
            TryPlaceObject();
        }
    }

    private Vector3 ConvertLatLonToARPosition(Vector2d targetLatLon)
    {
        // 1) Distance in meters from "session start lat/lon" to target lat/lon
        float distMetersX, distMetersZ;
        // A rough approach is to treat changes in latitude as deltaZ and changes in longitude as deltaX
        // or vice versa, depending on your coordinate system preference.

        float latScale = 111320f;   // meters per degree of latitude (approx)
        float lonScale = 111320f * Mathf.Cos((float)(_sessionStartLatLon.x * Mathf.Deg2Rad));

        float deltaLat = (float)(targetLatLon.x - _sessionStartLatLon.x);
        float deltaLon = (float)(targetLatLon.y - _sessionStartLatLon.y);

        // We'll do: X = deltaLon * lonScale, Z = deltaLat * latScale
        distMetersX = deltaLon * lonScale;
        distMetersZ = deltaLat * latScale;

        // 2) Construct the local position in the AR coordinate space
        //    We assume the AR session's origin was near the device’s starting position.
        //    The device’s initial lat/lon => (0,0,0).
        //    So we offset from that.
        return new Vector3(distMetersX, 0f, distMetersZ);
    }

    private void SpawnObject(Vector3 position, Quaternion rotation)
    {
        if (_objectPlaced) return;

        _objectPlaced = true;
        GameObject obj = Instantiate(_objectToPlace, position, rotation);
        obj.name = _objectName;

        XRObjectManager.Instance.AddObject(_objectName, obj);

        Debug.Log($"Object '{_objectName}' placed at {position} with rotation {rotation}.");
        Continue();
    }

    private void TryPlaceObject()
    {
        // Perform a raycast from the center of the screen
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (_arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;

            // Instantiate the object at the hit position
            GameObject obj = Instantiate(_objectToPlace, hitPose.position, hitPose.rotation);
            obj.name = _objectName;

            XRObjectManager.Instance.AddObject(_objectName, obj);

            Debug.Log($"Object '{_objectName}' placed at {hitPose.position}");

            Continue();
        }
        else
        {
            Debug.Log("Raycast did not hit any planes.");

        }
    }

    public override string GetSummary()
    {
        return "Places an object at a specified location";
    }
}