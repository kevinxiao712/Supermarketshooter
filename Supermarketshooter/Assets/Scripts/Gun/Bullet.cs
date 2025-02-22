using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 3f;
    void OnEnable()
    {
        Invoke("Deactivate", lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Deactivate();
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
