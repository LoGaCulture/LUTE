using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveMenu), true)]
public class SaveMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button(new GUIContent("Delete Save Data", "Deletes save data associated with this save key from playerprefs.")))
        {
            var saveMenu = target as SaveMenu;

            if (saveMenu != null)
            {
                SaveManager.DeleteSave(saveMenu.SaveKey);
                saveMenu.ResetSave();
                GraphWindow.ShowNotification("Save Data Deleted");
            }
        }
        base.OnInspectorGUI();
    }
}