using UnityEngine;

public class Eggs_Part : Gun_Piece_Base
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
                gun.DamageMuliplayer = 2;
                gun.playerHealth.maxHealth = 50;
                if (gun.playerHealth.currentHealth > 50)
                    gun.playerHealth.currentHealth = 50;
                // Example: Neutral state
                break;
            case GunPieceState.Back:
                gun.playerHealth.maxHealth = 100;
                gun.DamageMuliplayer = 1;
                gun.shootForce = shootForce;
                gun.magazineSize = magazineSize;
                gun.isFiringBullets = true;
                // Example: Debuff or different behavior
                break;
        }
    }
}
