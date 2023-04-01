using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> players = new();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnect;

        if (!IsHost)
            return;

        GameObject host = NetworkManager.Singleton.ConnectedClients[0].PlayerObject.gameObject;

        players.Add(host);
    }

    private void OnPlayerConnect(ulong clientId)
    {
        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;

        players.Add(player);
    }
}
