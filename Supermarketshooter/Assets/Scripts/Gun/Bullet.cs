using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float lifetime = 3f;

    public int damage = 10;

    void OnEnable()
    {

        Invoke(nameof(Deactivate), lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 2) Check if the collided object has a PlayerHealth script
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // 3) Inflict damage on the player
            playerHealth.TakeDamage(damage);
        }

        // 4) Deactivate the bullet (whether it hits a player or something else)
        Deactivate();
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
