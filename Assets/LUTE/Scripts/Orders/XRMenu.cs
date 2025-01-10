using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[OrderInfo("XR",
              "XRMenu",
              "Toggles the XR Camera on/off using a custom button")]
[AddComponentMenu("")]
public class XRMenu : Order
{
    [Tooltip("Custom icon to display for this menu")]
    [SerializeField] protected Sprite customButtonIcon;
    [Tooltip("A custom popup class to use to display this menu - if one is in the scene it will be used instead")]
    [SerializeField] protected PopupIcon setIconButton;
    [Tooltip("If true, the popup icon will be displayed, otherwise it will be hidden")]
    [SerializeField] protected bool showIcon = true;

    [Header("XR Settings")]
    [SerializeField]
    public PlaneDetectionMode planeDetectionMode = PlaneDetectionMode.Horizontal;
    [SerializeField]
    public GameObject planeVisualiser;
    [SerializeField]
    public GameObject pointCloudVisualiser;

    public override void OnEnter()
    {
        if (setIconButton != null)
        {
            PopupIcon.ActivePopupIcon = setIconButton;
        }

        var popupIcon = PopupIcon.GetPopupIcon();
        if (popupIcon != null)
        {
            if (customButtonIcon != null)
            {
                popupIcon.SetIcon(customButtonIcon);
            }
        }
        if (showIcon)
        {
            popupIcon.SetActive(true);
        }

        UnityEngine.Events.UnityAction action = () =>
        {
            //if the plane visualiser is not null, set it to the plane visualiser of the XR object
            if (planeVisualiser != null)
            {
                var planeManager = XRManager.Instance.GetXRObject().GetComponentInChildren<ARPlaneManager>();
                planeManager.planePrefab = planeVisualiser;
            }

            //if the point cloud visualiser is not null, set it to the point cloud visualiser of the XR object
            if (pointCloudVisualiser != null)
            {
                var pointCloudManager = XRManager.Instance.GetXRObject().GetComponentInChildren<ARPointCloudManager>();
                pointCloudManager.pointCloudPrefab = pointCloudVisualiser;
            }
        };
        popupIcon.SetAction(action);
        popupIcon.MoveToNextOption();

        Continue();
    }

    public override string GetSummary()
    {
        return "Creates a button which will toggle the XR camera on/off";
    }
}