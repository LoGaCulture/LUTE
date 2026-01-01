using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class InteractorEvent : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

        if(onInteracted == null)
        {
            onInteracted = new UnityEvent();
        }
        
    }


    public UnityEvent onInteracted;

}

