using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PipeGridDataEditor : EditorWindow
{
	static readonly string s_GridDataFolder = $"{Application.dataPath.TrimEnd('/')}/Resources/PipeGridData";

	bool m_bClearOnDelete = true;

	string m_FileName = "";
	readonly string[] m_FileOptions = new string[2]
	{
		"Use Existing JSON",
		"Create New JSON"
	};
	int m_SelectedFileOptionIndex = 0;
	int m_LastSelectedFileOptionIndex = 0;
	string[] m_Files;
	int m_SelectedFileIndex = 0;
	int m_LastSelectedFileIndex = 0;

	public PipeGridData PipeGridData;
	SerializedObject m_SerializedObject;
	SerializedProperty m_SerializedProperty;

	Vector2 m_ScrollPos;

	[MenuItem("Window/Edit Pipe Grid Data")]
	public static void ShowWindow() => GetWindow<PipeGridDataEditor>("Pipe Grid Data Editor");

	public void CreateGUI()
	{
		m_SerializedObject = new SerializedObject(this);
		m_SerializedProperty = m_SerializedObject.FindProperty("PipeGridData");
	}

	void OnGUI()
	{
		m_bClearOnDelete = EditorGUILayout.Toggle("Clear on Delete", m_bClearOnDelete);

		m_SelectedFileOptionIndex = EditorGUILayout.Popup("Choose File to Use", m_SelectedFileOptionIndex, m_FileOptions);
		if (m_SelectedFileOptionIndex == 0)
		{
			if (GUILayout.Button("Refresh") || m_Files == null || m_LastSelectedFileOptionIndex != m_SelectedFileOptionIndex)
			{
				m_LastSelectedFileOptionIndex = m_SelectedFileOptionIndex;
				GetFiles();
				m_SelectedFileIndex = 0;
				m_LastSelectedFileIndex = -1;
				m_FileName = "";
			}
			m_SelectedFileIndex = EditorGUILayout.Popup("Select an Existing File", m_SelectedFileIndex, m_Files);
			m_FileName = m_Files.Length > 0 ? m_Files[m_SelectedFileIndex] : "";
			if (!string.IsNullOrWhiteSpace(m_FileName) && m_LastSelectedFileIndex != m_SelectedFileIndex)
			{
				m_LastSelectedFileIndex = m_SelectedFileIndex;
				LoadFile();
			}
		}
		else
		{
			if (m_LastSelectedFileOptionIndex != m_SelectedFileOptionIndex)
			{
				m_LastSelectedFileOptionIndex = m_SelectedFileOptionIndex;
				m_FileName = "";
			}
			m_FileName = EditorGUILayout.TextField("New File Name", m_FileName).Trim();
		}

		m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

		m_SerializedObject.Update();
		EditorGUILayout.PropertyField(m_SerializedProperty, true);
		m_SerializedObject.ApplyModifiedProperties();

		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Clear Input")) Clear();

		if (string.IsNullOrEmpty(m_FileName)) return;

		if (m_SelectedFileOptionIndex == 0)
		{
			if (GUILayout.Button($"Modify \"{m_FileName}.json\"")) SaveFile();
			if (GUILayout.Button($"Delete \"{m_FileName}.json\"")) DeleteFile();
		}
		else
		{
			if (GUILayout.Button($"Create \"{m_FileName}.json\"")) SaveFile();
		}
	}

	void GetFiles()
	{
		List<string> filesList = new List<string>();
		if (Directory.Exists(s_GridDataFolder))
		{
			string[] files = Directory.GetFiles(s_GridDataFolder, "*.json", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < files.Length; ++i)
			{
				string fileName = Path.GetFileNameWithoutExtension(files[i]);
				TextAsset json = Resources.Load<TextAsset>($"PipeGridData/{fileName}");
				try
				{
					_ = JsonUtility.FromJson<SerializablePipeGridData>(json.text).Deserialized;
					fileName += ".json";
					filesList.Add(fileName);
				}
				catch { }
			}
		}
		m_Files = filesList.ToArray();
	}

	void LoadFile()
	{
		string fileNameNoExt = Path.GetFileNameWithoutExtension(m_FileName);
		TextAsset json = Resources.Load<TextAsset>($"PipeGridData/{fileNameNoExt}");
		if (!json)
		{
			Debug.LogError($"\"{m_FileName}\" does not exist!");
			return;
		}
		try
		{
			PipeGridData = JsonUtility.FromJson<SerializablePipeGridData>(json.text).Deserialized;
			Debug.Log($"Successfully loaded \"{m_FileName}\"");
		}
		catch (ArgumentException)
		{
			Debug.LogError($"Invalid data in \"{m_FileName}\"");
		}
	}

	string FilePath => $"{s_GridDataFolder}/{m_FileName}";

	void SaveFile()
	{
		string json = JsonUtility.ToJson(PipeGridData.Serialized, prettyPrint: true);
		string message = $"Successfully {(File.Exists(FilePath) ? "modified" : "created")} \"{m_FileName}\"";
		Directory.CreateDirectory(s_GridDataFolder);
		File.WriteAllText(FilePath, json);
		AssetDatabase.Refresh();
		Debug.Log(message);
	}

	void DeleteFile()
	{
		if (!File.Exists(FilePath))
		{
			Debug.LogError($"\"{m_FileName}\" doesn't exist inside Assets/Resources/PipeGridData/");
			return;
		}
		File.Delete(FilePath);
		string message = $"Successfully deleted \"{m_FileName}\"";
		if (File.Exists($"{FilePath}.meta"))
		{
			File.Delete($"{FilePath}.meta");
			message += $" and \"{m_FileName}.meta\"";
		}
		AssetDatabase.Refresh();
		Debug.Log(message);
		if (m_bClearOnDelete) Clear();
	}

	void Clear()
	{
		m_FileName = "";
		m_SelectedFileOptionIndex = 0;
		m_LastSelectedFileOptionIndex = -1;
		m_SelectedFileIndex = 0;
		m_LastSelectedFileIndex = -1;
		m_Files = null;
		PipeGridData = new PipeGridData();
	}
}
