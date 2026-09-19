using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameUserSettingsManager : MonoBehaviour
{
    public const float MIN_SENS_MULT_UI = 0.5f;
    public const float MAX_SENS_MULT_UI = 2f;

    public static readonly string[] AntiAliasingTypes = new string[13]
    {
        "None",
        "FXAA",
        "SMAA (Low)",
        "SMAA (Medium)",
        "SMAA (High)",
        "TAA (Very Low)",
        "TAA (Low)",
        "TAA (Medium)",
        "TAA (High)",
        "TAA (Very High)",
        "MSAA (2X)",
        "MSAA (4X)",
        "MSAA (8X)"
    };

    // For a two-dropdown AA UI: the first dropdown picks one of these, the second (if the mode has any
    // qualities) picks one of AntiAliasingQualities[mode]. Split/ComposeAntiAliasingType convert between
    // that pair and the single AntiAliasingTypes string AntiAliasingApplier/GameUserSettings actually store
    public static readonly string[] AntiAliasingModes = new string[] { "None", "FXAA", "SMAA", "TAA", "MSAA" };

    public static readonly Dictionary<string, string[]> AntiAliasingQualities = new Dictionary<string, string[]>
    {
        { "None", new string[0] },
        { "FXAA", new string[0] },
        { "SMAA", new string[] { "Low", "Medium", "High" } },
        { "TAA", new string[] { "Very Low", "Low", "Medium", "High", "Very High" } },
        { "MSAA", new string[] { "2X", "4X", "8X" } }
    };

    /// <summary>
    /// Splits a combined AntiAliasingTypes entry (eg. "SMAA (Medium)") into its mode ("SMAA") and
    /// quality ("Medium"). Quality is null for modes with none (eg. "FXAA" -> ("FXAA", null))
    /// </summary>
    public static (string Mode, string Quality) SplitAntiAliasingType(string fullType)
    {
        int parenIndex = fullType.IndexOf(" (");
        if (parenIndex < 0) return (fullType, null);
        string mode = fullType.Substring(0, parenIndex);
        string quality = fullType.Substring(parenIndex + 2, fullType.Length - parenIndex - 3);
        return (mode, quality);
    }

    public static string ComposeAntiAliasingType(string mode, string quality) => string.IsNullOrEmpty(quality) ? mode : $"{mode} ({quality})";

    const string FILE_NAME = "user_settings.json";

    // The only single-key actions exposed for user rebinding
    public static readonly (string ActionMap, string Action)[] RebindableActions = new (string, string)[]
    {
        ("Player", "Jump"),
        ("Player", "Sprint"),
        ("Player", "Interact")
    };

    // Composite actions exposed for user rebinding, one part at a time
    public static readonly (string ActionMap, string Action)[] RebindableCompositeActions = new (string, string)[]
    {
        ("Player", "Move")
    };

    public static GameUserSettingsManager Instance { get; private set; }

    [SerializeField]
    AudioMixer m_AudioMixer;
    [SerializeField]
    InputActionAsset m_InputActions;

    InputActionRebindingExtensions.RebindingOperation m_ActiveRebindOperation;

    GameUserSettings m_UserSettings;
    GameUserSettings m_TempSettings;

    public (int HorizontalResolution, int VerticalResolution) Resolution
    {
        get => (m_UserSettings.HorizontalResolution, m_UserSettings.VerticalResolution);
        set => (m_TempSettings.HorizontalResolution, m_TempSettings.VerticalResolution) = value;
    }

    public int VSyncCount
    {
        get => m_UserSettings.VSyncCount;
        set => m_TempSettings.VSyncCount = value;
    }

    public string AntiAliasing
    {
        get => m_UserSettings.AntiAliasing;
        set => m_TempSettings.AntiAliasing = value;
    }

    public float MasterVolume
    {
        get => m_UserSettings.MasterVolume;
        set => m_TempSettings.MasterVolume = value;
    }

    public float MusicVolume
    {
        get => m_UserSettings.MusicVolume;
        set => m_TempSettings.MusicVolume = value;
    }

    public float SoundEffectsVolume
    {
        get => m_UserSettings.SoundEffectsVolume;
        set => m_TempSettings.SoundEffectsVolume = value;
    }

    public float HorizontalSensitivityMultiplier
    {
        get => m_UserSettings.HorizontalSensitivityMultiplier;
        set => m_TempSettings.HorizontalSensitivityMultiplier = value;
    }

    public float VerticalSensitivityMultiplier
    {
        get => m_UserSettings.VerticalSensitivityMultiplier;
        set => m_TempSettings.VerticalSensitivityMultiplier = value;
    }

    string m_Path;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            m_Path = Path.Combine(Application.persistentDataPath, FILE_NAME);
            m_TempSettings = LoadSettings();
            ApplyKeybindOverrides(m_TempSettings.InputBindingOverridesJson);
            SaveSettings();
            SceneManager.sceneLoaded += OnSceneLoaded;
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AntiAliasingApplier.Apply(m_UserSettings.AntiAliasing);

    void Start() => ApplySettings();

    GameUserSettings LoadSettings()
    {
        if (!File.Exists(m_Path)) return new GameUserSettings();
        try
        {
            string contents = File.ReadAllText(m_Path);
            GameUserSettings userSettings = JsonUtility.FromJson<GameUserSettings>(contents);
            userSettings.HorizontalResolution = Mathf.Max(userSettings.HorizontalResolution, 640);
            userSettings.VerticalResolution = Mathf.Max(userSettings.VerticalResolution, 480);
            userSettings.VSyncCount = Mathf.Clamp(userSettings.VSyncCount, 0, 2);
            userSettings.MasterVolume = Mathf.Clamp01(userSettings.MasterVolume);
            userSettings.MusicVolume = Mathf.Clamp01(userSettings.MusicVolume);
            userSettings.SoundEffectsVolume = Mathf.Clamp01(userSettings.SoundEffectsVolume);
            userSettings.HorizontalSensitivityMultiplier = Mathf.Max(userSettings.HorizontalSensitivityMultiplier, 0.01f);
            userSettings.VerticalSensitivityMultiplier = Mathf.Max(userSettings.VerticalSensitivityMultiplier, 0.01f);
#if UNITY_EDITOR
            Debug.Log("Sucessfully loaded user settings");
#endif
            return userSettings;
        }
        catch
        {
            Debug.LogError($"Error reading \"{m_Path}\"! Using default values");
            return new GameUserSettings();
        }
    }

    public void SaveSettings()
    {
        m_TempSettings.InputBindingOverridesJson = m_InputActions.SaveBindingOverridesAsJson();
        m_UserSettings = new GameUserSettings(m_TempSettings);
        ApplySettings();
        try
        {
            string json = JsonUtility.ToJson(m_UserSettings, prettyPrint: true);
            File.WriteAllText(m_Path, json);
        }
        catch
        {
            Debug.LogError("Failed to save user settings");
        }
    }

    void ApplySettings()
    {
        Screen.SetResolution(m_UserSettings.HorizontalResolution, m_UserSettings.VerticalResolution, fullscreen: true);
        QualitySettings.vSyncCount = m_UserSettings.VSyncCount;
        AntiAliasingApplier.Apply(m_UserSettings.AntiAliasing);
        m_AudioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(m_UserSettings.MasterVolume, 0.0001f, 1f)) * 20f);
        m_AudioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(m_UserSettings.MusicVolume, 0.0001f, 1f)) * 20f);
        m_AudioMixer.SetFloat("SoundEffectsVolume", Mathf.Log10(Mathf.Clamp(m_UserSettings.SoundEffectsVolume, 0.0001f, 1f)) * 20f);
    }

    public void ClearTempSettings()
    {
        m_TempSettings = new GameUserSettings(m_UserSettings);
        ApplyKeybindOverrides(m_TempSettings.InputBindingOverridesJson);
    }

    #region Keybinds
    void ApplyKeybindOverrides(string json)
    {
        if (string.IsNullOrEmpty(json)) m_InputActions.RemoveAllBindingOverrides();
        else m_InputActions.LoadBindingOverridesFromJson(json);
    }

    InputAction FindRebindableAction(string actionMapName, string actionName)
    {
        InputAction action = m_InputActions.FindActionMap(actionMapName)?.FindAction(actionName);
        if (action == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: Could not find action \"{actionName}\" in map \"{actionMapName}\" to rebind");
#endif
        }
        return action;
    }

    public string GetBindingDisplayString(string actionMapName, string actionName, int bindingIndex = 0) => FindRebindableAction(actionMapName, actionName)?.GetBindingDisplayString(bindingIndex) ?? "";

    public List<(int BindingIndex, string PartName, string CurrentDisplayString)> GetCompositePartBindings(string actionMapName, string actionName)
    {
        List<(int, string, string)> parts = new List<(int, string, string)>();
        InputAction action = FindRebindableAction(actionMapName, actionName);
        if (action == null) return parts;
        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; ++i)
        {
            if (bindings[i].isPartOfComposite) parts.Add((i, bindings[i].name, action.GetBindingDisplayString(i)));
        }
        return parts;
    }

    public void StartRebind(string actionMapName, string actionName, int bindingIndex = 0, System.Action onComplete = null, System.Action onCancel = null)
    {
        if (m_ActiveRebindOperation != null) return;
        InputAction action = FindRebindableAction(actionMapName, actionName);
        if (action == null) return;
        action.Disable();
        m_ActiveRebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsHavingToMatchPath("<Keyboard>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnCancel(_ =>
            {
                action.Enable();
                FinishRebind();
                onCancel?.Invoke();
            })
            .OnComplete(_ =>
            {
                action.Enable();
                FinishRebind();
                onComplete?.Invoke();
            })
            .Start();
    }

    void FinishRebind()
    {
        m_ActiveRebindOperation?.Dispose();
        m_ActiveRebindOperation = null;
    }

    public void ResetKeybind(string actionMapName, string actionName, int bindingIndex = 0) => FindRebindableAction(actionMapName, actionName)?.RemoveBindingOverride(bindingIndex);
    #endregion Keybinds
}
