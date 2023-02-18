using DitzeGames.Effects;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static Vector3 bossPosition { get; private set; }
    [SerializeField] int type = 0;
    [SerializeField] GameObject explosion;
    [SerializeField] LayerMask visiblityMask;

    [SerializeField] AudioSource gunSource;
    [SerializeField] Transform bulletPoint;
    [SerializeField] Transform bulletParent;
    [Space, SerializeField] BossWeapon[] bossWeapons;
    [SerializeField] Transform model;
    Slider HealthSlider;
    bool firing;
    GameObject player;
    RaycastHit hit;
    float FireDelay;
    int reward;
    float currentHealth;
    float totalHealth;
    ScriptableWeapon[] currentWeapons;

    public void Init(ScriptableWeapon[] weapon, float health, float fireDelay, Slider healthSlider)
    {
        if(health <= 10) reward = 10;
        else if(health >= 11) reward = 20;
        else if(type == 1) reward = 500;
        else reward = 30;
        currentHealth = health;
        player = PlayerManager.instance.gameObject;
        currentWeapons = weapon;
        FireDelay = fireDelay;
        if(type == 1)
        {
            totalHealth = health;
            HealthSlider = healthSlider;
            for(int x = 0; x < bossWeapons.Length; x++)
            {
                if(x == 0) bossWeapons[0].Init(weapon[0], 50, fireDelay*2);
                else bossWeapons[x].Init(weapon[1], 30, fireDelay);
            }
            bossPosition = this.transform.position;
        }
    }

    void Update()
    {
        if(type != 1) bulletParent.transform.LookAt(player.transform);
        else
        {
            var lookPos = player.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            model.transform.rotation = rotation;
        }
    }

    void FixedUpdate()
    {
        if(type == 1) return;
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
        currentHealth -= damage;
        if(type == 1) HealthSlider.value = currentHealth / totalHealth;
        if(currentHealth <= 0) Die();
    }

    void Die()
    {
        PlayerManager.instance.AddScore(reward);
        if(type == 1) PlayerManager.instance.DefeatBoss();
        CameraEffects.ShakeOnce();
        Destroy(Instantiate(explosion, transform.position, transform.rotation), 3);
        Destroy(gameObject);
    }

    IEnumerator FireAtPlayer()
    {
        while(firing)
        {
            if(currentWeapons[0].automatic)
            {
                int x = 0;
                while(x<currentWeapons[0].magSize/2)
                {
                    gunSource.PlayOneShot(currentWeapons[0].gunSound);
                    Destroy(Instantiate(currentWeapons[0].muzzleFlash, bulletPoint.position, bulletPoint.rotation), 0.05f);
                    Instantiate(currentWeapons[0].bullet, bulletPoint.position, bulletPoint.transform.rotation).GetComponent<Bullet>().Instantiate(1, currentWeapons[0].shake, currentWeapons[0].bulletSpeed, currentWeapons[0].bulletDamage, currentWeapons[0].collateralDamage, currentWeapons[0].collateralRadius, currentWeapons[0].lineLength, currentWeapons[0].bulletColor, currentWeapons[0].impactParticles);
                    yield return new WaitForSeconds(currentWeapons[0].fireRate);
                    x+=1;
                }
            }
            else
            {
                gunSource.PlayOneShot(currentWeapons[0].gunSound);
                Destroy(Instantiate(currentWeapons[0].muzzleFlash, bulletPoint.position, bulletPoint.rotation), 0.05f);
                Instantiate(currentWeapons[0].bullet, bulletPoint.position, bulletPoint.transform.rotation).GetComponent<Bullet>().Instantiate(1, currentWeapons[0].shake, currentWeapons[0].bulletSpeed, currentWeapons[0].bulletDamage, currentWeapons[0].collateralDamage, currentWeapons[0].collateralRadius, currentWeapons[0].lineLength, currentWeapons[0].bulletColor, currentWeapons[0].impactParticles);
            }
            yield return new WaitForSeconds(FireDelay);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.transform.CompareTag("Player") || type != 1) return;
        PlayerManager.instance.TriggerBoss();
    }
}