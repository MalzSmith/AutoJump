using System.Linq;
using System.Reflection;
using ClientPlugin.Patches;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using HarmonyLib;
using Sandbox.Game.GameSystems;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

#if !DEV_BUILD
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "AutoJump";
    public static Plugin Instance { get; private set; }
    private SettingsGenerator settingsGenerator;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Init(object gameInstance)
    {
        Instance = this;
        Instance.settingsGenerator = new();
        
        JumpDrivePatch.ApplyPatches();
    }

    public void Dispose()
    {
        Instance = null;
    }

    public void Update()
    {
        AutoJumpLogic.Update();
    }

    // ReSharper disable once UnusedMember.Global
    public void OpenConfigDialog()
    {
        Instance.settingsGenerator.SetLayout<Simple>();
        MyGuiSandbox.AddScreen(Instance.settingsGenerator.Dialog);
    }
}
