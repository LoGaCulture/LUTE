using UnityEngine;

[OrderInfo("XR",
              "Destroy XR Object",
              "Destroys a named spawned XR Object")]
[AddComponentMenu("")]
public class DestroyXRObject : Order
{
    [Tooltip("Name of the XR object to destroy")]
    [SerializeField] private string _objectName;

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

        // Remove the object from XRObjectManager and destroy it
        XRObjectManager.Instance.RemoveObject(_objectName);
        Destroy(xrObject);

        Debug.Log($"Destroyed XR Object '{_objectName}'.");
        Continue();
    }

    public override string GetSummary()
    {
        return $"Destroys XR Object '{_objectName}'.";
    }
}
