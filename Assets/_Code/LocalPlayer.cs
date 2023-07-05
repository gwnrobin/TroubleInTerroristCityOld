using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LocalPlayer : Player
{
    protected override void Start()
    {
        if (!(GetComponent<NetworkObject>().IsLocalPlayer))
        {
            Destroy(this);

            return;
        }
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        _charAnimStates.action = (int)actionState.Val;
        _charAnimStates.movement = (int)movementState.Val;
        _charAnimStates.pose = (int)poseState.Val;

        charAnimData.Value = _charAnimData;
        charAnimStates.Value = _charAnimStates;
    }
}
