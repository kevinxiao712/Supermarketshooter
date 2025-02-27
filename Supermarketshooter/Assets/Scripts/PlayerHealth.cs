using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Maximum health for the player.
    public int maxHealth = 100;
    // Current health, set at runtime.
    public int currentHealth;

    // Initialize current health at the start.

    public Transform[] respawnPoints;
    void Start()
    {
        currentHealth = maxHealth;
    }
    void Awake()
    {
        // If no respawn points have been assigned manually, search for them by tag.
        if (respawnPoints == null || respawnPoints.Length == 0)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
            respawnPoints = new Transform[spawnPoints.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                respawnPoints[i] = spawnPoints[i].transform;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player took damage, current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        Debug.Log("Player healed, current health: " + currentHealth);
    }

    private void Die()
    {
        Debug.Log("Player has died.");
    }

    private void Respawn()
    {
        if (respawnPoints != null && respawnPoints.Length > 0)
        {
            int index = Random.Range(0, respawnPoints.Length);
            Transform respawnPoint = respawnPoints[index];
            transform.position = respawnPoint.position;
            Debug.Log("Player respawned at respawn point: " + index);
        }
        else
        {
            Debug.LogWarning("No respawn points assigned!");
        }
        // Reset the player's health after respawning.
        currentHealth = maxHealth;
    }
}
