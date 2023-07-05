using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RemotePlayer : Player
{
    protected override void Start()
    {
        if(GetComponent<NetworkObject>().IsLocalPlayer)
        {
            Destroy(this);

            return;
        }
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        _charAnimData = charAnimData.Value;
        _charAnimStates = charAnimStates.Value;

        actionState.Set((FPSActionState)_charAnimStates.action);
        movementState.Set((FPSMovementState)_charAnimStates.movement);
        poseState.Set((FPSPoseState)_charAnimStates.pose);

        ChangeActionState(actionState.Val);
        ChangeMovementState(movementState.Val);
        ChangePoseState(poseState.Val);

        MoveInput.Set(_charAnimData.moveInput);
    }

    private void ChangeActionState(FPSActionState state)
    {
        switch (state)
        {
            case FPSActionState.Aiming:
                Aim.ForceStart();
                break;
            case FPSActionState.PointAiming:
                PointAim.ForceStart();
                break;
            case FPSActionState.Ready:
                Holster.ForceStart();
                break;
            case FPSActionState.None:
                Aim.ForceStop();
                PointAim.ForceStop();
                Holster.ForceStop();
                break;
        }
    }

    private void ChangeMovementState(FPSMovementState state)
    {
        switch (state)
        {
            case FPSMovementState.Walking:
                Walk.ForceStart();
                break;
            case FPSMovementState.Sprinting:
                Sprint.ForceStart();
                break;
            case FPSMovementState.Idle:
                Walk.ForceStop();
                Sprint.ForceStop();
                break;
        }
    }
    private void ChangePoseState(FPSPoseState state)
    {
        switch (state)
        {
            case FPSPoseState.Crouching:
                Crouch.ForceStart();
                break;
            case FPSPoseState.Standing:
                Crouch.ForceStop();
                break;
        }
    }
}
