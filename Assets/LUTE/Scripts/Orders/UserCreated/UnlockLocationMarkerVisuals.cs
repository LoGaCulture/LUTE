using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Map",
             "Unlock Location Marker Visuals",
             "If a marker is being prevented from updating the visuals based on a relevant location status, this Order will ensure it is unlocked to do so.")]
    [AddComponentMenu("")]
    public class UnlockLocationMarkerVisuals : Order
    {
        [Tooltip("The location to unlock the marker for.")]
        [SerializeField] protected LocationData location;

        public override void OnEnter()
        {
            if (location.locationRef == null)
            {
                Continue();
                return;
            }

            location.Value.ForcePermanentChange = false;
            Continue();
        }

        public override string GetSummary()
        {
            return location.locationRef == null ? "Error: No location selected" : "Unlock " + location.Value.name + " marker visuals";
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 253, 255, 255);
        }
    }
}
