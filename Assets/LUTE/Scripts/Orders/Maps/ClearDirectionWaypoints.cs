using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Map",
        "Clear Direction Waypoints",
        "Clears all waypoints in the directions factory and destroys the direction paths.")]
    public class ClearDirectionWaypoints : Order
    {
        [Tooltip("The directions factory in which the waypoints are stored.")]
        [SerializeField] protected LUTEDirectionsFactory directionsFactory;

        public override void OnEnter()
        {
            if (directionsFactory == null)
            {
                Continue();
                return;
            }

            directionsFactory.ClearWaypoints();

            Continue();
        }

        public override string GetSummary()
        {
            if (directionsFactory != null)
                return "    Clear waypoints in directions factory.";

            return "    Error: No directions factory provided.";
        }

        public override Color GetButtonColour()
        {
            return new Color32(235, 191, 217, 255);
        }
    }
}
