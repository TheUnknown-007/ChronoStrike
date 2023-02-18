using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class ScriptableWeapon : ScriptableObject
{   
    public bool automatic;
    public int magSize;
    public float lineLength;
    public float fireRate;
    public float spreadFactor;
    public float bulletSpeed;
    public float bulletDamage;
    public float reloadTime;
    public float collateralDamage;
    public float collateralRadius;
    public bool shake;
    public AudioClip gunSound;
    public GameObject impactParticles;
    public GameObject bullet;
    public GameObject muzzleFlash;
    public Gradient bulletColor;
}
