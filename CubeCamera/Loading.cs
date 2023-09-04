using ICities;
using UnityEngine;

namespace CubeCamera;

public class Loading : LoadingExtensionBase
{
    internal static Updater? UpdaterInstance;
    
    public override void OnLevelLoaded(LoadMode mode)
    {
        var mainCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<CameraController>().GetComponent<Camera>() 
                         ?? throw new NullReferenceException(Mod.Info.Name + ": can't find the main camera");
        
        UpdaterInstance = mainCamera.gameObject.AddComponent<Updater>();
    }

    public override void OnLevelUnloading()
    {
        if (UpdaterInstance == null) return;
        
        UnityEngine.Object.Destroy(UpdaterInstance);
        UpdaterInstance = null;
    }
}