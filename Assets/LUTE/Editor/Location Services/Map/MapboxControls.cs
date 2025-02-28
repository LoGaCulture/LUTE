using LoGaCulture.LUTE;
using Mapbox.Unity.Map;
using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Represents a class that controls the map functionality which simply visualizes the camera which the map is being previewed on during edit time
/// This class handles user input events such as keyboard, mouse, and scroll wheel interactions
/// It also provides methods for panning and zooming the map
/// </summary>
public class MapboxControls : EventWindow
{
    public static BasicFlowEngine engine;

    protected Vector2 rightClickPos;

    private bool showLocationPopup = false;
    private Rect locationPopupRect;
    private GUIStyle currentStyle;
    private GUIStyle addButtonStyle;
    private RenderTexture mapTexture;
    //pan keyboard
    private float xMove = 0;
    private float zMove = 0;

    protected static MapboxControls window;
    protected static MapCameraMovement mapMovement;
    protected static int forceRepaintCount = 0;
    protected static float spawnScale = 1.0f;
    protected bool camRendered = false;
    protected Event e;

    private static string currentLocationName = "New Location";
    private static string currentLocationString;
    private static Camera mapCamera;
    private static AbstractMap abstractMap;
    private static LUTEMapManager mapManager;

    static MapboxControls()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (window == null)
            return;

        var allMarkers = mapManager.GetSpawnedLocationMarkers();
        if (allMarkers.Count <= 0)
            return;

        foreach (var marker in allMarkers)
        {
            if (marker == null)
                continue;

            marker.ForceUpdateInEditor = true;
        }
    }

    public static void ShowWindow()
    {
        window = (MapboxControls)GetWindow(typeof(MapboxControls), false, "LUTE Map");
    }

    public static void RemoveLocation(LocationVariable location)
    {
        mapManager?.RemoveLocationMarker(location);
    }

    private void OnEnable()
    {
        var engine = GraphWindow.GetEngine();

        if (engine == null)
        {
            return;
        }

        abstractMap = engine.GetAbstractMap();

        if (abstractMap == null)
        {
            return;
        }

        mapMovement = abstractMap.gameObject.GetComponent<MapCameraMovement>();
        mapManager = engine.GetMapManager();

        // Create a camera if none exists - ensure you set a tag and culling mask to only map
        // First ensure that there is a tag to assign to the map otherwise create one
        if (!InternalEditorUtility.tags.Contains("ToolCam"))
        {
            InternalEditorUtility.AddTag("ToolCam");
        }

        mapCamera = mapMovement.ReferenceCamera;
        if (mapCamera == null)
        {
            Debug.LogError("No camera found to render the map! Please ensure this is setup ");
        }

        // We use the editor preview of the abstractmap class to render the map in editor mode
        abstractMap.IsEditorPreviewEnabled = true;
        abstractMap.ResetMap();

        // Finally we process the locations to ensure they are displayed on the map
        mapManager.ProcessLocationsEditor();
        mapManager.SpawnMarkers();
    }

    private void OnDisable()
    {
        if (abstractMap != null && abstractMap.IsEditorPreviewEnabled)
            abstractMap.DisableEditorPreview();

        mapMovement.dragStartedOnUI = false;
        mapManager.ClearAllMarkers();
    }

    private void OnGUI()
    {
        InitStyles();

        // Create the RenderTexture without immediate rendering
        if (mapTexture == null)
        {
            mapTexture = new RenderTexture((int)window.position.width,
                                           (int)window.position.height, 24,
                                           RenderTextureFormat.ARGB32);
        }

        // // Update RenderTexture properties if needed
        if (mapTexture.width != (int)window.position.width || mapTexture.height != (int)window.position.height)
        {
            mapTexture.Release(); // Release the old texture
            mapTexture.width = (int)window.position.width;
            mapTexture.height = (int)window.position.height;
            mapTexture.depth = 24;
            mapTexture.Create(); // Recreate the texture with updated dimensions
        }

        // When ready to render, ensure the camera is set up correctly
        mapCamera.targetTexture = mapTexture; // Set the target texture on the camera
        mapCamera.Render();

        // Draw the camera texture to the window
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height), mapCamera.targetTexture);

        BeginWindows();
        if (showLocationPopup)
        {
            var locationLength = 200 + "Location: ".Length + currentLocationString.Length * 3;
            float windowWidth = locationLength; // Calculate the width based on the length of the current location string

            currentStyle.padding.top = -20;

            locationPopupRect = GUI.Window(0, new Rect(rightClickPos.x, rightClickPos.y, windowWidth, 80),
                DrawLocationWindow, "Adding New Location", currentStyle);
        }
        EndWindows();

        if (e != null)
            base.HandleEvents(e);
        else
        {
            e = Event.current;
        }

        if (xMove > 0 || zMove > 0)
        {
            forceRepaintCount = 1;
        }

        mapMovement.PanMapUsingKeyBoard(xMove, zMove);
        mapMovement.PanMapUsingTouchOrMouseEditor(e);

        wantsMouseEnterLeaveWindow = true;

        if (e.type == EventType.MouseLeaveWindow)
        {
            mapMovement.dragStartedOnUI = false;
        }

        if (mapMovement.dragStartedOnUI)
            forceRepaintCount = 1;

        if (forceRepaintCount > 0)
            Repaint();

#if UNITY_2020_1_OR_NEWER
        //Force exit gui once repainted
        GUIUtility.ExitGUI();
