using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PipeGridDataGenerator))]
public class PipeGridDataGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		PipeGridDataGenerator generator = (PipeGridDataGenerator)target;
		generator.FileName = generator.FileName.Trim();
		if (GUILayout.Button("Clear Inputted Data")) generator.ClearData();
		if (generator.FileName.Length > 0)
		{
			if (GUILayout.Button($"Load \"{generator.FileName}.json\"")) generator.LoadFile();
			if (GUILayout.Button($"Create/Modify \"{generator.FileName}.json\"")) generator.SaveFile();
			if (GUILayout.Button($"Delete \"{generator.FileName}.json\"")) generator.DeleteFile();
		}
		EditorUtility.SetDirty(target);
	}
}
