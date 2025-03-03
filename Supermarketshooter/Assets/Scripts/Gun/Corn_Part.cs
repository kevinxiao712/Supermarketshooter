using UnityEngine;

public class Corn_Part : Gun_Piece_Base
{
    public override void ApplyStateEffects()
    {

        // Apply visual or gameplay changes based on state
        switch (currentState)
        {
            case GunPieceState.Forward:
                gun.allowButtonHold = allowButtonHold;
                gun.timeBetweenShooting = timeBetweenShooting;
                gun.damage = damage;
                gun.timeBetweenShots = timeBetweenShots;
                gun.bulletsPerTap = bulletsPerTap;
                gun.spread = spread;
                // Example: Modify gun stats
                break;
            case GunPieceState.Mid:
                Debug.Log("add damage");
                // Example: Neutral state
                break;
            case GunPieceState.Back:
                gun.shootForce = shootForce;
                gun.magazineSize = magazineSize;
                gun.isFiringBullets = true;
                // Example: Debuff or different behavior
                break;
        }
    }
}
