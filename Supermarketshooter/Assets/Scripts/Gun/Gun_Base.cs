using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEditor;
using Unity.Netcode;

public class Gun_Base : NetworkBehaviour
{
    // Bullet settings
    public GameObject bulletPrefab;
    public float shootForce, upwardForce;

    // Gun settings
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    public bool isFiringBullets = true;
    public float damage;

    // Internal tracking variables
    private int bulletsLeft, bulletsShot;
    private bool shooting, readyToShoot;
    private List<GameObject> bulletPool = new List<GameObject>();
    public bool PopingOut;
    // Recoil settings
    public Rigidbody playerRb;
    public float recoilForce;

    // References
    public Camera fpsCam;
    public Transform attackPoint;
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;
    public bool allowInvoke = true;

    // Pickup system
    public NetworkVariable<int> activeGunPieceForBullet = new NetworkVariable<int>(0); // for Networking
    public List<Gun_Piece_Base> activeGunPieces = new List<Gun_Piece_Base>();
    private List<GameObject> activeGunPiecesObject = new List<GameObject>();
    //holders;
    private List<Gun_Piece_Base> collectedGunPieces = new List<Gun_Piece_Base>();
    private List<GameObject> collectedGunPiecesObject = new List<GameObject>();
    public List<Transform> gunPartPositions;
    private int maxGunPieces = 3;
    public Gun_Piece_Base hoveredPart;
    Material oldMaterial;
    public Material targetedMaterial;

    // Multiplayer fields
    [SerializeField] private BulletList bulletList;

    //References
    public Playermovement playermovement;
    public GameObject playerObject;
    public PlayerHealth playerHealth;
    //gun Passives
    [HideInInspector]
     public int DamageMuliplayer = 1;


