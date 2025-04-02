using Mapbox.Directions;
using Mapbox.Unity;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Handles the creation of directions between an array of points on the map.
    /// One can use this and draggable direction markers to create dynamic directions on the map.
    /// </summary>
    public class LUTEDirectionsFactory : MonoBehaviour
    {
        [SerializeField] protected AbstractMap map;
        [SerializeField] protected BasicFlowEngine engine;
        [SerializeField] protected MeshModifier[] meshModifiers;
        [SerializeField] protected Material meshMaterial;
        [Tooltip("The waypoints to create directions between.")]
        [SerializeField] protected Transform[] waypoints;
        [Tooltip("The routing profile to use for the directions.")]
        [SerializeField] protected RoutingProfile routingType = RoutingProfile.Walking;
        [Tooltip("The layer mask to use for the directions.")]
        [SerializeField] protected LayerMask directionLayerMask;
        [Tooltip("The frequency at which the directions are updated in seconds.")]
        [Range(0.05f, 50)]
        [SerializeField] protected float updateFrequency = 2;
        [Tooltip("Whether to destroy the direction object on player arrival. Clears all waypoints.")] // Could create another panel to inform players of the arrival.
        [SerializeField] protected bool destroyOnPlayerArrival = true;
        [Tooltip("Whether to query the directions whenever the map updates; this ensures the player will 'delete' the path as they traverse it.")]
        [SerializeField] protected bool queryWithMap = true;
        [Tooltip("Whether to query the directions based on the defined update time; helps to update the direction path based on the API.")]
        [SerializeField] protected bool queryWithTime = true;

        [VariableProperty(typeof(LocationVariable))]
        [SerializeField] protected LocationVariable[] locs;

        private List<Vector3> cachedWaypoints;
        private Directions directions;
        private int counter;
        private GameObject directionsGameObject;
        private bool recalculateNext;

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

        protected virtual void Awake()
        {
            if (map == null)
            {
                map = FindFirstObjectByType<AbstractMap>();
            }
            directions = MapboxAccess.Instance.Directions;

            if (queryWithMap)
            {
                map.OnInitialized += Query;
                map.OnUpdated += Query;
            }
        }

        protected virtual void Start()
        {
            cachedWaypoints = new List<Vector3>(waypoints.Length);
            foreach (var item in waypoints)
            {
                if (item != null)
                {
                    cachedWaypoints.Add(item.position);
                }
            }
            recalculateNext = false;

            foreach (var item in meshModifiers)
            {
                item.Initialize();
            }

            if (queryWithTime)
                StartCoroutine(QueryTimer());
        }

        protected virtual void OnDestroy()
        {
            map.OnInitialized -= Query;
            map.OnUpdated -= Query;
        }

        protected virtual IEnumerator QueryTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateFrequency);
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i].position != cachedWaypoints[i])
                    {
                        recalculateNext = true;
                        cachedWaypoints[i] = waypoints[i].position;
                    }
                }

                if (recalculateNext)
                {
                    Query();
                    recalculateNext = false;
                }
            }
        }

        private bool IsWithinRadius(Vector2d vecVal)
        {
            var mapManager = engine.GetMapManager();
            var tracker = mapManager.TrackerPos();
            var trackerPos = tracker;

            if (LocationProvider.CurrentLocation.LatitudeLongitude == null)
            {
                return false;
            }

            if (vecVal == null)
            {
                return false;
            }

            var deviceLoc = engine.DemoMapMode ? trackerPos : LocationProvider.CurrentLocation.LatitudeLongitude;

            var radiusInMeters = (LogaConstants.DefaultRadius * 2.5f);

            // Use double for more precision
            double r = 6371000.0; // Earth radius in meters

            // Convert to radians
            double lat1 = deviceLoc.x * Math.PI / 180.0;
            double lon1 = deviceLoc.y * Math.PI / 180.0;
            double lat2 = vecVal.x * Math.PI / 180.0;
            double lon2 = vecVal.y * Math.PI / 180.0;

            // Haversine formula with more precise calculation
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = r * c;

            return distance <= radiusInMeters;
        }

        private void HandleDirectionsResponse(DirectionsResponse response)
        {
            if (response == null || response.Routes == null || response.Routes.Count == 0)
            {
                return;
            }

            var meshData = new MeshData();
            var dat = new List<Vector3>();

            foreach (var point in response.Routes[0].Geometry)
            {
                var gp = (map.GeoToWorldPosition(point, true));
                dat.Add(gp);
            }

            var feat = new VectorFeatureUnity();
            feat.Points.Add(dat);

            foreach (var mod in meshModifiers.Where(x => x.Active))
            {
                mod.Run(feat, meshData, map.WorldRelativeScale);
            }

            CreateDirectionObject(meshData);
        }

        private GameObject CreateDirectionObject(MeshData data)
        {
            if (directionsGameObject != null)
            {
                Destroy(directionsGameObject);
            }

            directionsGameObject = new GameObject("DirectionWaypointDrawer");

            var mesh = directionsGameObject.AddComponent<MeshFilter>().mesh;
            mesh.subMeshCount = data.Triangles.Count;

            mesh.SetVertices(data.Vertices);

            int counter = data.Triangles.Count;
            for (int i = 0; i < counter; i++)
            {
                var triangle = data.Triangles[i];
                mesh.SetTriangles(triangle, i);
            }

            counter = data.UV.Count;
            for (int i = 0; i < counter; i++)
            {
                var uv = data.UV[i];
                mesh.SetUVs(i, uv);
            }

            mesh.RecalculateNormals();

            // Assigning default colors (fully visible)
            List<Color> colors = new List<Color>();
            for (int i = 0; i < data.Vertices.Count; i++)
            {
                colors.Add(Color.white); // Fully visible (alpha = 1)
            }

            mesh.SetColors(colors); // Assign colors

            directionsGameObject.AddComponent<MeshRenderer>().material = meshMaterial;
            directionsGameObject.layer = (int)Mathf.Log(directionLayerMask.value, 2);

            directionsGameObject.AddComponent<DirectionsObjectUpdater>().SetupDirectionObject(this.map);

            return directionsGameObject;
        }

        public virtual void UpdatePathVisibility(List<Vector3> hideStartEndPairs, bool hide = true, float maxWaitTime = 3f)
        {
            StartCoroutine(UpdatePathSections(hideStartEndPairs, hide, maxWaitTime));
        }

        private IEnumerator UpdatePathSections(List<Vector3> hideStartEndPairs, bool hide = true, float maxWaitTime = 5f)
        {
            float elapsedTime = 0f;
            float waitInterval = 0.1f;

            while (directionsGameObject == null && elapsedTime < maxWaitTime)
            {
                elapsedTime += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }

            if (directionsGameObject == null) yield break; // Exit if still null after max wait time

            Debug.Log("Updating path visibility");

            MeshFilter meshFilter = directionsGameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.mesh == null) yield break;

            Mesh mesh = meshFilter.mesh;
            List<Color> colors = new List<Color>(mesh.vertexCount);
            mesh.GetColors(colors);
            List<Vector3> vertices = new List<Vector3>(mesh.vertices); // Get vertex positions

            for (int j = 0; j < hideStartEndPairs.Count; j += 2)
            {
                int startIndex = FindClosestVertexIndex(vertices, hideStartEndPairs[j]);
                int endIndex = FindClosestVertexIndex(vertices, hideStartEndPairs[j + 1]);

                if (startIndex != -1 && endIndex != -1)
                {
                    if (startIndex > endIndex)
                    {
                        int temp = startIndex;
                        startIndex = endIndex;
                        endIndex = temp;
                    }

                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        colors[i] = new Color(1, 1, 1, hide ? 0 : 1);
                    }
                }
            }

            mesh.SetColors(colors);
        }

        private int FindClosestVertexIndex(List<Vector3> vertices, Vector3 target)
        {
            int closestIndex = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < vertices.Count; i++)
            {
                float distance = Vector3.Distance(vertices[i], target);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        public virtual void Query()
        {
            var count = waypoints.Length;

            if (count <= 1)
            {
                if (directionsGameObject != null)
                    Destroy(directionsGameObject);
                return;
            }

            var wp = new Vector2d[count];
            for (int i = 0; i < count; i++)
            {
                if (waypoints[i] != null)
                {
                    wp[i] = map.WorldToGeoPosition(waypoints[i].position);
                }
            }

            if (count > 0 && destroyOnPlayerArrival)
            {
                Vector2d lastWaypoint = wp[count - 1];
                if (IsWithinRadius(lastWaypoint))
                {
                    ClearWaypoints();
                    return;
                }
            }

            var directionResource = new DirectionResource(wp, routingType);
            directionResource.Steps = true;
            directions.Query(directionResource, HandleDirectionsResponse);
        }

        public virtual void AddWaypoint(Transform waypoint)
        {
            var list = new List<Transform>(waypoints);
            list.Add(waypoint);
            waypoints = list.ToArray();
            cachedWaypoints.Add(waypoint.position);
            recalculateNext = true;
            Query();
        }

        public virtual void AddWaypoint(Vector2d waypoint)
        {
            var list = new List<Transform>(waypoints);

            var newTransform = new GameObject().transform;
            newTransform.gameObject.name = "DummyWaypoint";
            newTransform.position = map.GeoToWorldPosition(waypoint, true);
            list.Add(newTransform);

            waypoints = list.ToArray();
            cachedWaypoints.Add(newTransform.position);
            recalculateNext = true;
        }

        public virtual void ClearWaypoints()
        {
            if (directionsGameObject != null)
                Destroy(directionsGameObject);
            waypoints = new Transform[0];
            cachedWaypoints.Clear();
            recalculateNext = true;
        }

        public virtual void UpdateMeshModifiers(MeshModifier newMod)
        {
            // Clear the current list of modifiers
            Array.Clear(meshModifiers, 0, meshModifiers.Length);
            // Add only one modifier as we only allow one modifier
            meshModifiers[0] = newMod;
            meshModifiers[0].Initialize();
        }

        public virtual void UpdateMeshMaterial(Material newMat)
        {
            meshMaterial = newMat;
            if (directionsGameObject != null)
            {
                directionsGameObject.GetComponent<MeshRenderer>().material = meshMaterial;
            }
        }

        public virtual void HideSections(List<Vector3> hideStartEndPairs)
        {
            UpdatePathVisibility(hideStartEndPairs);
        }
    }
}