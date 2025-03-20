using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(LUTELocationInfo))]
    public class LUTELocationInfoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var t = target as LUTELocationInfo;

            // A button to reset this location to default
            if (GUILayout.Button("Reset Location"))
            {
                t.LocationStatus = t.DefaultLocationStatus;
                t.LocationDisabled = t.DefaultDisabledStatus;

                string locName = t.LocationName;

                Debug.Log($"Location {locName} has been reset to default.");
            }
        }
    }
}