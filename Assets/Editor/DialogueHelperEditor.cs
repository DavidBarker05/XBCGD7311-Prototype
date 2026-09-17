using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DialogueEditor : EditorWindow
{
    static readonly string s_DialogueFolder = $"{Application.dataPath.TrimEnd('/')}/Resources/Dialogue";

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

    string FilePath => GetFullPath(m_SelectedFileOptionIndex == 0 ? null : m_DirectoryName, m_FileName);

    string GetFullPath(string directory, string file)
    {
        if (string.IsNullOrWhiteSpace(file)) return null;
        string path = file;
        if (!string.IsNullOrWhiteSpace(directory)) path = Path.Combine(directory, path).Replace('\\', '/');
        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) path += ".json";
        return $"{s_DialogueFolder}/{path}";
    }

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
            if (GUILayout.Button($"Modify \"{m_FileName}\"")) SaveFile();
            if (GUILayout.Button($"Delete \"{m_FileName}\"")) DeleteFile();
        }
        else
        {
            string partialPath = FilePath.TrimStart($"{s_DialogueFolder}/");
            if (GUILayout.Button($"Create \"{partialPath}\"")) SaveFile();
        }
    }

    void GetFiles()
    {
        List<string> filesList = new List<string>();
        if (Directory.Exists(s_DialogueFolder))
        {
            string[] files = Directory.GetFiles(s_DialogueFolder, "*.json", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; ++i)
            {
                string relativePath = files[i].Replace('\\', '/')[(s_DialogueFolder.Length + 1)..]; // Keeps any subdirectory, e.g. "Tutorial/TutorialStart.json"
                string resourcePath = Path.ChangeExtension(relativePath, null);
                TextAsset json = Resources.Load<TextAsset>($"Dialogue/{resourcePath}");
                try
                {
                    _ = JsonUtility.FromJson<SerializedDialogue>(json.text).Deserialized;
                    filesList.Add(relativePath);
                }
                catch { }
            }
        }
        m_Files = filesList.ToArray();
    }

    void GetDirectories()
    {
        string[] subDirectories = Directory.Exists(s_DialogueFolder) ? Directory.GetDirectories(s_DialogueFolder, "*", SearchOption.AllDirectories) : Array.Empty<string>();
        for (int i = 0; i < subDirectories.Length; ++i)
        {
            subDirectories[i] = subDirectories[i].Replace('\\', '/')[(s_DialogueFolder.Length + 1)..]; // Relative to s_DialogueFolder, matching what GetFullPath expects
        }
        m_Directories = new string[subDirectories.Length + 1];
        m_Directories[0] = "";
        subDirectories.CopyTo(m_Directories, 1);
    }

    void LoadFile()
    {
        string resourcePath = Path.ChangeExtension(m_FileName, null); // Preserves any subdirectory, unlike Path.GetFileNameWithoutExtension
        TextAsset json = Resources.Load<TextAsset>($"Dialogue/{resourcePath}");
        if (!json)
        {
            Debug.LogError($"\"{m_FileName}\" does not exist!");
            return;
        }
        try
        {
            Dialogue = JsonUtility.FromJson<SerializedDialogue>(json.text).Deserialized;
            Debug.Log($"Successfully loaded \"{m_FileName}\"");
        }
        catch (ArgumentException)
        {
            Debug.LogError($"Invalid data in \"{m_FileName}\"");
        }
    }

    void SaveFile()
    {
        string json = JsonUtility.ToJson(Dialogue.Serialized, prettyPrint: true);
        string path = FilePath;
        string message = $"Successfully {(File.Exists(path) ? "modified" : "created")} \"{m_FileName}\"";
        Directory.CreateDirectory(Path.GetDirectoryName(path)); // Not just s_DialogueFolder - m_DirectoryName may add a subdirectory that doesn't exist yet
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        Debug.Log(message);
    }

    void DeleteFile()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogError($"\"{m_FileName}\" doesn't exist inside Assets/Resources/Dialogue/");
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
        m_SelectedDirectoryOptionIndex = 0;
        m_LastSelectedDirectoryOptionIndex = -1;
        m_SelectedDirectoryIndex = 0;
        m_DirectoryName = "";
        m_Directories = null;
        Dialogue = null;
    }
}