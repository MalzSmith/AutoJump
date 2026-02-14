using System;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRageMath;

namespace ClientPlugin;

public class AutoJumpLogic
{
    public static AutoJumpLogic Instance = new();
    private AutoJumpLogic() { }
    
    private ulong _lastCheckFrame;
    private long _trackedJumpDrive;
    
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
    }

    public void ToggleAutoJump(MyJumpDrive jumpDrive)
    {
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (jumpDrive.EntityId == _trackedJumpDrive)
        {
            _trackedJumpDrive = 0;
        }
        else
        {
            _trackedJumpDrive = jumpDrive.EntityId;
        }
    }

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

        if (!MyEntities.TryGetEntityById(_trackedJumpDrive, out var entity) || entity is not MyJumpDrive jumpDrive)
        {
            _trackedJumpDrive = 0;
            return;
        }

        if (jumpDrive.Closed || jumpDrive.MarkedForClose)
        {
            _trackedJumpDrive = 0;
            return;
        }

        if (!jumpDrive.IsWorking || !jumpDrive.IsFunctional)
            return;

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
