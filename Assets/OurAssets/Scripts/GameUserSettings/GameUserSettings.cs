using UnityEngine;

[System.Serializable]
public class GameUserSettings
{
    #region Graphics Settings
    /// <summary>
    /// Screen's horizontal rezolution
    /// </summary>
    public int HorizontalResolution = 1920;
    /// <summary>
    /// Screen's vertical rezolution
    /// </summary>
    public int VerticalResolution = 1080;
    /// <summary>
    /// <para>
    /// Screen vsync count
    /// </para>
    /// 
    /// <para>
    /// Value options:
    /// <list type="bullet">
    /// <item> 0 = Off </item>
    /// <item> 1 = On </item>
    /// <item> 2 = On (Double Buffer) </item>
    /// </list>
    /// </para>
    /// </summary>
    public int VSyncCount = 1;
    /// <summary>
    /// <para>
    /// Anti-Aliasing Type
    /// </para>
    /// <para>
    /// Value options:
    /// <list type="bullet">
    /// <item> None </item>
    /// <item> FXAA </item>
    /// <item> SMAA (Low) </item>
    /// <item> SMAA (Medium) </item>
    /// <item> SMAA (High) </item>
    /// <item> TAA (Very Low) </item>
    /// <item> TAA (Low) </item>
    /// <item> TAA (Medium) </item>
    /// <item> TAA (High) </item>
    /// <item> TAA (Very High) </item>
    /// <item> MSAA (2X) </item>
    /// <item> MSAA (4X) </item>
    /// <item> MSAA (8X) </item>
    /// </list>
    /// </para>
    /// </summary>
    public string AntiAliasing = "None";
    #endregion Graphics Settings

    #region Sound Settings
    /// <summary>
    /// Volume setting that affects all sound types (value must be between 0 and 1)
    /// </summary>
    public float MasterVolume = 1f;
    /// <summary>
    /// Volume setting that affects only music (value must be between 0 and 1)
    /// </summary>
    public float MusicVolume = 1f;
    /// <summary>
    /// Volume setting that affects only sound effects (value must be between 0 and 1)
    /// </summary>
    public float SoundEffectsVolume = 1f;
    #endregion Sound Settings

    #region Control Settings
    #region Sensitivity Multipliers
    /// <summary>
    /// Sensitivity multiplier for horizontal movement
    /// </summary>
    public float HorizontalSensitivityMultiplier = 1f;
    /// <summary>
    /// Sensitivity multiplier for vertical movement
    /// </summary>
    public float VerticalSensitivityMultiplier = 1f;
    #endregion Sensitivity Multipliers

    #region Keybinds
    /// <summary>
    /// Keybind overrides, if action is blank then it isn't overriden
    /// </summary>
    public string InputBindingOverridesJson = "";
    #endregion Keybinds
    #endregion Control Settings

    /// <summary>
    /// Default constructor
    /// </summary>
    public GameUserSettings() { }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="other">The other <see cref="GameUserSettings"/> to copy values from</param>
    public GameUserSettings(GameUserSettings other)
    {
        HorizontalResolution = other.HorizontalResolution;
        VerticalResolution = other.VerticalResolution;
        VSyncCount = other.VSyncCount;
        AntiAliasing = other.AntiAliasing;
        MasterVolume = other.MasterVolume;
        MusicVolume = other.MusicVolume;
        SoundEffectsVolume = other.SoundEffectsVolume;
        HorizontalSensitivityMultiplier = other.HorizontalSensitivityMultiplier;
        VerticalSensitivityMultiplier = other.VerticalSensitivityMultiplier;
        InputBindingOverridesJson = other.InputBindingOverridesJson;
    }
}
