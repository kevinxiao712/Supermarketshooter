using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerHandler : NetworkBehaviour
{
    public static MultiplayerHandler Instance { get; private set; }

    [SerializeField] private BulletList bulletList;

    private void Awake()
    {
        Instance = this;
    }

    public void Shoot()
    {

    }
}
