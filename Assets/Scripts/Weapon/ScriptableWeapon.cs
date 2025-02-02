using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class ScriptableWeapon : ScriptableObject
{   
    public int upgradeKills;
    public bool automatic;
    public int magSize;
    public float fireRate;
    public float burstRate;
    public int bulletsPerBurst;
    public float spreadFactor;
    public float bulletSpeed;
    public float bulletDamage;
    public float lineLength;
    public float reloadTime;
    public float recoilMagnitude;
    public float recoilReturnSpeed;
    public float recoilSnappines;
    public float weaponRecoil;
    public float swaySmooth;
    public float swayMultiplier;
    public float collateralDamage;
    public float collateralRadius;
    public bool shake;
    public AudioClip gunSound;
    public GameObject impactParticles;
    public GameObject bullet;
    public GameObject muzzleFlash;
    public Gradient bulletColor;
}
