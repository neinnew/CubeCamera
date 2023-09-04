using ColossalFramework;
using ColossalFramework.IO;
using CubeCamera.Textures;
using JetBrains.Annotations;

namespace CubeCamera;

/// <summary>
/// Main mod class.
/// </summary>
public static class CubeCamera
{
    public static int FaceSize = ModConfig.Defaults.FaceSize;
    public static int EquirectangularWidth = ModConfig.Defaults.EquirectangularWidth;
    public static int EquirectangularHeight = ModConfig.Defaults.EquirectangularHeight;
    public static string MappingFormat = ModConfig.Defaults.MappingFormat;
    public static string CubemapGrid = ModConfig.Defaults.CubemapGrid;
    public static FileFormat SaveFileFormat = ModConfig.Defaults.SaveFileFormat;
    public static UnityEngine.ScaleMode ScreenScaleMode = ModConfig.Defaults.ScreenScaleMode;

    /// <summary>
    /// Returns a texture based on the current config.
    /// </summary>
    public static TextureBase Texture => MappingFormat switch
    {
        nameof(Pieces) => new Pieces(FaceSize),
        nameof(Cubemap) => new Cubemap(FaceSize, Cubemap.GridPreset.GetByName(CubemapGrid)),
        nameof(Equirectangular) => new Equirectangular(FaceSize, EquirectangularWidth, EquirectangularHeight),
        _ => throw new ArgumentOutOfRangeException()
    };
    
    private static Updater? Updater => Loading.UpdaterInstance;

    public static ContinuousMode? ContinuousMode;
    
    public static readonly string ModFolder = Path.Combine(DataLocation.localApplicationData, "CubeCamera");

    static CubeCamera()
    {
        Directory.CreateDirectory(ModFolder);
    }
    
    /// <summary>
    /// Take a screenshot based on mod config and save it to the mod folder.
    /// </summary>
    public static void Take()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        switch (MappingFormat)
        {
            case nameof(Pieces): 
                Take(Texture, PathUtils.MakeUniquePath(Path.Combine(ModFolder, timestamp)), String.Empty, SaveFileFormat);
                break;
            case nameof(Cubemap): 
            case nameof(Equirectangular):
                Take(Texture, ModFolder, timestamp, SaveFileFormat);
                break;
            
            default: goto case ModConfig.Defaults.MappingFormat;
        }
    }

    /// <summary>
    /// Take a screenshot and save it to the path.
    /// </summary>
    /// <param name="texture">type of texture</param>
    /// <param name="saveDir">directory to be saved</param>
    /// <param name="fileName">name of the file</param>
    /// <param name="fileFormat">file format</param>
    [PublicAPI]
    public static void Take(TextureBase texture, string saveDir, string fileName, FileFormat fileFormat = FileFormat.PNG)
    {
        if (Updater == null) return;
        
        Directory.CreateDirectory(saveDir);

        Updater.Texture = texture;
        Updater.FreeCamera = true;
        
        Updater.StartCycle(onPhaseEnd: delegate
        {
            texture.Save(saveDir, fileName, fileFormat);
            Updater.FreeCamera = false;
        });
        
        // Stop immediately so that only one cycle runs.
        Updater.Stop();
    }


    /// <summary>
    /// Take screenshots continuously based on mod config.
    /// </summary>
    /// <param name="pause">pause or play simulation</param>
    /// <param name="saveCallback">callback called after saving an image</param>
    public static void TakeContinuous(bool? pause = null, Action<int>? saveCallback = null)
    {
        if (ContinuousMode is null) return;
        TakeContinuous(Texture, Path.Combine(ModFolder, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")), ContinuousMode.Poses, SaveFileFormat, pause, saveCallback);
    }        
    
    /// <summary>
    /// Take screenshots continuously.
    /// </summary>
    /// <param name="texture">type of texture</param>
    /// <param name="saveDir">directory to be saved</param>
    /// <param name="targetPoses">list of target poses</param>
    /// <param name="fileFormat">file format</param>
    /// <param name="pause">pause or play simulation</param>
    /// <param name="saveCallback">callback called after saving an image</param>
    [PublicAPI]
    public static void TakeContinuous(TextureBase texture, string saveDir, List<Pose> targetPoses, FileFormat fileFormat = FileFormat.PNG, bool? pause = null, Action<int>? saveCallback = null)
    {
        if (Updater == null) return;
        
        Directory.CreateDirectory(saveDir);
        
        Updater.Texture = texture;
        Updater.FreeCamera = true;
        
        if (pause is bool b)
        {
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                Singleton<SimulationManager>.instance.SimulationPaused = b;
                Singleton<SimulationManager>.instance.ForcedSimulationPaused = b;
            });
        }
        
        var enumerator = targetPoses.GetEnumerator();
        int i = -1;
        
        Updater.StartCycle(onPhaseContinue: delegate
        {
            bool completed = !enumerator.MoveNext();
            i++;
            
            Updater.TargetPose = enumerator.Current;
            
            // It's not ready yet for in first cycle.
            if (i == 0) return;
            
            texture.Save(saveDir, i.ToString(), fileFormat);
            saveCallback?.Invoke(i);
            
            if (completed)
            {
                Updater.FreeCamera = false;
                Updater.Stop();
            }
        });
    }

    /// <summary>
    /// Start draw on the screen.
    /// </summary>
    /// <param name="freeCamera">with free camera or not</param>
    public static void Draw(bool? freeCamera = null)
    {
        if (Updater == null) return;
        
        Updater.Texture = Texture;
        if (freeCamera is bool b) Updater.FreeCamera = b;
        Updater.StartCycle();
        Updater.StartDraw();
    }

    /// <summary>
    /// Force stop on a running cycle.
    /// </summary>
    public static void ForceStop()
    {
        if (Updater == null) return;
        
        Updater.FreeCamera = false;
        Updater.Stop();
    }
}