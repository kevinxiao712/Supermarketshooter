using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float lifetime = 3f;
    public GameObject prefab;

    // does not work with Networked pool
    //void OnEnable()
    //{
    //    Invoke("Deactivate", lifetime);
    //}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Playermovement>())
        {
            // Deactivate();
            Debug.Log("hit!");
        }
    }

    void Deactivate()
    {
        //MultiplayerHandler.Instance.DeactivateBullet(this);
        //gameObject.SetActive(false);

        // if already returned to pool
        if (!NetworkObject.IsSpawned) return;
        // return it to the pool
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
    }
}
