using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public float respawnDelay = 2f; // Delay before respawning

    private Transform[] respawnPoints;
    private bool isRespawning = false;


    private Playermovement playerMovement;
    private Rigidbody rb;
    void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<Playermovement>();
        rb = GetComponent<Rigidbody>();


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
        if (isRespawning) return; // Prevent taking damage while respawning

        currentHealth -= damage;
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
        Debug.Log("Player healed, current health: " + currentHealth);
    }

    private IEnumerator Respawn()
    {
        isRespawning = true;
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


        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Unfreeze player
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}
