using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class SeedGenManager : NetworkBehaviour
{
    /// <summary>
    /// Seed used to restock the produce.
    /// </summary>
    public static int seed;
    private NetworkVariable<int> netSeed = new NetworkVariable<int>(0);

    /// <summary>
    /// Bool for whether the player is the server host.
    /// </summary>
    public bool isThisHost = false;

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
        // Seed gen on start and the seed is then set for each other player afterwords
            seed = GenerateNewSeed();
            InitializeSeed(seed);
            SendRestockSignalToAll();
            StartCoroutine(RestockTimer(5));
    }

    /// <summary>
    /// All players will call this upon starting which will set the seed to the host
    /// cannot do this at Start() as there is not host at the start
    /// </summary>
    public void PlayerJoinOrHost()
    {
        if (IsHost)
        {
            seed = GenerateNewSeed();
            InitializeSeed(seed);
            SendRestockSignalToAll();
            StartCoroutine(RestockTimer(5));
        }
        else
        {
            MultiplayerHandler.Instance.GetHostSeed();
        }
    }

    public override void OnNetworkSpawn()
    {
        netSeed.OnValueChanged += (int prevValue, int newValue) =>
        {
            SetSeed();
        };
    }

    private void SetSeed()
    {
        InitializeSeed(seed);
        SendRestockSignalToAll();
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

    /// <summary>
    /// Makes all players restock the item spawn points.
    /// </summary>
    public void SendRestockSignalToAll() {
        // Restock self
        Restock(1, 1, 1, 1, 1);

        // Send Restock signal to each other player.
        // TODO: Send signal to all players!
    }

    /// <summary>
    /// Processes all produce spawn points to add a new item to them.
    /// </summary>
    /// <param name="appleProb"></param>
    /// <param name="cornProb"></param>
    /// <param name="eggProb"></param>
    /// <param name="milkProb"></param>
    /// <param name="oilProb"></param>
    public void Restock(float appleProb, float cornProb, float drumstickProb, float eggProb, float oilProb) {
        // Fill out the spawn data list with the new probabilities
        allSpawnData[0].probability = appleProb;
        allSpawnData[1].probability = cornProb;
        allSpawnData[2].probability = drumstickProb;

        // Destroy all items that have not been grabbed from the produce spawners
        foreach (ProduceSpawner produceSpawner in allProduceSpawners) {
            produceSpawner.DestroyItemOnProduceSpawner();
        }

        // Sum probabilities to get the proper probabilities for each item to spawn
        int i = 0;
        foreach (ProduceSpawnData spawnData in allSpawnData) {
            spawnDenominator += spawnData.probability;
            allSpawnData[i].probability = spawnDenominator;
            i++;
        }

        // Spawn an item at each produce spawner based on the seed
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