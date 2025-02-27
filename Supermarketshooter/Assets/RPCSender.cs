using UnityEngine;
using Unity.Netcode;

public class RPCSender : NetworkBehaviour
{
    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {

            }
        }
    }
}
