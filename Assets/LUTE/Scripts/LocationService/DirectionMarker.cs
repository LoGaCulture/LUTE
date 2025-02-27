//using Mapbox.Unity.Location;
//using Mapbox.Unity.MeshGeneration.Factories;
//using UnityEngine;

////Handler which uses Directions Factory to draw directions between a supplied transform and the player position
//public class DirectionMarker : MonoBehaviour
//{
//    [SerializeField] protected DirectionsFactory directionsFactory;
//    [SerializeField] protected BasicFlowEngine engine;


//    private void Start()
//    {

//        if (directionsFactory == null)
//            directionsFactory = FindObjectOfType<DirectionsFactory>();
//        if(directionsFactory == null ) 
//        {
//            Debug.LogError("No Direction Factory supplied");
//            return;
//        }

//        if(engine == null)
//            engine = FindObjectOfType<BasicFlowEngine>();
//        if(engine == null )
//        {
//            Debug.LogError("No engine found for directions");
//            return;
//        }
//    }

//    ILocationProvider _locationProvider;
//    ILocationProvider LocationProvider
//    {
//        get
//        {
//            if (_locationProvider == null)
//            {
//                _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
//            }

//            return _locationProvider;
//        }
//    }

//    private void Update()
//    {
//        //Ensure that the first location provided is either the tracker or device location
//        if( directionsFactory == null ) { return; }

//        //Set the first location to draw locations between either player/device location
//        var location2D = engine.DemoMapMode ? engine.GetMap().TrackerPos() : LocationProvider.CurrentLocation.LatitudeLongitude;
//        var location = new Vector3((float)location2D.x, 0, (float)location2D.y);
//        directionsFactory.SetInitialPosition(location);
//    }

//    public void SetLocation(Transform location)
//    {
//        if (directionsFactory == null) { return; }

//        //Set the second location (the target) to the location supplied by the player choice
//        directionsFactory.SetTargetPosition(location);
//    }
//}