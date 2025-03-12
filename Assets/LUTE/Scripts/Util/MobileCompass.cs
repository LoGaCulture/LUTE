using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// A simple class that will point a rect transform true north using the compass.
    /// Works on mobile devices (iOS/Android) with a compass sensor.
    /// Could make use of Mapbox or third party APIs to get the true north.
    /// </summary>
    public class MobileCompass : MonoBehaviour
    {
        // Reference to the RectTransform of the compass needle (or the entire compass UI)
        public RectTransform compassNeedle;

        void Start()
        {
            // Enable the compass (available on most smartphones)
            Input.compass.enabled = true;
            // Enable the gyroscope (optional, but useful for smoother rotations)
            Input.gyro.enabled = true;
        }

        void Update()
        {
            // Get the current heading (in degrees) relative to the true north
            float heading = Input.compass.trueHeading;

            // Rotate the compass needle in the opposite direction to point north
            compassNeedle.localEulerAngles = new Vector3(0, 0, -heading);
        }
    }
}
