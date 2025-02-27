namespace Mapbox.Examples
{
    using UnityEngine;

    public class SpawnOnMap : MonoBehaviour
    {
        //[SerializeField] public Transform tracker;
        //[SerializeField] AbstractMap _map;

        //[Geocode] protected List<string> _locationStrings = new List<string>();

        //[SerializeField] protected BasicFlowEngine engine;
        //[SerializeField] protected DirectionsFactory _directionPrefab;

        //[SerializeField] private GameObject _radiusCirclePrefab;

        //private List<LocationMarker> _spawnedObjects;
        //private List<LUTELocationInfo> _locationData = new List<LUTELocationInfo>();
        //private float _radiusInMeters = LogaConstants.DefaultRadius;

        //[SerializeField] public LocationMarker _markerPrefab;
        //[SerializeField] public float _spawnScale = 5f;

        //void Awake()
        //{
        //    InitializeEngine();
        //    if (engine == null)
        //    {
        //        engine = FindObjectOfType<BasicFlowEngine>();
        //    }
        //    if (engine == null)
        //    {
        //        return;
        //    }
        //    if (_map == null)
        //        _map = LocationProviderFactory.Instance.mapManager;

        //    ProcessNodes();
        //    CreateMarkers();
        //    CreateRadiusCircles();
        //}

        //private void CreateRadiusCircles()
        //{
        //    foreach (var marker in _spawnedObjects)
        //    {
        //        CreateRadiusCircle(marker);
        //    }
        //}

        //private void CreateRadiusCircle(LocationMarker marker)
        //{
        //    if (marker == null || marker.RadiusRenderer == null) return;
        //    GameObject radiusCircle = Instantiate(marker.RadiusRenderer.gameObject, marker.transform);
        //    radiusCircle.transform.localPosition = Vector3.zero;

        //    UpdateRadiusCircleScale(radiusCircle, marker.transform.position);
        //    marker.RadiusObject = radiusCircle;
        //}

        //private const float MIN_SCALE = 0.1f;
        //private const float MAX_SCALE = 1000f;

        //private void UpdateRadiusCircleScale(GameObject radiusCircle, Vector3 centerPosition)
        //{
        //    // Calculate scale based on zoom level
        //    float zoomLevel = _map.Zoom;
        //    float metersPerPixel = CalculateMetersPerPixel(zoomLevel, centerPosition);
        //    float pixelScale = _radiusInMeters / metersPerPixel;

        //    // Apply scale, ensuring it's within acceptable bounds
        //    float scale = Mathf.Clamp(pixelScale, MIN_SCALE, MAX_SCALE);

        //    // Check for NaN or Infinity
        //    if (float.IsNaN(scale) || float.IsInfinity(scale))
        //    {
        //        scale = 1f; // Fallback to a default scale
        //    }

        //    radiusCircle.transform.localScale = new Vector3(scale, scale, 1);
        //}

        //private float CalculateMetersPerPixel(float zoomLevel, Vector3 centerPosition)
        //{
        //    // Convert center position to geo coordinates
        //    Vector2d centerGeoPosition = _map.WorldToGeoPosition(centerPosition);

        //    // Calculate meters per pixel at the equator for the current zoom level
        //    float metersPerPixelAtEquator = 156543.03f / Mathf.Pow(2, zoomLevel);

        //    // Adjust for the current latitude
        //    float latitudeRadians = Mathf.Deg2Rad * (float)centerGeoPosition.x;
        //    float metersPerPixel = metersPerPixelAtEquator * Mathf.Cos(latitudeRadians);

        //    return metersPerPixel;
        //}

        //private void InitializeEngine()
        //{
        //    if (engine == null)
        //        engine = BasicFlowEngine.CachedEngines.FirstOrDefault();
        //    if (engine == null)
        //        engine = FindObjectOfType<BasicFlowEngine>();
        //    if (engine == null)
        //        Debug.LogError("No engine found");
        //}

        //private void ProcessNodes()
        //{
        //    _locationData.Clear();
        //    var nodes = engine.gameObject.GetComponents<Node>();

        //    foreach (var node in nodes)
        //    {
        //        if (node == null) continue;

        //        ProcessNodeLocation(node);
        //        ProcessNodeOrders(node);
        //    }
        //}

        //public void ProcessLocationInfo()
        //{
        //    if (engine == null)
        //        return;

        //    ClearLocations();
        //    var locationVars = engine.GetComponents<LocationVariable>();
        //    foreach (var location in locationVars)
        //    {
        //        if (location.Value.LatLongString() != Vector2d.zero)
        //            AddUniqueLocation(location);
        //    }
        //}

        //public void ClearLocations()
        //{
        //    _locationData.Clear();
        //    if (_spawnedObjects != null)
        //    {
        //        foreach (var obj in _spawnedObjects)
        //        {
        //            DestroyImmediate(obj.gameObject);
        //        }
        //        _spawnedObjects.Clear();
        //    }
        //}

        //private void ProcessNodeLocation(Node node)
        //{
        //    LocationClickEventHandler handler = node._EventHandler as LocationClickEventHandler;

        //    if (node.NodeLocation != null)
        //    {
        //        AddUniqueLocation(node.NodeLocation);
        //    }
        //    else if (handler != null)
        //    {
        //        AddUniqueLocation(handler.Location.locationRef);
        //    }
        //    else if (node._EventHandler != null)
        //    {
        //        if (node._EventHandler.GetType() == typeof(ConditionalEventHandler))
        //        {
        //            var conditionalEventHandler = node._EventHandler as ConditionalEventHandler;
        //            foreach (var condition in conditionalEventHandler.Conditions)
        //            {
        //                ProcessIfOrderLocation(condition);
        //            }
        //        }
        //    }
        //}

        //private void ProcessNodeOrders(Node node)
        //{
        //    if (node.OrderList == null || node.OrderList.Count == 0) return;

        //    foreach (var order in node.OrderList)
        //    {
        //        ProcessOrderLocations(order);
        //        ProcessIfOrderLocation(order);
        //    }
        //}

        //private void ProcessOrderLocations(Order order)
        //{
        //    var locations = new List<LocationVariable>();
        //    order.GetLocationVariables(ref locations);
        //    foreach (var location in locations)
        //    {
        //        AddUniqueLocation(location);
        //    }
        //}

        //private void ProcessIfOrderLocation(Order order)
        //{
        //    if (order is If ifOrder)
        //    {
        //        var locationVariable = ifOrder.ReferencesLocation();
        //        if (locationVariable != null)
        //        {
        //            AddUniqueLocation(locationVariable);
        //        }
        //    }
        //}

        //private LUTELocationInfo? AddUniqueLocation(LocationVariable location)
        //{
        //    if (location == null)
        //        return null;

        //    Vector2d latLong = location.Value.LatLongString();

        //    if (LocationExists(latLong))
        //    {
        //        const double epsilon = 0.000001; // Adjust as needed
        //        // return the existing location data that was found
        //        return _locationData.Find(data =>
        //            Math.Abs(data.LatLongString().x - latLong.x) < epsilon &&
        //            Math.Abs(data.LatLongString().y - latLong.y) < epsilon);
        //    }
        //    else
        //    {
        //        var newLocationData = new LUTELocationInfo
        //        {
        //            infoID = location.Value.infoID,
        //            Position = location.Value.Position,
        //            Name = location.Value.Name,
        //            Sprite = location.Value.Sprite,
        //            InProgressSprite = location.Value.InProgressSprite,
        //            CompletedSprite = location.Value.CompletedSprite,
        //            Color = location.Value.Color,
        //            ShowName = location.Value.ShowName,
        //            _LocationStatus = location.Value._LocationStatus,
        //            NodeComplete = location.Value.NodeComplete,
        //            ExecuteNode = location.Value.ExecuteNode,
        //            Interactable = location.Value.Interactable,
        //            SaveInfo = location.Value.SaveInfo,
        //            showRadius = location.Value.showRadius,
        //            defaultRadiusColour = location.Value.defaultRadiusColour,
        //            visitedRadiusColour = location.Value.visitedRadiusColour,
        //            completedRadiusColour = location.Value.completedRadiusColour,
        //            IndependentMarkerUpdating = location.Value.IndependentMarkerUpdating,
        //            AllowClickWithoutLocation = location.Value.AllowClickWithoutLocation
        //        };

        //        _locationData.Add(newLocationData);
        //        return newLocationData;
        //    }
        //}

        //private bool LocationExists(Vector2d position)
        //{
        //    const double epsilon = 0.000001; // Adjust as needed
        //    return _locationData.Any(loc =>
        //        Math.Abs(loc.LatLongString().x - position.x) < epsilon &&
        //        Math.Abs(loc.LatLongString().y - position.y) < epsilon);
        //}

        //public void CreateMarkers()
        //{
        //    _spawnedObjects = new List<LocationMarker>();
        //    foreach (var locationData in _locationData)
        //    {
        //        CreateMarker(locationData);
        //    }
        //}

        //private void CreateMarker(LUTELocationInfo locationData)
        //{
        //    var instance = Instantiate(_markerPrefab);
        //    var cameraBillboard = instance.GetComponent<LocationMarker>();
        //    cameraBillboard.SetCanvasCam(GetComponent<QuadTreeCameraMovement>()?._referenceCameraGame);
        //    cameraBillboard.SetInfo(locationData);
        //    cameraBillboard.SetMarkerSprite(locationData.Sprite);
        //    cameraBillboard.SetEngine(engine);

        //    var mainCamera = GetComponent<QuadTreeCameraMovement>()?._referenceCameraGame;

        //    instance.transform.localPosition = _map.GeoToWorldPosition(locationData.LatLongString(), true);
        //    instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);

        //    // Set additional properties based on locationData

        //    _spawnedObjects.Add(cameraBillboard);
        //}

        //// This could use LocationData rather than getting the marker class
        //public GameObject HideLocationMarker(LocationVariable location)
        //{
        //    if (location == null || _map == null || _spawnedObjects == null)
        //    {
        //        Debug.LogWarning("Invalid input or uninitialized objects in HideLocationMarker");
        //        return null;
        //    }

        //    LocationMarker locationMarker = _spawnedObjects.Find(marker =>
        //        marker != null &&
        //        marker.locationInfo.infoID != null &&
        //        marker.locationInfo.infoID == location.Value.infoID
        //    );

        //    if (locationMarker != null)
        //    {
        //        locationMarker.gameObject.SetActive(false);
        //        return locationMarker.gameObject;
        //    }
        //    return null;
        //}

        //public GameObject ShowLocationMarker(LocationVariable location, bool updateText = false, string updatedText = "")
        //{
        //    if (location == null || _map == null || _spawnedObjects == null)
        //    {
        //        Debug.LogWarning("Invalid input or uninitialized objects in ShowLocationMarker");
        //        return null;
        //    }

        //    LocationMarker locationMarker = _spawnedObjects.Find(marker =>
        //        marker != null &&
        //        marker.locationInfo.infoID != null &&
        //        marker.locationInfo.infoID == location.Value.infoID
        //    );

        //    if (locationMarker == null)
        //    {
        //        AddUniqueLocation(location);
        //        CreateMarker(_locationData[_locationData.Count - 1]);
        //        locationMarker = _spawnedObjects[_spawnedObjects.Count - 1];
        //    }

        //    if (locationMarker != null)
        //    {
        //        if (updateText && !string.IsNullOrEmpty(updatedText))
        //        {
        //            int locationIndex = _locationData.FindIndex(data => data.Name == location.Key);
        //            if (locationIndex == -1)
        //            {
        //                Debug.LogWarning($"Location {location.Key} not found in _locationData");
        //                return null;
        //            }

        //            // Find the location data and update the name
        //            var locationData = _locationData[locationIndex];
        //            locationData.Name = updatedText;
        //            _locationData[locationIndex] = locationData;
        //        }
        //        locationMarker.gameObject.SetActive(true);
        //        return locationMarker.gameObject;
        //    }
        //    return null;
        //}

        //private void DrawDirections()
        //{
        //    // Go through all nodes
        //    List<Node> nodes = engine.gameObject.GetComponents<Node>().ToList();
        //    foreach (Node node in nodes)
        //    {
        //        // Go through all orders
        //        foreach (Order order in node.OrderList)
        //        {
        //            // Check if order is type of If and uses node variable
        //            if (order.GetType() == typeof(If))
        //            {
        //                If ifOrder = order as If;
        //                foreach (ConditionExpression condition in ifOrder.conditions)
        //                {
        //                    if (condition.AnyVariable.variable is NodeVariable)
        //                    {
        //                        Node targetNode = (condition.AnyVariable.variable as NodeVariable).Value;
        //                        // Check if both nodes use location
        //                        if (node.NodeLocation != null && targetNode.NodeLocation != null)
        //                        {
        //                            // Spawn a direction factory object using prefab
        //                            if (_directionPrefab == null)
        //                            {
        //                                Debug.LogError("Direction prefab is null");
        //                                return;
        //                            }
        //                            DirectionsFactory directionFactory = Instantiate(_directionPrefab);
        //                            Transform[] waypoints = new Transform[2];

        //                            foreach (var location in _spawnedObjects)
        //                            {
        //                                var nodeLatLon = node.NodeLocation.Value.LatLongString();
        //                                var targetNodeLatLon = targetNode.NodeLocation.Value.LatLongString();
        //                                if (location.gameObject.transform.localPosition == _map.GeoToWorldPosition(nodeLatLon, true))
        //                                {
        //                                    waypoints[0] = location.transform;
        //                                }
        //                                if (location.gameObject.transform.localPosition == _map.GeoToWorldPosition(targetNodeLatLon, true))
        //                                {
        //                                    waypoints[1] = location.transform;
        //                                }

        //                                if (waypoints[0] != null && waypoints[1] != null)
        //                                {
        //                                    directionFactory.SetWaypoints(waypoints, _map);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //if node calls to another node and both nodes have a location
        //                if (order.GetType() == typeof(NextNode))
        //                {
        //                    NextNode nextNode = order as NextNode;
        //                    Node targetNode = nextNode.targetNode;

        //                    if (node.NodeLocation != null && targetNode.NodeLocation != null)
        //                    {
        //                        // Spawn a direction factory object using prefab
        //                        if (_directionPrefab == null)
        //                        {
        //                            Debug.LogError("Direction prefab is null");
        //                            return;
        //                        }
        //                        DirectionsFactory directionFactory = Instantiate(_directionPrefab);
        //                        Transform[] waypoints = new Transform[2];

        //                        foreach (var location in _spawnedObjects)
        //                        {
        //                            var nodeLatLon = node.NodeLocation.Value.LatLongString();
        //                            var targetNodeLatLon = targetNode.NodeLocation.Value.LatLongString();
        //                            if (location.transform.localPosition == _map.GeoToWorldPosition(nodeLatLon, true))
        //                            {
        //                                waypoints[0] = location.transform;
        //                            }
        //                            if (location.transform.localPosition == _map.GeoToWorldPosition(targetNodeLatLon, true))
        //                            {
        //                                waypoints[1] = location.transform;
        //                            }

        //                            if (waypoints[0] != null && waypoints[1] != null)
        //                            {
        //                                directionFactory.SetWaypoints(waypoints, _map);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        // Check if the node has a target unlock node and location
        //        if (node.TargetUnlockNode != null)
        //        {
        //            Node targetUnlockNode = node.TargetUnlockNode;
        //            // Check if both nodes use location (not null)
        //            if (node.NodeLocation != null && targetUnlockNode.NodeLocation != null)
        //            {
        //                // Spawn a direction factory object using prefab
        //                if (_directionPrefab == null)
        //                {
        //                    Debug.LogError("Direction prefab is null");
        //                    return;
        //                }
        //                DirectionsFactory directionFactory = Instantiate(_directionPrefab);
        //                Transform[] waypoints = new Transform[2];

        //                waypoints[0] = new GameObject().transform;
        //                waypoints[0].gameObject.name = "Waypoint 0";
        //                var latLon = node.NodeLocation.Value.LatLongString();
        //                Vector3 pos1 = new Vector3((float)latLon.x, 0, (float)latLon.y);
        //                waypoints[0].transform.localPosition = pos1;
        //                waypoints[1] = new GameObject().transform;
        //                var latLon2 = targetUnlockNode.NodeLocation.Value.LatLongString();
        //                Vector3 pos2 = new Vector3((float)latLon2.x, 0, (float)latLon2.y);
        //                waypoints[1].transform.localPosition = pos2;
        //                waypoints[1].gameObject.name = "Waypoint 1";

        //                directionFactory.SetWaypoints(waypoints, _map);
        //            }
        //        }
        //    }
        //}
        //private void Update()
        //{
        //    UpdateMarkers();
        //    UpdateTracker();
        //}

        //private void UpdateRadiusCircle(int index)
        //{
        //    var marker = _spawnedObjects[index];
        //    //var radiusCircle = _radiusCircles[index];
        //    var radiusCircle = marker.RadiusObject;
        //    var locationData = _locationData[index];
        //    var infoObject = Resources.FindObjectsOfTypeAll<LUTELocationInfo>().FirstOrDefault(x => x.infoID == locationData.infoID);

        //    if (marker == null || radiusCircle == null) return;

        //    if (infoObject.showRadius)
        //    {
        //        radiusCircle.SetActive(true);
        //        //radiusCircle.GetComponent<SpriteRenderer>().color = locationData.radiusColor;
        //    }
        //    else
        //    {
        //        radiusCircle.SetActive(false);
        //        return;
        //    }

        //    // Update position
        //    radiusCircle.transform.localPosition = new Vector3(0, 0.6f, 0);

        //    // Update scale
        //    UpdateRadiusCircleScale(radiusCircle, marker.transform.position);

        //    // Update visibility based on zoom level
        //    bool isVisible = _map.Zoom > 11;
        //    radiusCircle.SetActive(isVisible);
        //}

        //public void UpdateMarkers()
        //{
        //    for (int i = 0; i < _locationData.Count && i < _spawnedObjects.Count; i++)
        //    {
        //        UpdateMarker(i);
        //    }
        //}

        //private void UpdateMarker(int index)
        //{
        //    var spawnedObject = _spawnedObjects[index];
        //    var locationData = _locationData[index];

        //    // Whilst we have a list of location info this is not the actual reference to the SO
        //    // We need to search for this and use the info on this object not the reference in the list
        //    //var infoObject = Resources.FindObjectsOfTypeAll<LUTELocationInfo>().FirstOrDefault(x => x.infoID == locationData.infoID);

        //    var locationInfo = engine.GetComponents<LocationVariable>().FirstOrDefault(x => x.Value.infoID == locationData.infoID);
        //    LUTELocationInfo info = null;

        //    if (locationInfo != null)
        //    {
        //        info = locationInfo.Value;
        //    }

        //    if (spawnedObject == null) return;

        //    UpdateMarkerSprite(spawnedObject, info);
        //    UpdateMarkerPosition(spawnedObject, info.LatLongString());
        //    UpdateMarkerScale(spawnedObject);
        //    UpdateMarkerBillboard(spawnedObject, info);
        //    UpdateRadiusCircle(index);
        //}

        //private void UpdateMarkerSprite(LocationMarker spawnedObject, LUTELocationInfo info)
        //{
        //    switch (info._LocationStatus)
        //    {
        //        case LUTELocationInfo.LocationStatus.Unvisited:
        //            spawnedObject.SetMarkerSprite(info.Sprite);
        //            spawnedObject.SetRadiusColour(info.defaultRadiusColour);
        //            break;
        //        case LUTELocationInfo.LocationStatus.Visited:
        //            spawnedObject.SetMarkerSprite(info.InProgressSprite);
        //            spawnedObject.SetRadiusColour(info.visitedRadiusColour);
        //            break;
        //        case LUTELocationInfo.LocationStatus.Completed:
        //            spawnedObject.SetMarkerSprite(info.CompletedSprite);
        //            spawnedObject.SetRadiusColour(info.completedRadiusColour);
        //            break;
        //    }
        //}

        //private void UpdateMarkerPosition(LocationMarker spawnedObject, Vector2d location)
        //{
        //    spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
        //}

        //private void UpdateMarkerScale(LocationMarker spawnedObject)
        //{
        //    float zoomFactor = Mathf.InverseLerp(22, 0, _map.Zoom);

        //    // Calculate the scale based on the zoomFactor

        //    if (_map.Zoom <= 11)
        //    {
        //        _spawnScale = 0.0f;
        //    }
        //    else
        //        _spawnScale = Mathf.Lerp(2.0f, 2.5f, zoomFactor);

        //    spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
        //}

        //private void UpdateMarkerBillboard(LocationMarker spawnedObject, LUTELocationInfo locationData)
        //{
        //    var billboard = spawnedObject;
        //    var cam = GetComponent<QuadTreeCameraMovement>()?._referenceCameraGame;
        //    billboard.SetCanvasCam(cam);

        //    var displayName = locationData.Name.Replace("_", " ");
        //    billboard.SetMarkerText(displayName);

        //    billboard.SetMarkerColor(locationData.Color);
        //    billboard.SetMarkerName(locationData.ShowName);
        //}

        //private void UpdateTracker()
        //{
        //    var mapCam = GetComponent<QuadTreeCameraMovement>()?._referenceCamera;
        //    if (mapCam == null) return;

        //    bool shouldShowTracker = engine.DemoMapMode && mapCam.enabled;
        //    var playerMovement = LocationProviderFactory.Instance.PlayerMapMovement;
        //    if (playerMovement != null)
        //    {
        //        playerMovement.enabled = !shouldShowTracker;
        //    }
        //}

        //// Uncomment and adapt this method if you want to re-enable the right-click functionality
        ///*
        //private void HandleRightClick()
        //{
        //    if (Input.GetMouseButtonUp(1))
        //    {
        //        var mousePosScreen = Input.mousePosition;
        //        var cam = GetComponent<QuadTreeCameraMovement>()?._referenceCameraGame;
        //        if (cam == null) return;

        //        mousePosScreen.z = cam.transform.localPosition.y;
        //        var pos = cam.ScreenToWorldPoint(mousePosScreen);
        //        var latlongDelta = _map.WorldToGeoPosition(pos);

        //        AddNewLocation(latlongDelta);
        //    }
        //}

        //private void AddNewLocation(Vector2d position)
        //{
        //    var newLocationData = new LocationData
        //    {
        //        Position = position,
        //        Name = $"New Location {_locationData.Count + 1}",
        //        // Set default values for Sprite, Color, and ShowName as needed
        //    };

        //    _locationData.Add(newLocationData);
        //    CreateMarkerForLocation(newLocationData);
        //}

        //private void CreateMarkerForLocation(LocationData locationData)
        //{
        //    var instance = Instantiate(_markerPrefab);
        //    var billboard = instance.GetComponent<CameraBillboard>();

        //    UpdateMarkerPosition(billboard, locationData.Position);
        //    UpdateMarkerScale(billboard);
        //    UpdateMarkerBillboard(billboard, locationData);

        //    _spawnedObjects.Add(billboard);
        //}
        //*/

        //public Vector2d TrackerPos()
        //{
        //    var trackerPos = tracker.localPosition;
        //    var cam = GetComponent<QuadTreeCameraMovement>()?._referenceCamera;
        //    var pos = cam.ScreenToWorldPoint(trackerPos);

        //    var latlongDelta = _map.WorldToGeoPosition(trackerPos);

        //    return latlongDelta;
        //}

        //public Vector3 TrackerPosWorld()
        //{
        //    return tracker.localPosition;
        //}

        //public bool ToggleMap()
        //{
        //    var _mapCam = GetComponent<QuadTreeCameraMovement>()?._referenceCamera;
        //    //set the tracker cam to this cam
        //    tracker.GetComponent<CameraBillboard>()?.SetCanvasCam(_mapCam);
        //    if (_mapCam)
        //    {
        //        _mapCam.enabled = !_mapCam.enabled;

        //        return _mapCam.enabled;
        //    }
        //    return false;
        //}

        //public void RemoveLocationMarker(LocationVariable location)
        //{
        //    if (_spawnedObjects == null || location == null || _spawnedObjects.Count <= 0)
        //    {
        //        return;
        //    }

        //    var locationMarker = _spawnedObjects.Find(marker =>
        //                   marker != null && marker.locationInfo != null && marker.locationInfo.infoID == location.Value.infoID);
        //    if (locationMarker != null)
        //    {
        //        _spawnedObjects.Remove(locationMarker);
        //        DestroyImmediate(locationMarker.gameObject);
        //    }
        //}
    }
}