using UnityEngine;

namespace CubeCamera.Textures;

public class Equirectangular : ConvertibleTexture
{
    public sealed override RenderTexture Converted { get; init; }

    public Equirectangular(int faceSize, int width, int height) : base(faceSize, kernelName: nameof(Equirectangular))
    {
        Faces.EnableRandomWrite();
        
        Converter.SetInt(NameIDs.FaceSize, faceSize);
        Converter.SetInt(NameIDs.Width, width);
        Converter.SetInt(NameIDs.Height, height);
        
        Converted = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Converted.enableRandomWrite = true;
        Converted.Create();
    }

    public override void Convert()
    {
        Converter.SetTexture(KernelID, NameIDs.FrontFace, Faces.Front);
        Converter.SetTexture(KernelID, NameIDs.LeftFace, Faces.Left);
        Converter.SetTexture(KernelID, NameIDs.RightFace, Faces.Right);
        Converter.SetTexture(KernelID, NameIDs.BackFace, Faces.Back);
        Converter.SetTexture(KernelID, NameIDs.TopFace, Faces.Top);
        Converter.SetTexture(KernelID, NameIDs.BottomFace, Faces.Bottom);
        Converter.SetTexture(KernelID, NameIDs.Result, Converted);

        Converter.GetKernelThreadGroupSizes(KernelID, out var threadX, out var threadY, out _);
        var threadGroupX = Mathf.CeilToInt((float)Converted.width / threadX);
        var threadGroupY = Mathf.CeilToInt((float)Converted.height / threadY);
        Converter.Dispatch(KernelID, threadGroupX, threadGroupY, 1);
    }
}