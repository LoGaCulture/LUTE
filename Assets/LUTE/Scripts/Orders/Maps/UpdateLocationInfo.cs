using UnityEngine;


namespace LoGaCulture.LUTE
{

    [OrderInfo("Map",
                     "Update Location Info",
                     "Updates the status of a given location.")]
    [AddComponentMenu("")]
    public class UpdateLocationInfo : Order
    {
        [Tooltip("The location to update.")]
        [SerializeField] protected LocationData location;
        [Tooltip("The status to update the location to.")]
        [SerializeField] protected LocationStatus status;
        [Tooltip("The label to use if the status is set to custom. No case sensitivity.")]
        [SerializeField] protected string customLabel;
        [Tooltip("If true, the change will be permanent and cannot be changed back unless called from another order where this is false.")]
        [SerializeField] protected bool forcePermanentChange;

        public override void OnEnter()
        {
            if (location.locationRef == null || location.Value == null)
            {
                Continue();
                return;
            }
            location.Value.LocationStatus = status;
            location.Value.CustomStatusLabel = customLabel;
            location.Value.ForcePermanentChange = forcePermanentChange;

            Continue();
        }

        public override string GetSummary()
        {
            return location.locationRef == null ? "Error: No location selected" : "Updates " + location.Value.name + " to " + status;
        }

        public override Color GetButtonColour()
        {
            return new Color32(184, 253, 255, 255);
        }
    }
}
