using UnityEngine;

namespace CubeCamera;

public struct Pose
{
    public Vector3 Position;
    public Quaternion Rotation;

    public Pose()
    {
        Position = new Vector3();
        Rotation = new Quaternion();
    }
    
    public Pose(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }
}