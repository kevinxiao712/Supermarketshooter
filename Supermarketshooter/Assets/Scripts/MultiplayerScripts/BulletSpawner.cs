using UnityEngine;
using Unity.Netcode;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnBulletsStart;
    }

   private void SpawnBulletsStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnBulletsStart;
        NetworkObjectPool.Singleton.OnNetworkSpawn();
        for(int i = 0; i < 30; ++i)
        {
            SpawnBullets();
        }
    }

    private void SpawnBullets()
    {
       // NetworkObject netObj = NetworkObjectPool.Singleton.GetNetworkObject(prefab,)
    }
}
