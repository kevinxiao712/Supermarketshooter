using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEditor;
public class Gun_Base : MonoBehaviour
{
    // Bullet settings
    public GameObject bulletPrefab;
    public float shootForce, upwardForce;

    // Gun settings
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    // Internal tracking variables
    private int bulletsLeft, bulletsShot;
    private bool shooting, readyToShoot, reloading;
    private List<GameObject> bulletPool = new List<GameObject>();

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
    private List<Gun_Piece_Base> collectedGunPieces = new List<Gun_Piece_Base>();
    private List<GameObject> collectedGunPiecesObject = new List<GameObject>();
    public List<Transform> gunPartPositions;
    private int maxGunPieces = 3;
    public Gun_Piece_Base hoveredPart;
    Material oldMaterial;
    public Material targetedMaterial;
    private void Awake()
    {
        // Initialize bullets and set gun to ready state
        bulletsLeft = magazineSize;
        readyToShoot = true;
        PreloadBullets();
    }

    private void Update()
    {
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
        shooting = allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        // Handle reloading input
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        // Handle shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
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

        // Retrieve a bullet from the pool
        GameObject bullet = GetBullet();
        bullet.transform.position = attackPoint.position;
        bullet.transform.rotation = Quaternion.LookRotation(directionWithSpread);
        bullet.SetActive(true);

        // Apply force to bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = directionWithSpread.normalized * shootForce + fpsCam.transform.up * upwardForce;

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

    private void Reload()
    {
        // Start reload sequence
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        // Refill magazine
        bulletsLeft = magazineSize;
        reloading = false;
    }

    private void PreloadBullets()
    {
        // Create a pool of bullets
        for (int i = 0; i < magazineSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    private GameObject GetBullet()
    {
        // Retrieve an inactive bullet from the pool
        foreach (GameObject bullet in bulletPool)
        {
            if (!bullet.activeInHierarchy)
                return bullet;
        }

        // If all bullets are in use, create a new one
        GameObject newBullet = Instantiate(bulletPrefab);
        bulletPool.Add(newBullet);
        return newBullet;
    }

    public void PickUpPart (GameObject gun_Piece)
    {
        if(collectedGunPieces.Count==3)
        {
            PopGunPart();
        }
        AddPart(gun_Piece);
    }
    public void PopGunPart()
    {
        collectedGunPieces.RemoveAt(0);
        GameObject gunOBJ = collectedGunPiecesObject[0];
        collectedGunPiecesObject.RemoveAt(0);
        Destroy(gunOBJ);

    }
    public void AddPart(GameObject gun_Piece)
    {
        collectedGunPiecesObject.Add(gun_Piece);
        collectedGunPieces.Add(gun_Piece.GetComponent<Gun_Piece_Base>());

        //setUpvisuals
        for (int i = 0; i < collectedGunPiecesObject.Count; i++)
        {
            collectedGunPiecesObject[i].transform.parent = gunPartPositions[i];
            collectedGunPiecesObject[i].transform.position = gunPartPositions[i].position;
            collectedGunPiecesObject[i].transform.rotation = gunPartPositions[i].rotation;

            //SetData
            collectedGunPieces[i].gun = this;
           collectedGunPieces[i].UpdateState(i);

        }
    }

   
}
