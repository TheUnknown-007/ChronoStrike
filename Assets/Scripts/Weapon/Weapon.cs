using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public ScriptableWeapon weaponData;
    [SerializeField] Transform recoilObject;
    [SerializeField] Animator GunAnim;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Transform[] bulletPoints;
    bool canFire;
    bool firing;
    bool reloading;
    int magCount;

    Vector3 currentRotation;
    Vector3 targetRotation;

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
        // Recoil
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, weaponData.recoilReturnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, weaponData.recoilSnappines * Time.fixedDeltaTime);
        recoilObject.localRotation = Quaternion.Euler(currentRotation);
        
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
                ShootWeapon();
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
                magCount--;
                ShootWeapon();
                if(magCount <= 0) 
                {
                    canFire = false;
                    break;
                }
                yield return new WaitForSeconds(weaponData.fireRate);
            }
    }

    void ShootWeapon()
    {
        targetRotation += new Vector3(weaponData.recoilMagnitude, 0, 0);

        PlayerManager.instance.AmmoChange(weaponData.magSize, magCount);
        audioSource.PlayOneShot(weaponData.gunSound);
        float xRot = Random.Range(-weaponData.spreadFactor, weaponData.spreadFactor);
        float yRot = Random.Range(-weaponData.spreadFactor, weaponData.spreadFactor);
        foreach(Transform bulletPoint in bulletPoints)
        {
            Quaternion shootDir = Quaternion.Euler(bulletPoint.rotation.eulerAngles.x + xRot, bulletPoint.rotation.eulerAngles.y + yRot, bulletPoint.rotation.eulerAngles.z);
            GameObject flash = Instantiate(weaponData.muzzleFlash, bulletPoint.position, bulletPoint.rotation);
            flash.transform.rotation = flash.transform.rotation * Quaternion.Euler(0, 0, Random.Range(0, -90));
            Destroy(flash, 0.05f);
            Instantiate(weaponData.bullet, bulletPoint.position, shootDir).GetComponent<Bullet>().Instantiate(0, weaponData.shake, weaponData.bulletSpeed, weaponData.bulletDamage, weaponData.collateralDamage, weaponData.collateralRadius, weaponData.lineLength, weaponData.bulletColor, weaponData.impactParticles);
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
