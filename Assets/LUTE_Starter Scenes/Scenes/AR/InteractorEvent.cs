using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class InteractorEvent : MonoBehaviour
{

    ARRaycastManager raycastManager;

    // Start is called before the first frame update
    void Start()
    {
       
        raycastManager = XRManager.Instance.GetXRObject().GetComponent<ARRaycastManager>();

        if(onInteracted == null)
        {
            onInteracted = new UnityEvent();
        }
        
    }


    public UnityEvent onInteracted;

    private void Update()
    {
        //// Check for touch input
        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        // Perform a raycast from the touch position
        //        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        //        RaycastHit hit;
        //        if (Physics.Raycast(ray, out hit))
        //        {
        //            // Check if this object was hit
        //            if (hit.transform == transform)
        //            {
        //                // Invoke the event
        //                onInteracted.Invoke();
        //            }
        //        }
        //    }
        //}
    }
}

