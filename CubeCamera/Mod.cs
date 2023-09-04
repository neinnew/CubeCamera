using ICities;
using JetBrains.Annotations;

namespace CubeCamera;

public class Mod : IUserMod
{
    public string Name => Info.Name;

    public string Description => Info.Description;
    
    public static class Info
    {
        public static readonly string Name = "Cube Camera";
        public static readonly string Description = "take a 360 spherical panorama.";
    }

    [UsedImplicitly]
    public void OnEnabled()
    {
        ModConfig.Load();
    }

    [UsedImplicitly]
    public void OnSettingsUI(UIHelperBase helper)
    {
        SettingsUI.OnSettingsUI(helper);
    }
}