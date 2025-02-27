using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    // Scriptable object for a list of LUTELocationStatusDisplay objects
    [CreateAssetMenu(menuName = "LUTE/Location/Location Status Display List")]
    public class LUTELocationStatusDisplayList : ScriptableObject
    {
        /// <summary>
        /// A simple class that uses the LocationStatus enum to return information details to help render a location marker correctly.
        /// </summary>
        [Serializable]
        public class LUTELocationStatusDisplay
        {
            public LocationStatus status = LocationStatus.Unvisited;
            public LUTELocationDisplayOptions locationDisplayOptions;
            public string CustomStatusLabel;

            public LUTELocationStatusDisplay(LocationStatus status, LUTELocationDisplayOptions locationDisplayOptions)
            {
                this.status = status;
                this.locationDisplayOptions = locationDisplayOptions;
            }
        }

        public List<LUTELocationStatusDisplay> list = new List<LUTELocationStatusDisplay>();
    }
}