#endif

    }

    private void InitStyles()
    {
        if (currentStyle == null)
        {
            currentStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f)), // Subtle dark transparent bg
                    textColor = Color.black,
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(5, 5, 5, 5), // Reduce padding
                margin = new RectOffset(2, 2, 2, 2) // Reduce margins
            };
        }

        if (addButtonStyle == null)
        {
            addButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14, // Larger text
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.1f, 0.6f, 0.1f, 1f)) }, // Green background
                hover = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.2f, 0.8f, 0.2f, 1f)) }, // Brighter on hover
                active = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.05f, 0.5f, 0.05f, 1f)) }, // Darker on click
                padding = new RectOffset(10, 10, 5, 5), // Extra padding
            };
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void DrawLocationWindow(int id)
    {
        InitStyles();
        // Create a GUI box allowing a custom name and showing current location
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name: ");
        currentLocationName = EditorGUILayout.TextField(currentLocationName);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Location: ");
        GUILayout.Label(currentLocationString);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace(); // Push content down

        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace(); // Push content to the right
            if (GUILayout.Button("+ Add", addButtonStyle, GUILayout.Height(30), GUILayout.Width(100)))
            {
                AddNewLocation();
            }
            GUILayout.FlexibleSpace(); // Push content to the left
        }
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace(); // Push content up
        GUILayout.EndVertical();
    }

    protected override void OnKeyDown(Event e)
    {
        switch (e.keyCode)
        {
            case KeyCode.A:
                xMove = -.75f;
                break;
            case KeyCode.D:
                xMove = .75f;
                break;
            case KeyCode.W:
                zMove = .75f;
                break;
            case KeyCode.S:
                zMove = -.75f;
                break;
        }

        e.Use();
    }

    protected override void OnKeyUp(Event e)
    {
        xMove = 0;
        zMove = 0;
    }
    private void AddNewLocation()
    {
        LUTELocationInfo newLocationInfo = ScriptableObject.CreateInstance<LUTELocationInfo>();


        string name = GetUniqueLocationName(currentLocationName);

        AssetDatabase.CreateAsset(newLocationInfo, "Assets/LUTE/Resources/LocationServices/" + name + ".asset");

        newLocationInfo.SetNewPosition(currentLocationString);
        newLocationInfo.LocationName = name;
        newLocationInfo.InfoID = name + "_ID";

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newLocationInfo;

        LocationVariable locVar = new LocationVariable();
        LocationVariable newVar = VariableSelectPopupWindowContent.AddVariable(locVar.GetType(), currentLocationName, newLocationInfo) as LocationVariable;

        currentLocationName = "New Location";
        currentLocationString = "";

        mapManager.ClearAllMarkers();
        mapManager.ProcessLocationsEditor();
        mapManager.SpawnMarkers();

        showLocationPopup = false;
    }

    // Returns a new node key that is guaranteed not to clash with any existing Node in the Engine.
    public virtual string GetUniqueLocationName(string originalKey, LocationVariable ignoreVar = null)
    {
        int suffix = 0;
        string baseKey = originalKey.Trim();

        // No empty keys allowed
        if (baseKey.Length == 0)
        {
            return "Empty Location Name";
        }

        var locations = engine.Variables.FindAll(x => x is LocationVariable);

        string key = baseKey;
        while (true)
        {
            bool collision = false;
            for (int i = 0; i < locations.Count; i++)
            {
                var loc = locations[i] as LocationVariable;
                if (loc == ignoreVar || loc.Value.LocationName == null)
                {
                    continue;
                }
                if (loc.Value.LocationName.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    collision = true;
                    suffix++;
                    key = baseKey + " " + suffix;
                }
            }

            if (!collision)
            {
                return key;
            }
        }
    }

    protected override void OnMouseDown(Event e)
    {
        switch (e.button)
        {
            case MouseButton.Left:
                {
                    mapMovement.dragStartedOnUI = true;
                    showLocationPopup = false;
                    e.Use();
                    break;
                }
        }
    }

    protected override void OnMouseUp(Event e)
    {
        switch (e.button)
        {
            case MouseButton.Left:
                {
                    Selection.activeObject = engine.GetAbstractMap();
                    mapMovement.dragStartedOnUI = false;
                    e.Use();
                    break;
                }
            case MouseButton.Right:
                {
                    var mousePosScreen = e.mousePosition;
                    //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                    //http://answers.unity3d.com/answers/599100/view.html
                    var cam = mapCamera;

                    var centreY = Screen.height / 2;
                    var y = centreY - (mousePosScreen.y - centreY);
                    var mousePos = new Vector3(mousePosScreen.x, y, cam.transform.localPosition.y);

                    var pos = cam.ScreenToWorldPoint(mousePos);

                    var latlongDelta = abstractMap.WorldToGeoPosition(pos);

                    var newLocationString = string.Format("{0}, {1}", latlongDelta.x, latlongDelta.y);

                    currentLocationString = newLocationString;

                    rightClickPos = e.mousePosition;
                    showLocationPopup = true;

                    e.Use();

                    break;
                }
        }
    }

    protected override void OnScrollWheel(Event e)
    {
        mapMovement.ZoomMapUsingTouchOrMouse(-e.delta.y / 2);
        e.delta = Vector2.zero;
        forceRepaintCount = 1;
    }

    static string ReplaceUnderscoresWithSpace(string input)
    {
        return input.Replace('_', ' ');
    }

    private Texture2D CreateSmoothBackgroundTexture()
    {
        Texture2D texture = new Texture2D(1, 1);
        Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Near-black with slight transparency
        texture.SetPixel(0, 0, backgroundColor);
        texture.Apply();
        return texture;
    }
}