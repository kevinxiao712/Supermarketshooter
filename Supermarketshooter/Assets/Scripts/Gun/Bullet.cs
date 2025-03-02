using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float lifetime = 3f;
    void OnEnable()
    {
        Invoke("Deactivate", lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<Playermovement>())
        Deactivate();
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
