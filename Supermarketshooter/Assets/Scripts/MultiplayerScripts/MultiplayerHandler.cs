using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerHandler : NetworkBehaviour
{
    public static MultiplayerHandler Instance { get; private set; }

    //private NetworkVariable<List<GameObject>> bulletPool;

    [SerializeField] private BulletList bulletList;


    private void Awake()
    {
        Instance = this;
    }


    public void PreSpawnBullets(int magazineSize, int bulletTypeIndex, Gun_Base source, RpcParams rpcParams = default)
    {
        PreSpawnBulletsRPC(magazineSize, bulletTypeIndex, source.NetworkObject, rpcParams);
    }


    [Rpc(SendTo.Server)]
    private void PreSpawnBulletsRPC(int magazineSize, int bulletTypeIndex, NetworkObjectReference shooter, RpcParams rpcParams = default)
    {
        Debug.Log("Server rpc PreSpawn started");

        // Client that called this RPC. Used to send bullet info back to that client specifically
        var clientID = rpcParams.Receive.SenderClientId;

        // grab bullet prefab from the list of bullets
        Bullet bulletPrefab = bulletList.listBullets[bulletTypeIndex];

        // Check if client that created bullet is connected, then tell them stuff
        if (NetworkManager.ConnectedClients.ContainsKey(clientID))
        {
            for (int i = 0; i < magazineSize; i++)
            {
                // Create new bullet
                GameObject newBullet = Instantiate(bulletPrefab.gameObject);
                //newBullet.SetActive(false);

                // Get Network Object bullshit
                NetworkObject newBulletNetObj = newBullet.GetComponent<NetworkObject>();
                newBulletNetObj.Spawn(true); // bool for destroy when owner is destroyed

                // Add bullet to bullet list
                AddToBulletListRPC(shooter, newBulletNetObj);
            }
        }
    }

    // Shoot Bullets func call
    public void GenerateBullets(int bulletTypeIndex, Gun_Base source, RpcParams rpcParams = default)
    {
        Debug.Log("Shoot Bullets started");
        GenerateBulletsRPC(bulletTypeIndex, source.NetworkObject, rpcParams);
    }

    [Rpc(SendTo.Server)]
    private void GenerateBulletsRPC(int bulletTypeIndex, NetworkObjectReference shooter, RpcParams rpcParams = default)
    {
        Debug.Log("sent to server, Generate Bullets started");

        // Client that called this RPC. Used to send bullet info back to that client specifically
        var clientID = rpcParams.Receive.SenderClientId;

        // grab bullet prefab from the list of bullets
        Bullet bulletPrefab = bulletList.listBullets[bulletTypeIndex];

        // Get the shooter actual from the network reference
        shooter.TryGet(out NetworkObject player);
        // Get the gunbase of that shooter
        Gun_Base gunBase = player.GetComponentInChildren<Gun_Base>();

        // Create new bullet
        GameObject newBullet = Instantiate(bulletPrefab.gameObject);
        newBullet.transform.position = gunBase.attackPoint.position;
        newBullet.SetActive(false);

        // Get Network Object bullshit
        NetworkObject newBulletNetObj = newBullet.GetComponent<NetworkObject>();
        newBulletNetObj.Spawn(true); // bool for destroy when owner is destroyed
        newBulletNetObj.transform.position = gunBase.attackPoint.position;

        // Check if client that created bullet is connected, then tell them stuff
        if (NetworkManager.ConnectedClients.ContainsKey(clientID))
        {
            var client = NetworkManager.ConnectedClients[clientID];
            // client.PlayerObject;
            AddToBulletListRPC(shooter, newBulletNetObj);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddToBulletListRPC(NetworkObjectReference shooter, NetworkObjectReference bulletToAdd)
    {
        // Get the shooter actual from the network reference
        shooter.TryGet(out NetworkObject player);
        // Get the gunbase of that shooter
        Gun_Base gunBase = player.GetComponentInChildren<Gun_Base>();

        // Get the bullet actual from the network reference
        bulletToAdd.TryGet(out NetworkObject bTA);
        GameObject bullet = bTA.gameObject;

        // Add to bullet pool on player
        gunBase.AddBulletToBulletPool(bullet);
    }

    public void DeactivateBullet(Bullet bullet)
    {
        DeactivateBulletRPC(bullet.NetworkObject);
    }

    [Rpc(SendTo.Everyone)]
    private void DeactivateBulletRPC(NetworkObjectReference bulletNetObjRef)
    {
        // Get the bullet actual from the network reference
        bulletNetObjRef.TryGet(out NetworkObject bTA);
        GameObject bullet = bTA.gameObject;

        bullet.SetActive(false);
    }
}
