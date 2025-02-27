using Mapbox.Unity.Map;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Class that represents a location marker object on the map.
    /// These are created elsewhere in LUTE and then accessed using the LUTEMapManager/Spawner.
    /// Handles the rendering of a map pin and utilises click interfaces to trigger events when the marker is pressed.
    /// In this scenario the marker updates the visuals based on the location status which is defined in the displaylist on the locationinfo/variable.
    /// If the display options are missing then we should somehow find the default list of options.
    /// </summary>
    public class LocationMarker : MonoBehaviour, IPointerClickHandler
    {
        private Camera markerCamera;
        private Canvas markerCanvas;
        private BasicFlowEngine engine; // Used to access Nodes and other variables.
        private AbstractMap map; // Used to access the map and its settings.
        //private LUTELocationInfo locationInfo; // Used to define visuals and update location graphics.
        private LocationVariable locVar; // The related variable with the corresponding info ID
        private LocationStatus priorStatus = LocationStatus.Unvisited;
        private bool preventUpdatingVisuals = false; // When a status gets updated the user has an option to ensure that the other settings will never change after the fact

        [Tooltip("The location pin sprite renderer")]
        [SerializeField] protected SpriteRenderer markerSpriteRenderer;
        [Tooltip("The radius sprite renderer")]
        [SerializeField] protected SpriteRenderer radiusSpriteRenderer; // Ideally a white circle so we can change colour easily
        [Tooltip("The text mesh that will be used to render the marker label")]
        [SerializeField] protected TextMesh markerTextMesh;
        [Tooltip("Whether this marker should change its visuals independently of any related Nodes or objects that may also update the visuals")]
        [SerializeField] protected bool updateVisuals;
        [Tooltip("Whether this location marker can be interacted with without location being satisfied")]
        [SerializeField] protected bool allowClickWithoutLocation;
        [Tooltip("Whether this location marker can be interacted with on the map")]
        [SerializeField] protected bool interactable;

        public LocationVariable LocationVariable { get => locVar; }
        public SpriteRenderer MarkerSpriteRenderer { get => markerSpriteRenderer; }
        public SpriteRenderer RadiusRenderer { get => radiusSpriteRenderer; }
        public TextMesh MarkerTextMesh { get => markerTextMesh; }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable)
                return;

            // Ensure that there is a valid location variable that is associated with this location

            if (locVar == null)
            {
                return;
            }

            // In all cases we send a signal that the location has been clicked (if we found a matching location variable and the location marker can be interacted with)
            LocationServiceSignals.DoLocationClicked(locVar);

            if (!locVar.Value.AllowNodeControls)
            {
                return;
            }

            // Ensure that we have a reference to the engine and information on the Node to execute if we are attempting to execute a Node when clicked
            if (engine == null || locVar.Value.ExecuteNode == null)
            {
                return;
            }

            // If we do not require a location to be met then we can execute the Node on click
            if (allowClickWithoutLocation)
            {
                engine.ExecuteNode(locVar.Value.ExecuteNode);
            }
            else
            {
                // Ensure that the location in question has been met before we execute the Node
                if (locVar.Evaluate(ComparisonOperator.Equals, null))
                {
                    // Even if the location has been completed we can still execute the Node - multiple executions is controlled by the Node itself not here
                    engine.ExecuteNode(locVar.Value.ExecuteNode);
                }
            }
        }

        // Sets up the canvas to use the marker camera to render it
        public void SetCanvasCam(Camera cam)
        {
            markerCamera = cam;
            if (markerCanvas == null)
                markerCanvas = GetComponentInChildren<Canvas>();
            if (markerCanvas != null)
                markerCanvas.worldCamera = cam;
        }

        public void SetInfo(LocationVariable locVar, BasicFlowEngine engine)
        {
            if (locVar == null || engine == null)
                return;
            this.locVar = locVar;
            this.engine = engine;
            this.map = engine.GetAbstractMap();
        }

        public void HideMarker()
        {
            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void ShowMarker()
        {
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }
        }

        protected void OnEnable()
        {
            LocationServiceSignals.OnLocationComplete += OnLocationComplete;
            NodeSignals.OnNodeEnd += OnNodeEnd;
        }

        protected void OnDestroy()
        {
            LocationServiceSignals.OnLocationComplete -= OnLocationComplete;
            NodeSignals.OnNodeEnd -= OnNodeEnd;
        }

        protected void Start()
        {
            locVar = engine.GetComponents<LocationVariable>().FirstOrDefault(x => x.Value.InfoID == locVar.Value.InfoID);
        }

        protected void Update()
        {
            // Ensure that location marker always faces the rendering camera
            if (markerCamera != null)
            {
                transform.LookAt(transform.position + markerCamera.transform.rotation * Vector3.forward, markerCamera.transform.rotation * Vector3.up);
            }

            if (locVar.Value == null)
            {
                return;
            }

            if (locVar.Value.StatusDisplayOptionsList == null)
            {
                locVar.Value.StatusDisplayOptionsList = engine.GetMapManager().DefaultLocationDisplayList;
            }

            // Constantly check the location status and update the visuals if required.
            locVar.Evaluate(ComparisonOperator.Equals, null);

            if (!updateVisuals)
            {
                return;
            }

            // Switch on the location status then find the display options for that status and update the visuals
            switch (locVar.Value.LocationStatus)
            {
                case LocationStatus.Unvisited:
                    LUTELocationDisplayOptions displayOptionsUnvisisted = locVar.Value.StatusDisplayOptionsList.list.FirstOrDefault(x => x.status == LocationStatus.Unvisited).locationDisplayOptions;
                    if (displayOptionsUnvisisted != null)
                    {
                        UpdateVisuals(displayOptionsUnvisisted);
                    }
                    break;
                case LocationStatus.Visited:
                    LUTELocationDisplayOptions displayOptionsVisisted = locVar.Value.StatusDisplayOptionsList.list.FirstOrDefault(x => x.status == LocationStatus.Visited).locationDisplayOptions;
                    if (displayOptionsVisisted != null)
                    {
                        UpdateVisuals(displayOptionsVisisted);
                    }
                    break;
                case LocationStatus.Completed:
                    LUTELocationDisplayOptions displayOptionsCompleted = locVar.Value.StatusDisplayOptionsList.list.FirstOrDefault(x => x.status == LocationStatus.Completed).locationDisplayOptions;
                    if (displayOptionsCompleted != null)
                    {
                        UpdateVisuals(displayOptionsCompleted);
                    }
                    break;
                case LocationStatus.Custom:
                    foreach (var display in locVar.Value.StatusDisplayOptionsList.list)
                    {
                        if (display.status == LocationStatus.Custom && display.CustomStatusLabel == locVar.Value.CustomStatusLabel)
                        {
                            LUTELocationDisplayOptions displayOptionsCustom = display.locationDisplayOptions;
                            if (displayOptionsCustom != null)
                            {
                                UpdateVisuals(displayOptionsCustom, display);
                            }
                            break;
                        }
                    }
                    break;
                default:
                    break;

                    // Feel free to add any other status types here
                    // Just ensure that the list you have reference to includes the status in the List
            }
        }

        private void UpdateVisuals(LUTELocationDisplayOptions displayOptions, LUTELocationStatusDisplayList.LUTELocationStatusDisplay displayOption = null)
        {
            if (displayOptions == null)
                return;

            if (locVar.Value.ForcePermanentChange)
            {
                preventUpdatingVisuals = true;
            }
            else
            {
                preventUpdatingVisuals = false;
            }

            SetMarkerPosition();

            if (preventUpdatingVisuals && displayOption == null)
            {
                return;
            }

            ApplyVisuals(displayOptions);
        }

        private void ApplyVisuals(LUTELocationDisplayOptions displayOptions)
        {
            SetMarkerText(locVar.Value.LocationName, displayOptions.ShowName, displayOptions.NameLabelColor);
            SetMarkerSprite(displayOptions.MarkerSprite, displayOptions.ShowSprite);
            SetMarkerPosition();
            UpdateRadius(displayOptions.RadiusColour, displayOptions.ShowRadius);
        }


        private void SetMarkerText(string text, bool showText, Color textColour)
        {
            if (markerTextMesh == null)
                markerTextMesh = GetComponentInChildren<TextMesh>();
            if (markerTextMesh)
            {
                if (showText)
                {
                    markerTextMesh.color = textColour;
                    markerTextMesh.text = text;
                }
                else
                    markerTextMesh.text = "";
            }
        }

        private void SetMarkerSprite(Sprite sprite, bool showSprite)
        {
            if (markerSpriteRenderer == null)
                markerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (markerSpriteRenderer)
                markerSpriteRenderer.sprite = showSprite ? sprite : null;

            if (map == null)
                return;

            // Update scale and enabled state based on zoom factor
        }

        private void SetMarkerPosition()
        {
            // Update position so the marker does not move with the map
            transform.localPosition = map.GeoToWorldPosition(locVar.Value.LatLongString(), true);
        }

        private void UpdateRadius(Color color, bool showRadius)
        {
            if (radiusSpriteRenderer == null)
                return;
            if (radiusSpriteRenderer)
                radiusSpriteRenderer.color = color;
            radiusSpriteRenderer.enabled = showRadius;

            // Update scale and enabled state based on zoom factor
            // Radius needs to be accurate to the meters we have set
        }

        private void OnLocationComplete(LocationVariable location)
        {
            if (locVar.Value == null)
                return;

            if (location.Value.LocationStatus != LocationStatus.Completed)
            {
                locVar.Value.LocationStatus = LocationStatus.Visited;
                if (locVar.Value.SaveInfo)
                {
                    SaveLocationInfo();
                }
            }
            else
                return;
        }

        private void OnNodeEnd(Node node)
        {
            if (locVar.Value == null || node == null)
                return;

            if (node.CanUpdateLocationToComplete(locVar))
            {
                locVar.Value.LocationStatus = LocationStatus.Completed;
                if (locVar.Value.SaveInfo)
                {
                    SaveLocationInfo();
                }
            }
        }

        private void SaveLocationInfo()
        {
            if (locVar.Value.LocationStatus != locVar.Value.LocationStatus)
            {
                locVar.Value.LocationStatus = locVar.Value.LocationStatus;

                var saveManager = LogaManager.Instance.SaveManager;
                saveManager.AddSavePoint("ObjectInfo" + locVar.Value.LocationName, "A list of location info to be stored " + System.DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy"), false);
            }
        }
    }
}