    private void Awake()
    {
        // Initialize bullets and set gun to ready state
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    public void Start()
    {
        if (!IsOwner) return;
        // PreloadBullets();
        transform.parent = fpsCam.transform;
    }

    private void Update()
    {
        // Do nothing if the player giving input is not the active player
        if (!IsOwner) return;

        HandleInput();
        HandlePickup();
        HighlightPartOnHover();
        // Update ammo UI if available
        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
    }
    public void HighlightPartOnHover()
    {
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.GetComponent<Gun_Piece_Base>() != null)
            {
                if (hoveredPart == null)
                {
                    hoveredPart = hit.collider.GetComponent<Gun_Piece_Base>();
                    oldMaterial = hoveredPart.gameObject.GetComponent<MeshRenderer>().material;
                    hoveredPart.gameObject.GetComponent<MeshRenderer>().material = targetedMaterial;
                }
            }
            else if (hoveredPart != null)
            {
                hoveredPart.gameObject.GetComponent<MeshRenderer>().material = oldMaterial;
                hoveredPart = null;
            }
        }
    }

    private void HandleInput()
    {
        // Check for shooting input
        if (activeGunPieces.Count < 3)
        {
            Debug.Log("less then 3"); 
            return;
        }

        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);
        
        // Handle reloading input
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !PopingOut) ReloadPopOut();
        if (readyToShoot && shooting && !PopingOut && bulletsLeft <= 0) ReloadPopOut();

        // Handle shooting
        if (readyToShoot && shooting && !PopingOut && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    private void HandlePickup()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Press E to pick up a gun piece
        {
            Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Gun_Piece_Base gunPiece = hit.collider.GetComponent<Gun_Piece_Base>();
                if (gunPiece != null)
                {
                    PickUpPart(gunPiece.gameObject);
                }
            }
        }
    }


    private void Shoot()
    {
        readyToShoot = false;

        // Calculate bullet direction with spread
        Vector3 directionWithSpread = CalculateSpreadDirection();

        if (isFiringBullets)
        { // Retrieve a bullet from the pool

            MultiplayerHandler.Instance.SpawnBullets_RPC(0, NetworkObject, directionWithSpread);

            //GameObject bullet = GetBullet();
            //bullet.GetComponent<Bullet>().damageMult = DamageMuliplayer;
            //ServerChangeBulletTransformRPC(bullet.GetComponent<NetworkObject>(), directionWithSpread);
        }


        // Instantiate muzzle flash if available
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        // Reduce ammo count
        bulletsLeft--;
        bulletsShot++;

        // Handle recoil and shot reset
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
            playerRb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
        }

        // Handle burst shots
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }

    [Rpc(SendTo.Server)]
    private void ServerChangeBulletTransformRPC(NetworkObjectReference bulletNetObjRef, Vector3 directionWithSpread)
    {
        EveryoneChangeBulletTransformRPC(bulletNetObjRef, directionWithSpread);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EveryoneChangeBulletTransformRPC(NetworkObjectReference bulletNetObjRef, Vector3 directionWithSpread)
    {
        bulletNetObjRef.TryGet(out NetworkObject bulletNetObj);
        Bullet bullet = bulletNetObj.GetComponent<Bullet>();

        bullet.transform.position = attackPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);
        bullet.gameObject.SetActive(true);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = directionWithSpread.normalized * shootForce + fpsCam.transform.up * upwardForce;
    }

    private Vector3 CalculateSpreadDirection()
    {
        // Determine base bullet trajectory
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : ray.GetPoint(75);
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        // Apply random spread
        return directionWithoutSpread + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0);
    }

    private void ResetShot()
    {
        // Allow shooting again
        readyToShoot = true;
        allowInvoke = true;
    }

    private void ReloadPopOut()
    {
        // Start reload sequence
        PopingOut = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        if(collectedGunPieces.Count>0)
        {
            PopGunPart();
            AddPart(collectedGunPiecesObject[0]);
            collectedGunPiecesObject.RemoveAt(0);
            collectedGunPieces.RemoveAt(0);
            bulletsLeft = magazineSize;
        }
        else
        {
            PopGunPart();
            UpdateGunParts();
        }
        PopingOut = false;

    }

    private void PreloadBullets()
    {
        // All object spawning happens server side
        MultiplayerHandler.Instance.PreSpawnBullets(magazineSize, 0, this);
    }

    private GameObject GetBullet()
    {
        // Retrieve an inactive bullet from the pool
        foreach (GameObject bullet in bulletPool)
        {
            if (!bullet.activeInHierarchy)
            {
                Debug.Log("Bullet Found!");
                return bullet;
            }
        }

        Debug.Log("Generating new bullet...");
        // Generate new bullet
        MultiplayerHandler.Instance.GenerateBullets(0, this);
        UpdateBullet(bulletPool[bulletPool.Count - 1]);
        foreach (GameObject bullet in bulletPool)
        {
            bullet.GetComponent<Bullet>().gun = this;
        }
        // Return the newest bullet generated
        return bulletPool[bulletPool.Count - 1];

        //// If all bullets are in use, create a new one
        //GameObject newBullet = Instantiate(bulletPrefab);
        //bulletPool.Add(newBullet);
        //return newBullet;
    }

    // Allows the server to create the bullets but still save the bullets to the client
    public void AddBulletToBulletPool(GameObject bulletToAdd)
    {
        bulletPool.Add(bulletToAdd);
    }

    public void PickUpPart(GameObject gun_Piece)
    {
        if (activeGunPieces.Count == 3)
        {
            collectedGunPiecesObject.Add(gun_Piece);
            collectedGunPieces.Add(gun_Piece.GetComponent<Gun_Piece_Base>());
            gun_Piece.SetActive(false);
        }
        else
        {
            AddPart(gun_Piece);
            UpdateAllBullets();
        }
    }
    public void PopGunPart()
    {
        activeGunPieces.RemoveAt(0);
        GameObject gunOBJ = activeGunPiecesObject[0];
        activeGunPiecesObject.RemoveAt(0);
        Destroy(gunOBJ);

    }
    public void AddPart(GameObject gun_Piece)
    {
        gun_Piece.SetActive(true);
        activeGunPiecesObject.Add(gun_Piece);
        activeGunPieces.Add(gun_Piece.GetComponent<Gun_Piece_Base>());
        UpdateGunParts();
        // Set networked var for shooting type
        switch (activeGunPieces[0])
        {
            case Bottle_Part:
                activeGunPieceForBullet.Value = 0;
                break;
            case Eggs_Part:
                activeGunPieceForBullet.Value = 1;
                break;
            case Corn_Part:
                activeGunPieceForBullet.Value = 2;
                break;
            case DrumStick_Part:
                activeGunPieceForBullet.Value = 3;
                break;
            case Apple_Part:
                activeGunPieceForBullet.Value = 4;
                break;
            default:
                break;
        }
    }

    public void UpdateGunParts()
    {
        //setUpvisuals
        for (int i = 0; i < activeGunPiecesObject.Count; i++)
        {
            activeGunPiecesObject[i].transform.parent = gunPartPositions[i];
            activeGunPiecesObject[i].transform.position = gunPartPositions[i].position;
            activeGunPiecesObject[i].transform.rotation = gunPartPositions[i].rotation;

            //SetData
            activeGunPieces[i].gun = this;
            activeGunPieces[i].UpdateState(i);
        }
        if(activeGunPieces.Count==3)
        {
            bulletsLeft = magazineSize;
            UpdateAllBullets();
        }
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    public void UpdateAllBullets()
    {
        if (activeGunPieces[0] != null)
        {
            foreach (GameObject bullet in bulletPool)
            {
                bullet.GetComponent<Bullet>().SetNewType(activeGunPieces[0]);
            }
        }
    }

    public void UpdateBullet(GameObject bullet)
    {
      bullet.GetComponent<Bullet>().SetNewType(activeGunPieces[0]);
    }
}
