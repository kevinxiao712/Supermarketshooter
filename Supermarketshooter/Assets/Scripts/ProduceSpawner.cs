using UnityEngine;

public class ProduceSpawner : MonoBehaviour
{
    public ProduceSpawnData[] allSpawnData;
    
    

    private float spawnDenominator = 0;
    private float spawnNumerator = 0;

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
                Instantiate(spawnData.produceReference, gameObject.transform);
                break;
            }
        }
    }
}

[System.Serializable]
public struct ProduceSpawnData {
    public GameObject produceReference;
    public float probability;
}
