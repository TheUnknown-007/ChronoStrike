using System.Collections;
using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    ScriptableObject weapon;
    [SerializeField] GameObject explosion;
    [SerializeField] LayerMask visiblityMask;

    [SerializeField] AudioSource gunSource;
    [SerializeField] Transform bulletPoint;
    [SerializeField] Transform bulletParent;
    bool firing;
    GameObject player;
    RaycastHit hit;
    float FireDelay;

    float currentHealth;
    ScriptableWeapon currentWeapon;

    public void Init(ScriptableWeapon weapon, float health, float fireDelay)
    {
        currentHealth = health;
        player = PlayerManager.instance.gameObject;
        currentWeapon = weapon;
        FireDelay = fireDelay;
    }

    void Update()
    {
        bulletParent.transform.LookAt(player.transform);
    }

    void FixedUpdate()
    {
        if(Physics.Raycast(bulletPoint.position, bulletPoint.transform.forward, out hit, 1000, visiblityMask))
        {
            Debug.DrawLine(bulletPoint.position, hit.point);
            if(hit.collider.CompareTag("Player") && !PlayerManager.instance.isDead)
            {
                if(firing) return;
                firing = true;
                StartCoroutine(FireAtPlayer());
            }
            else
            {
                StopAllCoroutines();
                firing = false;
            }
        }
        else
        {
            StopAllCoroutines();
            firing = false;
        }
    }

    public void AddDamage(float damage)
    {
        return;
    }

    IEnumerator FireAtPlayer()
    {
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