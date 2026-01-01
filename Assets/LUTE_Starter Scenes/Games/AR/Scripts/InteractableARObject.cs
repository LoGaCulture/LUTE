using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableARObject : MonoBehaviour
{

    class ScaleUpButton : MonoBehaviour
    {
        void OnMouseDown()
        {
            currentlySelectedInteractable.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    class ScaleDownButton : MonoBehaviour
    {
        void OnMouseDown()
        {
            currentlySelectedInteractable.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    public static GameObject currentlySelectedInteractable;

    //is scaleable, movable and rotatable
    public bool isScaleable = false;
    public bool isMovable = false;
    public bool isRotatable = false;

    // Start is called before the first frame update
    void Start()
    {
        currentlySelectedInteractable = this.gameObject;

        //if scalable, movable or rotatable, add a collider to the object
        if(isScaleable || isMovable || isRotatable)
        {
            if(GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }
        }

        //if the object is scaleable, add two buttons on the screen for scaling

        //if(isScaleable)
        //{
        //    GameObject scaleUpButton = new GameObject("ScaleUpButton");
        //    scaleUpButton.transform.parent = this.transform;
        //    scaleUpButton.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
        //    scaleUpButton.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //    scaleUpButton.AddComponent<BoxCollider>();
        //    scaleUpButton.AddComponent<ScaleUpButton>();

        //    GameObject scaleDownButton = new GameObject("ScaleDownButton");
        //    scaleDownButton.transform.parent = this.transform;
        //    scaleDownButton.transform.localPosition = new Vector3(-0.5f, 0.5f, 0);
        //    scaleDownButton.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //    scaleDownButton.AddComponent<BoxCollider>();
        //    scaleDownButton.AddComponent<ScaleDownButton>();
        //}
        
    }

    // Update is called once per frame
    void Update()
    {

       //check if the user clicked on the object
       if(Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit))
                {
                    if(hit.collider.gameObject == this.gameObject)
                    {
                        currentlySelectedInteractable = this.gameObject;
                    }
                }
            }
        }

       if(currentlySelectedInteractable != this.gameObject)
        {
            return;
        }

        
        //if the object is scaleable and the user is touching the screen with two fingers
        if(isScaleable && Input.touchCount == 2)
        {
            //get the two touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            //get the previous touch positions
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            //get the previous distance between the touches
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;

            //get the current distance between the touches
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            //get the difference between the current and previous distances
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            //get the current scale of the object
            Vector3 scale = transform.localScale;

            //scale the object
            scale -= new Vector3(deltaMagnitudeDiff, deltaMagnitudeDiff, deltaMagnitudeDiff);

            //set the scale of the object
            transform.localScale = scale;
        }

        //if the object is movable and the user is touching the screen with one finger
        if(isMovable && Input.touchCount == 1)
        {
            //get the touch
            Touch touch = Input.GetTouch(0);

            //get the touch position
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 10));

            //set the position of the object
            transform.position = touchPosition;
        }

        //if the object is rotatable and the user is touching the screen with two fingers
        if(isRotatable && Input.touchCount == 2)
        {
            //get the two touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            //get the previous touch positions
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            //get the previous angle between the touches
            float prevTouchDeltaAngle = Mathf.Atan2((touchZeroPrevPos.y - touchOnePrevPos.y), (touchZeroPrevPos.x - touchOnePrevPos.x));

            //get the current angle between the touches
            float touchDeltaAngle = Mathf.Atan2((touchZero.position.y - touchOne.position.y), (touchZero.position.x - touchOne.position.x));

            //get the difference between the current and previous angles
            float deltaAngle = prevTouchDeltaAngle - touchDeltaAngle;

            //get the current rotation of the object
            Vector3 rotation = transform.localEulerAngles;

            //rotate the object
            rotation.z += deltaAngle * Mathf.Rad2Deg;

            //set the rotation of the object
            transform.localEulerAngles = rotation;
        }

    }
}
