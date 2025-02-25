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
                gun.shootForce = shootForce;
                gun.timeBetweenShooting = timeBetweenShooting;

                // Example: Modify gun stats
                break;
            case GunPieceState.Mid:
                Debug.Log("add damage");
                // Example: Neutral state
                break;
            case GunPieceState.Back:
                gun.timeBetweenShots = timeBetweenShots;
                gun.bulletsPerTap = bulletsPerTap;
                gun.spread = spread;
                gun.magazineSize = magazineSize;
                // Example: Debuff or different behavior
                break;
        }
    }
}
