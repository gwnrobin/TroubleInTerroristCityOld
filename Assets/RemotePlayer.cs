using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    private void Awake()
    {
        if(GetComponent<NetworkObject>().IsLocalPlayer)
        {
            Destroy(this);
        }
    }
}
