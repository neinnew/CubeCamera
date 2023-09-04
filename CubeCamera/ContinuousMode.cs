using UnityEngine;

namespace CubeCamera;

public class ContinuousMode
{
    public readonly Pose StartPoint;
    public readonly Pose EndPoint;
    public readonly uint Division;
    public readonly List<Pose> Poses;

    public ContinuousMode(Pose startPoint, Pose endPoint, uint division)
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
        Division = division;
        Poses = new List<Pose>();
        
        for (int i = 0; i < Division; i++)
        {
            Poses.Add(new Pose(
                position: Vector3.Lerp(StartPoint.Position, EndPoint.Position, 1 / (float)Division * i),
                rotation: Quaternion.Lerp(StartPoint.Rotation, EndPoint.Rotation, 1 / (float)Division * i)
            ));
        }
    }
}