using UnityEngine;

namespace LoGaCulture.LUTE
{
    [EventHandlerInfo("Location",
                  "On Location",
                  "Executes when a specific location has been visited (with overload parameters).")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class LocationClickEventHandler : EventHandler
    {
        [Tooltip("The location to check against.")]
        [SerializeField] protected LocationData location;
        [Tooltip("Whether the event should be triggered when relative location marker is pressed.")]
        [SerializeField] protected bool requiresPress;
        [Tooltip("Whether the event should be triggered when the player enters the location automatically.")]
        [SerializeField] protected bool autoTrigger = true;
        [Tooltip("If waiting for marker press, allow it to be pressed without the location satisfied.")]
        [SerializeField] protected bool requiresLocation;
        [Tooltip("If true, override the default display list that the location uses.")]
        [SerializeField] protected LUTELocationStatusDisplayList overrideLocationDisplayList;

        public LocationData Location
        {
            get { return location; }
        }

        private void OnEnable()
        {
            LocationServiceSignals.OnLocationClicked += OnLocationClicked;
        }

        private void OnDisable()
        {
            LocationServiceSignals.OnLocationClicked -= OnLocationClicked;
        }

        private void Update()
        {
            // Setting this for editor purposes
            autoTrigger = requiresPress ? false : true;
            requiresPress = autoTrigger ? false : requiresPress;
            requiresLocation = autoTrigger ? true : requiresLocation;

            if (!Application.isPlaying)
            {
                return;
            }

            if (overrideLocationDisplayList != null && location.Value != null)
            {
                location.Value.StatusDisplayOptionsList = overrideLocationDisplayList;
            }
            else
            {
                if (parentNode.GetEngine().GetMapManager().DefaultLocationDisplayList != null)
                {
                    location.Value.StatusDisplayOptionsList = parentNode.GetEngine().GetMapManager().DefaultLocationDisplayList;
                }
            }

            if (!autoTrigger)
            {
                return;
            }

            if (location.Value != null)
            {
                if (Application.isPlaying)
                {
                    bool locationMet = location.locationRef.Evaluate(ComparisonOperator.Equals, null);
                    if (locationMet && CheckLocationStatus())
                    {
                        // This may be an issue as we could potentially execute node multiple times?
                        ExecuteNode();
                    }
                }
            }
        }

        protected void OnLocationClicked(LocationVariable location)
        {
            if (!requiresPress || location.Value == null)
            {
                return;
            }

            if (location.Value.InfoID != this.location.Value.InfoID)
            {
                return;
            }

            bool locationMet = false;
            if (!requiresLocation)
            {
                locationMet = true;
            }
            else
            {
                locationMet = location.Evaluate(ComparisonOperator.Equals, null);
            }

            if (locationMet && CheckLocationStatus())
            {
                ExecuteNode();
            }
        }

        protected bool CheckLocationStatus()
        {
            if (location.Value.LocationDisabled)
                return false;


            return true;
        }
    }
}