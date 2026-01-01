using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

public class XRManager : MonoBehaviour
{
    // Singleton instance
    private static XRManager _instance;

    /// <summary>
    /// Gets the singleton instance of XRManager.
    /// </summary>
    public static XRManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = GameObject.FindFirstObjectByType<XRManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach XRManager
                    GameObject xrManagerObject = new GameObject("XRManager");
                    _instance = xrManagerObject.AddComponent<XRManager>();



                    // Optionally, make the XRManager persist across scenes
                    // Uncomment the following line if needed
                    // DontDestroyOnLoad(xrManagerObject);
                }
            }
            return _instance;
        }
    }

    private GameObject _xrPrefab;
    private GameObject _spawnedXRObject;
    private bool _isInitialized = false;

    /// <summary>
    /// Initializes the XR environment by loading the XR prefab and starting XR subsystems.
    /// </summary>
    /// <returns>True if initialization was successful, false otherwise.</returns>
    public bool InitializeXR()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("XR is already initialized.");
            return false;
        }

        if (XRGeneralSettings.Instance != null)
        {
            var xrManager = XRGeneralSettings.Instance.Manager;

            // Check if the XR subsystems are already initialized
            if (xrManager.isInitializationComplete && xrManager.activeLoader != null)
            {
                Debug.Log("XR subsystems are already initialized.");
            }
            else
            {
                xrManager.InitializeLoaderSync();
                if (xrManager.activeLoader == null)
                {
                    Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
                    return false;
                }
                else
                {
                    xrManager.StartSubsystems();
                }
            }
        }
        else
        {
            Debug.LogError("XRGeneralSettings.Instance is null.");
            return false;
        }

        // Load XR prefab from Resources folder
        if (_xrPrefab == null)
        {
            _xrPrefab = Resources.Load<GameObject>("Prefabs/XR-New");
            if (_xrPrefab == null)
            {
                Debug.LogError("XR prefab not found in Resources/Prefabs folder.");
                return false;
            }
        }

        _isInitialized = true;
        return true;
    }

    /// <summary>
    /// Toggles the XR object's active state.
    /// </summary>
    /// <returns>The new active state of the XR object.</returns>
    public bool ToggleXR()
    {
        if (!_isInitialized)
        {
            InitializeXR();
        }

        if (_spawnedXRObject != null)
        {
            bool isActive = !_spawnedXRObject.activeSelf;
            SetXRActive(isActive);
            return isActive;
        }
        else
        {
            Debug.LogWarning("XR object is not initialized.");
            return false;
        }
    }

    /// <summary>
    /// Sets the XR object's active state.
    /// </summary>
    /// <param name="active">True to activate XR, false to deactivate.</param>
    public void SetXRActive(bool active)
    {
        if (!_isInitialized)
        {
            InitializeXR();
        }

        if (_spawnedXRObject != null)
        {
            // Optionally handle main camera activation/deactivation
            GameObject mainCamera = GameObject.Find("Camera");
            if (mainCamera != null)
            {
                mainCamera.SetActive(!active);
            }

            _spawnedXRObject.SetActive(active);
        }
        else
        {
            Debug.LogWarning("XR object is not initialized.");
            // Instantiate XR prefab if not already in the scene


            _spawnedXRObject = GameObject.Find("XR-New");
            if (_spawnedXRObject == null)
            {
                _spawnedXRObject = Instantiate(_xrPrefab);
                _spawnedXRObject.name = "XR-New";

            }
        }
    }

    /// <summary>
    /// Gets the spawned XR object.
    /// </summary>
    /// <returns>The spawned XR GameObject, or null if not initialized.</returns>
    public GameObject GetXRObject()
    {
        if (!_isInitialized)
        {
            InitializeXR();
        }

        return _spawnedXRObject;
    }

    /// <summary>
    /// Toggles plane detection on or off.
    /// </summary>
    /// <param name="active">True to enable plane detection, false to disable.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool TogglePlaneDetection(bool active)
    {
        if (!_isInitialized)
        {
            InitializeXR();
        }

        if (_spawnedXRObject != null)
        {
            ARPlaneManager planeManager = _spawnedXRObject.GetComponentInChildren<ARPlaneManager>();
            if (planeManager != null)
            {
                planeManager.enabled = active;
                SetAllPlanesActive(active);
                return true;
            }
            else
            {
                Debug.LogWarning("ARPlaneManager not found in XR object.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("XR object is not initialized.");
            return false;
        }
    }

    /// <summary>
    /// Toggles point cloud visualization on or off.
    /// </summary>
    /// <param name="active">True to enable point cloud, false to disable.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool TogglePointCloud(bool active)
    {
        if (!_isInitialized)
        {
            InitializeXR();
        }

        if (_spawnedXRObject != null)
        {
            ARPointCloudManager pointCloudManager = _spawnedXRObject.GetComponentInChildren<ARPointCloudManager>();
            if (pointCloudManager != null)
            {
                pointCloudManager.enabled = active;
                return true;
            }
            else
            {
                Debug.LogWarning("ARPointCloudManager not found in XR object.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("XR object is not initialized.");
            return false;
        }
    }

    /// <summary>
    /// Sets all detected planes to active or inactive.
    /// </summary>
    /// <param name="active">True to activate all planes, false to deactivate.</param>
    private void SetAllPlanesActive(bool active)
    {
        if (_spawnedXRObject != null)
        {
            ARPlaneManager planeManager = _spawnedXRObject.GetComponentInChildren<ARPlaneManager>();
            if (planeManager != null)
            {
                foreach (var plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(active);
                }
            }
            else
            {
                Debug.LogWarning("ARPlaneManager not found in XR object.");
            }
        }
        else
        {
            Debug.LogWarning("XR object is not initialized.");
        }
    }

    /// <summary>
    /// Stops XR subsystems and resets initialization state when destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (_isInitialized && XRGeneralSettings.Instance != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
        _isInitialized = false;
    }
}
