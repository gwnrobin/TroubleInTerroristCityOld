using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LocalPlayer : NetworkBehaviour
{
    public UnityEvent e;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!GetComponent<NetworkObject>().IsLocalPlayer)
            return;

        e.Invoke();
    }
}
