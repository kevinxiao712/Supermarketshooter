using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Base Health")]
    public int baseMaxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    public Slider healthSlider;

    // The time delay before the player respawns after dying
    public float respawnDelay = 2f;

    private Transform[] respawnPoints;
    private bool isRespawning = false;

    [Header("Extra Health & Regen Settings")]
    public bool hasExtraHealthAndRegen = false; // One toggle for both +50 HP and regen
    private int extraHealthAmount = 50;         // Additional HP for the player
    private int regenAmountPerSecond = 5;       // HP regenerated per second
    private int finalMaxHealth;                 // Effective max health (base or base+extra)

    private Playermovement playerMovement;
    private Rigidbody rb;

    void Start()
    {
        // Decide final max health at game start
        finalMaxHealth = hasExtraHealthAndRegen
            ? (baseMaxHealth + extraHealthAmount)
            : baseMaxHealth;

        currentHealth = finalMaxHealth;

        // Set up references
        playerMovement = GetComponent<Playermovement>();
        rb = GetComponent<Rigidbody>();

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
            healthSlider.maxValue = finalMaxHealth;
            healthSlider.value = currentHealth;
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

        // Start the regeneration coroutine
        StartCoroutine(HealthRegenCoroutine());
    }

    public void EnableExtraHealthAndRegen()
    {
        if (!hasExtraHealthAndRegen)
        {
            hasExtraHealthAndRegen = true;

            // Recalculate final max HP
            finalMaxHealth = baseMaxHealth + extraHealthAmount;

            // Clamp current health to the new final max
            currentHealth = Mathf.Clamp(currentHealth, 0, finalMaxHealth);

            // Update the slider if we have one
            if (healthSlider != null)
            {
                healthSlider.maxValue = finalMaxHealth;
                healthSlider.value = currentHealth;
            }
        }
    }


    public void DisableExtraHealthAndRegen()
    {
        if (hasExtraHealthAndRegen)
        {
            hasExtraHealthAndRegen = false;

            finalMaxHealth = baseMaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, finalMaxHealth);

            if (healthSlider != null)
            {
                healthSlider.maxValue = finalMaxHealth;
                healthSlider.value = currentHealth;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isRespawning) return; // Prevent taking damage while respawning
        if (!IsOwner) return; // only do this if it is called by the owner

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
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

        // Use finalMaxHealth instead of base
        currentHealth = Mathf.Min(currentHealth + amount, finalMaxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("Player healed, current health: " + currentHealth);
    }

    private IEnumerator Respawn()
    {
        isRespawning = true;
        Debug.Log("Player has died. Respawning in " + respawnDelay + " seconds...");

        // Disable movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Freeze physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return new WaitForSeconds(respawnDelay);

        // If you have multiple respawn points, pick one at random
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

        // Restore health to finalMaxHealth
        currentHealth = finalMaxHealth;

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // Re-enable movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Unfreeze physics
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        isRespawning = false;
    }

    private IEnumerator HealthRegenCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (hasExtraHealthAndRegen && !isRespawning && currentHealth < finalMaxHealth)
            {
                Heal(regenAmountPerSecond);
            }
        }
    }
}
