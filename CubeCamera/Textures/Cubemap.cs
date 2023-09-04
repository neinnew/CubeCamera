using UnityEngine;

namespace CubeCamera.Textures;

public partial class Cubemap : ConvertibleTexture
{
    private readonly Face?[,] _grid;
    
    private int GridRow => _grid.GetLength(0);
    private int GridColumn => _grid.GetLength(1);
    
    public sealed override RenderTexture Converted { get; init; }

    public Cubemap(int faceSize, Face?[,]? grid = null) : base(faceSize, kernelName: nameof(Cubemap))
    {
        Faces.EnableRandomWrite();
        
        _grid = grid ?? GridPreset.Cross4x3;
        
        if (GridRow > 6 || GridColumn > 6 || GridRow == 0 || GridColumn == 0 )
        {
            Debug.LogWarning(Mod.Info.Name + ": Invalid Cubemap Grid");
            _grid = GridPreset.Cross4x3;
        }

        SetGridOffsets(faceSize);
        
        Converted = new RenderTexture(faceSize * GridColumn, faceSize * GridRow, 24, RenderTextureFormat.ARGB32);
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
    
    private void SetGridOffsets(int faceSize)
    {
        var counts = new Dictionary<Face, uint> { { Face.Front, 0 }, { Face.Left, 0 }, { Face.Right, 0 }, { Face.Back, 0 }, { Face.Top, 0 }, { Face.Bottom, 0 } };

        for (int row = 0; row < GridRow; ++row)
        {
            for (int column = 0; column < GridColumn; ++column)
            {
                if (_grid[row, column] is not Face face) continue;

                counts[face]++;
                
                int nameID = face switch
                {
                    Face.Front => NameIDs.FrontOffset,
                    Face.Left => NameIDs.LeftOffset,
                    Face.Right => NameIDs.RightOffset,
                    Face.Back => NameIDs.BackOffset,
                    Face.Top => NameIDs.TopOffset,
                    Face.Bottom => NameIDs.BottomOffset,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                int widthOffset = column * faceSize;
                int heightOffset = (GridRow - 1 - row) * faceSize;
                
                Converter.SetInts(nameID, widthOffset, heightOffset);
            }
        }

        foreach (var count in counts)
        {
            if (count.Value == 0)
            {
                Debug.LogWarning($"{Mod.Info.Name}: Face {count.Key} does not exist in cubemap grid.");
            }
            else if (count.Value > 1)
            {
                Debug.LogWarning($"{Mod.Info.Name}: Face {count.Key} exists more than one in cubemap grid");
            }
        }
    }   
}