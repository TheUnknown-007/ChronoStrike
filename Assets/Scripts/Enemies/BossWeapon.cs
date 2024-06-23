using System.Collections;
using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public LayerMask visiblityMask;

    [SerializeField] AudioSource gunSource;
    [SerializeField] Transform bulletParent;
    public Transform bulletPoint;
    public bool firing;
    float FireDelay;
    GameObject player;
    ScriptableWeapon currentWeapon;

    public void Init(ScriptableWeapon weapon, float fireDelay)
    {
        player = PlayerManager.instance.gameObject;
        currentWeapon = weapon;
        FireDelay = fireDelay;
    }

    void Update()
    {
        bulletParent.transform.LookAt(player.transform);
    }

    public void StartShooting()
    {
        StartCoroutine(FireAtPlayer());
    }

    public void AddDamage(float damage)
    {
        return;
    }

    public IEnumerator FireAtPlayer()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        while(firing)
        {
            if(currentWeapon.automatic)
            {
                int x = 0;
                while(x<currentWeapon.magSize/2)
                {
                    gunSource.PlayOneShot(currentWeapon.gunSound);
                    Destroy(Instantiate(currentWeapon.muzzleFlash, bulletPoint.position, bulletPoint.rotation), 0.05f);
                    Instantiate(currentWeapon.bullet, bulletPoint.position, bulletPoint.transform.rotation).GetComponent<Bullet>().Instantiate(1, currentWeapon.shake, currentWeapon.bulletSpeed, currentWeapon.bulletDamage, currentWeapon.collateralDamage, currentWeapon.collateralRadius, currentWeapon.lineLength, currentWeapon.bulletColor, currentWeapon.impactParticles);
                    yield return new WaitForSeconds(currentWeapon.fireRate);
                    x+=1;
                }
            }
            else
            {
                gunSource.PlayOneShot(currentWeapon.gunSound);
                Destroy(Instantiate(currentWeapon.muzzleFlash, bulletPoint.position, bulletPoint.rotation), 0.05f);
                Instantiate(currentWeapon.bullet, bulletPoint.position, bulletPoint.transform.rotation).GetComponent<Bullet>().Instantiate(1, currentWeapon.shake, currentWeapon.bulletSpeed, currentWeapon.bulletDamage, currentWeapon.collateralDamage, currentWeapon.collateralRadius, currentWeapon.lineLength, currentWeapon.bulletColor, currentWeapon.impactParticles);
            }
            yield return new WaitForSeconds(FireDelay);
        }
    }
}