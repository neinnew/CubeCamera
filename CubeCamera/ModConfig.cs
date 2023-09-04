using System.Xml.Serialization;
using CubeCamera.Textures;

namespace CubeCamera;

public class ModConfig
{
    // Settings file name.
    [XmlIgnore] 
    private static readonly string SettingsFilePath = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "CubeCamera.xml");

    // File version.
    [XmlAttribute("Version")] 
    public int Version = 0;

    [XmlElement("FaceSize")]
    public int FaceSize { get => CubeCamera.FaceSize; set => CubeCamera.FaceSize = value; }
    
    [XmlElement("EquirectangularWidth")]
    public int EquirectangularWidth { get => CubeCamera.EquirectangularWidth; set => CubeCamera.EquirectangularWidth = value; }
    
    [XmlElement("EquirectangularHeight")]
    public int EquirectangularHeight { get => CubeCamera.EquirectangularHeight; set => CubeCamera.EquirectangularHeight = value; }
    
    [XmlElement("SaveFileFormat")]
    public FileFormat SaveFileFormat { get => CubeCamera.SaveFileFormat; set => CubeCamera.SaveFileFormat = value; }
    
    [XmlElement("MappingFormat")]
    public string MappingFormat { get => CubeCamera.MappingFormat; set => CubeCamera.MappingFormat = value; }
    
    [XmlElement("CubemapGrid")]
    public string CubemapGrid { get => CubeCamera.CubemapGrid; set => CubeCamera.CubemapGrid = value; }
    
    [XmlElement("ScreenScaleMode")]
    public UnityEngine.ScaleMode ScreenScaleMode { get => CubeCamera.ScreenScaleMode; set => CubeCamera.ScreenScaleMode = value; }

    internal static void Load()
    {
        try
        {
            // Check to see if configuration file exists.
            if (File.Exists(SettingsFilePath))
            {
                // Read it.
                using var reader = new StreamReader(SettingsFilePath);
                var xmlSerializer = new XmlSerializer(typeof(ModConfig));
                if (xmlSerializer.Deserialize(reader) is not ModConfig)
                {
                    UnityEngine.Debug.Log($"{Mod.Info.Name}: couldn't deserialize settings file");
                }
            }
            else
            {
                UnityEngine.Debug.Log($"{Mod.Info.Name}:  no settings file found");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    internal static void Save()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(SettingsFilePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModConfig));
                xmlSerializer.Serialize(writer, new ModConfig());
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    public static class Defaults
    {
        public const int FaceSize = 1024;
        public const int EquirectangularWidth = 4096;
        public const int EquirectangularHeight = 2048;
        public const FileFormat SaveFileFormat = FileFormat.PNG;
        public const string MappingFormat = nameof(Cubemap);
        public const string CubemapGrid = nameof(Cubemap.GridPreset.Cross4x3);
        public const UnityEngine.ScaleMode ScreenScaleMode = UnityEngine.ScaleMode.ScaleToFit;
    }
}