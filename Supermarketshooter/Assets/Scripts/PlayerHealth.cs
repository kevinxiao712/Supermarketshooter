using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    public Slider healthSlider;


    [Header("Randomized Objects")]
    public GameObject[] randomObjects;



    private PlayerHealth playerHealth; 

    public float respawnDelay = 2f; // Delay before respawning

    private Transform[] respawnPoints;
    private bool isRespawning = false;

    [Header("Death Text")]
    public GameObject deadText;

    private Playermovement playerMovement;
    private Rigidbody rb;
    void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<Playermovement>();
        rb = GetComponent<Rigidbody>();

        if (deadText != null)
        {
            deadText.SetActive(false);
        }
        if (healthSlider == null)
        {
            GameObject sliderObj = GameObject.FindGameObjectWithTag("Health");
            if (sliderObj != null)
            {
                healthSlider = sliderObj.GetComponent<Slider>();
            }
        }
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        if (randomObjects != null && randomObjects.Length > 0)
        {
            // Disable all first (so only one ends up active)
            foreach (GameObject obj in randomObjects)
            {
                obj.SetActive(false);
            }

            // Pick one index from 0 to randomObjects.Length - 1
            int randomIndex = Random.Range(0, randomObjects.Length);
            randomObjects[randomIndex].SetActive(true);
        }
        else
        {
            Debug.LogWarning("No randomObjects assigned or array is empty!");
        }




        // Find all respawn points dynamically
        GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
        respawnPoints = new Transform[respawnObjects.Length];

        for (int i = 0; i < respawnObjects.Length; i++)
        {
            respawnPoints[i] = respawnObjects[i].transform;
        }

        if (respawnPoints.Length == 0)
        {
            Debug.LogWarning("No respawn points found in the scene!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsOwner) return;
        if (isRespawning) return; // Prevent taking damage while respawning

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
        Debug.Log("Player took damage, current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(Respawn());
        }

    }

    public void Heal(int amount)
    {
        if (isRespawning) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
          
        }
        Debug.Log("Player healed, current health: " + currentHealth);
    }

    private IEnumerator Respawn()
    {
        isRespawning = true;


        if (deadText != null)
        {
            deadText.SetActive(true);
        }
        Debug.Log("Player has died. Respawning in " + respawnDelay + " seconds...");
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true; // Stops physics interactions during respawn
        }

        yield return new WaitForSeconds(respawnDelay); // Wait before respawning

        if (respawnPoints.Length > 0)
        {
            int index = Random.Range(0, respawnPoints.Length);
            transform.position = respawnPoints[index].position;
            Debug.Log("Player respawned at respawn point: " + index);
        }
        else
        {
            Debug.LogWarning("No respawn points available!");
        }

        // Restore health and re-enable movement
        currentHealth = maxHealth;
        isRespawning = false;
        if (healthSlider != null)
        {
            healthSlider.value = 1f; // or (float) currentHealth / maxHealth
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Unfreeze player
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        deadText.SetActive(false);
    }
}
