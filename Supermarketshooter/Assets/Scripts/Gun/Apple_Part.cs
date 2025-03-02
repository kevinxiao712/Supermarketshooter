using UnityEngine;

public class Apple_Part : Gun_Piece_Base{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void ApplyStateEffects()
    {
        allowButtonHold = false;
        shootForce = 50;
       timeBetweenShooting = 1.5f;
       timeBetweenShots = 0;
       bulletsPerTap = 5;
       spread = 3;
      magazineSize = 10;

        // Apply visual or gameplay changes based on state
        switch (currentState)
        {
            case GunPieceState.Forward:
                gun.allowButtonHold = allowButtonHold;
                gun.shootForce = shootForce;
                gun.timeBetweenShooting = timeBetweenShooting;
                gun.damage = damage; 

                // Example: Modify gun stats
                break;
            case GunPieceState.Mid:
                Debug.Log("add health");
                // Example: Neutral state
                break;
            case GunPieceState.Back:
                gun.timeBetweenShots = timeBetweenShots;
                gun.bulletsPerTap = bulletsPerTap;
                gun.spread = spread;
                gun.magazineSize = magazineSize;
                gun.isFiringBullets = true;
                // Example: Debuff or different behavior
                break;
        }
    }
}
