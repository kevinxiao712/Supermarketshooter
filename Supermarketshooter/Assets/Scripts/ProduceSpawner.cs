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
    /// Spawns the given produce at the spawner location.
    /// </summary>
    /// <param name="spawnObj"></param>
    public void SpawnProduce(GameObject spawnObj) {
        heldObj = Instantiate(spawnObj, gameObject.transform.position, Quaternion.identity, gameObject.transform);
    }

    /// <summary>
    /// Destroy whatever Produce this spawner is currently holding.
    /// </summary>
    public void DestroyItemOnProduceSpawner() {
        if (heldObj != null) {
            // find all game objects in a small radius; If they are the previously held gameobject, destroy them!
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.01f);
            foreach (var hitCollider in hitColliders) {
                if (hitCollider.gameObject == heldObj) {

                    Destroy(hitCollider.gameObject);
                }
            }
        }
    }
}