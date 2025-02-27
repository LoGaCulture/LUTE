using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// Class that handles the movement of the map camera.
    /// Typically contains the main cameras that are used to render the map and therefore is accessed by map managers and other map related behaviours.
    /// </summary>
    public class MapCameraMovement : MonoBehaviour
    {
        [HideInInspector]
        public bool dragStartedOnUI = false; // Ensures the editor controls know when we are moving the map

        [Tooltip("Whether to allow panning of the map.")]
        [SerializeField] protected bool allowPan = true;
        [Tooltip("The speed that is used when panning the map.")]
        [Range(0.0f, 1.0f)]
        [SerializeField] protected float panSpeed = 1.0f;
        [Tooltip("The speed of map movement when using keyboard input.")]
        [SerializeField] float keysSensitivity = 2f;
        [Tooltip("Whether to allow zooming of the map.")]
        [SerializeField] protected bool allowZoom = true;
        [Tooltip("The speed that is used when zooming the map.")]
        [Range(0.0f, 1.0f)]
        [SerializeField] protected float zoomSpeed = 0.25f;
        [Range(0, 21)]
        [SerializeField] protected float minZoomLevel = 0.0f;
        [Range(0, 21)]
        [SerializeField] protected float maxZoomLevel = 21.0f;
        [Tooltip("Whether to allow rotation of the map.")]
        [SerializeField] protected bool allowRotate = true;
        [Tooltip("The camera that is used to render the map during runtime.")]
        [SerializeField] protected Camera gameCamera;
        [Tooltip("The camera that is used to render the map during editor mode.")]
        [SerializeField] protected Camera editorCamera;
        [Tooltip("The reference to the map component - will attempt to find this automatically if not provided.")]
        [SerializeField] protected AbstractMap map;
        [Tooltip("Whether to use degree method to render and move map around.")]
        [SerializeField] protected bool useDegree = false;

        private Vector3 origin;
        private Vector3 mousePosition;
        private Vector3 mousePositionPrevious;
        private bool shouldDrag;
        private bool initialised = false;
        private Plane groundPlane = new Plane(Vector3.up, 0);
        private float rotationZ = 0.0f; // Vertical rotation input
        private Vector3 defaultRotation;

        public static MapCameraMovement Instance { get; private set; } // Static reference to this class

        public virtual Camera ReferenceCamera
        {
            get
            {
                return Application.isPlaying ? gameCamera : editorCamera;
            }
        }

        public virtual void ZoomMapUsingTouchOrMouse(float zoomFactor)
        {
            var zoom = Mathf.Max(minZoomLevel, Mathf.Min(map.Zoom + zoomFactor * zoomSpeed, maxZoomLevel));
            if (Math.Abs(zoom - map.Zoom) > float.Epsilon)
            {
                map.UpdateMap(map.CenterLatitudeLongitude, zoom);
            }
        }

        public virtual void PanMapUsingKeyBoard(float xMove, float zMove)
        {
            if (Math.Abs(xMove) > 0.0f || Math.Abs(zMove) > 0.0f)
            {
                // Get the number of degrees in a tile at the current zoom level.
                // Divide it by the tile width in pixels ( 256 in our case)
                // to get degrees represented by each pixel.
                // Keyboard offset is in pixels, therefore multiply the factor with the offset to move the center.
                float factor = panSpeed * (Conversions.GetTileScaleInDegrees((float)map.CenterLatitudeLongitude.x, map.AbsoluteZoom));

                var latitudeLongitude = new Vector2d(map.CenterLatitudeLongitude.x + zMove * factor * 2.0f, map.CenterLatitudeLongitude.y + xMove * factor * 4.0f);

                map.UpdateMap(latitudeLongitude, map.Zoom);
            }
        }

        public virtual void PanOrRotateCamera(float zInput)
        {
            rotationZ += zInput * keysSensitivity;

            gameCamera.transform.localEulerAngles = new Vector3(defaultRotation.x, defaultRotation.y, rotationZ);
        }

        public virtual void PanMapUsingTouchOrMouse()
        {
            if (useDegree)
            {
                UseDegreeConversion();
            }
            else
            {
                UseMeterConversion();
            }
        }

        public virtual void PanMapUsingTouchOrMouseEditor(Event e)
        {
            UseMeterConversionEditor(e);
        }

        protected virtual void Start()
        {
            defaultRotation = gameCamera.transform.localEulerAngles;
        }

        protected virtual void Update()
        {
            if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
            {
                dragStartedOnUI = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                dragStartedOnUI = false;
            }
        }

        protected virtual void HandleTouch()
        {
            float zoomFactor = 0.0f;

            switch (Input.touchCount)
            {
                case 1:
                    {
                        if (allowPan)
                        {
                            PanMapUsingTouchOrMouse();
                        }

                        Touch touch = Input.GetTouch(0);
                        float touchX = touch.deltaPosition.x;
                        float touchY = touch.deltaPosition.y;

                        if (allowRotate)
                        {
                            PanOrRotateCamera(touchX);
                        }
                    }
                    break;
                case 2:
                    {
                        // Store both touches
                        Touch touchZero = Input.GetTouch(0);
                        Touch touchOne = Input.GetTouch(1);

                        // Find the position in the previous frame of each touch
                        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                        // Find the magnitude of the vector (the distance) between the touches in each frame
                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                        // Find the difference in the distances between each frame
                        zoomFactor = 0.01f * (prevTouchDeltaMag - touchDeltaMag);
                    }
                    if (allowZoom)
                    {
                        ZoomMapUsingTouchOrMouse(zoomFactor);
                    }
                    break;
                default:
                    break;
            }
        }

        protected virtual void HandleMouseAndKeyboard()
        {
            float scrollDelta = 0.0f;
            scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            ZoomMapUsingTouchOrMouse(scrollDelta);

            // Pan using keyboard
            float xMove = Input.GetAxis("Horizontal");
            float yMove = Input.GetAxis("Vertical");

            if (allowPan)
            {
                PanMapUsingKeyBoard(xMove, yMove);
            }

            // Pan using mouse
            if (Input.GetMouseButton(1))
            {
                float xMouse = Input.GetAxis("Mouse X");
                float yMouse = Input.GetAxis("Mouse Y");

                if (allowRotate)
                {
                    PanOrRotateCamera(xMouse);
                }
            }

            if (allowPan)
            {
                PanMapUsingTouchOrMouse();
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (gameCamera == null)
            {
                gameCamera = GetComponent<Camera>();
            }
            if (map == null)
            {
                map = GetComponent<AbstractMap>();
            }
            if (map == null)
            {
                map = FindFirstObjectByType<AbstractMap>();
            }
            map.OnInitialized += () =>
            {
                initialised = true;
            };
        }

        private void LateUpdate()
        {
            if (!initialised)
            {
                return;
            }

            if (!dragStartedOnUI)
            {
                if (Input.touchSupported && Input.touchCount > 0)
                {
                    HandleTouch();
                }
                else
                {
                    HandleMouseAndKeyboard();
                }
            }
        }

        private void UseDegreeConversion()
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var mousePosScreen = Input.mousePosition;
                //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                //http://answers.unity3d.com/answers/599100/view.html
                mousePosScreen.z = gameCamera.transform.localPosition.y;
                mousePosition = gameCamera.ScreenToWorldPoint(mousePosScreen);

                if (shouldDrag == false)
                {
                    shouldDrag = true;
                    origin = gameCamera.ScreenToWorldPoint(mousePosScreen);
                }
            }
            else
            {
                shouldDrag = false;
            }

            if (shouldDrag == true)
            {
                var changeFromPreviousPosition = mousePositionPrevious - mousePosition;
                if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
                {
                    mousePositionPrevious = mousePosition;
                    var offset = origin - mousePosition;

                    if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
                    {
                        if (null != map)
                        {
                            // Get the number of degrees in a tile at the current zoom level.
                            // Divide it by the tile width in pixels ( 256 in our case)
                            // to get degrees represented by each pixel.
                            // Mouse offset is in pixels, therefore multiply the factor with the offset to move the center.
                            float factor = panSpeed * Conversions.GetTileScaleInDegrees((float)map.CenterLatitudeLongitude.x, map.AbsoluteZoom) / map.UnityTileSize;

                            var latitudeLongitude = new Vector2d(map.CenterLatitudeLongitude.x + offset.z * factor, map.CenterLatitudeLongitude.y + offset.x * factor);
                            map.UpdateMap(latitudeLongitude, map.Zoom);
                        }
                    }
                    origin = mousePosition;
                }
                else
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                    mousePositionPrevious = mousePosition;
                    origin = mousePosition;
                }
            }
        }

        private void UseMeterConversion()
        {
            if (Input.GetMouseButtonUp(1))
            {
                var mousePosScreen = Input.mousePosition;
                //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                //http://answers.unity3d.com/answers/599100/view.html
                mousePosScreen.z = gameCamera.transform.localPosition.y;
                var pos = gameCamera.ScreenToWorldPoint(mousePosScreen);

                var latlongDelta = map.WorldToGeoPosition(pos);
                // Debug.Log("Latitude: " + latlongDelta.x + " Longitude: " + latlongDelta.y);
            }

            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                var mousePosScreen = Input.mousePosition;
                //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                //http://answers.unity3d.com/answers/599100/view.html
                mousePosScreen.z = gameCamera.transform.localPosition.y;
                mousePosition = gameCamera.ScreenToWorldPoint(mousePosScreen);

                if (shouldDrag == false)
                {
                    shouldDrag = true;
                    origin = gameCamera.ScreenToWorldPoint(mousePosScreen);
                }
            }
            else
            {
                shouldDrag = false;
            }

            if (shouldDrag == true)
            {
                var changeFromPreviousPosition = mousePositionPrevious - mousePosition;
                if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
                {
                    mousePositionPrevious = mousePosition;
                    var offset = origin - mousePosition;

                    if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
                    {
                        if (null != map)
                        {
                            float factor = panSpeed * Conversions.GetTileScaleInMeters((float)0, map.AbsoluteZoom) / map.UnityTileSize;
                            var latlongDelta = Conversions.MetersToLatLon(new Vector2d(offset.x * factor, offset.z * factor));
                            var newLatLong = map.CenterLatitudeLongitude + latlongDelta;

                            map.UpdateMap(newLatLong, map.Zoom);
                        }
                    }
                    origin = mousePosition;
                }
                else
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                    mousePositionPrevious = mousePosition;
                    origin = mousePosition;
                }
            }
        }

        private void UseMeterConversionEditor(Event e)
        {
            if (dragStartedOnUI)
            {
                var mousePosScreen = e.mousePosition;
                //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                //http://answers.unity3d.com/answers/599100/view.html
                var newLocVec = new Vector3(mousePosScreen.x, mousePosScreen.y, gameCamera.transform.localPosition.y);
                mousePosition = gameCamera.ScreenToWorldPoint(newLocVec);

                if (shouldDrag == false)
                {
                    shouldDrag = true;
                    origin = gameCamera.ScreenToWorldPoint(newLocVec);
                }
            }
            else
            {
                shouldDrag = false;
            }

            if (shouldDrag == true)
            {
                var changeFromPreviousPosition = mousePositionPrevious - mousePosition;
                if (Mathf.Abs(changeFromPreviousPosition.x) > 0.0f || Mathf.Abs(changeFromPreviousPosition.y) > 0.0f)
                {
                    mousePositionPrevious = mousePosition;
                    var offset = origin - mousePosition;

                    if (Mathf.Abs(offset.x) > 0.0f || Mathf.Abs(offset.z) > 0.0f)
                    {
                        if (null != map)
                        {
                            float factor = panSpeed * Conversions.GetTileScaleInMeters((float)0, map.AbsoluteZoom) / map.UnityTileSize;
                            var latlongDelta = Conversions.MetersToLatLon(new Vector2d(offset.x * factor, -offset.z * factor));
                            var newLatLong = map.CenterLatitudeLongitude + latlongDelta;

                            map.UpdateMap(newLatLong, map.Zoom);
                        }
                    }
                    origin = mousePosition;
                }
                else
                {
                    mousePositionPrevious = mousePosition;
                    origin = mousePosition;
                }
            }
        }
    }
}
