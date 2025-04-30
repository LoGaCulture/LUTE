using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// A class that handles location markers and the creation of them.
    /// The class is often utilised to store information about markers and then modify them during runtime (such as hiding or revealing them).
    /// </summary>
    public class LUTEMapManager : MonoBehaviour
    {
        [Tooltip("The object that contains the position of the player when using 'demo' mode.")]
        [SerializeField] protected Transform playerTracker;
        [Tooltip("The marker prefab that will be created for locations that are used during runtime.")]
        [SerializeField] protected LocationMarker markerPrefab;
        [Tooltip("The scale of the marker when it is created.")]
        [SerializeField] protected float markerScale = 1.0f;
        [Tooltip("The reference to the map component this will be found automatically if not provided.")]
        [SerializeField] protected AbstractMap map;
        [Tooltip("The public reference to the flow engine component - this will be found automatically if not provided.")]
        [SerializeField] protected BasicFlowEngine engine;
        [Tooltip("Reference to the map movement - tries to find it automatically.")]
        [SerializeField] protected MapCameraMovement mapMovement;
        [SerializeField] protected LUTELocationStatusDisplayList defaultLocationDisplayList;
        [Tooltip("A default sprite used if display options are missing for any given location marker")]
        [SerializeField] protected Sprite missingLocationMarkerSprite;

        private List<LocationMarker> spawnedLocationMarkers = new List<LocationMarker>();

        public float MarkerScale => markerScale;
        public LUTELocationStatusDisplayList DefaultLocationDisplayList
        {
            get { return defaultLocationDisplayList; }
        }
        public Sprite MissingLocationMarkerSprite
        {
            get { return missingLocationMarkerSprite; }
        }

        public List<LocationMarker> GetSpawnedLocationMarkers() => spawnedLocationMarkers;

        public MapCameraMovement MapCameraMovement => mapMovement;

        // Populated list of all the locations found during runtime.
        // Either found in an EventHandler or condition order.
        private List<LocationVariable> allLocationsAtRuntime = new List<LocationVariable>();

        public virtual void ProcessLocations()
        {
            var allNodes = engine.GetComponents<Node>();

            foreach (Node node in allNodes)
            {
                node.GetAllLocations(ref allLocationsAtRuntime);
            }
        }

        // During runtime we only find locations related to Nodes 
        // In editor mode, we want to see all location variables that are available.
        public virtual void ProcessLocationsEditor()
        {
            var allNodes = engine.Variables.FindAll(x => x is LocationVariable);
            foreach (LocationVariable location in allNodes)
            {
                allLocationsAtRuntime.Add(location);
            }
        }

        public Vector2d TrackerPos()
        {
            var trackerPos = playerTracker.localPosition;
            var cam = mapMovement.ReferenceCamera;
            var pos = cam.ScreenToWorldPoint(trackerPos);

            var latlongDelta = map.WorldToGeoPosition(trackerPos);

            return latlongDelta;
        }

        public Transform TrackerTransform()
        {
            return playerTracker.transform;
        }

        public virtual void SpawnMarkers()
        {
            foreach (LocationVariable location in allLocationsAtRuntime)
            {
                if (location == null || location.Value == null)
                {
                    continue;
                }

                SpawnMarker(location);
            }
        }

        public virtual void HideLocationMarker(LocationVariable location)
        {
            if (location == null || location.Value == null || spawnedLocationMarkers.Count <= 0)
            {
                return;
            }

            var relatedLocationMarkers = spawnedLocationMarkers.FindAll(spawnedLocationMarkers =>
                spawnedLocationMarkers != null && spawnedLocationMarkers.LocationVariable.Value != null &&
                spawnedLocationMarkers.LocationVariable.Value.InfoID == location.Value.InfoID);

            foreach (var locationMarker in relatedLocationMarkers)
            {
                locationMarker.SetHiddenStatus(true);
            }
        }

        public virtual void ShowLocationMarker(LocationVariable location)
        {
            if (location == null || location.Value == null || spawnedLocationMarkers.Count <= 0)
            {
                return;
            }

            var relatedLocationMarkers = spawnedLocationMarkers.FindAll(spawnedLocationMarkers =>
                spawnedLocationMarkers != null && spawnedLocationMarkers.LocationVariable.Value != null &&
                spawnedLocationMarkers.LocationVariable.Value.InfoID == location.Value.InfoID);

            if (relatedLocationMarkers.Count <= 0)
            {
                SpawnMarker(location);
            }
            else
            {
                foreach (var locationMarker in relatedLocationMarkers)
                {
                    locationMarker.ResetHiddenStatus(true);
                }
            }
        }

        public virtual bool ToggleMap()
        {
            var mapCam = mapMovement.ReferenceCamera;

            if (mapCam)
            {
                mapCam.enabled = !mapCam.enabled;

                return mapCam.enabled;
            }
            return false;
        }

        public virtual void RemoveLocationMarker(LocationVariable location)
        {
            if (spawnedLocationMarkers == null || location == null || spawnedLocationMarkers.Count <= 0)
            {
                return;
            }

            var relatedLocationMarkers = spawnedLocationMarkers.FindAll(spawnedLocationMarkers =>
                spawnedLocationMarkers != null && spawnedLocationMarkers.LocationVariable.Value != null &&
                spawnedLocationMarkers.LocationVariable.Value.InfoID == location.Value.InfoID);

            foreach (var locationMarker in relatedLocationMarkers)
            {
                spawnedLocationMarkers.Remove(locationMarker);
                DestroyImmediate(locationMarker.gameObject);
            }
        }

        public virtual void ClearAllMarkers()
        {
            foreach (var marker in spawnedLocationMarkers)
            {
                marker.ForceUpdateInEditor = false;
                DestroyImmediate(marker.gameObject);
            }
            spawnedLocationMarkers.Clear();
            allLocationsAtRuntime.Clear();
        }

        public virtual List<Transform> GetMarkerTransforms(LocationVariable locationVar)
        {
            if (locationVar == null || locationVar.Value == null)
            {
                return null;
            }

            var relatedLocationMarkers = spawnedLocationMarkers.FindAll(spawnedLocationMarkers =>
                spawnedLocationMarkers != null && spawnedLocationMarkers.LocationVariable.Value != null &&
                spawnedLocationMarkers.LocationVariable.Value.InfoID == locationVar.Value.InfoID);

            return relatedLocationMarkers.ConvertAll(marker => marker.transform);
        }

        protected virtual void Awake()
        {
            if (engine == null)
            {
                engine = BasicFlowEngine.CachedEngines[0];
            }
            if (engine == null)
            {
                engine = FindFirstObjectByType<BasicFlowEngine>();
            }
            if (engine == null)
            {
                Debug.LogError("No BasicFlowEngine found in scene.");
                return;
            }

            if (map == null)
            {
                map = engine.GetAbstractMap();
            }
            if (map == null)
            {
                Debug.LogError("No AbstractMap found in scene.");
                return;
            }

            if (mapMovement == null)
            {
                mapMovement = GetComponent<MapCameraMovement>();
            }
            if (mapMovement == null)
            {
                mapMovement = FindFirstObjectByType<MapCameraMovement>();
            }
            if (mapMovement == null)
            {
                Debug.LogError("No MapCameraMovement found in scene.");
                return;
            }
        }

        protected virtual void Start()
        {
            ProcessLocations();
            SpawnMarkers();
        }

        protected virtual void Update()
        {
            UpdateTracker();
        }

        protected virtual void SpawnMarker(LocationVariable location)
        {
            if (location == null || location.Value == null)
            {
                return;
            }

            // Check if a marker with the same location info already exists
            var existingMarker = spawnedLocationMarkers.Find(marker =>
                marker != null &&
                marker.LocationVariable != null &&
                marker.LocationVariable.Value != null &&
                marker.LocationVariable.Value.InfoID == location.Value.InfoID);

            if (existingMarker != null)
            {
                // A marker with the same location info already exists, so we skip spawning
                return;
            }

            // Instantiate a new marker
            var marker = Instantiate(markerPrefab);
            marker.SetCanvasCam(mapMovement.ReferenceCamera);
            marker.SetInfo(location, engine);
            marker.transform.localScale = Vector3.one * markerScale;
            marker.transform.localPosition = map.GeoToWorldPosition(location.Value.LatLongString(), true);

            // Add the new marker to the list
            spawnedLocationMarkers.Add(marker);
        }

        private void UpdateTracker()
        {
            var mapCam = mapMovement.ReferenceCamera;
            if (mapCam == null) return;

            bool shouldShowTracker = engine.DemoMapMode && mapCam.enabled;
            if (playerTracker != null)
            {
                var astroController = playerTracker.GetComponent<Mapbox.Examples.AstronautMouseController>();
                if (astroController != null)
                {
                    astroController.enabled = shouldShowTracker;
                }
                else
                    return;

                var raycastPlane = playerTracker.GetComponentInChildren<MeshCollider>();
                if (raycastPlane != null)
                {
                    raycastPlane.enabled = shouldShowTracker;
                }
            }
        }
    }
}