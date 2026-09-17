using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class AntiAliasingApplier
{
    struct Settings
    {
        public int MsaaSampleCount;
        public AntialiasingMode Mode;
        public AntialiasingQuality Quality;
        public TemporalAAQuality TaaQuality;
    }

    public static void Apply(string antiAliasingType)
    {
        Settings settings = Parse(antiAliasingType);

        if (UniversalRenderPipeline.asset) UniversalRenderPipeline.asset.msaaSampleCount = settings.MsaaSampleCount;

        foreach (Camera camera in Camera.allCameras)
        {
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.antialiasing = settings.Mode;
            cameraData.antialiasingQuality = settings.Quality;
            cameraData.taaSettings.quality = settings.TaaQuality;
        }
    }

    static Settings Parse(string antiAliasingType) => antiAliasingType switch
    {
        "FXAA" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.FastApproximateAntialiasing },
        "SMAA (Low)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing, Quality = AntialiasingQuality.Low },
        "SMAA (Medium)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing, Quality = AntialiasingQuality.Medium },
        "SMAA (High)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing, Quality = AntialiasingQuality.High },
        "TAA (Very Low)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.TemporalAntiAliasing, TaaQuality = TemporalAAQuality.VeryLow },
        "TAA (Low)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.TemporalAntiAliasing, TaaQuality = TemporalAAQuality.Low },
        "TAA (Medium)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.TemporalAntiAliasing, TaaQuality = TemporalAAQuality.Medium },
        "TAA (High)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.TemporalAntiAliasing, TaaQuality = TemporalAAQuality.High },
        "TAA (Very High)" => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.TemporalAntiAliasing, TaaQuality = TemporalAAQuality.VeryHigh },
        "MSAA (2X)" => new Settings() { MsaaSampleCount = 2, Mode = AntialiasingMode.None },
        "MSAA (4X)" => new Settings() { MsaaSampleCount = 4, Mode = AntialiasingMode.None },
        "MSAA (8X)" => new Settings() { MsaaSampleCount = 8, Mode = AntialiasingMode.None },
        _ => new Settings() { MsaaSampleCount = 1, Mode = AntialiasingMode.None } // "None", or anything unrecognised
    };
}
