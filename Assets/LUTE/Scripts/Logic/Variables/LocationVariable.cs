using LoGaCulture.LUTE;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using UnityEngine;

[VariableInfo("", "Location")]
[AddComponentMenu("")]
[System.Serializable]
public class LocationVariable : BaseVariable<LUTELocationInfo>
{
    ILocationProvider _locationProvider;
    ILocationProvider LocationProvider
    {
        get
        {
            if (_locationProvider == null)
            {
                _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
            }

            return _locationProvider;
        }
    }

    public override bool Evaluate(ComparisonOperator comparisonOperator, LUTELocationInfo value)
    {
        // If location is disabled then we are likely in a scenario where the location is not available thus we should return true
        // Any other logic should be handled by the class that has called this method
        if (Value.locationDisabled)
        {
            LocationServiceSignals.DoLocationComplete(this);
            return true;
        }
        bool condition = false;
        Vector2 location = Vector2.zero;
        switch (comparisonOperator)
        {
            case ComparisonOperator.Equals:
                condition = IsWithinRadius();
                break;
            case ComparisonOperator.NotEquals:
                condition = !IsWithinRadius();
                break;
            default:
                condition = base.Evaluate(comparisonOperator, value);
                break;
        }

        if (condition)
        {
            LocationServiceSignals.DoLocationComplete(this);
        }

        return condition;
    }

    private bool IsWithinRadius()
    {
        var engine = GetEngine();
        var map = engine.GetMap();
        var tracker = map.TrackerPos();
        var trackerPos = tracker;

        Vector2d vecVal = Value.LatLongString();
        var deviceLoc = LocationProvider.CurrentLocation.LatitudeLongitude;
        if (engine.DemoMapMode)
        {
            deviceLoc = trackerPos;
        }
        if (deviceLoc == null)
        {
            return false;
        }

        var radiusInMeters = LogaConstants.DefaultRadius + Value.RadiusIncrease;
        float r = 6371000.0f; // Earth radius in meters

        // Determine distance between target and tracker in radians
        float dLat = (float)(Mathf.Deg2Rad * (vecVal.x - deviceLoc.x));
        float dLon = (float)(Mathf.Deg2Rad * (vecVal.y - deviceLoc.y));

        // Haversine formula
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
            Mathf.Cos((float)(Mathf.Deg2Rad * deviceLoc.x)) * Mathf.Cos((float)(Mathf.Deg2Rad * vecVal.x)) *
            Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        float distance = r * c;

        return distance <= radiusInMeters;
    }

    public override bool SupportsArithmetic(SetOperator setOperator)
    {
        return true;
    }

    public override void Apply(SetOperator setOperator, LUTELocationInfo value)
    {
        switch (setOperator)
        {
            // case SetOperator.Negate:
            //     Value = Value * -1;
            //     break;
            // case SetOperator.Add:
            //     Value += value;
            //     break;
            // case SetOperator.Subtract:
            //     Value -= value;
            //     break;
            //             case SetOperator.Multiply:
            // #if UNITY_2019_2_OR_NEWER
            //                 Value *= value;
            // #else
            //                 var tmpM = Value;
            //                 tmpM.Scale(value);
            //                 Value = tmpM;
            // #endif
            //                 break;
            //             case SetOperator.Divide:
            // #if UNITY_2019_2_OR_NEWER
            //                 Value /= value;
            // #else
            //                 var tmpD = Value;
            //                 tmpD.Scale(new Vector2(1.0f / value.x, 1.0f / value.y));
            //                 Value = tmpD;
            // #endif
            //                 break;
            default:
                base.Apply(setOperator, value);
                break;
        }
    }
}

/// Container for a Vector2 variable reference or constant value.
[System.Serializable]
public struct LocationData
{
    [SerializeField]
    [VariableProperty("<Value>", typeof(LocationVariable))]
    public LocationVariable locationRef;

    [SerializeField]
    public LUTELocationInfo locationVal;

    public LocationData(LUTELocationInfo v)
    {
        locationVal = v;
        locationRef = null;
    }

    public static implicit operator LUTELocationInfo(LocationData locationInfo)
    {
        return locationInfo.Value;
    }

    public LUTELocationInfo Value
    {
        get { return (locationRef == null) ? locationVal : locationRef.Value; }
        set { if (locationRef == null) { locationVal = value; } else { locationRef.Value = value; } }
    }

    public string GetDescription()
    {
        return "";
        if (locationRef == null)
        {
            return locationVal.ToString();
        }
        else
        {
            return locationRef.Key;
        }
    }
}
