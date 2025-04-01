using UnityEngine;

[OrderInfo("Map",
              "MapMenu",
              "Creates a custom icon to open and clsoe the map (avoiding the need to put this choice in a sub menu)")]
[AddComponentMenu("")]
public class MapMenu : GenericButton
{
    public override void OnEnter()
    {
        var engine = GetEngine();
        if (engine == null)
        {
            Continue();
            return;
        }
        var mapManager = engine.GetMapManager();
        if (mapManager == null)
        {
            Continue();
            return;
        }

        var popupIcon = SetupButton();

        UnityEngine.Events.UnityAction action = () =>
    {
        mapManager.ToggleMap();
    };

        SetAction(popupIcon, action, "MapButton");

        Continue();
    }

    public override string GetSummary()
    {
        return "Creating a custom icon to open/close the map (if one exists)";
    }
}