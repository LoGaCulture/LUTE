using UnityEngine;

[OrderInfo("Map",
             "Toggle Map",
             "Toggles the map on or off")]
[AddComponentMenu("")]
public class ToggleMap : Order
{
    [Tooltip("If map should be resized on enable")]
    [SerializeField] protected bool setSize;
    [Tooltip("Size of resized map")]
    [SerializeField] protected Vector2 _size;

    public override void OnEnter()
    {
        var engine = GetEngine();
        var mapManager = engine.GetMapManager();
        if (mapManager != null)
            mapManager.ToggleMap();

        Continue();
    }

    public override string GetSummary()
    {
        return setSize ? "Resizes on enable to: " + _size : "does not resize on enable";
    }

    public override Color GetButtonColour()
    {
        return new Color32(216, 228, 170, 255);
    }
}