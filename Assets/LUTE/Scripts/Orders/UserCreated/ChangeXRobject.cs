using UnityEngine;

[OrderInfo("XR",
              "Change XR Object",
              "Changes XR object interactable properties")]
[AddComponentMenu("")]
public class ChangeXRObject : Order
{
    [Tooltip("Name of the XR object to modify")]
    [SerializeField] private string _objectName;

    [Tooltip("Prefab to replace the XR object (optional)")]
    [SerializeField] private GameObject _replacementPrefab;

    [Tooltip("Allow the object to be movable")]
    [SerializeField] private bool moveable;

    [Tooltip("Allow the object to be rotatable")]
    [SerializeField] private bool rotateable;

    [Tooltip("Allow the object to be scalable")]
    [SerializeField] private bool scaleable;

    public override void OnEnter()
    {
        // Get the XR object by name
        GameObject xrObject = XRObjectManager.Instance.GetObject(_objectName);

        if (xrObject == null)
        {
            Debug.LogError($"XR Object with name '{_objectName}' not found.");
            Continue();
            return;
        }

        // Modify the XRGrabInteractable properties
        var grabInteractable = xrObject.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = moveable;
            grabInteractable.trackRotation = rotateable;
            grabInteractable.trackScale = scaleable;
        }
        else
        {
            Debug.LogWarning("XRGrabInteractable component not found on XR object.");
        }

        // Replace the object if a replacement prefab is provided
        if (_replacementPrefab != null)
        {
            // Instantiate the replacement prefab at the current object's position and rotation
            GameObject replacement = Instantiate(_replacementPrefab, xrObject.transform.position, xrObject.transform.rotation);

            // Add the replacement to the XRObjectManager
            XRObjectManager.Instance.AddObject(_objectName, replacement);

            // Destroy the original object
            Destroy(xrObject);
        }

        Continue();
    }

    public override string GetSummary()
    {
        return $"Changes properties of XR Object '{_objectName}' and optionally replaces it.";
    }
}
