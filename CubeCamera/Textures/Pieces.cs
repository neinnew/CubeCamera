using UnityEngine;

namespace CubeCamera.Textures;

public class Pieces : TextureBase
{
    public Pieces(int faceSize) : base(faceSize) { }

    private Texture2D? _frontTexture;
    private Texture2D? _leftTexture;
    private Texture2D? _rightTexture;
    private Texture2D? _backTexture;
    private Texture2D? _topTexture;
    private Texture2D? _bottomTexture;

    ~Pieces()
    {
        UnityEngine.Object.Destroy(_frontTexture);
        UnityEngine.Object.Destroy(_leftTexture);
        UnityEngine.Object.Destroy(_rightTexture);
        UnityEngine.Object.Destroy(_backTexture);
        UnityEngine.Object.Destroy(_topTexture);
        UnityEngine.Object.Destroy(_bottomTexture);
    }
    
    public override void Save(string directory, string fileName, FileFormat format)
    {
        _frontTexture ??= new Texture2D(Faces.Size, Faces.Size, TextureFormat.RGB24, false);
        _leftTexture ??= new Texture2D(Faces.Size, Faces.Size, TextureFormat.RGB24, false);
        _rightTexture ??= new Texture2D(Faces.Size, Faces.Size, TextureFormat.RGB24, false);
        _backTexture ??= new Texture2D(Faces.Size, Faces.Size, TextureFormat.RGB24, false);
        _topTexture ??= new Texture2D(Faces.Size, Faces.Size, TextureFormat.RGB24, false);
        _bottomTexture ??= new Texture2D(Faces.Size, Faces.Size, TextureFormat.RGB24, false);

        ReadTexture(Faces.Front , _frontTexture);
        ReadTexture(Faces.Left , _leftTexture);
        ReadTexture(Faces.Right , _rightTexture);
        ReadTexture(Faces.Back , _backTexture);
        ReadTexture(Faces.Top , _topTexture);
        ReadTexture(Faces.Bottom , _bottomTexture);
        
        Save(_frontTexture, directory, fileName + "_front", format);
        Save(_leftTexture, directory, fileName + "_left", format);
        Save(_rightTexture, directory, fileName + "_right", format);
        Save(_backTexture, directory, fileName + "_back", format);
        Save(_topTexture, directory, fileName + "_top", format);
        Save(_bottomTexture, directory, fileName + "_bottom", format);
    }
}