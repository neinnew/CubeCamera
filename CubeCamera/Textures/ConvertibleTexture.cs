using UnityEngine;

namespace CubeCamera.Textures;

public abstract class ConvertibleTexture : TextureBase
{
    /// <summary>
    /// Converted result render texture.
    /// </summary>
    public abstract RenderTexture Converted { get; init; }
    
    private Texture2D? _convertedTexture;

    protected readonly ComputeShader Converter = AssetBundle.LoadAsset<ComputeShader>("Assets/Converter.compute");
    protected readonly int KernelID;
    
    private static readonly AssetBundle AssetBundle = AssetBundle.LoadFromFile(Path.Combine(
        ColossalFramework.Plugins.PluginManager.instance
            .FindPluginInfo(System.Reflection.Assembly.GetExecutingAssembly()).modPath, "assetbundle"));

    protected ConvertibleTexture(int faceSize, string kernelName) : base(faceSize)
    {
        KernelID = Converter.FindKernel(kernelName);
    }

    ~ConvertibleTexture()
    {
        UnityEngine.Object.Destroy(Converted);
        UnityEngine.Object.Destroy(_convertedTexture);
    }

    public abstract void Convert();
    
    public override void Save(string directory, string fileName, FileFormat format)
    {
        _convertedTexture ??= new Texture2D(Converted.width, Converted.height, TextureFormat.RGB24, false);
        ReadTexture(Converted, _convertedTexture);
        Save(_convertedTexture, directory, fileName, format);
    }
    
    /// <summary>
    /// List of property IDs from converter compute shader.
    /// </summary>
    protected static class NameIDs
    {
        public static readonly int FrontFace = Shader.PropertyToID("_FrontFace");
        public static readonly int LeftFace = Shader.PropertyToID("_LeftFace");
        public static readonly int RightFace = Shader.PropertyToID("_RightFace");
        public static readonly int BackFace = Shader.PropertyToID("_BackFace");
        public static readonly int TopFace = Shader.PropertyToID("_TopFace");
        public static readonly int BottomFace = Shader.PropertyToID("_BottomFace");
        
        public static readonly int Result = Shader.PropertyToID("_Result");
        
        public static readonly int FrontOffset = Shader.PropertyToID("_FrontOffset");
        public static readonly int LeftOffset = Shader.PropertyToID("_LeftOffset");
        public static readonly int RightOffset = Shader.PropertyToID("_RightOffset");
        public static readonly int BackOffset = Shader.PropertyToID("_BackOffset");
        public static readonly int TopOffset = Shader.PropertyToID("_TopOffset");
        public static readonly int BottomOffset = Shader.PropertyToID("_BottomOffset");
        
        public static readonly int FaceSize = Shader.PropertyToID("_FaceSize");
        public static readonly int Width = Shader.PropertyToID("_Width");
        public static readonly int Height = Shader.PropertyToID("_Height");
    }
}