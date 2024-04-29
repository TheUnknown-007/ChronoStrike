using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public ScriptableWeapon weaponData;
    [SerializeField] Animator GunAnim;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Transform bulletPoint;
    bool canFire;
    bool firing;
    bool reloading;
    int magCount;

    void Start()
    {
        magCount = weaponData.magSize;
        canFire = true;
        firing = false;
    }

    void OnEnable()
    {
        PlayerManager.instance.AmmoChange(weaponData.magSize, weaponData.magSize);
    }

    void Update()
    {
        if(canFire && !firing && Input.GetButtonDown("Fire1")) StartCoroutine(Shoot());
        if(firing && weaponData.automatic && Input.GetButtonUp("Fire1")) 
        {
            firing = false;
            StopAllCoroutines();
        }
        if(!reloading && magCount < weaponData.magSize && Input.GetButtonDown("Reload")) 
        {
            firing = false;
            StopAllCoroutines();
            StartCoroutine(Reload());
        }
    }

    IEnumerator Shoot()
    {
        firing = true;
        if(!weaponData.automatic)
        {
            for(int i = 0; i < weaponData.bulletsPerBurst && canFire; i++)
            {
                magCount--;
                PlayerManager.instance.AmmoChange(weaponData.magSize, magCount);
                audioSource.PlayOneShot(weaponData.gunSound);
                float xRot = Random.Range(-weaponData.spreadFactor, weaponData.spreadFactor);
                float yRot = Random.Range(-weaponData.spreadFactor, weaponData.spreadFactor);
                Quaternion shootDir = Quaternion.Euler(bulletPoint.rotation.eulerAngles.x + xRot, bulletPoint.rotation.eulerAngles.y + yRot, bulletPoint.rotation.eulerAngles.z);
                Destroy(Instantiate(weaponData.muzzleFlash, bulletPoint.position, bulletPoint.rotation), 0.05f);
                Instantiate(weaponData.bullet, bulletPoint.position, shootDir).GetComponent<Bullet>().Instantiate(0, weaponData.shake, weaponData.bulletSpeed, weaponData.bulletDamage, weaponData.collateralDamage, weaponData.collateralRadius, weaponData.lineLength, weaponData.bulletColor, weaponData.impactParticles);
                if(magCount <= 0) canFire = false;
                yield return new WaitForSeconds(weaponData.burstRate);
            }
            yield return new WaitForSeconds(weaponData.fireRate);
            firing = false;
            yield break;
        }
        else
            while(firing)
            {
                float xRot = Random.Range(-weaponData.spreadFactor, weaponData.spreadFactor);
                float yRot = Random.Range(-weaponData.spreadFactor, weaponData.spreadFactor);
                Quaternion shootDir = Quaternion.Euler(bulletPoint.rotation.eulerAngles.x + xRot, bulletPoint.rotation.eulerAngles.y + yRot, bulletPoint.rotation.eulerAngles.z);

                magCount--;
                PlayerManager.instance.AmmoChange(weaponData.magSize, magCount);
                audioSource.PlayOneShot(weaponData.gunSound);
                Destroy(Instantiate(weaponData.muzzleFlash, bulletPoint.position, bulletPoint.rotation), 0.05f);
                Instantiate(weaponData.bullet, bulletPoint.position, shootDir).GetComponent<Bullet>().Instantiate(0, weaponData.shake, weaponData.bulletSpeed, weaponData.bulletDamage, weaponData.collateralDamage, weaponData.collateralRadius, weaponData.lineLength, weaponData.bulletColor, weaponData.impactParticles);
                if(magCount <= 0) 
                {
                    canFire = false;
                    break;
                }
                yield return new WaitForSeconds(weaponData.fireRate);
            }
    }

    IEnumerator Reload()
    {
        reloading = true;
        canFire = false;
        GunAnim.Play("Reload");
        yield return new WaitForSeconds(weaponData.reloadTime);
        GunAnim.Play("Idle");
        magCount = weaponData.magSize;
        PlayerManager.instance.AmmoChange(weaponData.magSize, magCount);
        canFire = true;
        reloading = false;
    }

    public void ResetMag()
    {
        GunAnim.Play("Idle");
        magCount = weaponData.magSize;
        PlayerManager.instance.AmmoChange(weaponData.magSize, magCount);
        canFire = true;
        reloading = false;
    }
}
