using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DialogueEditor : EditorWindow
{
    static readonly string s_ResourcesFolder = $"{Application.dataPath.TrimEnd('/')}/Resources";

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

    string m_DirectoryName = "";
    readonly string[] m_DirectoryOptions = new string[2]
    {
        "Use Existing Subdirectory",
        "Create New Subdirectory"
    };
    int m_SelectedDirectoryOptionIndex = 0;
    int m_LastSelectedDirectoryOptionIndex = 0;
    string[] m_Directories;
    int m_SelectedDirectoryIndex = 0;

    public Dialogue Dialogue;
    SerializedObject m_SerializedObject;
    SerializedProperty m_SerializedProperty;

    Vector2 m_ScrollPos;

    [MenuItem("Window/Edit Dialogue")]
    public static void ShowWindow() => GetWindow<DialogueEditor>("Dialogue Editor");

    public void CreateGUI()
    {
        m_SerializedObject = new SerializedObject(this);
        m_SerializedProperty = m_SerializedObject.FindProperty("Dialogue");
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
                m_DirectoryName = "";
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
                m_DirectoryName = "";
                m_SelectedDirectoryOptionIndex = 0;
                m_LastSelectedDirectoryOptionIndex = -1;
                Dialogue = null;
            }
            m_SelectedDirectoryOptionIndex = EditorGUILayout.Popup("Choose Directory to Use", m_SelectedDirectoryOptionIndex, m_DirectoryOptions);
            if (m_LastSelectedDirectoryOptionIndex == 0)
            {
                if (GUILayout.Button("Refresh") || m_Directories == null || m_LastSelectedDirectoryOptionIndex != m_SelectedDirectoryOptionIndex)
                {
                    GetDirectories();
                    m_LastSelectedDirectoryOptionIndex = m_SelectedDirectoryOptionIndex;
                    m_SelectedDirectoryIndex = 0;
                }
                m_SelectedDirectoryIndex = EditorGUILayout.Popup("Select an Existing Directory", m_SelectedDirectoryIndex, m_Directories);
                m_DirectoryName = m_Directories.Length > 0 ? m_Directories[m_SelectedDirectoryIndex] : "";
            }
            else
            {
                if (m_LastSelectedDirectoryOptionIndex != m_SelectedDirectoryOptionIndex)
                {
                    m_FileName = "";
                    m_LastSelectedDirectoryOptionIndex = m_SelectedDirectoryOptionIndex;
                }
                m_DirectoryName = EditorGUILayout.TextField("New Directory Name", m_DirectoryName).Trim();
            }
            m_FileName = EditorGUILayout.TextField("New File Name", m_FileName).Trim();
        }

        Dialogue ??= new Dialogue();

        m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

        m_SerializedObject.Update();
        EditorGUILayout.PropertyField(m_SerializedProperty, true);
        m_SerializedObject.ApplyModifiedProperties();

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Clear Input")) Clear();

        if (string.IsNullOrEmpty(m_FileName)) return;

        if (m_SelectedFileOptionIndex == 0)
        {
            if (GUILayout.Button($"Modify \"{m_FileName}\"")) ModifyFile();
            if (GUILayout.Button($"Delete \"{m_FileName}\"")) DeleteFile();
        }
        else
        {
            string path = GetFullPath(m_DirectoryName, m_FileName);
            string partialPath = path.TrimStart($"{s_ResourcesFolder}/");
            if (GUILayout.Button($"Create \"{partialPath}\"")) CreateFile(path, partialPath);
        }
    }

    void GetFiles()
    {
        List<string> _filesList = new List<string>();
        string[] _files = Directory.GetFiles(s_ResourcesFolder, "*.json", SearchOption.AllDirectories);
        for (int i = 0; i < _files.Length; ++i)
        {
            _files[i] = _files[i].Replace('\\', '/');
            _files[i] = _files[i][s_ResourcesFolder.Length..].TrimStart('/').TrimEnd(".json");
            TextAsset json = Resources.Load<TextAsset>(_files[i]);
            try
            {
                Dialogue _dialogue = JsonUtility.FromJson<SerializedDialogue>(json.text).Deserialized;
                _files[i] += ".json";
                _filesList.Add(_files[i]);
            }
            catch { }
        }
        m_Files = _filesList.ToArray();
    }

    void GetDirectories()
    {
        string[] _subDirectories = Directory.GetDirectories(s_ResourcesFolder, "*", SearchOption.AllDirectories);
        for (int i = 0; i < _subDirectories.Length; ++i)
        {
            _subDirectories[i] = _subDirectories[i].Replace('\\', '/');
            _subDirectories[i] = _subDirectories[i].Substring(Application.dataPath.Length).TrimStart('/');
        }
        m_Directories = new string[_subDirectories.Length + 1];
        m_Directories[0] = "Resources";
        _subDirectories.CopyTo(m_Directories, 1);
    }

    void LoadFile()
    {
        string jsonPath = m_FileName.TrimEnd(".json");
        TextAsset json = Resources.Load<TextAsset>(jsonPath);
        if (!json)
        {
            Debug.LogError($"\"{jsonPath}.json\" does not exist!");
            return;
        }
        try
        {
            Dialogue = JsonUtility.FromJson<SerializedDialogue>(json.text).Deserialized;
            Debug.Log($"Successfully loaded \"{jsonPath}.json\"");
        }
        catch (ArgumentException)
        {
            Debug.LogError($"Invalid data in \"{jsonPath}.json\"");
        }
    }

    string GetFullPath(string directory, string file)
    {
        if (string.IsNullOrWhiteSpace(file)) return null;
        string path = file;
        if (!string.IsNullOrWhiteSpace(directory)) path = Path.Combine(directory, path).Replace('\\', '/');
        if (!path.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase)) path = $"Resources/{path}";
        if (!path.StartsWith($"{Application.dataPath.TrimEnd('/')}/", StringComparison.OrdinalIgnoreCase)) path = $"{Application.dataPath.TrimEnd('/')}/{path}";
        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) path += ".json";
        return path;
    }

    void SaveFile(string file)
    {
        string json = JsonUtility.ToJson(Dialogue.Serialised, prettyPrint: true);
        File.WriteAllText(file, json);
    }

    void ModifyFile()
    {
        string path = GetFullPath(m_DirectoryName, m_FileName);
        SaveFile(path);
        Debug.Log($"Successfully modified \"{path.TrimStart($"{s_ResourcesFolder}/")}\"");
    }

    void CreateFile(string path, string partialPath)
    {
        if (!File.Exists(path))
        {
            SaveFile(path);
            Debug.Log($"Successfully created \"{partialPath}\"");
        }
        else Debug.LogError($"\"{partialPath}\" already exists!");
    }

    void DeleteFile()
    {
        string fullPath = GetFullPath(m_DirectoryName, m_FileName);
        string message = $"Successfully deleted \"{m_FileName}\"";
        File.Delete(fullPath);
        if (File.Exists($"{fullPath}.json"))
        {
            File.Delete($"{fullPath}.json");
            message += $" and \"{m_FileName}.meta\"";
        }
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
        m_SelectedDirectoryOptionIndex = 0;
        m_LastSelectedDirectoryOptionIndex = -1;
        m_SelectedDirectoryIndex = 0;
        m_DirectoryName = "";
        m_Directories = null;
        Dialogue = null;
    }
}