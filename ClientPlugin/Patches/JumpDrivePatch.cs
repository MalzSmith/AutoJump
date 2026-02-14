using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ClientPlugin.Settings.Tools;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens.Terminal.Controls;
using Sandbox.Graphics.GUI;
using VRage.Game;
using VRageMath;

namespace ClientPlugin.Patches;

// ReSharper disable once UnusedType.Global
public static class JumpDrivePatch
{
    public static void ApplyPatches()
    {
        var harmony = new Harmony(Plugin.Name);


        var addScreenMethod = AccessTools.Method(typeof(MyGuiSandbox), nameof(MyGuiSandbox.AddScreen));
        var addScreenPatch = AccessTools.Method(typeof(JumpDrivePatch), nameof(AddScreenPrefix));
        harmony.Patch(addScreenMethod, prefix: addScreenPatch);
        
        var createTerminalControls =
            AccessTools.Method(typeof(MyJumpDrive), "CreateTerminalControls");
        var createTerminalControlsPostfix = AccessTools.Method(typeof(JumpDrivePatch), nameof(AddTerminalControl));
        harmony.Patch(createTerminalControls, postfix: createTerminalControlsPostfix);
    }

    public static bool AddScreenPrefix(MyGuiScreenBase screen)
    {
        if (!AutoJumpLogic.Instance.AutomaticJumpInitiated || screen is not MyGuiScreenMessageBox msgBox )
            return true;

        var callback = msgBox.ResultCallback;

        if (callback is null)
        {
            // Distance too short/obstacle/whatever - we should probably stop jumping.
            AutoJumpLogic.Instance.Stop();
            return true;
        }
        
        callback.Invoke(MyGuiScreenMessageBox.ResultEnum.YES);
        return false; // don't show the dialog
    }

    // ReSharper disable once UnusedMember.Local
    private static void AddTerminalControl()
    {
        if (MyTerminalControlFactory.GetActions(typeof(MyJumpDrive)).Any(a => a.Id == "AutoJump"))
        {
            return;
        }
        
        var action = new MyTerminalAction<MyJumpDrive>(
            "AutoJump",
            new("AutoJump"),
            MyTerminalActionIcons.TOGGLE)
        {
            ValidForGroups = false,
            InvalidToolbarTypes =
            [
                MyToolbarType.Character,
                MyToolbarType.ButtonPanel,
                MyToolbarType.Seat
            ],
            Action = AutoJumpLogic.Instance.ToggleAutoJump,
            Writer = (block, builder) => { builder.Append(AutoJumpLogic.Instance.IsAutoJumpEnabled(block) ? "On" : "Off"); }
        };

        MyTerminalControlFactory.AddAction(action);
    }
}