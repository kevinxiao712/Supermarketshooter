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
                gun.timeBetweenShooting = timeBetweenShooting;
                gun.damage = damage;
                gun.timeBetweenShots = timeBetweenShots;
                gun.bulletsPerTap = bulletsPerTap;
                gun.spread = spread;
                // Example: Modify gun stats
                break;
            case GunPieceState.Mid:
                gun.playerHealth.baseMaxHealth = 200;
                gun.playerHealth.currentHealth += 100;
                Debug.Log("add damage");
                // Example: Neutral state
                break;
            case GunPieceState.Back:
                if (gun.playerHealth.currentHealth > 100)
                    gun.playerHealth.currentHealth = 100;
                gun.playerHealth.baseMaxHealth = 100;
                gun.shootForce = shootForce;
                gun.magazineSize = magazineSize;
                gun.isFiringBullets = true;
                // Example: Debuff or different behavior
                break;
        }
    }
}
