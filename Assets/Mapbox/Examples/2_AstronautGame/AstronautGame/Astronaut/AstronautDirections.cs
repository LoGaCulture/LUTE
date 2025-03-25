using Mapbox.Directions;
using Mapbox.Unity;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapbox.Examples
{
    public class AstronautDirections : MonoBehaviour
    {
        AbstractMap _map;
        Directions.Directions _directions;
        Action<List<Vector3>> callback;

        void Awake()
        {
            _directions = MapboxAccess.Instance.Directions;
        }

        public void Query(Action<List<Vector3>> vecs, Transform start, Transform end, AbstractMap map)
        {
            if (callback == null)
                callback = vecs;

            _map = map;

            var wp = new Vector2d[2];
            wp[0] = map.WorldToGeoPosition(start.position);
            wp[1] = map.WorldToGeoPosition(end.position);
            var _directionResource = new DirectionResource(wp, RoutingProfile.Walking);
            _directionResource.Steps = true;
            _directions.Query(_directionResource, HandleDirectionsResponse);
        }

        void HandleDirectionsResponse(DirectionsResponse response)
        {
            if (null == response.Routes || response.Routes.Count < 1)
            {
                return;
            }

            var dat = new List<Vector3>();
            foreach (var point in response.Routes[0].Geometry)
            {
                var gp = (_map.GeoToWorldPosition(point, true));
                dat.Add(gp);
            }

            callback(dat);
        }
    }
}