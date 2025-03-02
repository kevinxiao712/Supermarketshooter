using UnityEngine;

public class ProduceSpawner : MonoBehaviour
{
    public GameObject heldObj = null;
    private SeedGenManager seedGenManager;

    private void Start() {
        seedGenManager = FindFirstObjectByType<SeedGenManager>();
        seedGenManager.AddProduceSpawner(this);
    }

    /// <summary>
    /// Destroys whatever Produce this spawner is holding, and replaces it with a new piece of produce.
    /// </summary>
    /// <param name="spawnObj"></param>
    public void SpawnProduce(GameObject spawnObj) {
        if (heldObj != null) {
            // find all game objects in a small radius; If they are the previously held gameobject, destroy them!
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.01f);
            foreach (var hitCollider in hitColliders) {
                if (hitCollider.gameObject == heldObj) {
                    Destroy(hitCollider.gameObject);
                }
            }
        }
        heldObj = Instantiate(spawnObj, gameObject.transform.position, Quaternion.identity, gameObject.transform);
    }

    /*
    public ProduceSpawnData[] allSpawnData;

    private float spawnDenominator = 0;
    private float spawnNumerator = 0;

    private void Awake() {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int i = 0;
        foreach (ProduceSpawnData spawnData in allSpawnData) {
            spawnDenominator += spawnData.probability;
            allSpawnData[i].probability = spawnDenominator;
            i++;
        }

        spawnNumerator = Random.Range(0, spawnDenominator);

        foreach (ProduceSpawnData spawnData in allSpawnData) {
            if (spawnNumerator <= spawnData.probability) {
                Instantiate(spawnData.produceReference, gameObject.transform.position, Quaternion.identity, gameObject.transform);
                break;
            }
        }
    }
    */
}

/*
[System.Serializable]
public struct ProduceSpawnData {
    public GameObject produceReference;
    public float probability;
}
*/