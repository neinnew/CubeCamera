using static CubeCamera.Textures.Face;

namespace CubeCamera.Textures;

public partial class Cubemap
{
    public static class GridPreset
    {
        public static Face?[,] GetByName(string name) => name switch
        {
            nameof(Cross4x3) => Cross4x3,
            nameof(Cross3x4) => Cross3x4,
            nameof(Pano2VR3x2) => Pano2VR3x2,
            nameof(Facebook3x2) => Facebook3x2,
            nameof(Row6x1) => Row6x1,
            nameof(Column1x6) => Column1x6,
            _ => Cross4x3
        };
        
        public static Face?[,] Cross4x3 => new Face?[3, 4]
        {
            { null, Top, null, null },
            { Left, Front, Right, Back },
            { null, Bottom, null, null}
        };
        
        public static Face?[,] Cross3x4 => new Face?[4, 3]
        {
            { null, Top, null },
            { Front, Right, Back },
            { null, Bottom, null},
            { null, Left, null},
        };

        public static Face?[,] Pano2VR3x2 => new Face?[2, 3]
        {
            { Front, Right, Back },
            { Left, Top, Bottom }
        };
        
        public static Face?[,] Facebook3x2 => new Face?[2, 3]
        {
            { Right, Left, Top },
            { Bottom, Front, Back }
        };
    
        public static Face?[,] Row6x1 => new Face?[1, 6]
        {
            {Right, Left, Top, Bottom, Front, Back}
        };
    
        public static Face?[,] Column1x6 => new Face?[6, 1]
        {
            {Right},
            {Left},
            {Top},
            {Bottom},
            {Front},
            {Back}
        };
    }
}