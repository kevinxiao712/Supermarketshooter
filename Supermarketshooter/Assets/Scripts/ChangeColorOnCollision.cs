using UnityEngine;

public class ChangeColorOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object has the tag "Bullet"
        if (collision.gameObject.CompareTag("Bullet"))
        {

            GetComponent<Renderer>().material.color = Color.red;


        }
    }
}
