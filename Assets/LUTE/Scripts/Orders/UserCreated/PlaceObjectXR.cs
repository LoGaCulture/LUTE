using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[OrderInfo("XR", "PlaceObjectOnPlane", "")]
[AddComponentMenu("")]
public class PlaceObjectXR : Order
{
    [Tooltip("The 3D object to place when clicked")]
    [SerializeField] private GameObject m_PrefabToPlace;

    [SerializeField] private string m_ObjectName;

    [SerializeField]
    [Tooltip("The Scriptable Object Asset that contains the ARRaycastHit event.")]
    private ARRaycastHitEventAsset raycastHitEvent;

    [SerializeField]
    public bool automaticallyPlaceObject = true;

    [SerializeField]
    public bool rotateable = true;
    [SerializeField]
    public bool scaleable = true;
    [SerializeField]
    public bool moveable = true;

    [SerializeField]
    public PlaneAlignment planeAlignment;

    // Tracks the spawned object in the scene, if any
    private GameObject m_SpawnedObject;
    private ARPlaneManager planeManager;
    private ObjectSpawner objectSpawner;

    /// <summary>
    /// Whether we're currently allowing new spawns.
    /// (Previously was ObjectSpawner.IsCurrentlyPlacingObject, but we keep it here now.)
    /// </summary>
    public static bool IsCurrentlyPlacingObject { get; private set; }

    private void OnObjectSpawned(GameObject obj)
    {
        Debug.Log("Object spawned!");

        var xrManager = XRManager.Instance;
        if (xrManager == null)
        {
            Debug.LogError("XRManager instance is null.");
            return;
        }

        var arObjectInstance = xrManager.GetXRObject();
        if (arObjectInstance == null)
        {
            Debug.LogError("XR object is not initialized.");
            return;
        }

        if (objectSpawner == null)
        {
            Debug.LogError("ObjectSpawner not found in XR object.");
            return;
        }

        // We only wanted one object? Then unsubscribe so we don't keep getting spawns.
        //objectSpawner.objectSpawned -= OnObjectSpawned;

        ObjectSpawnerWrapper objectSpawnerWrapper = objectSpawner.GetComponent<ObjectSpawnerWrapper>();
        if (objectSpawnerWrapper != null)
        {
            objectSpawnerWrapper.wrapperSpawned -= OnObjectSpawned;
        }

        // e.g., set up grab constraints
        var grabInteractable = obj.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = moveable;
            grabInteractable.trackRotation = rotateable;
            grabInteractable.trackScale = scaleable;
        }
        else
        {
            Debug.LogWarning("XRGrabInteractable component not found on spawned object.");
        }

        // (Optional) Remove the prefab from the spawner to avoid subsequent spawns
        if (objectSpawner.objectPrefabs.Contains(m_PrefabToPlace))
        {
            objectSpawner.objectPrefabs.Remove(m_PrefabToPlace);
        }

        // We're done placing, so disable new spawns
        IsCurrentlyPlacingObject = false;

        m_SpawnedObject = obj;

        // For some reason we need to wait a frame before enabling touches
        StartCoroutine(WaitForOneFrame());

        // Register in your object manager
        XRObjectManager.Instance.AddObject(m_ObjectName, obj);

