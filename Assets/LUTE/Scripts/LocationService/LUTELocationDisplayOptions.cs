using System;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    /// <summary>
    /// A class that holds options for displaying a location marker.
    /// Ideally there should a default set of options that you use and you may override this using the locationeventhandler class.
    /// Essentially ensuring that the location marker is updated when it is used in the game.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "LUTE/Location/Location Display Options")]
    public class LUTELocationDisplayOptions : ScriptableObject
    {
        [Header("Location Display")]
        [Tooltip("The colour of the name label")]
        public Color NameLabelColor = Color.white;
        [Tooltip("The Sprite for the location marker")]
        public Sprite MarkerSprite;
        [Tooltip("The colour of the marker radius")]
        public Color RadiusColour = Color.white;
        [Space]
        [Tooltip("Whether the name should be shown or not on the location marker")]
        public bool ShowName;
        [Tooltip("Whether to show the sprite or not.")]
        public bool ShowSprite = true;
        [Tooltip("Whether the radius of the location should be shown or not")]
        public bool ShowRadius = true;

        private bool defaultShowName;
        private bool defaultShowSprite;
        private bool defaultShowRadius;

        public bool DefaultShowName => defaultShowName;
        public bool DefaultShowSprite => defaultShowSprite;
        public bool DefaultShowRadius => defaultShowRadius;

        private void OnEnable()
        {
            defaultShowName = ShowName;
            defaultShowSprite = ShowSprite;
            defaultShowRadius = ShowRadius;
        }

        private void OnDisable()
        {
            ShowName = defaultShowName;
            ShowSprite = defaultShowSprite;
            ShowRadius = defaultShowRadius;
        }
    }
}
