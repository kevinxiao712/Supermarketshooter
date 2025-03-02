using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SeedGenManager : MonoBehaviour
{
    /// <summary>
    /// Seed used to restock the produce.
    /// </summary>
    public static int seed;

    /// <summary>
    /// Bool for whether the player is the server host.
    /// </summary>
    public bool isHost = false;

    /// <summary>
    /// The amount of time between restocks of the store.
    /// </summary>
    public float restockTimer = 30f;

    /// <summary>
    /// List of all produce spawners in the level, which is stocked at the start of the level.
    /// </summary>
    public List<ProduceSpawner> allProduceSpawners = new List<ProduceSpawner>();

    /// <summary>
    /// List with all Produce objects.
    /// </summary>
    public ProduceSpawnData[] allSpawnData;

    private float spawnDenominator = 0;
    private float spawnNumerator = 0;

    private void Start() {
        
        // Host should be able to generate and init the seed;
        // Host should also send the seed to other players, who then also need to init the seed.
        if (isHost) {
            seed = GenerateNewSeed();
            InitializeSeed(seed);
            SendRestockSignalToAll();
            StartCoroutine(RestockTimer(5));
        }
    }

    /// <summary>
    /// Adds a spawn point to the list of all produce spawners.
    /// </summary>
    /// <param name="produceSpawner"></param>
    public void AddProduceSpawner(ProduceSpawner produceSpawner) {
        allProduceSpawners.Add(produceSpawner);
    }

    /// <summary>
    /// Generates a seed based on the system's clock, and returns that seed.
    /// </summary>
    /// <returns></returns>
    private int GenerateNewSeed() {
        int s = (int)System.DateTime.Now.Ticks;
        return s;
    }

    /// <summary>
    /// Initializes the Random class using the given seed.
    /// </summary>
    /// <param name="s"></param>
    private void InitializeSeed(int s) {
        Random.InitState(s);
    }

    /// <summary>
    /// Get the seed value.
    /// </summary>
    /// <returns></returns>
    public static int GetSeed() {
        return seed;
    }

    public void SendRestockSignalToAll() {
        // Restock self
        Restock();

        // Send Restock signal to each other player.
        // TODO: Send signal to all players!
    }

    public void Restock() {
        int i = 0;
        foreach (ProduceSpawnData spawnData in allSpawnData) {
            spawnDenominator += spawnData.probability;
            allSpawnData[i].probability = spawnDenominator;
            i++;
        }

        foreach (ProduceSpawner produceSpawner in allProduceSpawners) {
            
            spawnNumerator = Random.Range(0, spawnDenominator);
            
            foreach (ProduceSpawnData spawnData in allSpawnData) {
                if (spawnNumerator <= spawnData.probability) {
                    produceSpawner.SpawnProduce(spawnData.produceReference);
                    break;
                }
            }

        }

    }

    public IEnumerator RestockTimer(float restockTimer) {
        yield return new WaitForSeconds(restockTimer);
        SendRestockSignalToAll();
        StartCoroutine(RestockTimer(this.restockTimer));
    }
}

[System.Serializable]
public struct ProduceSpawnData {
    public GameObject produceReference;
    public float probability;
}