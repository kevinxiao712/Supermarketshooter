using UnityEngine;

public class Bottle_Part : Gun_Piece_Base
{
    public Material stealthMaterial;
    Material baseMaterial;
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
                baseMaterial = gun.playerObject.GetComponent<MeshRenderer>().material;
                gun.playerObject.GetComponent<MeshRenderer>().material = stealthMaterial;
                break;
            case GunPieceState.Back:
                gun.shootForce = shootForce;
                gun.magazineSize = magazineSize;
                gun.isFiringBullets = true;
                gun.playerObject.GetComponent<MeshRenderer>().material = baseMaterial;
                // Example: Debuff or different behavior
                break;
        }
    }
}
