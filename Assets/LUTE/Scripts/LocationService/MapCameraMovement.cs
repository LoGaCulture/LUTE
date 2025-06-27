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
        [Range(0.0f, 5.0f)]
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
        [Tooltip("Whether to allow tilting of the map.")]
        /*[SerializeField] */
        protected bool allowTilt = false;
        [Tooltip("The camera that is used to render the map during runtime.")]
        [SerializeField] protected Camera gameCamera;
        [Tooltip("The camera that is used to render the map during editor mode.")]
        [SerializeField] protected Camera editorCamera;
        [Tooltip("The reference to the map component - will attempt to find this automatically if not provided.")]
        [SerializeField] protected AbstractMap map;
        [Tooltip("Whether to use degree method to render and move map around.")]
        [SerializeField] protected bool useDegree = false;

        [Header("Touch Interaction Settings")]
        [Tooltip("Minimum angle change to trigger rotation")]
        [SerializeField] protected float rotationThreshold = 3.2f;
        [Tooltip("Sensitivity of zoom interaction")]
        [SerializeField] protected float zoomSensitivity = 0.01f;
        [Tooltip("Sensitivity of rotation interaction")]
        [SerializeField] protected float rotationSensitivity = 4f;
        [Tooltip("Sensitivity of tilt interaction")]
        protected float tiltSensitivity = 0.5f;
        [Tooltip("Maximum tilt angle for the camera")]
        protected float maxTiltAngle = 60f;

        // Touch tracking
        private float currentTiltAngle = 0f;
        private Vector2 initialTouchZeroDelta;
        private Vector2 initialTouchOneDelta;

        private Vector3 origin;
        private Vector3 mousePosition;
        private Vector3 mousePositionPrevious;
        private bool shouldDrag;
        private bool initialised = false;
        private Plane groundPlane = new Plane(Vector3.up, 0);
        private float rotationZ = 0.0f; // Vertical rotation input
        private Vector3 defaultRotation;

        public float MinMapZoom => minZoomLevel;
        public float MaxMapZoom => maxZoomLevel;

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
            // Update the rotation value
            rotationZ += zInput * keysSensitivity;

            // Apply rotation to the map instead of the camera
            map.transform.localEulerAngles = new Vector3(0, rotationZ, 0);
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
            switch (Input.touchCount)
            {
                case 1: // One finger → Pan
                    HandleOneFinggerTouch();
                    break;
                case 2: // Two fingers → Multi-touch interactions
                    HandleTwoFingerTouch();
                    break;
                default:
                    break;
            }
        }

        private void HandleOneFinggerTouch()
        {
            if (!allowPan) return;

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Moved:
                    if (useDegree)
                    {
                        UseDegreeConversion();
                    }
                    else
                    {
                        UseMeterConversion();
                    }
                    break;
            }
        }

        private void HandleTwoFingerTouch()
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Simultaneous interaction detection
            bool canRotateAndZoom = CanPerformSimultaneousGestures(touchZero, touchOne);

            // Rotation detection
            if (allowRotate && IsRotationGesture(touchZero, touchOne))
            {
                HandleRotation(touchZero, touchOne);
            }

            // Zoom detection (can be simultaneous with rotation)
            if (allowZoom && IsPinchGesture(touchZero, touchOne))
            {
                HandleZoom(touchZero, touchOne);
            }

            // Tilt detection (prioritize rotation and zoom)
            if (allowTilt && IsTiltGesture(touchZero, touchOne) && !canRotateAndZoom)
            {
                HandleTilt(touchZero, touchOne);
            }
        }

        private bool CanPerformSimultaneousGestures(Touch touchZero, Touch touchOne)
        {
            // Detect if rotation and zoom can happen simultaneously
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Check if there's significant change in both rotation and distance
            return Mathf.Abs(Vector2.Angle(
                (touchZeroPrevPos - touchOnePrevPos).normalized,
                (touchZero.position - touchOne.position).normalized)) > rotationThreshold
                && Mathf.Abs(touchDeltaMag - prevTouchDeltaMag) > 10f;
        }

        private bool IsRotationGesture(Touch touchZero, Touch touchOne)
        {
            Vector2 prevDir = (touchZero.position - touchOne.position).normalized;
            Vector2 currentDir = ((touchZero.position + touchZero.deltaPosition) -
                                  (touchOne.position + touchOne.deltaPosition)).normalized;

            float rotationAngle = Vector2.Angle(prevDir, currentDir);
            return rotationAngle > rotationThreshold;
        }

        private bool IsPinchGesture(Touch touchZero, Touch touchOne)
        {
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            return Mathf.Abs(touchDeltaMag - prevTouchDeltaMag) > 10f;
        }

        private bool IsTiltGesture(Touch touchZero, Touch touchOne)
        {
            // Tilt is when both fingers move vertically at similar rates
            return Mathf.Abs(touchZero.deltaPosition.y - touchOne.deltaPosition.y) < 10f &&
                   (touchZero.deltaPosition.y > 5f || touchOne.deltaPosition.y > 5f);
        }

        private void HandleRotation(Touch touchZero, Touch touchOne)
        {
            Vector2 prevDir = (touchZero.position - touchOne.position).normalized;
            Vector2 currentDir = ((touchZero.position + touchZero.deltaPosition) -
                                  (touchOne.position + touchOne.deltaPosition)).normalized;

            float rotationAngle = Vector2.SignedAngle(prevDir, currentDir);
            map.transform.Rotate(Vector3.up, -rotationAngle * rotationSensitivity, Space.World);
        }

        private void HandleZoom(Touch touchZero, Touch touchOne)
        {
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float zoomFactor = (touchDeltaMag - prevTouchDeltaMag) * zoomSensitivity;
            ZoomMapUsingTouchOrMouse(zoomFactor);
        }

        private void HandleTilt(Touch touchZero, Touch touchOne)
        {
            // Average vertical movement of both fingers
            float avgDeltaY = (touchZero.deltaPosition.y + touchOne.deltaPosition.y) * 0.5f;

            // Update and clamp tilt angle
            currentTiltAngle = Mathf.Clamp(
                currentTiltAngle - avgDeltaY * tiltSensitivity,
                -maxTiltAngle,
                maxTiltAngle
            );

            // Apply tilt to camera
            gameCamera.transform.localRotation = Quaternion.Euler(currentTiltAngle, 0, 0);
        }

        protected virtual void TiltCamera(float tiltAmount)
        {
            gameCamera.transform.Rotate(Vector3.right, tiltAmount, Space.Self);
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

                if (allowTilt)
                {
                    TiltCamera(yMouse * 0.5f);
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
                            // Get the map's current rotation in the Y axis
                            float mapRotationY = map.transform.eulerAngles.y;
                            float rotationRadians = mapRotationY * Mathf.Deg2Rad;

                            // Rotate the offset vector to compensate for map rotation
                            float cosAngle = Mathf.Cos(rotationRadians);
                            float sinAngle = Mathf.Sin(rotationRadians);

                            float rotatedX = offset.x * cosAngle - offset.z * sinAngle;
                            float rotatedZ = offset.x * sinAngle + offset.z * cosAngle;

                            // Calculate the degree factor as before
                            float factor = panSpeed * Conversions.GetTileScaleInDegrees((float)map.CenterLatitudeLongitude.x, map.AbsoluteZoom) / map.UnityTileSize;

                            // Apply rotated offset
                            var latitudeLongitude = new Vector2d(
                                map.CenterLatitudeLongitude.x + rotatedZ * factor,
                                map.CenterLatitudeLongitude.y + rotatedX * factor
                            );
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
                            // Get the map's current rotation in the Y axis
                            float mapRotationY = map.transform.eulerAngles.y;

                            // Convert to radians for the rotation calculation
                            float rotationRadians = mapRotationY * Mathf.Deg2Rad;

                            // Create a rotation matrix to adjust the offset vector based on map rotation
                            float cosAngle = Mathf.Cos(rotationRadians);
                            float sinAngle = Mathf.Sin(rotationRadians);

                            // Apply rotation to the offset vector to get screen-aligned movement
                            float rotatedX = offset.x * cosAngle - offset.z * sinAngle;
                            float rotatedZ = offset.x * sinAngle + offset.z * cosAngle;

                            // Calculate map movement using the rotated offset
                            float factor = panSpeed * Conversions.GetTileScaleInMeters((float)0, map.AbsoluteZoom) / map.UnityTileSize;
                            var latlongDelta = Conversions.MetersToLatLon(new Vector2d(rotatedX * factor, rotatedZ * factor));
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
