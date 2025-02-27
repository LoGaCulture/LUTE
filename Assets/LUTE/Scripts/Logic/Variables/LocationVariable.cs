using LoGaCulture.LUTE;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using System;
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
        if (Value.LocationDisabled)
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
        var mapManager = engine.GetMapManager();
        var tracker = mapManager.TrackerPos();
        var trackerPos = tracker;

        if (LocationProvider.CurrentLocation.LatitudeLongitude == null)
        {
            return false;
        }

        Vector2d vecVal = Value.LatLongString();
        var deviceLoc = engine.DemoMapMode ? trackerPos : LocationProvider.CurrentLocation.LatitudeLongitude;

        var radiusInMeters = (LogaConstants.DefaultRadius * 3f) + Value.RadiusIncrease;

        // Use double for more precision
        double r = 6371000.0; // Earth radius in meters

        // Convert to radians
        double lat1 = deviceLoc.x * Math.PI / 180.0;
        double lon1 = deviceLoc.y * Math.PI / 180.0;
        double lat2 = vecVal.x * Math.PI / 180.0;
        double lon2 = vecVal.y * Math.PI / 180.0;

        // Haversine formula with more precise calculation
        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = r * c;

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
