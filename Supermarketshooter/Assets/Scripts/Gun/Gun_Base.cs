using System.Collections.Generic;
using UnityEngine;

public class Gun_Base : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int magazineSize = 10;
    public float fireRate = 0.2f;

    private List<GameObject> bulletPool = new List<GameObject>();
    private float nextFireTime = 0f;

    void Start()
    {
        // Preload magazine with inactive bullets
        for (int i = 0; i < magazineSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    void Fire()
    {
        GameObject bullet = GetAvailableBullet();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.SetActive(true);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = firePoint.forward * 20f; // Adjust speed as needed
    }

    GameObject GetAvailableBullet()
    {
        foreach (GameObject bullet in bulletPool)
        {
            if (!bullet.activeInHierarchy)
            {
                return bullet;
            }
        }

        // If no bullets available, create a new one
        GameObject newBullet = Instantiate(bulletPrefab);
        bulletPool.Add(newBullet);
        return newBullet;
    }
}

}
