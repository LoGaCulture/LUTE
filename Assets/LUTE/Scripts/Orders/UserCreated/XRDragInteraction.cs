using UnityEngine;


[OrderInfo("XR",
              "XRDragInteraction",
              "Handles XR drag interaction for a specified object.")]
[AddComponentMenu("")]
public class XRDragInteraction : Order
{
    [SerializeField] private string _objectName;

    private GameObject _gameObjectToDrag;
    private GameObject _transparentObject;

    [SerializeField] private Material _transparentMaterial;

    private Vector3 _scaleOfObject;

    [Tooltip("The offset to spawn the transparent object at")]
    [SerializeField] private Vector3 _dragOffset;

    [Tooltip("Minimum overlap percentage required to consider the puzzle solved")]
    [Range(0, 100)]
    [SerializeField] private float _minimumOverlapPercentage = 75f;

    public override void OnEnter()
    {
        _gameObjectToDrag = XRObjectManager.Instance.GetObject(_objectName);
        if (_gameObjectToDrag == null)
        {
            Debug.LogError($"GameObject with name '{_objectName}' not found in XRObjectManager.");
            Continue();
            return;
        }

        _scaleOfObject = _gameObjectToDrag.transform.localScale;

        StartDrag();
    }

    private void StartDrag()
    {
        // Instantiate a transparent version of the object to drag
        _transparentObject = Instantiate(_gameObjectToDrag, _gameObjectToDrag.transform.position + _dragOffset, _gameObjectToDrag.transform.rotation);
        _transparentObject.name = $"{_gameObjectToDrag.name}_Transparent";

        //get the xrgrabinteractable of the gameobject to drag and set the movable property to true
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = _gameObjectToDrag.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = true;
        }

        // Set tag to identify it if necessary
        _gameObjectToDrag.tag = "DragPiece";
        //and to all children
        foreach (Transform child in _gameObjectToDrag.transform)
        {
            child.gameObject.tag = "DragPiece";
        }


        //set the trackposition to false on the transparent object
        grabInteractable = _transparentObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = false;
        }


        // Add the OverlapDetector script to the transparent object
        var overlapDetector = _transparentObject.AddComponent<OverlapDetector>();
        overlapDetector.minimumOverlapPercentage = _minimumOverlapPercentage;

        // Set the callback function to be the OnPuzzleSolved function
        overlapDetector.PuzzleSolved += OnPuzzleSolved;

        // Set the collider of the transparent object to be a trigger, either in component or in children
        var collider = _transparentObject.GetComponent<MeshCollider>();
        if (collider != null)
        {
            // Set convex to true
            collider.convex = true;
            collider.isTrigger = true;
        }
        else
        {
            collider = _transparentObject.GetComponentInChildren<MeshCollider>();
            if (collider != null)
            {
                // Set convex to true
                collider.convex = true;
                collider.isTrigger = true;
            }
            else
            {
                // Add a box collider and fit it to the object
                BoxCollider boxCollider = _transparentObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                boxCollider.size = _transparentObject.GetComponent<Renderer>().bounds.size;
            }
        }

        // Check if rigidbody already exists
        var rigidbody = _transparentObject.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            // Add rigidbody to the transparent object
            rigidbody = _transparentObject.AddComponent<Rigidbody>();
        }

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        // Set materials to transparent
        foreach (Renderer renderer in _transparentObject.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = _transparentMaterial;
            }
            renderer.materials = materials;
        }
    }

    public void OnPuzzleSolved()
    {
        // Destroy the transparent object
        if (_transparentObject != null)
        {
            Destroy(_transparentObject);
        }

        // Disable the XRGrabInteractable component of the object to drag
        if (_gameObjectToDrag != null)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = _gameObjectToDrag.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = false;
            }
        }

        Continue();
    }

    public override string GetSummary()
    {
        return "Handles XR drag interaction for a specified object.";
    }
}