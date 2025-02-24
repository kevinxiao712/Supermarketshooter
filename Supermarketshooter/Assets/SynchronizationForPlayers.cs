using UnityEngine;
using Unity.Netcode;

public class SynchronizationForPlayers : NetworkBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private AudioListener listener;


    //public override void OnNetworkSpawn()
    //{
    //    if (IsOwner)
    //    {
    //        listener.enabled = true;
    //        //camera.priority
    //    }
    //    else
    //    {

    //    }
    //}
}
