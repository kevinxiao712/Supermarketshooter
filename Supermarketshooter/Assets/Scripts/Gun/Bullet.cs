using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float lifetime = 3f;
    public Rigidbody rb;
    public int damage;
    public Gun_Base gun;

    public PhysicsMaterial normal;
    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void OnEnable()
    {
        Invoke("Deactivate", lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<Playermovement>())
        Deactivate();
    }
    public void SetNewType(Gun_Piece_Base part)
    {
        gameObject.layer = 0;
        rb.useGravity = true;
        switch (part)
        {
            case Bottle_Part:

                break;
            case Eggs_Part:

                break;
            case Corn_Part:
                break;
            case DrumStick_Part:
                gameObject.layer = 7;
                rb.useGravity = false;

                break;
            case Apple_Part:

                break;
            default:
                break;
        }
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
