using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Ensures that the created direction path object updates alongside the map.
    /// This prevents the position from being incorrect if the map is moved or zoomed.
    /// </summary>
    public class DirectionsObjectUpdater : MonoBehaviour
    {
        [Tooltip("The map to update the object based on.")]
        [SerializeField] protected AbstractMap map;


        private Vector2d marker2D; // The initial position of the object in 2D space
        private float startZoom; // The initial zoom level of the map to calculate the scale of the object when first created

        public void SetupDirectionObject(AbstractMap _map)
        {
            this.map = _map;
            this.startZoom = map.Zoom;
            this.marker2D = map.WorldToGeoPosition(transform.localPosition);
        }

        private void Update()
        {
            transform.localPosition = map.GeoToWorldPosition(marker2D, true);
            float scale = Mathf.Pow(2, map.Zoom - startZoom);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
