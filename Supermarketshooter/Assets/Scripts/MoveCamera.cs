using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

     void Update()
    {
        if (cameraPosition)
        {
            // Follow the assigned transform
            transform.position = cameraPosition.position;
        }
    }
}
