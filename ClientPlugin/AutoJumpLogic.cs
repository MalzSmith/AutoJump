using System;
using System.Diagnostics.CodeAnalysis;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRageMath;

namespace ClientPlugin;

public class AutoJumpLogic
{
    // The maximum allowed change in angle compared to the initial state. Exceeding this value will cause jumping to stop.
    public const double AngularToleranceDegrees = 5;

    public static AutoJumpLogic Instance = new();

    private AutoJumpLogic()
    {
    }

    private ulong _lastCheckFrame;
    private long _trackedJumpDrive;
    private long _trackedCockpit;
    private Vector3D? _savedOrientation;

    public bool AutomaticJumpInitiated { get; private set; }


    public bool IsAutoJumpEnabled(MyJumpDrive jumpDrive)
    {
        return _trackedJumpDrive == jumpDrive.EntityId;
    }

    public bool IsAutoJumpEnabled()
    {
        return _trackedJumpDrive > 0;
    }

    public void Stop()
    {
        _trackedJumpDrive = 0;
        _trackedCockpit = 0;
        _savedOrientation = null;
    }

    public void ToggleAutoJump(MyJumpDrive jumpDrive)
    {
        if (jumpDrive.EntityId == _trackedJumpDrive)
        {
            Stop();
        }
        else
        {
            _trackedJumpDrive = jumpDrive.EntityId;
        }
    }

    [SuppressMessage("ReSharper", "DuplicatedSequentialIfBodies")]
    public void Update()
    {
        if (_trackedJumpDrive == 0)
            return;

        if (MySession.Static == null)
            return;

        var intervalFrames = (ulong)(Config.Current.CheckIntervalSeconds * 60);
        var currentFrame = MySandboxGame.Static.SimulationFrameCounter;
        if (currentFrame < _lastCheckFrame + intervalFrames)
            return;

        _lastCheckFrame = currentFrame;

        if (MySession.Static.ControlledEntity is null)
        {
            // Maybe we are on a nexus loading screen or something - we will wait until there is something controlled
            return;
        }
        
        if (MySession.Static.ControlledEntity is not MyCockpit controlledCockpit
            || (_trackedCockpit != 0 && _trackedCockpit != controlledCockpit.EntityId))
        {
            MyAPIGateway.Utilities.ShowNotification("Player has left the cockpit, automatic jumping disabled");
            Stop();
            return;
        }

        if (_trackedCockpit == 0)
        {
            _trackedCockpit = controlledCockpit.EntityId;
        }
        
        if (!MyEntities.TryGetEntityById(_trackedJumpDrive, out var entity) || entity is not MyJumpDrive jumpDrive)
        {
            Stop();
            return;
        }

        if (jumpDrive.Closed || jumpDrive.MarkedForClose)
        {
            Stop();
            return;
        }

        if (!jumpDrive.IsWorking || !jumpDrive.IsFunctional)
        {
            Stop();
            return;
        }

        var orientation = jumpDrive.WorldMatrix.GetOrientation().Forward;
        if (_savedOrientation is null)
        {
            _savedOrientation = orientation;
        }
        else
        {
            var angularDifference =
                MathHelper.ToDegrees(
                    Math.Acos(MathHelper.Clamp(Vector3D.Dot(_savedOrientation.Value, orientation), -1, 1))
                );

            if (angularDifference > AngularToleranceDegrees)
            {
                MyAPIGateway.Utilities.ShowNotification("Ship orientation changed, automatic jumping disabled.", 5000,
                    "Red");
                Stop();
                return;
            }
        }

        if (!jumpDrive.IsFull)
            return;

        if (jumpDrive.CubeGrid.GridSystems.JumpSystem.IsJumping)
            return;

        AutomaticJumpInitiated = true;

        try
        {
            ((IMyJumpDrive)jumpDrive).Jump();
            MyAPIGateway.Utilities.ShowNotification("Automatic jump initiated");
        }
        finally
        {
            AutomaticJumpInitiated = false;
        }
    }
}