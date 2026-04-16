using System;
using System.IO;
using UnityEngine;

public class PipeGridDataGenerator : MonoBehaviour
{
	[SerializeField]
	bool m_ClearOnDelete = true;
	[field: SerializeField]
	public string FileName { get; set; }
	[SerializeField]
	PipeGridData m_PipeGridData;

	static readonly string m_ResourcesFolder = $"{Application.dataPath}/Resources";
	static readonly string m_GridDataFolder = "PipeGridData";

	string m_LoadFilePath;
	string m_SaveDeleteFilePath;

#if UNITY_EDITOR
	void OnValidate()
	{
		m_LoadFilePath = $"{m_GridDataFolder}/{FileName}";
		m_SaveDeleteFilePath = $"{m_ResourcesFolder}/{m_GridDataFolder}/{FileName}.json";
	}
#endif

	public void ClearData()
	{
		FileName = "";
		m_PipeGridData = new PipeGridData();
	}

	public void LoadFile()
	{
		TextAsset json = Resources.Load<TextAsset>(m_LoadFilePath);
		if (json == null)
		{
			Debug.LogError($"\"{FileName}.json\" doesn't exist inside Assets/Resources/{m_GridDataFolder}/");
			return;
		}
		try
		{
			m_PipeGridData = JsonUtility.FromJson<PipeGridData>(json.text);
			Debug.Log($"Successfully loaded data from \"{FileName}.json\"");
		}
		catch (ArgumentException)
		{
			Debug.LogError($"Invalid data in {FileName}.json");
		}
	}

	public void SaveFile()
	{
		string json = JsonUtility.ToJson(m_PipeGridData, prettyPrint: true);
		string message = $"Successfully {(File.Exists(m_SaveDeleteFilePath) ? "modified" : "created")} \"{FileName}.json\"";
		File.WriteAllText(m_SaveDeleteFilePath, json);
		Debug.Log(message);
	}

	public void DeleteFile()
	{
		if (!File.Exists(m_SaveDeleteFilePath))
		{
			Debug.LogError($"\"{FileName}.json\" doesn't exist inside Assets/Resources/{m_GridDataFolder}/");
			return;
		}
		File.Delete(m_SaveDeleteFilePath);
		string message = $"Successfully deleted {FileName}.json";
		if (File.Exists($"{m_SaveDeleteFilePath}.meta"))
		{
			File.Delete($"{m_SaveDeleteFilePath}.meta");
			message += $" and {FileName}.json.meta";
		}
		if (m_ClearOnDelete) ClearData();
		Debug.Log(message);
	}
}
