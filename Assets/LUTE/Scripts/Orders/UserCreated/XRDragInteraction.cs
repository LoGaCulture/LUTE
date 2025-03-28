using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


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


    public UnityEvent onPuzzleSolvedEvent;



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

        //get the Transparent Material if it is not set from the resources
        if (_transparentMaterial == null)
        {
            _transparentMaterial = Resources.Load<Material>("Transparent Material");
        }


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
                // Fit the BoxCollider to the object's bounds
                var renderer = _transparentObject.GetComponent<Renderer>();
                if (renderer == null)
                {
                    renderer = _transparentObject.GetComponentInChildren<Renderer>();
                }

                if (renderer != null)
                {
                    boxCollider.isTrigger = true;
                    boxCollider.size = renderer.bounds.size;
                    boxCollider.center = renderer.bounds.center - _transparentObject.transform.position;
                }
                else
                {
                    Debug.LogWarning("No renderer found on the transparent object or its children to calculate bounds for BoxCollider.");
                }
            }
        }

        //set the collider of the gameobject to drag to be convex if it is a mesh collider
        collider = _gameObjectToDrag.GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.convex = true;
        }
        else
        {
            collider = _gameObjectToDrag.GetComponentInChildren<MeshCollider>();
            if (collider != null)
            {
                collider.convex = true;
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


        // Check if the transparent object is not null
        if (_transparentObject != null)
        {
            // Get all renderers, including the one on the parent (if it exists) and its children
            Renderer[] renderers = _transparentObject.GetComponents<Renderer>();
            Renderer[] childRenderers = _transparentObject.GetComponentsInChildren<Renderer>();

            // Combine parent and child renderers into a single list
            List<Renderer> allRenderers = new List<Renderer>(renderers);
            allRenderers.AddRange(childRenderers);

            foreach (Renderer renderer in allRenderers)
            {
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] != null)
                        {
                            materials[i] = _transparentMaterial;
                        }
                    }
                    renderer.materials = materials;
                }
            }
        }
        else
        {
            Debug.LogWarning("Transparent object is null. Cannot set materials to transparent.");
        }



    }

    public void OnPuzzleSolved()
    {

        // Call the event

        if (onPuzzleSolvedEvent != null)
            onPuzzleSolvedEvent.Invoke();

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
    public override Color GetButtonColour()
    {
        return new Color32(184, 253, 255, 255);
    }
}