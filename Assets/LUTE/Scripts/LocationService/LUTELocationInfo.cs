using Mapbox.Utils;
using System;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [Serializable]
    [CreateAssetMenu(menuName = "LUTE/Locations/Location Information")]
    public class LUTELocationInfo : ScriptableObject
    {
        [Serializable]
        public enum LocationStatus
        {
            Unvisited,
            Visited,
            Completed
        }

        public string infoID; // Unique ID for the location

        [Header("Location Info")]
        [Tooltip("The coordinates of the location in the format 'latitude, longitude'")]
        public string Position;

        [Header("Location Display")]
        [Tooltip("The name of the location")]
        public string Name;
        [Tooltip("The sprite to display at the location when not visited")]
        public Sprite Sprite;
        [Tooltip("The sprite to display at the location when visited but not complete")]
        public Sprite InProgressSprite;
        [Tooltip("The sprite to display at the location when visited and complete")]
        public Sprite CompletedSprite;
        [Tooltip("The colour of the name label")]
        public Color Color = Color.white;
        [Tooltip("Whether the name should be shown or not on the location marker")]
        public bool ShowName;
        [Tooltip("Whether the radius of the location should be shown or not")]
        public bool showRadius;
        [Tooltip("The colour of the radius of the location")]
        public Color defaultRadiusColour = LogaConstants.defaultRadiusColour;
        [Tooltip("The colour of the radius of the location when visited")]
        public Color visitedRadiusColour = LogaConstants.defaultRadiusColour;
        [Tooltip("The colour of the radius of the location when completed")]
        public Color completedRadiusColour = LogaConstants.defaultRadiusColour;

        [Header("Location Settings")]
        [Tooltip("Whether or not this location can be used (can be set with location failure handling)")]
        public bool locationDisabled = false;

        [Tooltip("The amount to increase the radius of the location by ")]
        [SerializeField] protected float radiusIncrease = 0.0f;
        [Tooltip("Whether the location can be interacted with or not (using mouse, touch etc. input)")]
        [SerializeField] protected bool interactable = true;
        [Tooltip("Whether the location info should be saved or not")]
        [SerializeField] protected bool saveInfo = true;
        [Tooltip("Whether the marker should update independently of any related nodes")]
        [SerializeField] protected bool indepedentMarkerUpdating;
        [Tooltip("Whether the location can be clicked without a location evaluated fully by player")]
        [SerializeField] protected bool allowClickWithoutLocation;
        [SerializeField] protected LocationStatus locationStatus = LocationStatus.Unvisited;

        [Header("Location Info Panel")]
        [Tooltip("The sprite to display on the locatio info panel")]
        [SerializeField] protected Sprite infoImage;
        [Tooltip("The display name of the location to display on a location info panel")]
        [SerializeField] protected string displayName;
        [Tooltip("The description of the location to display on a location info panel")]
        [SerializeField] protected string description;

        [Header("Node Location Settings")]
        [Tooltip("The node that will trigger this location to be completed once the node itself completes.")]
        [SerializeField] protected string nodeComplete;
        [Tooltip("The node to execute when marker is clicked")]
        [SerializeField] protected string executeNode;



        public LocationStatus _LocationStatus
        {
            get { return locationStatus; }
            set { locationStatus = value; }
        }
        public float RadiusIncrease
        {
            get { return radiusIncrease; }
            set { radiusIncrease = value; }
        }
        public string NodeComplete
        {
            get { return nodeComplete; }
            set { nodeComplete = value; }
        }
        public string ExecuteNode
        {
            get { return executeNode; }
            set { executeNode = value; }
        }
        public bool Interactable
        {
            get { return interactable; }
            set { interactable = value; }
        }
        public bool SaveInfo
        {
            get { return saveInfo; }
            set { saveInfo = value; }
        }

        public bool IndependentMarkerUpdating
        {
            get { return indepedentMarkerUpdating; }
            set { indepedentMarkerUpdating = value; }
        }
        public bool AllowClickWithoutLocation
        {
            get { return allowClickWithoutLocation; }
            set { allowClickWithoutLocation = value; }
        }

        public Sprite LocationImage
        {
            get { return infoImage; }
            set { infoImage = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        protected virtual void Awake()
        {
            infoID = GetInfoID();
        }

        protected virtual void OnEnable()
        {
            infoID = GetInfoID();
        }

        private string GetInfoID()
        {
            if (string.IsNullOrEmpty(infoID))
            {
                infoID = Guid.NewGuid().ToString();
            }
            return infoID;
        }

        public virtual Vector2d LatLongString()
        {
            return Mapbox.Unity.Utilities.Conversions.StringToLatLon(Position);
        }
    }
}
