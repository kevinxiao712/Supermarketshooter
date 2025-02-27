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


    public void PreSpawnBullets(int magazineSize, int bulletTypeIndex)
    {
        PreSpawnBulletsServerRPC(magazineSize, bulletTypeIndex);
    }


    [ServerRpc(RequireOwnership = false)]
    private void PreSpawnBulletsServerRPC(int magazineSize, int bulletTypeIndex, ServerRpcParams serverRpcParams = default)
    {
        // Client that called this RPC. Used to send bullet info back to that client specifically
        var clientID = serverRpcParams.Receive.SenderClientId;

        Bullet bulletPrefab = bulletList.listBullets[bulletTypeIndex];

        // Create a pool of bullets
        for (int i = 0; i < magazineSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab.gameObject);

            NetworkObject bulletNetObj = bullet.GetComponent<NetworkObject>();
            bulletNetObj.Spawn(true);

            bullet.SetActive(false);

            if (NetworkManager.ConnectedClients.ContainsKey(clientID))
            {
                // Check if client is connected then tell them stuff
                var client = NetworkManager.ConnectedClients[clientID];
            }
            //bulletPool.Add(bullet);
        }
    }

    // Shoot Bullets func call
    public void ShootBullets(int bulletTypeIndex, Gun_Base source, RpcParams rpcParams = default)
    {
        Debug.Log("Shoot Bullets started");
        Debug.Log(source.NetworkObjectId);
        ShootBulletsRPC(bulletTypeIndex, source.NetworkObject, rpcParams);
    }

    [Rpc(SendTo.Server)]
    public void ShootBulletsRPC(int bulletTypeIndex, NetworkObjectReference shooter, RpcParams rpcParams = default)
    {
        Debug.Log("Server rpc ShootBullets started");

        // Client that called this RPC. Used to send bullet info back to that client specifically
        var clientID = rpcParams.Receive.SenderClientId;

        // grab bullet prefab from the list of bullets
        Bullet bulletPrefab = bulletList.listBullets[bulletTypeIndex];

        // Create new bullet
        GameObject newBullet = Instantiate(bulletPrefab.gameObject);

        // Get Network Object bullshit
        NetworkObject newBulletNetObj = newBullet.GetComponent<NetworkObject>();
        newBulletNetObj.Spawn(true); // bool for destroy when owner is destroyed

        // Check if client that created bullet is connected, then tell them stuff
        if (NetworkManager.ConnectedClients.ContainsKey(clientID))
        {
            var client = NetworkManager.ConnectedClients[clientID];
            // client.PlayerObject;
            AddToBulletListRPC(shooter, newBulletNetObj);
        }
        //bulletPool.Add(newBullet);
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void AddToBulletListRPC(NetworkObjectReference shooter, NetworkObjectReference bulletToAdd)
    {
        if (IsClient)
        {
            Debug.Log("IsCLient pass");
            Debug.Log(shooter.NetworkObjectId);
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
    }
}
