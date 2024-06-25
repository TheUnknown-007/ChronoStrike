using DitzeGames.Effects;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Video;

public class Enemy : MonoBehaviour
{
    public static Vector3 bossPosition { get; private set; }
    [SerializeField] int type = 0;
    [SerializeField] float orbitSpeed;
    [SerializeField] float moveSpeed;
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
    float currentHealth;
    float totalHealth;
    ScriptableWeapon[] currentWeapons;
    bool alive = true;
    RoomBehaviour room;
    int currentPointIndex = 0;
    Vector2 target;
    Vector3 tempPos;

    Vector3 targetPosition;
    Vector3 initialPosition;

    public void Init(ScriptableWeapon[] weapon, float health, float fireDelay, Slider healthSlider, RoomBehaviour roomSpawner, int pointIndex)
    {
        initialPosition = transform.position;

        currentHealth = health;
        player = PlayerManager.instance.gameObject;
        currentWeapons = weapon;
        FireDelay = fireDelay;

        if(type == 1)
        {
            totalHealth = health;
            HealthSlider = healthSlider;
            bossWeapons[0].Init(weapon[0], fireDelay*2);
            for(int x = 1; x < bossWeapons.Length; x++)
                bossWeapons[x].Init(weapon[1], fireDelay);
            bossPosition = transform.position;
        }

        else if(type == 2)
        {
            room = roomSpawner;
            room.dronePointUsed[currentPointIndex] = false;
            currentPointIndex = pointIndex;
        }
    }

    public void StopShooting()
    {
        firing = false;
        foreach(BossWeapon bW in bossWeapons)
        {
            if(bW != bossWeapons[0]) 
            {
                bW.StopAllCoroutines();
                bW.firing = false;
            }
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

        if(type == 2)
        {
            if(firing)
            {
                targetPosition = transform.position;

                // Orbit around a fixed distance
                transform.RotateAround(PlayerManager.instance.transform.position, Vector3.up, Time.deltaTime*orbitSpeed);
                
                tempPos = transform.position;
                tempPos.y = PlayerManager.instance.transform.position.y;
                tempPos -= PlayerManager.instance.transform.position;

                transform.position = PlayerManager.instance.transform.position + tempPos.normalized * target.y;

                // Clamp the drone movement around the room bounds
                tempPos = transform.position;
                tempPos.x = Mathf.Clamp(tempPos.x, room.transform.position.x-11, room.transform.position.x+11);
                tempPos.z = Mathf.Clamp(tempPos.z, room.transform.position.z-10, room.transform.position.z+11);
                tempPos.y = Mathf.Clamp(target.x, 2, 5);

                // Save the calculated values
                transform.position = targetPosition;
                targetPosition = tempPos;
            }
            else targetPosition = initialPosition;

            // Move Towards the calculated position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime*moveSpeed);
        }
    }

    void FixedUpdate()
    {
        if(type == 1) 
        {
            bool found = false;
            foreach(BossWeapon bW in bossWeapons)
            {
                if(Physics.Raycast(bW.bulletPoint.position, bW.bulletPoint.transform.forward, out hit, 1000, bW.visiblityMask))
                {
                    if(hit.collider.CompareTag("Player"))
                    {
                        found = true;
                        break;
                    }
                }
            }

            if(found && firing) return;

            if(found)
            {
                firing = true;
                foreach(BossWeapon bW in bossWeapons)
                {
                    bW.firing = true;
                    bW.StartShooting();
                }
            }
            else StopShooting();

            return;          
        }

        if(Physics.Raycast(bulletPoint.position, bulletPoint.transform.forward, out hit, 1000, visiblityMask))
        {
            if(hit.collider.CompareTag("Player"))
            {
                if(firing) return;
                firing = true;
                StartCoroutine(FireAtPlayer());
                if(type == 2) EnableBehavior();
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

    public void EnableBehavior()
    {
        StartCoroutine(DroneHeightBehaviour());
        StartCoroutine(DroneBehaviour());
    }

    IEnumerator DroneHeightBehaviour()
    {
        yield return new WaitForSeconds(1f);
        while(true)
        {
            target.x += Random.Range(-3f,3f);
            target.x = Mathf.Clamp(target.x, 2, 5);
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator DroneBehaviour()
    {
        yield return new WaitForSeconds(1f);
        while(true)
        {
            target.y += Random.Range(-3f,3f);
            target.y = Mathf.Clamp(target.y, 4f,9f);
            yield return new WaitForSeconds(1.5f);
        }
    }

    public void AddDamage(float damage)
    {
        currentHealth -= damage;
        if(type == 1) HealthSlider.value = currentHealth / totalHealth;
        if(currentHealth <= 0 && alive) Die();
    }

    void Die()
    {
        alive = false;
        CameraEffects.ShakeOnce();
        Destroy(Instantiate(explosion, transform.position, transform.rotation), 3);
        Destroy(gameObject);
    }

    IEnumerator FireAtPlayer()
    {
        yield return new WaitForSeconds(Random.Range(0.25f, 0.5f));

        while(firing)
        {
            if(currentWeapons[0].automatic)
            {
                int x = 0;
                while(x<currentWeapons[0].magSize && firing)
                {
                    gunSource.PlayOneShot(currentWeapons[0].gunSound);
                    GameObject flash = Instantiate(currentWeapons[0].muzzleFlash, bulletPoint.position, bulletPoint.rotation);
                    flash.layer = 0;
                    foreach(Transform t in flash.transform) t.gameObject.layer = 0;
                    Destroy(flash, 0.05f);
                    Instantiate(currentWeapons[0].bullet, bulletPoint.position, bulletPoint.transform.rotation).GetComponent<Bullet>().Instantiate(1, currentWeapons[0].shake, currentWeapons[0].bulletSpeed, currentWeapons[0].bulletDamage, currentWeapons[0].collateralDamage, currentWeapons[0].collateralRadius, currentWeapons[0].lineLength, currentWeapons[0].bulletColor, currentWeapons[0].impactParticles);
                    yield return new WaitForSeconds(currentWeapons[0].fireRate);
                    x+=1;
                }
            }
            else
            {
                for(int i = 0; i < currentWeapons[0].bulletsPerBurst && firing; i++)
                {
                    gunSource.PlayOneShot(currentWeapons[0].gunSound);
                    GameObject flash = Instantiate(currentWeapons[0].muzzleFlash, bulletPoint.position, bulletPoint.rotation);
                    flash.layer = 0;
                    foreach(Transform t in flash.transform) t.gameObject.layer = 0;
                    Destroy(flash, 0.05f);
                    Instantiate(currentWeapons[0].bullet, bulletPoint.position, bulletPoint.transform.rotation).GetComponent<Bullet>().Instantiate(1, currentWeapons[0].shake, currentWeapons[0].bulletSpeed, currentWeapons[0].bulletDamage, currentWeapons[0].collateralDamage, currentWeapons[0].collateralRadius, currentWeapons[0].lineLength, currentWeapons[0].bulletColor, currentWeapons[0].impactParticles);
                    yield return new WaitForSeconds(currentWeapons[0].burstRate);
                }
                yield return new WaitForSeconds(currentWeapons[0].fireRate);
            }
            yield return new WaitForSeconds(FireDelay);
        }
    }
}