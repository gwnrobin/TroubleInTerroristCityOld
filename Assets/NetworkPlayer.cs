using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private int clientID;

    public GameObject player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        clientID = (int)OwnerClientId;

        if (IsServer)
        {
            player = SpawnPlayer();
        }
    }

    private GameObject SpawnPlayer()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        GameObject go = Instantiate(playerPrefab, spawnPoints[Random.Range(0, spawnPoints.Length - 1)].transform.position, Quaternion.identity);
        go.GetComponent<NetworkObject>().SpawnAsPlayerObject(OwnerClientId);
        go.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
        return go;
    }
}
