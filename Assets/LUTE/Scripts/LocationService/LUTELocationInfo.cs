using Mapbox.Utils;
using System;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// The status of a location.
    /// This is used to determine how the location should be displayed on the map within the scope of specific classes.
    /// </summary>
    [Serializable]
    public enum LocationStatus
    {
        Unvisited,
        Visited,
        Completed, // If there is a Node that uses this location, completed status is typically set when that Node is fully complete.
        Custom // Custom status for special cases
    }

    /// <summary>
    /// A class to store information about a location on the map.
    /// One could inherit this class for special cases.
    /// Typically the class is used to store information about a locations as scriptable objects which are then used in LocationVariables.
    /// </summary>
    [CreateAssetMenu(menuName = "LUTE/Location/Location Information")]
    [Serializable]
    public class LUTELocationInfo : ScriptableObject
    {
        [Tooltip("The ID of the location to be used in conditions and other checks.")]
        [SerializeField] private string infoID; // Unique ID for the location - this is sometimes created within editor code (such as when adding a new location to an editor map or in cases where this scriptable object is created not through context menus).
        [Tooltip("The name of the location.")]
        [SerializeField] private string locationName;

        [Header("Location Info")]
        [Tooltip("The coordinates of the location in the format 'latitude, longitude'")]
        [SerializeField] private string position;
        [SerializeField] private LocationStatus locationStatus = LocationStatus.Unvisited;
        [Tooltip("Whether the location information should be saved or not")]
        [SerializeField] private bool saveInfo = true;
        // The display options
        // Used to choose specific display options for this marker based on a specific location status
        [Tooltip("Different display options to use in different status scenarios - you should create a new list if you desire to overwrite this one.")]
        [SerializeField] private LUTELocationStatusDisplayList statusDisplayOptionsList;

        [Header("Location Settings")]
        [Tooltip("Whether or not this location can be used (can be set with location failure handling)")]
        [SerializeField] private bool locationDisabled = false;
        [Tooltip("The amount to increase the default radius of the location by ")]
        [SerializeField] private float radiusIncrease = 0.0f;
        [Tooltip("The amount to decrease the default radius of the location by ")]
        [SerializeField] private float radiusDecrease = 0.0f;
        [SerializeField] private bool forcePermanentChange;

        // These settings are to be used in conjunction with Nodes - the typical pattern of design is to force Nodes to trigger Locations but we allow this both ways because we are flexible and all about that OO design.
        [Header("Node Settings")]
        [Tooltip("Allow the location marker to execute Nodes")] // Allow this behaviour to encourage OO design but ensure that there is this switch to prohibit it where necessary.
        [SerializeField] private bool allowNodeControls = false; // Default to false as we only really execute Nodes using locations on eventhandlers or in conditions using the Update method.
        [Tooltip("The node to execute when marker is clicked")]
        [HideInInspector]
        [SerializeField] private Node executeNode;

        private string customStatusLabel; // To be used in conjunction with the custom status

        public string InfoID
        {
            get { return infoID; }
            set { infoID = value; }
        }

        public string LocationName
        {
            get { return locationName; }
            set { locationName = value; }
        }

        public string Position
        {
            get { return position; }
        }

        public LocationStatus LocationStatus
        {
            get { return locationStatus; }
            set { locationStatus = value; }
        }

        public bool SaveInfo
        {
            get { return saveInfo; }
            set { saveInfo = value; }
        }

        public bool AllowNodeControls
        {
            get { return allowNodeControls; }
            set { allowNodeControls = value; }
        }

        public Node ExecuteNode
        {
            get { return executeNode; }
            set { executeNode = value; }
        }

        public bool LocationDisabled
        {
            get { return locationDisabled; }
            set { locationDisabled = value; }
        }

        public float RadiusIncrease
        {
            get { return radiusIncrease; }
            set { radiusIncrease = value; }
        }

        public LUTELocationStatusDisplayList StatusDisplayOptionsList
        {
            get { return statusDisplayOptionsList; }
            set { statusDisplayOptionsList = value; }
        }

        public string CustomStatusLabel
        {
            get { return customStatusLabel; }
            set { customStatusLabel = value; }
        }

        public bool ForcePermanentChange
        {
            get { return forcePermanentChange; }
            set { forcePermanentChange = value; }
        }

        public virtual Vector2d LatLongString()
        {
            return Mapbox.Unity.Utilities.Conversions.StringToLatLon(position);
        }
    }
}