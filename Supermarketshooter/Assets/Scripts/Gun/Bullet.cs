using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public float lifetime = 8f;
    public Rigidbody rb;
    public int damage;
    public Gun_Base gun;
    Collider collider;
    public PhysicsMaterial normal;
    public PhysicsMaterial bouncy;
     Vector3 originalSize;
     Vector3 bigSize;
    public int damageMult = 1;
    public GameObject prefab;


    /// <summary>
    // Explosive Data
    /// </summary>
    /// 
    public bool isExplosive = false;
    [SerializeField]
    private ParticleSystem ParticleSystemPrefab;
    public int maxHits = 25;
    public float radius = 10f;
    public LayerMask hitLayer;
    public LayerMask blockExplosionLayer;
    public int maxDamage = 50;
    public int minDamage = 10;
    public float explosiveForce;
    private Collider[] hits;
    private bool butterBullet;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        bigSize = new Vector3(0.519999981f, 0.346014768f, 0.519999981f);
        originalSize = new Vector3(0.102316171f, 0.0680825114f, 0.102316171f);
        hits = new Collider[maxHits];
    }
    void OnEnable()
    {
    }

   

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        if (isExplosive)
        {
            Explode();
        }
      
        // 2) Check if the collided object has a PlayerHealth script
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // 3) Inflict damage on the player
            playerHealth.TakeDamage(damage * damageMult);
            if(butterBullet)
            {
                collision.gameObject.GetComponent<Playermovement>().GotButtered();
            }
        }

        // 4) Deactivate the bullet (whether it hits a player or something else)
            Deactivate();
    }
    public void SetNewType(Gun_Piece_Base part)
    {
        isExplosive = false;
        gameObject.layer = 10;
        rb.useGravity = true;
        collider.material = normal;
        transform.localScale = originalSize;
        
        switch (part)
        {
            case Bottle_Part:
                rb.useGravity = false;
                transform.localScale = bigSize;
                gameObject.layer = 9;

                break;
            case Eggs_Part:
                isExplosive = true;
                break;
            case Corn_Part:
                break;
            case DrumStick_Part:
                gameObject.layer = 7;
                rb.useGravity = false;
                break;
            case Apple_Part:
                collider.material = bouncy;

                break;
            default:
                break;
        }
    }

    void Deactivate()
    {
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
    }
    public void Explode()
    {
        hits = new Collider[maxHits];

        int currentHits = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, hitLayer);
        Instantiate(ParticleSystemPrefab, transform.position, Quaternion.identity);

        for (int i = 0; i < currentHits; i++)
        {
            if(hits[i].gameObject.GetComponent<PlayerHealth>())
            {
                float distance = Vector3.Distance(transform.position, hits[i].transform.position);

                if (!Physics.Raycast(transform.position, (hits[i].transform.position - transform.position).normalized, distance, blockExplosionLayer.value))
                {
                    hits[i].gameObject.GetComponent<PlayerHealth>().TakeDamage(Mathf.FloorToInt(Mathf.Lerp(maxDamage, minDamage, distance / radius)));
                }
            }
        }
    }

   
}
