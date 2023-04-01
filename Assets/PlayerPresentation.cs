using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public class PlayerPresentation : NetworkBehaviour
{
    private NetworkObject networkObject;

    [SerializeField] private List<GameObject> remoteOnly = new();

    private bool prevIsOwner
    {
        get => internalPrevIsOwner;
        set
        {
            if (value != internalPrevIsOwner)
            {
                internalPrevIsOwner = value;
                OnChangedOwner(internalPrevIsOwner);
            }
        }
    }

    private void OnChangedOwner(bool v)
    {
        if (v)
        {
            GetComponentInChildren<Camera>().enabled = true;
            GetComponentInChildren<AudioListener>().enabled = true;
            GetComponent<Player_Input>().enabled = true;
            //GetComponent<ClientNetworkTransform>().GiveAcces();
        }
    }

    private bool internalPrevIsOwner;

    public override void OnNetworkSpawn()
    {
        //if (networkObject == null)
        //    networkObject = GetComponent<NetworkObject>();
    }
}
