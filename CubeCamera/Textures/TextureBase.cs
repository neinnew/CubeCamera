using UnityEngine;

namespace CubeCamera.Textures;

public abstract class TextureBase
{
    public class FaceTextures
    {
        public readonly RenderTexture Front;
        public readonly RenderTexture Left;
        public readonly RenderTexture Right;
        public readonly RenderTexture Back;
        public readonly RenderTexture Top;
        public readonly RenderTexture Bottom;
        
        public readonly int Size;

        public FaceTextures(int size)
        {
            Size = size;
            Front = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            Left = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            Right = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            Back = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            Top = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
            Bottom = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
        }

        ~FaceTextures()
        {
            UnityEngine.Object.Destroy(Front);
            UnityEngine.Object.Destroy(Left);
            UnityEngine.Object.Destroy(Right);
            UnityEngine.Object.Destroy(Back);
            UnityEngine.Object.Destroy(Top);
            UnityEngine.Object.Destroy(Bottom);
        }

        public void EnableRandomWrite()
        {
            Front.enableRandomWrite = true;
            Left.enableRandomWrite = true;
            Right.enableRandomWrite = true;
            Back.enableRandomWrite = true;
            Top.enableRandomWrite = true;
            Bottom.enableRandomWrite = true;
        }
    }

    public readonly FaceTextures Faces;

    protected TextureBase(int faceSize)
    {
        Faces = new FaceTextures(faceSize);
    }

    public abstract void Save(string directory, string fileName, FileFormat format);
    
    protected static void ReadTexture(RenderTexture renderTexture, Texture2D texture2D)
    {
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = null;
    }

    protected static void Save(Texture2D texture, string directory, string fileName, FileFormat format)
    {
        var bytes = format switch
        {
            FileFormat.PNG => texture.EncodeToPNG(),
            FileFormat.JPG => texture.EncodeToJPG(),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        string extension = format switch
        {
            FileFormat.PNG => ".png",
            FileFormat.JPG => ".jpg",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        File.WriteAllBytes(Path.Combine(directory, fileName + extension), bytes);
    }
}