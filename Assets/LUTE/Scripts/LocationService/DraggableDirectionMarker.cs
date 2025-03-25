using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Simple draggable direction marker for the map.
    /// These can be used to draw directions during runtime by dragging the marker around the map.
    /// Implements the IBeginDragHandler, IDragHandler, and IEndDragHandler interfaces and requires a camera and map to be set.
    /// </summary>
    public class DraggableDirectionMarker : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("The main camera in the scene for the map. If not set, the direction marker cannot be interacted with.")]
        [SerializeField] protected Camera mapCamera;
        [SerializeField] protected AbstractMap map;

        private Vector3 offset;
        private float fixedYPos;
        private Vector2d marker2D;
        private bool mapInitialised;

        private void Awake()
        {
            if (map == null)
            {
                Debug.LogError("Map is not set. The direction marker cannot be interacted with.");
                return;
            }

            map.OnInitialized += SetInitialised;
        }

        private void OnDisable()
        {
            if (map == null)
            {
                return;
            }

            map.OnInitialized -= SetInitialised;
            mapInitialised = false;
        }

        private void SetInitialised()
        {
            mapInitialised = true;
            fixedYPos = transform.position.y;
            marker2D = map.WorldToGeoPosition(transform.position);
        }

        private void Update()
        {
            if (!mapInitialised)
            {
                return;
            }

            transform.localPosition = map.GeoToWorldPosition(marker2D, true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            offset = transform.position - GetMouseWorldPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 newPosition = GetMouseWorldPosition(eventData) + offset;
            newPosition.y = fixedYPos;
            transform.position = newPosition;
            marker2D = map.WorldToGeoPosition(transform.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Optional: Implement logic when the drag ends (e.g., snapping)
        }

        private Vector3 GetMouseWorldPosition(PointerEventData eventData)
        {
            if (mapCamera == null)
            {
                Debug.LogError("Map camera is not set. The direction marker cannot be interacted with.");
                return Vector3.zero;
            }

            Vector3 mousePosition = eventData.position;
            mousePosition.z = mapCamera.WorldToScreenPoint(transform.position).z;
            return mapCamera.ScreenToWorldPoint(mousePosition);
        }
    }
}
