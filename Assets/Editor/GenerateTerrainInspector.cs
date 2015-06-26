using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateTerrain))]
public class GenerateTerrainInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Reset"))
        {
            var generator = (GenerateTerrain) target;
            generator.Reset();
        }
    }
}
