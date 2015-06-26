using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AliensSpawner))]
public class AliensSpawnerInspector : Editor
{
    public override void OnInspectorGUI()
    {
 	    base.OnInspectorGUI();
        
        var spawner = (AliensSpawner) target;
        GUI.enabled = Application.isPlaying && !spawner.AreAliensActive;
        if (GUILayout.Button("New wave"))
        {
            spawner.StartSpawning();
        }
    }
}