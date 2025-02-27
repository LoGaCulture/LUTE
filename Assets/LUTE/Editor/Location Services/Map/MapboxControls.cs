using LoGaCulture.LUTE;
using Mapbox.Examples;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
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
    private RenderTexture mapTexture;

    //pan keyboard
    private float xMove = 0;
    //Returns true if any key was pressed.
    private float zMove = 0;

    protected static MapboxControls window;
    protected static QuadTreeCameraMovement map;
    protected static int forceRepaintCount = 0;
    protected static float spawnScale = 10;
    protected bool camRendered = false;
    protected Event e;

    private static string currentLocationName = "New Location";
    private static Sprite currentLocationSprite = null;
    private static string currentLocationString;
    private static Color locationColor = Color.white;
    private static bool currentLocationNameBool = true;

    private static Camera mapCam;
    private static CameraBillboard cameraBillboard;
    private static AbstractMap abstractMap;
    private static LUTEMapManager spawnOnMap;

    static MapboxControls()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        // Update the spawn on map trackers
        //if (spawnOnMap != null)
        //    spawnOnMap.UpdateMarkers();
    }
    public static void ShowWindow()
    {
        window = (MapboxControls)GetWindow(typeof(MapboxControls), false, "Map");
    }

    public static void RemoveLocation(LocationVariable location)
    {
        spawnOnMap?.RemoveLocationMarker(location);
    }

    private void OnEnable()
    {
        if (LocationProviderFactory.Instance != null)
        {
            abstractMap = LocationProviderFactory.Instance.mapManager;
        }
        if (abstractMap == null)
        {
            abstractMap = FindObjectOfType<AbstractMap>();
        }
        if (abstractMap == null)
        {
            return;
        }
        map = abstractMap.gameObject.GetComponent<QuadTreeCameraMovement>();
        spawnOnMap = abstractMap.gameObject.GetComponent<LUTEMapManager>(); // ensure that quadtreemovement requires spawn on map
        cameraBillboard = spawnOnMap.transform?.GetComponent<CameraBillboard>(); //ensure that tracker is set elsewhere - this is NOW WRONG

        //create a camera if none exists - ensure you set a tag and culling mask to only map
        //first ensure that there is a tag called map otherwise create one
        if (!InternalEditorUtility.tags.Contains("ToolCam"))
        {
            InternalEditorUtility.AddTag("ToolCam");
        }

        mapCam = GameObject.FindGameObjectWithTag("ToolCam")?.GetComponent<Camera>();
        if (mapCam == null)
        {
            mapCam = new GameObject("MapCameraTool").AddComponent<Camera>();
            mapCam.tag = "ToolCam";
            mapCam.cullingMask = 1 << LayerMask.NameToLayer("ToolCam");
            mapCam.fieldOfView = 26.99147f;
            mapCam.transform.position = new Vector3(mapCam.transform.position.x, 200, mapCam.transform.position.z);
            mapCam.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        //get the abstract map component and enable editor preview if it is not already enabled
        if (abstractMap != null)
        {
            abstractMap.IsEditorPreviewEnabled = true;
            abstractMap.ResetMap();
        }
        if (cameraBillboard != null && mapCam != null && cameraBillboard.GetCurrentCam() != mapCam)
            cameraBillboard.SetCanvasCam(mapCam);

        map._referenceCamera = mapCam; // This should probably be done on the movement script as we already have ref to tool cam

        spawnOnMap.ProcessLocations();
        //spawnOnMap.CreateMarkers();
    }

    private void OnDisable()
    {
        if (map != null)
        {
            if (abstractMap != null && abstractMap.IsEditorPreviewEnabled)
                abstractMap.DisableEditorPreview();
        }

        if (spawnOnMap == null || map == null)
        {
            return;
        }

        map._dragStartedOnUI = false;
        //spawnOnMap.ClearLocations();
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

        // When you're ready to render, ensure the camera is set up correctly
        mapCam.targetTexture = mapTexture; // Set the target texture on the camera
        mapCam.Render();

        // Draw the cam texture to the window
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height), mapCam.targetTexture);

        BeginWindows();
        if (showLocationPopup)
        {
            var locationLength = 200 + "Location: ".Length + currentLocationString.Length * 3;
            float windowWidth = locationLength; // Calculate the width based on the length of the current location string

            currentStyle.padding.top = -20;

            locationPopupRect = GUI.Window(0, new Rect(rightClickPos.x, rightClickPos.y, windowWidth, 150),
                DrawLocationWindow, "New Location", currentStyle);
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

        map.PanMapUsingKeyBoard(xMove, zMove);
        map.PanMapUsingTouchOrMouseEditor(e);

        wantsMouseEnterLeaveWindow = true;

        if (e.type == EventType.MouseLeaveWindow)
        {
            map._dragStartedOnUI = false;
        }

        if (map._dragStartedOnUI)
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
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/projectbrowsericonareabg.png") as Texture2D,
                    textColor = Color.white,
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
            };
        }
    }

    private void DrawLocationWindow(int id)
    {

        // Create a GUI box allowing a custom name and showing current location
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name: ");
        currentLocationName = EditorGUILayout.TextField(currentLocationName);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Show Name: ");
        currentLocationNameBool = EditorGUILayout.Toggle(currentLocationNameBool);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Icon: ");
        currentLocationSprite = EditorGUILayout.ObjectField(currentLocationSprite, typeof(Sprite), true) as Sprite;
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Location: ");
        GUILayout.Label(currentLocationString);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Add", EditorStyles.toolbarButton))
        {
            AddNewLocation();
        }
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
        int count = engine.GetComponents<LocationVariable>().Length;
        string name = count > 0 ? currentLocationName + count : currentLocationName;
        AssetDatabase.CreateAsset(newLocationInfo, "Assets/Resources/" + name + ".asset");
        var locString = Conversions.StringToLatLon(currentLocationString);
        //newLocationInfo.Position = currentLocationString;
        //newLocationInfo.Name = currentLocationName;
        if (currentLocationSprite == null)
        {
            var texture = LogaEditorResources.Default100;
            currentLocationSprite = Sprite.Create(
               texture,
               new Rect(0, 0, texture.width, texture.height),
               new Vector2(0.5f, 0.5f)
           );
        }
        //newLocationInfo.Sprite = currentLocationSprite;
        //newLocationInfo.Color = locationColor;
        //newLocationInfo.ShowName = currentLocationNameBool;

        AssetDatabase.SaveAssets();

        //EditorUtility.FocusProjectWindow();
        //Selection.activeObject = newLocationInfo;

        LocationVariable locVar = new LocationVariable();
        LocationVariable newVar = VariableSelectPopupWindowContent.AddVariable(locVar.GetType(), currentLocationName, newLocationInfo) as LocationVariable;
        currentLocationName = "New Location";
        locationColor = Color.white;
        currentLocationNameBool = true;
        currentLocationSprite = null;
        showLocationPopup = false;

        spawnOnMap.ProcessLocations();
        //spawnOnMap.CreateMarkers();
    }

    protected override void OnMouseDown(Event e)
    {
        switch (e.button)
        {
            case MouseButton.Left:
                {
                    map._dragStartedOnUI = true;
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
                    Selection.activeObject = engine.GetMap();
                    map._dragStartedOnUI = false;
                    e.Use();
                    break;
                }
            case MouseButton.Right:
                {
                    var mousePosScreen = e.mousePosition;
                    //assign distance of camera to ground plane to z, otherwise ScreenToWorldPoint() will always return the position of the camera
                    //http://answers.unity3d.com/answers/599100/view.html
                    var cam = mapCam;

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
        map.ZoomMapUsingTouchOrMouse(-e.delta.y / 2);
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