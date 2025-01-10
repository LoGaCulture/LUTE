using UnityEngine;

[OrderInfo("XR",
              "TogglePlane",
              "Toggle the plane on and off (visible and not)")]
[AddComponentMenu("")]
public class TogglePlane : Order
{

    [SerializeField]
    public bool toggle = true;

    public override void OnEnter()
    {

        XRManager.Instance.TogglePlaneDetection(toggle);


        Debug.Log("TogglePlane");

        Continue();

      //this code gets executed as the order is called
      //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   
      //Continue();
    }

  public override string GetSummary()
  {
 //you can use this to return a summary of the order which is displayed in the inspector of the order
      return "";
  }
}