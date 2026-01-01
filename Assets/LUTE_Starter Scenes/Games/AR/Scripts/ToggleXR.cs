using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[OrderInfo("XR",
              "Toggle XR",
              "Toggles the XR object on or off depending on its current state")]
[AddComponentMenu("")]
public class ToggleXR : Order
{
    [SerializeField]
    private bool _toggle = true;

    [SerializeField]
    private PlaneDetectionMode _planeDetectionMode;

    [SerializeField]
    private GameObject _planeVisualizer;

    [SerializeField]
    private GameObject _pointCloudVisualizer;

    public override void OnEnter()
    {
        // Get the XRManager instance
        var xrManager = XRManager.Instance;

        //enable the object
        // Toggle XR object
        xrManager.SetXRActive(_toggle);

        if (!_toggle)
        {
            Continue();
            return;
        }

        // Set the plane visualizer if provided
        if (_planeVisualizer != null)
        {
            var planeManager = xrManager.GetXRObject()?.GetComponentInChildren<ARPlaneManager>();
            if (planeManager != null)
            {
                planeManager.planePrefab = _planeVisualizer;
                planeManager.requestedDetectionMode = _planeDetectionMode;
            }
            else
            {
                Debug.LogWarning("ARPlaneManager not found in XR object.");
            }
        }

        // Set the point cloud visualizer if provided
        if (_pointCloudVisualizer != null)
        {
            var pointCloudManager = xrManager.GetXRObject()?.GetComponentInChildren<ARPointCloudManager>();
            if (pointCloudManager != null)
            {
                pointCloudManager.pointCloudPrefab = _pointCloudVisualizer;
            }
            else
            {
                Debug.LogWarning("ARPointCloudManager not found in XR object.");
            }
        }



        // Continue to the next order
        Continue();
    }


    public override string GetSummary()
    {
        return "Toggles the XR camera on or off depending on the chosen setting";
    }

    public override Color GetButtonColour()
    {
        return new Color32(184, 253, 255, 255);
    }
}
