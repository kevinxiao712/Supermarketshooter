using UnityEngine;
using System.Collections.Generic;

public class Gun_Piece_Base : MonoBehaviour
{
    public enum GunPieceState { Forward, Mid, Back }
    public GunPieceState currentState;
    // Bullet settings
    public GameObject bulletPrefab;
    public float shootForce, upwardForce;

    // Gun settings
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots, damage;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    // Recoil settings
    public Rigidbody playerRb;
    public float recoilForce;
    public Gun_Base gun;
    public GameObject Bullets;
    public bool isFiringBullets = true;

    public void UpdateState(int position)
    {
        switch (position)
        {
            case 2:
                currentState = GunPieceState.Forward;
                break;
            case 1:
                currentState = GunPieceState.Mid;
                break;
            case 0:
                currentState = GunPieceState.Back;
                break;
            default:
                Debug.LogWarning("Invalid position for Gun_Piece_Base");
                break;
        }
        ApplyStateEffects();
    }

    public virtual void ApplyStateEffects()
    {
        // Apply visual or gameplay changes based on state
        switch (currentState)
        {
            case GunPieceState.Forward:
                // Example: Modify gun stats
                break;
            case GunPieceState.Mid:
                // Example: Neutral state
                break;
            case GunPieceState.Back:
                // Example: Debuff or different behavior
                break;
        }
    }
}