        // Continue your "order" flow
        Continue();
    }

    /// <summary>
    /// We do a quick disable/enable to fix potential input/touch detection issues.
    /// </summary>
    IEnumerator WaitForOneFrame()
    {
        var grabInteractable = m_SpawnedObject.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
            yield return null;
            grabInteractable.enabled = true;
        }
    }

    public override void OnEnter()
    {
        var xrManager = XRManager.Instance;
        if (xrManager == null)
        {
            Debug.LogError("XRManager instance is null.");
            Continue();
            return;
        }

        GameObject arObjectInstance = xrManager.GetXRObject();
        if (arObjectInstance == null)
        {
            Debug.LogError("XR object is not initialized.");
            Continue();
            return;
        }

        planeManager = arObjectInstance.GetComponentInChildren<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager not found in XR object.");
            Continue();
            return;
        }

        objectSpawner = arObjectInstance.GetComponentInChildren<ObjectSpawner>();
        if (objectSpawner == null)
        {
            Debug.LogError("ObjectSpawner not found in XR object.");
            Continue();
            return;
        }

        //enable the objectSpawner gameobject
        objectSpawner.transform.gameObject.SetActive(true);

        // Listen for newly spawned objects
        //objectSpawner.objectSpawned += OnObjectSpawned;
        ObjectSpawnerWrapper wrapper = objectSpawner.GetComponent<ObjectSpawnerWrapper>();
        if (wrapper != null)
        {
            wrapper.wrapperSpawned += OnObjectSpawned;
        }


        // (Optional) add the prefab to the spawner's list if not already
        if (!objectSpawner.objectPrefabs.Contains(m_PrefabToPlace))
        {
            objectSpawner.objectPrefabs.Add(m_PrefabToPlace);
        }

        // Mark that we are now in "placing" mode
        IsCurrentlyPlacingObject = true;

        if (m_PrefabToPlace == null)
        {
            Debug.LogWarning($"{nameof(PlaceObjectXR)} on {name} has null m_PrefabToPlace. No effect in this scene.", this);
            Continue();
            return;
        }

        // Automatic placement: place as soon as the correct plane is found
        if (automaticallyPlaceObject)
        {
            planeManager.planesChanged += OnPlaneDetected;
        }
        else
        {
            // Manual: place upon raycast taps
            raycastHitEvent.eventRaised += PlaceObjectAt;
        }
    }

    private void PlaceObjectAt(object sender, ARRaycastHit hitPose)
    {
        // If we're not placing objects right now, do nothing
        if (!IsCurrentlyPlacingObject)
            return;

        // Add the eventual object reference to the manager (optional)
        XRObjectManager.Instance.AddObject(m_ObjectName, m_SpawnedObject);

        ObjectSpawner objectSpawner = XRManager.Instance.GetXRObject().GetComponentInChildren<ObjectSpawner>();
        if (objectSpawner != null)
        {
            // Attempt the spawn
            objectSpawner.TrySpawnObject(hitPose.pose.position, Vector3.up);

            // Only want to place once? Then unsubscribe
            raycastHitEvent.eventRaised -= PlaceObjectAt;

            Continue();
            return;
        }

        // Fallback if no spawner found
        if (m_SpawnedObject == null)
        {
            m_SpawnedObject = Instantiate(m_PrefabToPlace, hitPose.pose.position,
                                          hitPose.pose.rotation, hitPose.trackable.transform.parent);

            var interactable = m_SpawnedObject.AddComponent<InteractableARObject>();
            interactable.isScaleable = scaleable;
            interactable.isMovable = moveable;
            interactable.isRotatable = rotateable;

            raycastHitEvent.eventRaised -= PlaceObjectAt;
            //disable the objectSpawner
            objectSpawner.gameObject.SetActive(false);
            Continue();
        }
    }

    private void OnPlaneDetected(ARPlanesChangedEventArgs args)
    {
        if (!IsCurrentlyPlacingObject)
            return;

        foreach (var plane in args.added)
        {
            if (plane.alignment == planeAlignment)
            {
                Debug.Log("Placed object on newly added plane");
                objectSpawner.TrySpawnObject(plane.transform.position, Vector3.up);

                planeManager.planesChanged -= OnPlaneDetected;
                //disable the objectSpawner
                objectSpawner.gameObject.SetActive(false);
                Continue();
                return;
            }
        }

        foreach (var plane in args.updated)
        {
            if (plane.alignment == planeAlignment)
            {
                Debug.Log("Placed object on updated plane");
                objectSpawner.TrySpawnObject(plane.transform.position, Vector3.up);

                planeManager.planesChanged -= OnPlaneDetected;
                //disable the objectSpawner
                objectSpawner.gameObject.SetActive(false);
                Continue();
                return;
            }
        }
    }

    public override string GetSummary()
    {
        return "Places an object on a detected plane, either automatically or manually via AR Raycast.";
    }

    public override Color GetButtonColour()
    {
        return new Color32(184, 253, 255, 255);
    }
}

// -- The XRObjectManager class remains as you posted, unchanged below. --
public class XRObjectManager : MonoBehaviour
{
    private static XRObjectManager _instance;
    public static XRObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<XRObjectManager>();
                if (_instance == null)
                {
                    var xrObjectManagerObject = new GameObject("XRObjectManager");
                    _instance = xrObjectManagerObject.AddComponent<XRObjectManager>();
                    //DontDestroyOnLoad(xrObjectManagerObject); // optional
                }
            }
            return _instance;
        }
    }

    private Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>();

    public void AddObject(string name, GameObject obj)
    {
        if (_objects.ContainsKey(name))
        {
            Debug.LogWarning($"Object {name} already exists. Overwriting.");
            _objects[name] = obj;
        }
        else
        {
            _objects.Add(name, obj);
        }
    }

    public void RemoveObject(string name)
    {
        if (_objects.ContainsKey(name))
            _objects.Remove(name);
        else
            Debug.LogWarning($"Object {name} does not exist in XRObjectManager.");
    }

    public GameObject GetObject(string name)
    {
        if (_objects.ContainsKey(name))
            return _objects[name];

        Debug.LogError($"Object {name} not found in XRObjectManager.");
        return null;
    }
}
