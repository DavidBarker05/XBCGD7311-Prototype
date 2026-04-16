using System;
using System.IO;
using UnityEngine;

public class PipeGridDataGenerator : MonoBehaviour
{
	[field: SerializeField]
	public string FileName { get; set; }
	[SerializeField]
	PipeGridData m_PipeGridData;

	static readonly string m_ResourcesFolder = $"{Application.dataPath}/Resources";
	static readonly string m_GridDataFolder = "PipeGridData";

	string m_LoadFilePath;
	string m_SaveFilePath;

#if UNITY_EDITOR
	void OnValidate()
	{
		m_LoadFilePath = $"{m_GridDataFolder}/{FileName}";
		m_SaveFilePath = $"{m_ResourcesFolder}/{m_GridDataFolder}/{FileName}.json";
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
			PipeGridData m_PipeGridData = JsonUtility.FromJson<PipeGridData>(m_LoadFilePath);
			Debug.Log($"Successfully loaded data from {FileName}.json");
		}
		catch (ArgumentException ex)
		{
			Debug.LogError(ex.Message);
		}
	}

	public void SaveFile()
	{
		string json = JsonUtility.ToJson(m_PipeGridData, prettyPrint: true);
		string message = $"Successfully {(File.Exists(json) ? "modified" : "created")} {FileName}.json";
		File.WriteAllText(m_SaveFilePath, json);
		Debug.Log(message);
	}
}
