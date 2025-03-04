using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerHandler : NetworkBehaviour
{
    public static MultiplayerHandler Instance { get; private set; }

    //private NetworkVariable<List<GameObject>> bulletPool;

    [SerializeField] private BulletList bulletList;

    [SerializeField] private GameObject prefab;

    private void Start()
    {
        // NetworkManager.Singleton.OnClientStarted += SpawnBulletsStart;
    }

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
                GenerateBulletsRPC(bulletTypeIndex, shooter, rpcParams);

                //// Create new bullet
                //GameObject newBullet = Instantiate(bulletPrefab.gameObject);
                ////newBullet.SetActive(false);

                //// Get Network Object bullshit
                //NetworkObject newBulletNetObj = newBullet.GetComponent<NetworkObject>();
                //newBulletNetObj.Spawn(true); // bool for destroy when owner is destroyed

                //// Add bullet to bullet list
                //AddToBulletListRPC(shooter, newBulletNetObj, rpcParams);
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
    private void GenerateBulletsRPC(int bulletTypeIndex, NetworkObjectReference shooter, RpcParams rpcParams)
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
        //newBullet.transform.position = gunBase.attackPoint.position;
        newBullet.SetActive(false);

        // Get Network Object bullshit
        NetworkObject newBulletNetObj = newBullet.GetComponent<NetworkObject>();
        newBulletNetObj.Spawn(true); // bool for destroy when owner is destroyed
        newBulletNetObj.gameObject.SetActive(false);
        newBulletNetObj.transform.position = gunBase.attackPoint.position;

        // Check if client that created bullet is connected, then tell them stuff
        if (NetworkManager.ConnectedClients.ContainsKey(clientID))
        {
            var client = NetworkManager.ConnectedClients[clientID];
            // client.PlayerObject;
            AddToBulletListRPC(shooter, newBulletNetObj, rpcParams);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddToBulletListRPC(NetworkObjectReference shooter, NetworkObjectReference bulletToAdd, RpcParams rpcParams)
    {
        if (OwnerClientId != rpcParams.Receive.SenderClientId)
            return;

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

    [Rpc(SendTo.Server)]
    public void ServerChangeBulletTransform_RPC(NetworkObjectReference bulletNetObjRef, Vector3 directionWithSpread, NetworkObjectReference shooterNetRef, RpcParams rpcParams = default)
    {
        Debug.Log("ServerChangeBulletTransformRPC started sending change bullet to server");

        //bulletNetObjRef.TryGet(out NetworkObject bulletNetObj);
        //Bullet bullet = bulletNetObj.GetComponent<Bullet>();

        //shooterNetRef.TryGet(out NetworkObject shooterNetObj);
        //Gun_Base shooterGB = shooterNetObj.GetComponentInChildren<Gun_Base>();

        //bullet.transform.position = shooterGB.attackPoint.position;
        //bullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);
        //bullet.gameObject.SetActive(true);

        //Rigidbody rb = bullet.GetComponent<Rigidbody>();
        //rb.linearVelocity = directionWithSpread.normalized * shooterGB.shootForce + shooterGB.fpsCam.transform.up * shooterGB.upwardForce;

        EveryoneChangeBulletTransform_RPC(bulletNetObjRef, directionWithSpread, shooterNetRef);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EveryoneChangeBulletTransform_RPC(NetworkObjectReference bulletNetObjRef, Vector3 directionWithSpread, NetworkObjectReference shooterNetRef)
    {
        Debug.Log("EveryoneChangeBulletTransformRPC started sending change bullet to everyone");

        NetworkObject netObj =
           NetworkObjectPool.Singleton.GetNetworkObject(prefab, new Vector3(0, 3, 0), Quaternion.identity);

        Bullet bullet = netObj.GetComponent<Bullet>();

        bullet.prefab = prefab;
        if (!netObj.IsSpawned) netObj.Spawn(true);
        bullet.Invoke("Deactivate", bullet.lifetime);


        shooterNetRef.TryGet(out NetworkObject shooterNetObj);
        Gun_Base shooterGB = shooterNetObj.GetComponentInChildren<Gun_Base>();

        bullet.gameObject.SetActive(true);
        bullet.transform.position = shooterGB.attackPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = directionWithSpread.normalized * shooterGB.shootForce + shooterGB.fpsCam.transform.up * shooterGB.upwardForce;
    }

    /// <summary>
    /// preload the bullet pool
    /// </summary>
    public void SpawnBulletsStart()
    {
        NetworkObjectPool.Singleton.OnNetworkSpawn();
    }

    // Actual creation of bullets
    private void SpawnBullets(int bulletTypeIndex, NetworkObjectReference shooterNetRef, Vector3 directionWithSpread)
    {
        // Bullet bulletPrefab = bulletList.listBullets[bulletTypeIndex];

        // Get shooter information
        shooterNetRef.TryGet(out NetworkObject shooterNetObj);
        Gun_Base shooterGB = shooterNetObj.GetComponentInChildren<Gun_Base>();

        // Get bullet from pool
        NetworkObject netObj =
            NetworkObjectPool.Singleton.GetNetworkObject(prefab, shooterGB.attackPoint.position, Quaternion.identity);

        // Get bullet actual from network object reference
        Bullet bullet = netObj.GetComponent<Bullet>();

        // prepare bullet to be returned to pool realistically prefab can be set in bullet this is for prefab scalability
        bullet.prefab = prefab;

        Gun_Piece_Base bulletPart = new Apple_Part();
        Gun_Piece_Base damagePart = new Apple_Part();

        // Get bullet type from activeGunPieceForBullet enum
        switch (shooterGB.activeGunPieceForBullet.Value)
        {
            case 0:
                bulletPart = new Bottle_Part();
                break;
            case 1:
                bulletPart = new Eggs_Part();
                break;
            case 2:
                bulletPart = new Corn_Part();
                break;
            case 3:
                bulletPart = new DrumStick_Part();
                break;
            case 4:
                bulletPart = new Apple_Part();
                break;
            default:
                break;
        }
      

        // Change bullet to what it should be based on shooters gun type
        bullet.GetComponent<Bullet>().SetNewType(bulletPart, damagePart);

        // damage mult from shooters gun
        bullet.GetComponent<Bullet>().damageMult = shooterGB.DamageMuliplayer;

        // Spawn bullet on the network only if its not alr there
        if (!netObj.IsSpawned) netObj.Spawn(true);

        // Change how bullet flys
        bullet.transform.position = shooterGB.attackPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);
        bullet.gameObject.SetActive(true);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = directionWithSpread.normalized * shooterGB.shootForce + shooterGB.fpsCam.transform.up * shooterGB.upwardForce;

        //// Affect how bullet flys
        //bullet.transform.position = shooterGB.attackPoint.position;
        //bullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);

        //// spawn the bullet 
        //if (!netObj.IsSpawned) netObj.Spawn(true);
        //bullet.Invoke("Deactivate", bullet.lifetime); // start despawn countdown

        //// Apply force to bullet
        //Rigidbody rb = bullet.GetComponent<Rigidbody>();
        //rb.linearVelocity = directionWithSpread.normalized * shooterGB.shootForce + shooterGB.fpsCam.transform.up * shooterGB.upwardForce;
    }

    // allows clients to spawn bullets on server
    [Rpc(SendTo.Server)]
    public void SpawnBullets_RPC(int bulletTypeIndex, NetworkObjectReference shooterNetRef, Vector3 directionWithSpread)
    {
        SpawnBullets(bulletTypeIndex, shooterNetRef, directionWithSpread);
    }

    public void PoolReturn(Bullet bullet, int bulletPrefabIndex)
    {
        ReturnToPool_RPC(bullet.NetworkObject, bulletPrefabIndex);
    }

    [Rpc(SendTo.Server)]
    public void ReturnToPool_RPC(NetworkObjectReference bulletNetObjRef, int bulletPrefabIndex)
    {
        // Get bullet information
        bulletNetObjRef.TryGet(out NetworkObject bulletNetObj);
        //bulletNetObj.GetComponent<GameObject>().SetActive(false);

        // return to pool
        if (bulletNetObj.IsSpawned)
            NetworkObjectPool.Singleton.ReturnNetworkObject(bulletNetObj, bulletList.listBullets[bulletPrefabIndex].GetComponent<GameObject>());
        //bulletNetObj.Despawn();
    }

    [Rpc(SendTo.Server)]
    public void SetBulletFalse_RPC(NetworkObjectReference bulletNetObjRef)
    {
        //bulletNetObjRef.TryGet(out NetworkObject bulletNetObj);
        //bulletNetObj.GetComponent<Bullet>().gameObject.SetActive(false);
        SetBulletFalseEveryone_RPC(bulletNetObjRef);
    }

    [Rpc(SendTo.Everyone)]
    public void SetBulletFalseEveryone_RPC(NetworkObjectReference bulletNetObjRef)
    {
        bulletNetObjRef.TryGet(out NetworkObject bulletNetObj);
        bulletNetObj.GetComponent<Bullet>().gameObject.SetActive(false);
    }
    public void GetHostSeed()
    {
        GetHostSeedRPC();
    }

    [Rpc(SendTo.Server)]
    private void GetHostSeedRPC()
    {
        SpreadHostSeedRPC(SeedGenManager.seed);
    }

    [Rpc(SendTo.NotServer)]
    private void SpreadHostSeedRPC(int hostSeed)
    {
        SeedGenManager.seed = hostSeed;
    }

    public void Restock()
    {

    }
}
